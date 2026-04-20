using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace LawEditor.Services.WordServises
{
    public class WordFileWritingService
    {

        public void WriteWordFile(string filePath, Laws laws)
        {
            try
            {
                using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

                var mainPart = doc.AddMainDocumentPart();

                var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
                numberingPart.Numbering = new Numbering(
                    new AbstractNum(
                        new Level(
                            new NumberingFormat() { Val = NumberFormatValues.Decimal },
                            new LevelText() { Val = "%1." },
                            new StartNumberingValue() { Val = 1 }
                        )
                        { LevelIndex = 0 }
                    )
                    { AbstractNumberId = 1 },
                    new NumberingInstance(
                        new AbstractNumId() { Val = 1 }
                    )
                    { NumberID = 1 }
                );

                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // ── Создаём EndnotesPart сразу, всегда ──
                var endnotePart = mainPart.AddNewPart<EndnotesPart>();
                endnotePart.Endnotes = new Endnotes();

                endnotePart.Endnotes.Append(new Endnote(
                    new Paragraph(new Run(new SeparatorMark())))
                {
                    Type = FootnoteEndnoteValues.Separator,
                    Id = -1
                });
                endnotePart.Endnotes.Append(new Endnote(
                    new Paragraph(new Run(new ContinuationSeparatorMark())))
                {
                    Type = FootnoteEndnoteValues.ContinuationSeparator,
                    Id = 0
                });

                var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylePart.Styles = new Styles();

                // ── Строим маппинг: amendmentId (строка) → int endnoteId ──
                // Числовые ID остаются как есть, нечисловые (KM1, KQ1...) получают
                // уникальные ID начиная с 1001 чтобы не конфликтовать с числовыми.
                var amendments = laws.SourceData.FirstOrDefault(s => s.Id == 3);
                var idMapping = BuildIdMapping(amendments);

                // ── HEADER ──
                if (laws.UpperObjects.Count > 0 && laws.UpperObjects[0].Headers.Count > 0)
                {
                    var headerText = laws.UpperObjects[0].Headers[0].FullText;
                    if (!string.IsNullOrWhiteSpace(headerText))
                    {
                        var lines = headerText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                            body.Append(CreateParagraph(line, align: JustificationValues.Center, indent: false));
                    }
                    body.Append(CreateEmptyParagraph());
                }

                // ── CHAPTERS ──
                foreach (var chapter in laws.Chapters)
                {
                    string chapterOrdinal = ToAzerbaijaniOrdinal(chapter.Id);

                    body.Append(CreateParagraph($"{chapterOrdinal} BÖLMƏ",
                        bold: true, align: JustificationValues.Center, indent: false));

                    if (!string.IsNullOrEmpty(chapter.Title))
                        body.Append(CreateParagraph(chapter.Title,
                            bold: true, align: JustificationValues.Center, indent: false));

                    body.Append(CreateEmptyParagraph());

                    foreach (var section in chapter.Sections)
                    {
                        string sectionRoman = ToRoman(section.Id);

                        body.Append(CreateParagraph($"{sectionRoman} fəsil",
                            align: JustificationValues.Center, indent: false));

                        if (!string.IsNullOrEmpty(section.Title))
                            body.Append(CreateParagraph(section.Title,
                                align: JustificationValues.Center, indent: false));

                        body.Append(CreateEmptyParagraph());

                        foreach (var article in section.Articles)
                        {
                            string articleId = FormatArticleId(article.Id);

                            var articlePara = CreateParagraph(
                                $"Maddə {articleId}. {article.Title}",
                                bold: true,
                                align: JustificationValues.Both,
                                indent: true
                            );
                            AppendEndnoteRef(articlePara, article.EndnoteId, idMapping);
                            body.Append(articlePara);

                            body.Append(CreateEmptyParagraph());

                            foreach (var clause in article.Clauses)
                            {
                                Paragraph clausePara;

                                if (clause.Number > 0)
                                    clausePara = CreateParagraphWithEndnote(
                                        $"{ToRoman(clause.Number)}. {clause.Text}",
                                        clause.EndnoteId, idMapping);
                                else if (!string.IsNullOrEmpty(clause.Text))
                                    clausePara = CreateParagraphWithEndnote(
                                        clause.Text,
                                        clause.EndnoteId, idMapping);
                                else continue;

                                body.Append(clausePara);

                                foreach (var sub in clause.SubClauses)
                                {
                                    body.Append(CreateParagraphWithEndnote(
                                        $"{sub.Number}) {sub.Text}",
                                        sub.EndnoteId, idMapping));
                                }
                            }

                            body.Append(CreateEmptyParagraph());
                        }
                    }
                }

                // ── TRANSITIONAL ──
                var transitional = laws.SourceData.FirstOrDefault(s => s.Id == 1);

                if (transitional?.Source.Count > 0)
                {
                    body.Append(CreateParagraph("KEÇİD MÜDDƏALARI",
                        bold: true, align: JustificationValues.Center, indent: false));
                    body.Append(CreateEmptyParagraph());

                    foreach (var item in transitional.Source.OfType<TransitionalProvisions>())
                        body.Append(CreateParagraph($"{item.Id}. {item.Title}",
                            align: JustificationValues.Both));

                    body.Append(new Paragraph(new Run(new Text(""))));

                    if (!string.IsNullOrWhiteSpace(TransitionalProvisions.Date))
                    {
                        var para = CreateParagraph("",
                            bold: true, align: JustificationValues.Start, indent: false);

                        var run = para.GetFirstChild<Run>();
                        run.RemoveAllChildren<Text>();

                        var dateLines = TransitionalProvisions.Date.Split('\n');
                        for (int i = 0; i < dateLines.Length; i++)
                        {
                            run.Append(new Text(dateLines[i])
                            {
                                Space = SpaceProcessingModeValues.Preserve
                            });
                            if (i < dateLines.Length - 1)
                                run.Append(new Break());
                        }

                        body.Append(para);
                    }

                    body.Append(CreateEmptyParagraph());
                }

                // ── SOURCES ──
                var sources = laws.SourceData.FirstOrDefault(s => s.Id == 2);

                if (sources?.Source.Count > 0)
                {
                    body.Append(CreateParagraph(
                        "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                        bold: true, align: JustificationValues.Center, indent: false, fontSize: "20"));

                    body.Append(CreateEmptyParagraph());

                    foreach (var item in sources.Source.OfType<SourceDocumentsList>())
                    {
                        var para = new Paragraph();

                        var pPr = new ParagraphProperties(
                            new NumberingProperties(
                                new NumberingLevelReference() { Val = 0 },
                                new NumberingId() { Val = 1 }
                            ),
                            new Indentation() { Left = "720", Hanging = "360" },
                            new Justification() { Val = JustificationValues.Both }
                        );
                        para.Append(pPr);

                        RunProperties baseProps = new RunProperties(
                            new RunFonts()
                            {
                                Ascii = "Palatino Linotype",
                                HighAnsi = "Palatino Linotype",
                                ComplexScript = "Palatino Linotype"
                            },
                            new FontSize() { Val = "20" }
                        );

                        if (!string.IsNullOrEmpty(item.LinkText) && !string.IsNullOrEmpty(item.Url))
                        {
                            var rel = mainPart.AddHyperlinkRelationship(new Uri(item.Url), true);
                            var hyperlink = new Hyperlink() { Id = rel.Id };

                            hyperlink.Append(new Run(
                                new RunProperties(
                                    new Underline() { Val = UnderlineValues.Single },
                                    new Color() { Val = "0000FF" },
                                    new FontSize() { Val = "20" }
                                ),
                                new Text(item.LinkText)
                                {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));

                            para.Append(hyperlink);
                            para.Append(new Run(new Text(" ")
                            {
                                Space = SpaceProcessingModeValues.Preserve
                            }));

                            string rest = item.Title.Replace(item.LinkText, "").Trim();
                            if (!string.IsNullOrEmpty(rest))
                            {
                                para.Append(new Run(
                                    (RunProperties)baseProps.CloneNode(true),
                                    new Text(rest) { Space = SpaceProcessingModeValues.Preserve }
                                ));
                            }
                        }
                        else
                        {
                            para.Append(new Run(baseProps,
                                new Text(item.Title) { Space = SpaceProcessingModeValues.Preserve }));
                        }

                        body.Append(para);
                    }

                    body.Append(CreateEmptyParagraph());
                }

                // ── AMENDMENTS (ENDNOTES) ──
                if (amendments?.Source.Count > 0)
                {
                    body.Append(CreateParagraph(
                         "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                         bold: true, align: JustificationValues.Center, indent: false, fontSize: "20"));
                    foreach (var item in amendments.Source.OfType<ConstitutionalAmendment>())
                    {
                        if (string.IsNullOrWhiteSpace(item.Title)) continue;

                        if (!idMapping.TryGetValue(item.Id, out int finalId))
                            continue;

                        bool isNumeric = int.TryParse(item.Id, out _);
                        string prefix = isNumeric ? "" : item.Id + " ";

                        var endnotePara = new Paragraph();

                        var refMark = new Run(new EndnoteReferenceMark());
                        refMark.RunProperties = new RunProperties(
                            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });
                        endnotePara.Append(refMark);

                        if (!string.IsNullOrEmpty(item.LinkText) && !string.IsNullOrEmpty(item.Url))
                        {
                            string titleText = prefix + item.Title;
                            int linkIndex = titleText.IndexOf(item.LinkText, StringComparison.Ordinal);

                            string before = linkIndex > 0
                                ? titleText.Substring(0, linkIndex)
                                : "";

                            endnotePara.Append(new Run(
                                new Text((before.Length > 0 ? " " + before : " "))
                                {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));

                            var endnoteRel = endnotePart.AddHyperlinkRelationship(
                                new Uri(item.Url), true);

                            var hyperlink = new Hyperlink() { Id = endnoteRel.Id };
                            hyperlink.Append(new Run(
                                new RunProperties(
                                    new Underline() { Val = UnderlineValues.Single },
                                    new Color() { Val = "0000FF" }
                                ),
                                new Text(item.LinkText)
                                {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));
                            endnotePara.Append(hyperlink);

                            string after = linkIndex >= 0
                                ? titleText.Substring(linkIndex + item.LinkText.Length).Trim()
                                : "";

                            if (!string.IsNullOrEmpty(after))
                            {
                                endnotePara.Append(new Run(
                                    new Text(" " + after)
                                    {
                                        Space = SpaceProcessingModeValues.Preserve
                                    }
                                ));
                            }
                        }
                        else
                        {
                            string fullText = prefix + item.Title;
                            endnotePara.Append(new Run(
                                new Text(" " + fullText)
                                {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));
                        }

                        endnotePart.Endnotes.Append(new Endnote(endnotePara) { Id = finalId });
                    }
                }

                // ── СОХРАНЕНИЕ ──
                endnotePart.Endnotes.Save();
                mainPart.Document.Save();
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ── Строим маппинг amendmentId → int endnoteId ──────────────────────
        private Dictionary<string, int> BuildIdMapping(
            Models.ChangableSourse.SourceData? amendments)
        {
            var mapping = new Dictionary<string, int>();
            if (amendments == null) return mapping;

            int syntheticId = 1001;

            foreach (var item in amendments.Source.OfType<ConstitutionalAmendment>())
            {
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                if (mapping.ContainsKey(item.Id)) continue;

                if (int.TryParse(item.Id, out int numericId))
                    mapping[item.Id] = numericId;
                else
                    mapping[item.Id] = syntheticId++;
            }

            return mapping;
        }

        // ── ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ──────────────────────────────────────────

        private Paragraph CreateParagraph(
            string text,
            bool bold = false,
            JustificationValues? align = null,
            bool indent = true,
            string fontSize = "24")
        {
            var para = new Paragraph();
            var justification = align ?? JustificationValues.Start;

            var spacing = new SpacingBetweenLines()
            {
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto,
                Before = "0",
                After = "0"
            };

            var pPr = new ParagraphProperties();
            pPr.Append(new Justification() { Val = justification });
            pPr.Append(spacing);

            if (indent)
                pPr.Append(new Indentation() { FirstLine = "720" });

            para.Append(pPr);

            var run = new Run();
            var runProps = new RunProperties(
                new RunFonts()
                {
                    Ascii = "Palatino Linotype",
                    HighAnsi = "Palatino Linotype",
                    ComplexScript = "Palatino Linotype"
                },
                new FontSize() { Val = fontSize }
            );

            if (bold)
                runProps.Append(new Bold());

            run.RunProperties = runProps;
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);

            return para;
        }

        private Paragraph CreateParagraphWithEndnote(
            string text, string? endnoteId,
            Dictionary<string, int> idMapping)
        {
            var para = CreateParagraph(text, align: JustificationValues.Both);
            AppendEndnoteRef(para, endnoteId, idMapping);
            return para;
        }

        private void AppendEndnoteRef(
            Paragraph para, string? id,
            Dictionary<string, int> idMapping)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!idMapping.TryGetValue(id, out int refId)) return;

            var run = new Run(new EndnoteReference { Id = refId });
            run.RunProperties = new RunProperties(
                new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });

            para.Append(run);
        }

        private string FormatArticleId(decimal id) =>
            id == (int)id ? ((int)id).ToString() : id.ToString("0.0");

        private string ToRoman(int number)
        {
            string[] romans = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string result = "";

            for (int i = 0; i < values.Length; i++)
                while (number >= values[i])
                {
                    number -= values[i];
                    result += romans[i];
                }

            return result;
        }

        private string ToAzerbaijaniOrdinal(int n)
        {
            string[] arr = {
                "", "BIRINCI","IKINCI","ÜÇÜNCÜ","DÖRDÜNCÜ","BEŞINCI",
                "ALTINCI","YEDDINCI","SƏKKIZINCI","DOQQUZUNCU","ONUNCU"
            };
            return n < arr.Length ? arr[n] : n.ToString();
        }

        private Paragraph CreateEmptyParagraph() =>
            new Paragraph(new Run(new Text("")));
    }
}
