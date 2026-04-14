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
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());
                var endnotePart = mainPart.AddNewPart<EndnotesPart>();

                // ── Header ────────────────────────────────────────────────────
                if (laws.UpperObjects.Count > 0 && laws.UpperObjects[0].Headers.Count > 0) {
                    var headerText = laws.UpperObjects[0].Headers[0].FullText;
                    if (!string.IsNullOrWhiteSpace(headerText)) {
                        var headerLines = headerText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in headerLines)
                            body.AppendChild(CreateParagraph(line));
                    }
                }

                // ── Chapters / Sections / Articles ────────────────────────────
                foreach (var chapter in laws.Chapters) {
                    string chapterOrdinal = ToAzerbaijaniOrdinal(chapter.Id);
                    body.AppendChild(CreateParagraph($"{chapterOrdinal} BÖLMƏ", bold: true));
                    if (!string.IsNullOrEmpty(chapter.Title))
                        body.AppendChild(CreateParagraph(chapter.Title, bold: true));

                    foreach (var section in chapter.Sections) {
                        string sectionRoman = ToRoman(section.Id);
                        body.AppendChild(CreateParagraph($"{sectionRoman} fəsil"));
                        if (!string.IsNullOrEmpty(section.Title))
                            body.AppendChild(CreateParagraph(section.Title));

                        foreach (var article in section.Articles) {
                            string articleId = FormatArticleId(article.Id);

                            // Article: текст + superscript маркер эндноты если есть
                            var articlePara = new Paragraph();
                            var articleRun = new Run(new Text($"Maddə {articleId}. {article.Title}"));
                            articleRun.RunProperties = new RunProperties(new Bold());
                            articlePara.AppendChild(articleRun);
                            AppendEndnoteRef(articlePara, article.EndnoteId);
                            body.AppendChild(articlePara);

                            foreach (var clause in article.Clauses) {
                                Paragraph clausePara;

                                if (clause.Number > 0) {
                                    string clauseRoman = ToRoman(clause.Number);
                                    clausePara = CreateParagraphWithEndnote(
                                        $"{clauseRoman}. {clause.Text}",
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

                                body.AppendChild(clausePara);

                                foreach (var subClause in clause.SubClauses) {
                                    var subPara = CreateParagraphWithEndnote(
                                        $"{subClause.Number}) {subClause.Text}",
                                        subClause.EndnoteId
                                    );
                                    body.AppendChild(subPara);
                                }
                            }
                        }
                    }
                }

                // ── Transitional Provisions ───────────────────────────────────
                var transitionalData = laws.SourceData.FirstOrDefault(s => s.Id == 1);
                if (transitionalData?.Source.Count > 0) {
                    body.AppendChild(CreateParagraph("Keçİd müddəaları", bold: true));
                    foreach (var item in transitionalData.Source) {
                        if (item is TransitionalProvisions tp)
                            body.AppendChild(CreateParagraph($"{tp.Id}. {tp.Title}"));
                    }
                    body.AppendChild(new Paragraph(new Run(new Text(""))));
                    if (!string.IsNullOrWhiteSpace(TransitionalProvisions.Date))
                        body.AppendChild(CreateParagraph(TransitionalProvisions.Date));
                }

                // ── Source Documents ──────────────────────────────────────────
                var sourceData = laws.SourceData.FirstOrDefault(s => s.Id == 2);
                if (sourceData?.Source.Count > 0) {
                    body.AppendChild(CreateParagraph("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI", bold: true));
                    foreach (var item in sourceData.Source) {
                        if (item is SourceDocumentsList sd)
                            body.AppendChild(CreateParagraph($"{sd.Id}. {sd.Title}"));
                    }
                }

                // ── Constitutional Amendments (endnotes) ──────────────────────
                var amendmentsData = laws.SourceData.FirstOrDefault(s => s.Id == 3);
                if (amendmentsData?.Source.Count > 0) {
                    endnotePart.Endnotes = new Endnotes();

                    // Обязательные служебные эндноты
                    endnotePart.Endnotes.AppendChild(new Endnote(
                        new Paragraph(new Run(new SeparatorMark()))) { Type = FootnoteEndnoteValues.Separator, Id = -1 });

                    endnotePart.Endnotes.AppendChild(new Endnote(
                        new Paragraph(new Run(new ContinuationSeparatorMark()))) { Type = FootnoteEndnoteValues.ContinuationSeparator, Id = 0 });

                    foreach (var item in amendmentsData.Source) {
                        if (item is not ConstitutionalAmendment ca || string.IsNullOrEmpty(ca.Title))
                            continue;

                        bool isNumeric = int.TryParse(ca.Id, out int endnoteId);

                        // Для не-числовых (KM1, KQ1) — id пишем в начало текста,
                        // чтобы ReadAmendmentsFromEndnotes распознал через specialIdMatch ^([A-Z]+\d+)\s+(.+)
                        string fullText = isNumeric
                            ? ca.Title
                            : ca.Id + " " + ca.Title;

                        var endnoteParagraph = new Paragraph();

                        // Superscript маркер (EndnoteReferenceMark)
                        var superRun = new Run(new EndnoteReferenceMark());
                        superRun.RunProperties = new RunProperties(
                            new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }
                        );
                        endnoteParagraph.AppendChild(superRun);

                        // Если есть URL и LinkText — разбиваем Title на три части:
                        // [текст до ссылки] [hyperlink с LinkText] [текст после ссылки]
                        if (!string.IsNullOrWhiteSpace(ca.Url) &&
                            !string.IsNullOrWhiteSpace(ca.LinkText) &&
                            fullText.Contains(ca.LinkText)) {
                            int linkIdx = fullText.IndexOf(ca.LinkText);
                            string before = fullText[..linkIdx];
                            string after = fullText[(linkIdx + ca.LinkText.Length)..];

                            // Текст до ссылки
                            if (!string.IsNullOrEmpty(before))
                                endnoteParagraph.AppendChild(
                                    new Run(new Text(" " + before) { Space = SpaceProcessingModeValues.Preserve })
                                );

                            // Гиперссылка
                            var hyperlinkRel = endnotePart.AddHyperlinkRelationship(new Uri(ca.Url), true);
                            var hyperlink = new Hyperlink(
                                new Run(
                                    new RunProperties(new RunStyle { Val = "aa" }),
                                    new Text(ca.LinkText) { Space = SpaceProcessingModeValues.Preserve }
                                )) {
                                Id = hyperlinkRel.Id,
                                History = true
                            };
                            endnoteParagraph.AppendChild(hyperlink);

                            // Текст после ссылки
                            if (!string.IsNullOrEmpty(after))
                                endnoteParagraph.AppendChild(
                                    new Run(new Text(after) { Space = SpaceProcessingModeValues.Preserve })
                                );
                        }
                        else {
                            // Нет ссылки — просто весь текст
                            endnoteParagraph.AppendChild(
                                new Run(new Text(" " + fullText) { Space = SpaceProcessingModeValues.Preserve })
                            );
                        }

                        // Id для не-числовых берём как текущий count чтобы не было коллизий
                        int finalId = isNumeric
                            ? endnoteId
                            : endnotePart.Endnotes.Elements<Endnote>().Count();

                        endnotePart.Endnotes.AppendChild(
                            new Endnote(endnoteParagraph) { Id = finalId }
                        );
                    }
                }

                mainPart.Document.Save();
            }
            catch (IOException ex) {
                MessageBox.Show($"Fayl yazılarkən xəta baş verdi: {ex.Message}",
                                "Fayl xətası",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            catch (Exception ex) {
                MessageBox.Show($"Xəta baş verdi: {ex.Message}",
                                "Xəta",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // ── Вспомогательные методы ────────────────────────────────────────────

        /// <summary>
        /// Создаёт параграф с текстом. Опционально bold.
        /// </summary>
        private Paragraph CreateParagraph(string text, bool bold = false) {
            var para = new Paragraph();
            var run = new Run();
            if (bold)
                run.RunProperties = new RunProperties(new Bold());
            run.AppendChild(new Text(text));
            para.AppendChild(run);
            return para;
        }

        /// <summary>
        /// Создаёт параграф с текстом и superscript маркером эндноты (если endnoteId не null).
        /// </summary>
        private Paragraph CreateParagraphWithEndnote(string text, string? endnoteId, bool bold = false) {
            var para = new Paragraph();
            var run = new Run();
            if (bold)
                run.RunProperties = new RunProperties(new Bold());
            run.AppendChild(new Text(text));
            para.AppendChild(run);
            AppendEndnoteRef(para, endnoteId);
            return para;
        }

        /// <summary>
        /// Добавляет superscript EndnoteReference в конец параграфа, если endnoteId числовой.
        /// </summary>
        private void AppendEndnoteRef(Paragraph para, string? endnoteId) {
            if (string.IsNullOrWhiteSpace(endnoteId))
                return;
            if (!int.TryParse(endnoteId, out int refId))
                return;

            var refRun = new Run(new EndnoteReference { Id = refId });
            refRun.RunProperties = new RunProperties(
                new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }
            );
            para.AppendChild(refRun);
        }

        private string FormatArticleId(decimal id) {
            if (id == (int)id)
                return ((int)id).ToString();
            return id.ToString("0.0");
        }

        private string ToAzerbaijaniOrdinal(int number) {
            string[] ordinals = {
                "", "BIRINCI", "IKINCI", "ÜÇÜNCÜ", "DÖRDÜNCÜ",
                "BEŞINCI", "ALTINCI", "YEDDINCI", "SƏKKIZINCI",
                "DOQQUZUNCU", "ONUNCU", "ON BIRINCI", "ON IKINCI",
                "ON ÜÇÜNCÜ", "ON DÖRDÜNCÜ", "ON BEŞINCI"
            };

            if (number > 0 && number < ordinals.Length)
                return ordinals[number].ToUpper();

            return number.ToString();
        }

        private string ToRoman(int number) {
            if (number < 1) return "";

            string[] romanNumerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

            var result = "";
            for (int i = 0; i < values.Length; i++) {
                while (number >= values[i]) {
                    number -= values[i];
                    result += romanNumerals[i];
                }
            }
            return result;
        }
    }
}