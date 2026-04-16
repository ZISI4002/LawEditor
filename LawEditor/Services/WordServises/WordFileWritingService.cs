using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace LawEditor.Services.WordServises {
    public class WordFileWritingService {

        public void WriteWordFile(string filePath, Laws laws) {
            try {
                using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

                var mainPart = doc.AddMainDocumentPart();

                var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();

                numberingPart.Numbering = new Numbering(
                    new AbstractNum(
                        new Level(
                            new NumberingFormat() { Val = NumberFormatValues.Decimal },
                            new LevelText() { Val = "%1." },
                            new StartNumberingValue() { Val = 1 }
                        ) { LevelIndex = 0 }
                    ) { AbstractNumberId = 1 },
                    new NumberingInstance(
                        new AbstractNumId() { Val = 1 }
                    ) { NumberID = 1 }
                );

                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                var endnotePart = mainPart.AddNewPart<EndnotesPart>();

                var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylePart.Styles = new Styles();

                // HEADER
                if (laws.UpperObjects.Count > 0 && laws.UpperObjects[0].Headers.Count > 0) {
                    var headerText = laws.UpperObjects[0].Headers[0].FullText;

                    if (!string.IsNullOrWhiteSpace(headerText)) {
                        var lines = headerText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var line in lines) {
                            body.Append(CreateParagraph(line, align: JustificationValues.Center, indent: false));
                        }
                    }
                    // Пустая строка 
                    body.Append(CreateEmptyParagraph());
                }

                foreach (var chapter in laws.Chapters) {

                    string chapterOrdinal = ToAzerbaijaniOrdinal(chapter.Id);

                    body.Append(CreateParagraph($"{chapterOrdinal} BÖLMƏ",
                        bold: true,
                        align: JustificationValues.Center,
                        indent: false));

                    if (!string.IsNullOrEmpty(chapter.Title))
                        body.Append(CreateParagraph(chapter.Title,
                            bold: true,
                            align: JustificationValues.Center,
                            indent: false));

                    // Пустая строка 
                    body.Append(CreateEmptyParagraph());

                    foreach (var section in chapter.Sections) {

                        string sectionRoman = ToRoman(section.Id);

                        body.Append(CreateParagraph($"{sectionRoman} fəsil",
                            align: JustificationValues.Center,
                            indent: false));

                        if (!string.IsNullOrEmpty(section.Title))
                            body.Append(CreateParagraph(section.Title,
                                align: JustificationValues.Center,
                                indent: false));

                        // Пустая строка 
                        body.Append(CreateEmptyParagraph());

                        foreach (var article in section.Articles) {                            

                            string articleId = FormatArticleId(article.Id);

                            var articlePara = CreateParagraph(
                                $"Maddə {articleId}. {article.Title}",
                                bold: true,
                                align: JustificationValues.Both,
                                indent: true
                            );

                            AppendEndnoteRef(articlePara, article.EndnoteId);
                            body.Append(articlePara);

                            // Пустая строка 
                            body.Append(CreateEmptyParagraph());

                            foreach (var clause in article.Clauses) {
                                Paragraph clausePara;

                                if (clause.Number > 0) {
                                    clausePara = CreateParagraphWithEndnote(
                                        $"{ToRoman(clause.Number)}. {clause.Text}",
                                        clause.EndnoteId
                                    );
                                }
                                else if (!string.IsNullOrEmpty(clause.Text)) {
                                    clausePara = CreateParagraphWithEndnote(
                                        clause.Text,
                                        clause.EndnoteId
                                    );
                                }
                                else continue;

                                body.Append(clausePara);

                                foreach (var sub in clause.SubClauses) {
                                    body.Append(CreateParagraphWithEndnote(
                                        $"{sub.Number}) {sub.Text}",
                                        sub.EndnoteId
                                    ));
                                }
                            }

                            // Пустая строка
                            body.Append(CreateEmptyParagraph());
                        }

                    }
                }

                // TRANSITIONAL
                var transitional = laws.SourceData.FirstOrDefault(s => s.Id == 1);

                if (transitional?.Source.Count > 0) {
                    body.Append(CreateParagraph("KEÇİD MÜDDƏALARI",
                        bold: true,
                        align: JustificationValues.Center,
                        indent: false));

                    // Пустая строка
                    body.Append(CreateEmptyParagraph());

                    foreach (var item in transitional.Source.OfType<TransitionalProvisions>()) {
                        body.Append(CreateParagraph($"{item.Id}. {item.Title}",
                            align: JustificationValues.Both));
                    }

                    body.Append(new Paragraph(new Run(new Text(""))));

                    // 🔥 ДАТА (фикс)
                    if (!string.IsNullOrWhiteSpace(TransitionalProvisions.Date)) {

                        // берём ТВОЙ же стиль (лево + жирный)
                        var para = CreateParagraph("",
                            bold: true,
                            align: JustificationValues.Start,
                            indent: false);

                        var run = para.GetFirstChild<Run>();

                        // чистим пустой текст, который создался
                        run.RemoveAllChildren<Text>();

                        var lines = TransitionalProvisions.Date.Split('\n');

                        for (int i = 0; i < lines.Length; i++) {
                            run.Append(new Text(lines[i]) {
                                Space = SpaceProcessingModeValues.Preserve
                            });

                            if (i < lines.Length - 1)
                                run.Append(new Break()); // перенос строки
                        }

                        body.Append(para);
                    }

                    // Пустая строка
                    body.Append(CreateEmptyParagraph());
                }

                // SOURCES
                var sources = laws.SourceData.FirstOrDefault(s => s.Id == 2);

                if (sources?.Source.Count > 0) {
                    body.Append(CreateParagraph(
                        "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                        bold: true,
                        align: JustificationValues.Center,
                        indent: false,
                        fontSize: "20"
                    ));

                    body.Append(CreateEmptyParagraph());

                    foreach (var item in sources.Source.OfType<SourceDocumentsList>()) {

                        var para = new Paragraph();

                        // 🔥 как в Word (отступ + numbering)
                        var pPr = new ParagraphProperties(
                            new NumberingProperties(
                                new NumberingLevelReference() { Val = 0 },
                                new NumberingId() { Val = 1 }
                            ),
                            new Indentation() {
                                Left = "720",        // отступ слева
                                Hanging = "360"      // висячий отступ (очень важно!)
                            },
                            new Justification() { Val = JustificationValues.Both }
                        );

                        para.Append(pPr);

                        // 🔥 базовый стиль (10pt)
                        RunProperties baseProps = new RunProperties(
                            new RunFonts() {
                                Ascii = "Palatino Linotype",
                                HighAnsi = "Palatino Linotype",
                                ComplexScript = "Palatino Linotype"
                            },
                            new FontSize() { Val = "20" }
                        );

                        if (!string.IsNullOrEmpty(item.LinkText) && !string.IsNullOrEmpty(item.Url)) {
                            var rel = mainPart.AddHyperlinkRelationship(new Uri(item.Url), true);

                            // 🔥 ссылка
                            var hyperlink = new Hyperlink() { Id = rel.Id };

                            hyperlink.Append(new Run(
                                new RunProperties(
                                    new Underline() { Val = UnderlineValues.Single },
                                    new Color() { Val = "0000FF" },
                                    new FontSize() { Val = "20" }
                                ),
                                new Text(item.LinkText) {
                                    Space = SpaceProcessingModeValues.Preserve
                                }
                            ));

                            para.Append(hyperlink);

                            // 🔥 ВАЖНО: пробел с preserve
                            para.Append(new Run(new Text(" ") {
                                Space = SpaceProcessingModeValues.Preserve
                            }));

                            // 🔥 остальной текст
                            string rest = item.Title.Replace(item.LinkText, "").Trim();

                            if (!string.IsNullOrEmpty(rest)) {
                                para.Append(new Run(baseProps.CloneNode(true), new Text(rest) {
                                    Space = SpaceProcessingModeValues.Preserve
                                }));
                            }
                        }
                        else {
                            para.Append(new Run(baseProps, new Text(item.Title) {
                                Space = SpaceProcessingModeValues.Preserve
                            }));
                        }

                        body.Append(para);
                    }

                    body.Append(CreateEmptyParagraph());
                }

                // ENDNOTES
                var amendments = laws.SourceData.FirstOrDefault(s => s.Id == 3);

                if (amendments?.Source.Count > 0) {
                    endnotePart.Endnotes = new Endnotes();

                    endnotePart.Endnotes.Append(new Endnote(new Paragraph(new Run(new SeparatorMark()))) {
                        Type = FootnoteEndnoteValues.Separator,
                        Id = -1
                    });

                    endnotePart.Endnotes.Append(new Endnote(new Paragraph(new Run(new ContinuationSeparatorMark()))) {
                        Type = FootnoteEndnoteValues.ContinuationSeparator,
                        Id = 0
                    });

                    foreach (var item in amendments.Source.OfType<ConstitutionalAmendment>()) {
                        if (string.IsNullOrWhiteSpace(item.Title)) continue;

                        bool isNumeric = int.TryParse(item.Id, out int id);

                        string text = isNumeric ? item.Title : item.Id + " " + item.Title;

                        var para = new Paragraph();

                        var mark = new Run(new EndnoteReferenceMark());
                        mark.RunProperties = new RunProperties(
                            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });

                        para.Append(mark);

                        para.Append(new Run(new Text(" " + text)));

                        int finalId = isNumeric ? id : endnotePart.Endnotes.Elements<Endnote>().Count();

                        endnotePart.Endnotes.Append(new Endnote(para) { Id = finalId });
                    }
                }

                mainPart.Document.Save();
            }
            catch (IOException ex) {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        // ── MAGIC FORMATTING ──
        private Paragraph CreateParagraph(
    string text,
    bool bold = false,
    JustificationValues? align = null,
    bool indent = true,
    string fontSize = "24") // 🔥 добавили
{
            var para = new Paragraph();

            var justification = align ?? JustificationValues.Start;

            var spacing = new SpacingBetweenLines() {
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto,
                Before = "0",
                After = "0"
            };

            var indentProps = indent
                ? new Indentation() { FirstLine = "720" }
                : null;

            var pPr = new ParagraphProperties();
            pPr.Append(new Justification() { Val = justification });
            pPr.Append(spacing);

            if (indentProps != null)
                pPr.Append(indentProps);

            para.Append(pPr);

            var run = new Run();

            var runProps = new RunProperties(
                new RunFonts() {
                    Ascii = "Palatino Linotype",
                    HighAnsi = "Palatino Linotype",
                    ComplexScript = "Palatino Linotype"
                },
                new FontSize() { Val = fontSize } // 🔥 используем
            );

            if (bold)
                runProps.Append(new Bold());

            run.RunProperties = runProps;

            run.Append(new Text(text) {
                Space = SpaceProcessingModeValues.Preserve
            });

            para.Append(run);

            return para;
        }
        private Paragraph CreateHyperlinkParagraph(
            MainDocumentPart mainPart,
            string fullText,
            string linkText,
            string url,
            string fontSize = "20") // 🔥 10pt
            {
            var para = CreateParagraph("", align: JustificationValues.Both, indent: false, fontSize: fontSize);

            var run = para.GetFirstChild<Run>();
            run.RemoveAllChildren<Text>();

            int index = fullText.IndexOf(linkText);

            if (index < 0) {
                run.Append(new Text(fullText));
                return para;
            }

            string before = fullText.Substring(0, index);
            string after = fullText.Substring(index + linkText.Length);

            // текст ДО ссылки
            if (!string.IsNullOrEmpty(before))
                para.Append(new Run(new Text(before)));

            // 🔥 создаём relationship
            var rel = mainPart.AddHyperlinkRelationship(new Uri(url), true);

            var hyperlink = new Hyperlink() {
                Id = rel.Id
            };

            var linkRun = new Run(
                new RunProperties(
                    new Underline() { Val = UnderlineValues.Single },
                    new Color() { Val = "0000FF" },
                    new FontSize() { Val = fontSize }
                ),
                new Text(linkText)
            );

            hyperlink.Append(linkRun);
            para.Append(hyperlink);

            // текст ПОСЛЕ
            if (!string.IsNullOrEmpty(after))
                para.Append(new Run(new Text(after)));

            return para;
        }

        private Paragraph CreateParagraphWithEndnote(string text, string? endnoteId) {
            var para = CreateParagraph(text, align: JustificationValues.Both);
            AppendEndnoteRef(para, endnoteId);
            return para;
        }

        private void AppendEndnoteRef(Paragraph para, string? id) {
            if (!int.TryParse(id, out int refId)) return;

            var run = new Run(new EndnoteReference { Id = refId });
            run.RunProperties = new RunProperties(
                new VerticalTextAlignment { Val = VerticalPositionValues.Superscript });

            para.Append(run);
        }

        private string FormatArticleId(decimal id) =>
            id == (int)id ? ((int)id).ToString() : id.ToString("0.0");

        private string ToRoman(int number) {
            string[] romans = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string result = "";

            for (int i = 0; i < values.Length; i++)
                while (number >= values[i]) {
                    number -= values[i];
                    result += romans[i];
                }

            return result;
        }

        private string ToAzerbaijaniOrdinal(int n) {
            string[] arr = {
                "", "BIRINCI","IKINCI","ÜÇÜNCÜ","DÖRDÜNCÜ","BEŞINCI",
                "ALTINCI","YEDDINCI","SƏKKIZINCI","DOQQUZUNCU","ONUNCU"
            };
            return n < arr.Length ? arr[n] : n.ToString();
        }
        private Paragraph CreateEmptyParagraph() {
            return new Paragraph(
                new Run(new Text(""))
            );
        }
    }
}