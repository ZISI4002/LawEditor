using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LawEditor.Services.WordServises {
    public class WordFileProsesingService {
        private bool IsChapterLine(string line) {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string[] chapterPrefixes =
                { "birinci", "ikinci", "üçüncü", "dördüncü", "beşinci",
                  "altıncı", "yeddinci", "səkkizinci", "doqquzuncu" };

            string lower = line.ToLower().Trim();

            foreach (var prefix in chapterPrefixes) {
                string pattern = @"^" + prefix + @"\s+bölmə";

                if (Regex.IsMatch(lower, pattern))
                    return true;
            }

            return false;
        }

        enum Mode {
            Header,
            Chapters,
            Transitional,
            Sources
        }

        public Laws ReadWordFile(string filePath) {
            var law = new Laws();

            Chapter currentChapter = null;
            Section currentSection = null;
            Article currentArticle = null;
            Clause currentClause = null;

            Mode mode = Mode.Header;

            var headerBuilder = new StringBuilder();

            bool expectChapterTitle = false;
            bool expectSectionTitle = false;

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;

            // ── Основной цикл: читаем тело документа ──────────────────────────
            foreach (var para in body.Elements<Paragraph>()) {
                var raw = para.InnerText ?? "";
                var line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Пропускаем мусор
                if (line.Contains("INCLUDEPICTURE") ||
                    line.Contains("MERGEFORMATINET") ||
                    line.Contains("userway.org") ||
                    line.Contains("\\*"))
                    continue;

                // --- переключатели режимов ---

                if (line.Contains("Keçİd müddəaları", StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Transitional;
                    continue;
                }

                if (line.Contains("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI", StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Sources;
                    continue;
                }

                // Строка "KONSTİTUSİYAYA EDİLMİŞ..." встречается прямо перед
                // section break (sectPr) — после неё в теле документа больше
                // ничего нет. Список изменений хранится в сносках (endnotes),
                // которые мы читаем отдельно ниже. Просто пропускаем строку.
                if (line.Contains("KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                // --- HEADER ---
                if (mode == Mode.Header) {
                    if (IsChapterLine(line)) {
                        law.Header = headerBuilder.ToString().Trim();
                        mode = Mode.Chapters;
                        expectChapterTitle = true;
                        continue;
                    }

                    headerBuilder.AppendLine(line);
                    continue;
                }

                // --- ожидаем название Chapter ---
                if (expectChapterTitle) {
                    currentChapter = new Chapter(line);
                    law.Chapters.Add(currentChapter);

                    expectChapterTitle = false;
                    continue;
                }

                // --- ожидаем название Section ---
                if (expectSectionTitle) {
                    currentSection = new Section(line);
                    currentChapter?.Sections.Add(currentSection);

                    expectSectionTitle = false;
                    continue;
                }

                if (mode == Mode.Chapters) {
                    // BÖLMƏ
                    if (IsChapterLine(line)) {
                        expectChapterTitle = true;
                        currentSection = null;
                        continue;
                    }

                    // I fəsil
                    if (Regex.IsMatch(line, @"^[IVX]+\s*fəsil", RegexOptions.IgnoreCase)) {
                        expectSectionTitle = true;
                        continue;
                    }

                    // ARTICLE
                    var artMatch = Regex.Match(line, @"^Maddə\s+([\d\.]+)\.\s*(.*)");

                    if (artMatch.Success) {
                        string idStr = artMatch.Groups[1].Value;
                        string title = artMatch.Groups[2].Value;

                        float id = 0;

                        if (idStr.Length == 4 && idStr.All(char.IsDigit))
                            id = int.Parse(idStr) / 10f;
                        else
                            float.TryParse(idStr, out id);

                        currentArticle = new Article(id, title);
                        currentSection?.Articles.Add(currentArticle);
                        currentClause = null;
                        continue;
                    }

                    // CLAUSE (I. II.)
                    if (Regex.IsMatch(line, @"^[IVX]+\.\s")) {
                        string text = Regex.Replace(line, @"^[IVX]+\.\s*", "");
                        currentClause = currentArticle?.AddClause(text);
                        continue;
                    }

                    // SUBCLAUSE (1)
                    if (Regex.IsMatch(line, @"^\d+\)")) {
                        string text = Regex.Replace(line, @"^\d+\)\s*", "");
                        currentClause?.AddSubClause(text);
                        continue;
                    }

                    // обычный текст
                    if (currentArticle != null) {
                        if (currentClause == null)
                            currentClause = currentArticle.AddClause(line);
                        else
                            currentClause.Text += " " + line;

                        continue;
                    }
                }

                // TRANSITIONAL
                if (mode == Mode.Transitional) {
                    law.transitionalProvisions.Add(new TransitionalProvisions(line));
                    continue;
                }

                // SOURCES
                if (mode == Mode.Sources) {
                    law.sourceDocumentsLists.Add(new SourceDocumentsList(line));
                    continue;
                }
            }

            if (law.Header == null)
                law.Header = headerBuilder.ToString().Trim();

            // ── Читаем сноски (endnotes) — там хранятся изменения ─────────────
            ReadAmendmentsFromEndnotes(doc, law);

            return law;
        }

        /// <summary>
        /// Список конституционных изменений в этом документе хранится
        /// в сносках (endnotes), а не в теле. Word отображает их как
        /// отдельную "вторую часть" после горизонтальной линии,
        /// поэтому Ctrl+A их не захватывает.
        /// </summary>
        private void ReadAmendmentsFromEndnotes(WordprocessingDocument doc, Laws law) {
            var endnotesPart = doc.MainDocumentPart?.EndnotesPart;
            if (endnotesPart == null)
                return;

            foreach (var endnote in endnotesPart.Endnotes.Elements<Endnote>()) {
                // Пропускаем служебные сноски (разделители)
                var type = endnote.Type;
                if (type != null &&
                    (type.Value == FootnoteEndnoteValues.Separator ||
                    type.Value == FootnoteEndnoteValues.ContinuationSeparator))
                    continue;

                // Собираем весь текст сноски
                var sb = new StringBuilder();
                foreach (var para in endnote.Elements<Paragraph>()) {
                    var text = para.InnerText?.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }

                var content = sb.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(content))
                    law.constitutionalAmendments.Add(new ConstitutionalAmendment(content));
            }
        }
    }
}