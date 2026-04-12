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


namespace LawEditor.Services.WordServises
{
    public class WordFileProsesingService
    {
        // ── Проверка строки-заголовка bölmə ───────────────────────────────────
        private bool IsChapterLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string[] chapterPrefixes =
                { "birinci", "ikinci", "üçüncü", "dördüncü", "beşinci",
                  "altıncı", "yeddinci", "səkkizinci", "doqquzuncu" };

            string lower = line.ToLower().Trim();

            foreach (var prefix in chapterPrefixes)
            {
                if (Regex.IsMatch(lower, @"^" + prefix + @"\s+bölmə"))
                    return true;
            }

            return false;
        }

        enum Mode {
            Header,
            Chapters,
            Transitional,
            TransitionalDone,
            Sources
        }

        // ── Получить текст гиперссылки из параграфа ───────────────────────────
        private (string? linkText, string? rId) ExtractHyperlink(Paragraph para)
        {
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
        private string? ExtractEndnoteRefId(Paragraph para)
        {
            var endRef = para.Descendants<EndnoteReference>().FirstOrDefault();
            return endRef?.Id?.Value.ToString();
        }

        // ── Получить полный текст параграфа ───────────────────────────────────
        private string GetParagraphText(Paragraph para) {
            var sb = new StringBuilder();
            string? prevText = null;

            foreach (var run in para.Elements<Run>()) {
                if (run.Descendants<EndnoteReference>().Any())
                    continue;

                var rPr = run.GetFirstChild<RunProperties>();
                var vertAlign = rPr?.GetFirstChild<VerticalTextAlignment>();
                bool isSuperscript = vertAlign?.Val?.Value == VerticalPositionValues.Superscript;

                string text = run.GetFirstChild<Text>()?.Text ?? "";

                // Если суперскрипт и предыдущий текст был числом — вставляем точку
                if (isSuperscript && prevText != null && prevText.TrimEnd().Last() is >= '0' and <= '9')
                    sb.Append('.');

                sb.Append(text);
                if (!string.IsNullOrEmpty(text))
                    prevText = text;
            }

            foreach (var hl in para.Elements<Hyperlink>()) {
                foreach (var run in hl.Elements<Run>())
                    sb.Append(run.GetFirstChild<Text>()?.Text ?? "");
            }

            return sb.ToString().Trim();
        }

        // ── Строим словарь rId → URL из коллекции relationships ──────────────
        private Dictionary<string, string> BuildRelationshipMap(
            IEnumerable<HyperlinkRelationship> relationships)
        {
            var map = new Dictionary<string, string>();
            foreach (var rel in relationships)
            {
                if (!map.ContainsKey(rel.Id))
                    map[rel.Id] = rel.Uri?.ToString() ?? "";
            }
            return map;
        }

        // ── Основной метод ────────────────────────────────────────────────────
        public Laws ReadWordFile(string filePath)
        {
            var law = new Laws();

            ObservableCollection<TransitionalProvisions> transitional = new ObservableCollection<TransitionalProvisions>();
            ObservableCollection<SourceDocumentsList> sources = new ObservableCollection<SourceDocumentsList>();
            ObservableCollection<ConstitutionalAmendment> amendments = new ObservableCollection<ConstitutionalAmendment>();
            ObservableCollection<Models.ChangableData.Header> headers = new ObservableCollection<Models.ChangableData.Header>();

            Chapter? currentChapter = null;
            Section? currentSection = null;
            Article? currentArticle = null;
            Clause? currentClause = null;
            TransitionalProvisions? currentTransitional = null;


            Mode mode = Mode.Header;
            var headerBuilder = new StringBuilder();

            bool expectChapterTitle = false;
            bool expectSectionTitle = false;

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;

            var docRelationships = BuildRelationshipMap(
                doc.MainDocumentPart.HyperlinkRelationships);

            foreach (var para in body.Elements<Paragraph>())
            {
                var line = GetParagraphText(para);

                if (string.IsNullOrWhiteSpace(line) && mode != Mode.Transitional)
                    continue;

                if (line.Contains("INCLUDEPICTURE") ||
                    line.Contains("MERGEFORMATINET") ||
                    line.Contains("userway.org") ||
                    line.Contains("\\*"))
                    continue;

                if (line.Contains("Keçİd müddəaları", StringComparison.OrdinalIgnoreCase))
                {
                    mode = Mode.Transitional;
                    continue;
                }

                if (line.Contains("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                        StringComparison.OrdinalIgnoreCase))
                {
                    mode = Mode.Sources;
                    continue;
                }

                if (line.Contains("KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                // --- HEADER ---
                var header = new Models.ChangableData.Header();
                if (mode == Mode.Header)
                {
                    if (IsChapterLine(line))
                    {
                          header.Id = 1;
                          header.FullText = headerBuilder.ToString().Trim();
                          headers.Add(header);
                        mode = Mode.Chapters;
                        expectChapterTitle = true;
                        continue;
                    }
                    headerBuilder.AppendLine(line);
                    continue;
                }

                // --- ожидаем название Chapter ---
                if (expectChapterTitle)
                {
                    currentChapter = new Chapter(line);
                    law.Chapters.Add(currentChapter);
                    expectChapterTitle = false;
                    continue;
                }

                // --- ожидаем название Section ---
                if (expectSectionTitle)
                {
                    currentSection = new Section(line);
                    currentChapter?.Sections.Add(currentSection);
                    expectSectionTitle = false;
                    continue;
                }

                if (mode == Mode.Chapters)
                {
                    // BÖLMƏ
                    if (IsChapterLine(line))
                    {
                        expectChapterTitle = true;
                        currentSection = null;
                        continue;
                    }

                    // I fəsil
                    if (Regex.IsMatch(line, @"^[IVX]+\s*fəsil", RegexOptions.IgnoreCase))
                    {
                        expectSectionTitle = true;
                        continue;
                    }

                    // ARTICLE
                    var artMatch = Regex.Match(line, @"^Maddə\s+([\d\.]+)\.\s*(.*)");
                    if (artMatch.Success) {
                        string idStr = artMatch.Groups[1].Value;
                        string title = artMatch.Groups[2].Value;

                        decimal id = 0;
                        decimal.TryParse(idStr,
                            System.Globalization.NumberStyles.Number,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out id);

                        string? endnoteRefId = ExtractEndnoteRefId(para);

                        currentArticle = new Article(id, title,endnoteRefId);
                        currentSection?.Articles.Add(currentArticle);
                        currentClause = null;
                        continue;
                    }

                    // CLAUSE (I. II.)
                    if (Regex.IsMatch(line, @"^[IVX]+\.\s"))
                    {
                        string text = Regex.Replace(line, @"^[IVX]+\.\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para);
                        currentClause = currentArticle?.AddClause(text, endnoteId: endnoteRefId);
                        continue;
                    }

                    // SUBCLAUSE (1)
                    if (Regex.IsMatch(line, @"^\d+\)"))
                    {
                        string text = Regex.Replace(line, @"^\d+\)\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para);
                        currentClause?.AddSubClause(text, endnoteId: endnoteRefId);
                        continue;
                    }

                    // обычный текст — если уже есть Clause, то это SubClause
                    if (currentArticle != null)
                    {
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
                    var provMatch = Regex.Match(line, @"^(\d+)\.\s+(.+)");
                    if (provMatch.Success) {
                        string idStr = provMatch.Groups[1].Value;
                        string title = provMatch.Groups[2].Value;

                        int.TryParse(idStr,
                            System.Globalization.NumberStyles.Number,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out int id);

                        currentTransitional = new TransitionalProvisions(title) {
                            Id = id
                        };
                        transitional.Add(currentTransitional);
                    }
                    //если строка пустая и массив не пустой, то меняем мод на TransitionalDone,
                    //чтобы дальше не добавлять пустые строки к последнему пункту
                    else if (string.IsNullOrWhiteSpace(line) && transitional.Count > 0) {
                        mode = Mode.TransitionalDone;
                    }
                    else if (currentTransitional != null) {
                        currentTransitional.Title += "\n" + line;
                    }                                        
                    continue;
                }
                if (mode == Mode.TransitionalDone) {
                    TransitionalProvisions.Date = TransitionalProvisions.Date += line + "\n";
                     continue;
                }

                // SOURCES
                if (mode == Mode.Sources)
                {
                    var (linkText, rId) = ExtractHyperlink(para);
                    string? url = rId != null && docRelationships.TryGetValue(rId, out var u)
                        ? u : null;

                    sources.Add(new SourceDocumentsList(line, linkText, url));
                    continue;
                }
            }

            if (headers[0].FullText == null)
                headers[0].FullText = headerBuilder.ToString().Trim();

            ReadAmendmentsFromEndnotes(doc, amendments);
            // Копируем коллекции в law.UpperObjects
            law.UpperObjects.Add(new UpperObject
            {
                Id = 1,
                ObjectName = "HEADER",
                Headers = headers,
            });

            // Копируем коллекции в law.SourceData
            law.SourceData.Add(new SourceData
            {
                Id = 1,
                Type = "KEÇİD MÜDDƏALARI",
                Source = new ObservableCollection<object>(transitional.Cast<object>())
            });

            law.SourceData.Add(new SourceData
            {
                Id = 2,
                Type = "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                Source = new ObservableCollection<object>(sources.Cast<object>())
            });

            law.SourceData.Add(new SourceData
            {
                Id = 3,
                Type = "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                Source = new ObservableCollection<object>(amendments.Cast<object>())
            });
            
            return law;
        }

        // ── Читаем amendments из endnotes ─────────────────────────────────────
        private void ReadAmendmentsFromEndnotes(WordprocessingDocument doc, ObservableCollection<ConstitutionalAmendment> amendments)
        {
            var endnotesPart = doc.MainDocumentPart?.EndnotesPart;
            if (endnotesPart == null)
                return;

            var endnoteRelationships = BuildRelationshipMap(
                endnotesPart.HyperlinkRelationships);

            int numericCounter = 1;

            foreach (var endnote in endnotesPart.Endnotes.Elements<Endnote>())
            {
                var type = endnote.Type;
                if (type != null &&
                    (type.Value == FootnoteEndnoteValues.Separator ||
                     type.Value == FootnoteEndnoteValues.ContinuationSeparator))
                    continue;

                var sb = new StringBuilder();
                foreach (var run in endnote.Descendants<Run>())
                {
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

                if (specialIdMatch.Success)
                {
                    // Буквенный id — счётчик не трогаем
                    amendmentId = specialIdMatch.Groups[1].Value;
                    amendmentTitle = specialIdMatch.Groups[2].Value.Trim();
                }
                else
                {
                    // Числовой id — берём счётчик и увеличиваем
                    amendmentId = numericCounter.ToString();
                    amendmentTitle = content;
                    numericCounter++;
                }

                string? linkText = null;
                string? url = null;

                var firstParaWithLink = endnote.Elements<Paragraph>()
                    .FirstOrDefault(p => p.Elements<Hyperlink>().Any());

                if (firstParaWithLink != null)
                {
                    var hyperlink = firstParaWithLink.Elements<Hyperlink>().First();
                    string? rId = hyperlink.Id?.Value;

                    linkText = string.Concat(
                        hyperlink.Elements<Run>().Select(r => r.InnerText)
                    ).Trim();

                    if (rId != null && endnoteRelationships.TryGetValue(rId, out var u))
                        url = u;
                }

                amendments.Add(
                    new ConstitutionalAmendment(
                        amendmentId,
                        amendmentTitle,
                        string.IsNullOrWhiteSpace(linkText) ? null : linkText,
                        url));
            }
        }
    }
}