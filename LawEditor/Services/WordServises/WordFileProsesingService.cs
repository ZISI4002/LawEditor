using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace LawEditor.Services.WordServises {
    public class WordFileProsesingService {
        // ── Проверка строки-заголовка bölmə ───────────────────────────────────
        private bool IsChapterLine(string line) {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string[] chapterPrefixes =
                { "birinci", "ikinci", "üçüncü", "dördüncü", "beşinci",
                  "altıncı", "yeddinci", "səkkizinci", "doqquzuncu" };

            string lower = line.ToLower().Trim();

            foreach (var prefix in chapterPrefixes) {
                if (Regex.IsMatch(lower, @"^" + prefix + @"\s+bölmə"))
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

        // ── Получить текст гиперссылки из параграфа ───────────────────────────
        private (string? linkText, string? rId) ExtractHyperlink(Paragraph para) {
            var hyperlink = para.Elements<Hyperlink>().FirstOrDefault();
            if (hyperlink == null)
                return (null, null);

            string? rId = hyperlink.Id?.Value;
            string linkText = string.Concat(
                hyperlink.Elements<Run>()
                         .Select(r => r.GetFirstChild<Text>()?.Text ?? "")
            ).Trim();

            return (string.IsNullOrWhiteSpace(linkText) ? null : linkText, rId);
        }

        // ── Получить endnoteReference id из параграфа ─────────────────────────
        private string? ExtractEndnoteRefId(Paragraph para) {
            var endRef = para.Descendants<EndnoteReference>().FirstOrDefault();
            return endRef?.Id?.Value.ToString();
        }

        // ── Получить полный текст параграфа ───────────────────────────────────
        private string GetParagraphText(Paragraph para) {
            var sb = new StringBuilder();

            foreach (var run in para.Elements<Run>()) {
                if (run.Descendants<EndnoteReference>().Any())
                    continue;
                sb.Append(run.GetFirstChild<Text>()?.Text ?? "");
            }

            foreach (var hl in para.Elements<Hyperlink>()) {
                foreach (var run in hl.Elements<Run>())
                    sb.Append(run.GetFirstChild<Text>()?.Text ?? "");
            }

            return sb.ToString().Trim();
        }

        // ── Строим словарь rId → URL из коллекции relationships ──────────────
        private Dictionary<string, string> BuildRelationshipMap(
            IEnumerable<HyperlinkRelationship> relationships) {
            var map = new Dictionary<string, string>();
            foreach (var rel in relationships) {
                if (!map.ContainsKey(rel.Id))
                    map[rel.Id] = rel.Uri?.ToString() ?? "";
            }
            return map;
        }

        // ── Основной метод ────────────────────────────────────────────────────
        public Laws ReadWordFile(string filePath) {
            var law = new Laws();

            var transitional = law.SourceData
                .OfType<SourceData<TransitionalProvisions>>()
                .FirstOrDefault();

            var sources = law.SourceData
                .OfType<SourceData<SourceDocumentsList>>()
                .FirstOrDefault();

            var amendments = law.SourceData
                .OfType<SourceData<ConstitutionalAmendment>>()
                .FirstOrDefault();

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

            var docRelationships = BuildRelationshipMap(
                doc.MainDocumentPart.HyperlinkRelationships);

            foreach (var para in body.Elements<Paragraph>()) {
                var line = GetParagraphText(para);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Contains("INCLUDEPICTURE") ||
                    line.Contains("MERGEFORMATINET") ||
                    line.Contains("userway.org") ||
                    line.Contains("\\*"))
                    continue;

                if (line.Contains("Keçİd müddəaları", StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Transitional;
                    continue;
                }

                if (line.Contains("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                        StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Sources;
                    continue;
                }

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

                        decimal id = 0;
                        if (idStr.Length == 4 && idStr.All(char.IsDigit))
                            id = int.Parse(idStr) / 10;
                        else
                            decimal.TryParse(idStr, out id);

                        string? endnoteRefId = ExtractEndnoteRefId(para);

                        currentArticle = new Article(id, title);
                        currentArticle.EndnoteId = endnoteRefId;
                        currentSection?.Articles.Add(currentArticle);
                        currentClause = null;
                        continue;
                    }

                    // CLAUSE (I. II.)
                    if (Regex.IsMatch(line, @"^[IVX]+\.\s")) {
                        string text = Regex.Replace(line, @"^[IVX]+\.\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para);
                        currentClause = currentArticle?.AddClause(text, endnoteId: endnoteRefId);
                        continue;
                    }

                    // SUBCLAUSE (1)
                    if (Regex.IsMatch(line, @"^\d+\)")) {
                        string text = Regex.Replace(line, @"^\d+\)\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para);
                        currentClause?.AddSubClause(text, endnoteId: endnoteRefId);
                        continue;
                    }

                    // обычный текст — если уже есть Clause, то это SubClause
                    if (currentArticle != null) {
                        string? endnoteRefId = ExtractEndnoteRefId(para);
                        if (currentClause == null)
                            currentClause = currentArticle.AddClause(line, endnoteId: endnoteRefId);
                        else
                            currentClause.AddSubClause(line, endnoteId: endnoteRefId);
                        continue;
                    }
                }

                // TRANSITIONAL
                if (mode == Mode.Transitional) {
                    transitional.Source.Add(new TransitionalProvisions(line));
                    continue;
                }

                // SOURCES
                if (mode == Mode.Sources) {
                    var (linkText, rId) = ExtractHyperlink(para);
                    string? url = rId != null && docRelationships.TryGetValue(rId, out var u)
                        ? u : null;

                    sources.Source.Add(
                        new SourceDocumentsList(line, linkText, url));
                    continue;
                }
            }

            if (law.Header == null)
                law.Header = headerBuilder.ToString().Trim();

            ReadAmendmentsFromEndnotes(doc, amendments);

            return law;
        }

        // ── Читаем amendments из endnotes ─────────────────────────────────────
        private void ReadAmendmentsFromEndnotes(WordprocessingDocument doc,SourceData<ConstitutionalAmendment> amendments) {
            var endnotesPart = doc.MainDocumentPart?.EndnotesPart;
            if (endnotesPart == null)
                return;

            var endnoteRelationships = BuildRelationshipMap(
                endnotesPart.HyperlinkRelationships);

            int numericCounter = 1;

            foreach (var endnote in endnotesPart.Endnotes.Elements<Endnote>()) {
                var type = endnote.Type;
                if (type != null &&
                    (type.Value == FootnoteEndnoteValues.Separator ||
                     type.Value == FootnoteEndnoteValues.ContinuationSeparator))
                    continue;

                var sb = new StringBuilder();
                foreach (var run in endnote.Descendants<Run>()) {
                    if (run.GetFirstChild<EndnoteReferenceMark>() != null)
                        continue;
                    sb.Append(run.InnerText);
                }

                string content = sb.ToString().Trim();
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                string amendmentId;
                string amendmentTitle;

                // Проверяем начало: буквы + цифра + пробел → буквенный id
                // Например "KM1 текст" или "KQ1 текст"
                var specialIdMatch = Regex.Match(content, @"^([A-Z]+\d+)\s+(.+)");

                if (specialIdMatch.Success) {
                    // Буквенный id — счётчик не трогаем
                    amendmentId = specialIdMatch.Groups[1].Value;
                    amendmentTitle = specialIdMatch.Groups[2].Value.Trim();
                }
                else {
                    // Числовой id — берём счётчик и увеличиваем
                    amendmentId = numericCounter.ToString();
                    amendmentTitle = content;
                    numericCounter++;
                }

                string? linkText = null;
                string? url = null;

                var firstParaWithLink = endnote.Elements<Paragraph>()
                    .FirstOrDefault(p => p.Elements<Hyperlink>().Any());

                if (firstParaWithLink != null) {
                    var hyperlink = firstParaWithLink.Elements<Hyperlink>().First();
                    string? rId = hyperlink.Id?.Value;

                    linkText = string.Concat(
                        hyperlink.Elements<Run>().Select(r => r.InnerText)
                    ).Trim();

                    if (rId != null && endnoteRelationships.TryGetValue(rId, out var u))
                        url = u;
                }

                amendments.Source.Add(
                    new ConstitutionalAmendment(
                        amendmentId,
                        amendmentTitle,
                        string.IsNullOrWhiteSpace(linkText) ? null : linkText,
                        url));
            }
        }
    }
}