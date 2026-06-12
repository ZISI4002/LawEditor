using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Table = LawEditor.Models.SpecialElements.Table;


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
            TransitionalDone,
            Sources
        }

        // ── Отслеживаем последний объект, которому можно назначить таблицу ───
        enum LastContext {
            None,
            Chapter,
            Section,
            Article,
            Clause,
            SubClause
        }

        // ── Получить текст и URL ссылки из параграфа (HYPERLINK field code) ──
        private (string? linkText, string? url) ExtractHyperlink(Paragraph para) {
            var runs = para.Elements<Run>().ToList();

            bool insideField = false;
            string? url = null;
            var linkTextSb = new StringBuilder();

            foreach (var run in runs) {
                var fldChar = run.Descendants<FieldChar>().FirstOrDefault();
                var instrText = run.Descendants<FieldCode>().FirstOrDefault();

                if (fldChar != null) {
                    if (fldChar.FieldCharType == FieldCharValues.Begin) {
                        insideField = true;
                        continue;
                    }
                    if (fldChar.FieldCharType == FieldCharValues.Separate) {
                        continue;
                    }
                    if (fldChar.FieldCharType == FieldCharValues.End) {
                        insideField = false;
                        continue;
                    }
                }

                if (instrText != null) {
                    string instr = instrText.Text ?? "";
                    var match = Regex.Match(instr, @"HYPERLINK\s+""([^""]+)""");
                    if (match.Success)
                        url = match.Groups[1].Value;
                    continue;
                }

                // Текст ссылки идёт ПОСЛЕ separate, ДО end —
                // т.е. когда url уже найден (поле открыто и instrText распознан),
                // но fldChar end ещё не встретился.
                if (url != null && insideField == false) {
                    // separate уже встретился (insideField сброшен на нём не должен быть)
                }
            }

            // Второй проход: текст ссылки — это runs между separate и end.
            // Переделаем логику линейно с состоянием.
            url = null;
            linkTextSb.Clear();
            int state = 0; // 0=outside, 1=afterBegin(reading instrText), 2=afterSeparate(reading link text)

            foreach (var run in runs) {
                var fldChar = run.Descendants<FieldChar>().FirstOrDefault();
                var instr = run.Descendants<FieldCode>().FirstOrDefault();

                if (fldChar != null) {
                    if (fldChar.FieldCharType == FieldCharValues.Begin) { state = 1; continue; }
                    if (fldChar.FieldCharType == FieldCharValues.Separate) { state = 2; continue; }
                    if (fldChar.FieldCharType == FieldCharValues.End) { state = 0; continue; }
                }

                if (state == 1 && instr != null) {
                    var match = Regex.Match(instr.Text ?? "", @"HYPERLINK\s+""([^""]+)""");
                    if (match.Success)
                        url = match.Groups[1].Value;
                    continue;
                }

                if (state == 2) {
                    var t = run.GetFirstChild<Text>()?.Text;
                    if (!string.IsNullOrEmpty(t))
                        linkTextSb.Append(t);
                }
            }

            string linkText = linkTextSb.ToString().Trim();

            if (string.IsNullOrWhiteSpace(linkText) || url == null)
                return (null, null);

            return (linkText, url);
        }

        // ── Строим словарь xmlId → amendmentId из эндноутов ──────────────────
        private Dictionary<string, string> BuildEndnoteIdMap(WordprocessingDocument doc) {
            var map = new Dictionary<string, string>();
            var endnotesPart = doc.MainDocumentPart?.EndnotesPart;
            if (endnotesPart == null) return map;

            int numericCounter = 1;
            foreach (var endnote in endnotesPart.Endnotes.Elements<Endnote>()) {
                var type = endnote.Type;
                if (type != null &&
                    (type.Value == FootnoteEndnoteValues.Separator ||
                     type.Value == FootnoteEndnoteValues.ContinuationSeparator))
                    continue;

                var sb = new StringBuilder();
                foreach (var run in endnote.Descendants<Run>()) {
                    if (run.GetFirstChild<EndnoteReferenceMark>() != null) continue;
                    sb.Append(run.InnerText);
                }

                string content = sb.ToString().Trim();
                if (string.IsNullOrWhiteSpace(content)) continue;

                string xmlId = endnote.Id?.Value.ToString() ?? "";
                var specialIdMatch = Regex.Match(content, @"^([A-Z]+\d+)\s+(.+)");
                string amendmentId = specialIdMatch.Success
                    ? specialIdMatch.Groups[1].Value
                    : numericCounter++.ToString();

                if (!string.IsNullOrEmpty(xmlId))
                    map[xmlId] = amendmentId;
            }
            return map;
        }

        // ── Получить endnoteReference id из параграфа ─────────────────────────
        private string? ExtractEndnoteRefId(Paragraph para, Dictionary<string, string> endnoteIdMap) {
            foreach (var run in para.Elements<Run>()) {
                var endRef = run.GetFirstChild<EndnoteReference>();
                if (endRef == null) continue;

                if (endRef.CustomMarkFollows != null && endRef.CustomMarkFollows.Value) {
                    string customMark = string.Concat(
                        run.Elements<Text>().Select(t => t.Text ?? "")
                    ).Trim();
                    if (!string.IsNullOrWhiteSpace(customMark))
                        return customMark;
                }

                string xmlId = endRef.Id?.Value.ToString() ?? "";
                return endnoteIdMap.TryGetValue(xmlId, out var mappedId) ? mappedId : xmlId;
            }
            return null;
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
            IEnumerable<HyperlinkRelationship> relationships) {
            var map = new Dictionary<string, string>();
            foreach (var rel in relationships) {
                if (!map.ContainsKey(rel.Id))
                    map[rel.Id] = rel.Uri?.ToString() ?? "";
            }
            return map;
        }

        // ── Читаем таблицу Word и строим объект Table ─────────────────────────
        private Table ReadTable(DocumentFormat.OpenXml.Wordprocessing.Table wordTable) {

            var table = new Table();

            var rows = wordTable.Elements<TableRow>().ToList();
            if (rows.Count == 0)
                return table;

            string GetCellText(TableCell cell) {
                var parts = cell.Elements<Paragraph>()
                                .Select(p => GetParagraphText(p))
                                .Where(t => !string.IsNullOrWhiteSpace(t));
                return string.Join(" ", parts).Trim();
            }

            // Первая строка → заголовки
            foreach (var cell in rows[0].Elements<TableCell>())
                table.Headers.Add(GetCellText(cell));

            // Остальные строки → данные
            for (int i = 1; i < rows.Count; i++) {
                var rowData = new TableRowData();
                foreach (var cell in rows[i].Elements<TableCell>())
                    rowData.Cells.Add(GetCellText(cell));
                table.Rows.Add(rowData);
            }

            return table;
        }

        // ── Назначаем таблицу последнему активному объекту ────────────────────
        private void AssignTable(
            Table table,
            LastContext lastContext,
            Chapter? currentChapter,
            Section? currentSection,
            Article? currentArticle,
            Clause? currentClause,
            SubClause? currentSubClause) {

            switch (lastContext) {
                case LastContext.SubClause:
                    if (currentSubClause != null) currentSubClause.Table = table;
                    break;
                case LastContext.Clause:
                    if (currentClause != null) currentClause.Table = table;
                    break;
                case LastContext.Article:
                    if (currentArticle != null) currentArticle.Table = table;
                    break;
                case LastContext.Section:
                    if (currentSection != null) currentSection.Table = table;
                    break;
                case LastContext.Chapter:
                    if (currentChapter != null) currentChapter.Table = table;
                    break;
            }
        }

        // ── Основной метод ────────────────────────────────────────────────────
        public Laws ReadWordFile(string filePath) {
            var law = new Laws();

            Table.ResetCounter();

            ObservableCollection<TransitionalProvisions> transitional = new ObservableCollection<TransitionalProvisions>();
            ObservableCollection<SourceDocumentsList> sources = new ObservableCollection<SourceDocumentsList>();
            ObservableCollection<ConstitutionalAmendment> amendments = new ObservableCollection<ConstitutionalAmendment>();
            ObservableCollection<Models.ChangableData.Header> headers = new ObservableCollection<Models.ChangableData.Header>();

            Chapter currentChapter = null;
            Section currentSection = null;
            Article currentArticle = null;
            Clause currentClause = null;
            SubClause currentSubClause = null;
            TransitionalProvisions currentTransitional = null;
            TransitionalProvisions.Date = null;

            LastContext lastContext = LastContext.None;

            Mode mode = Mode.Header;
            var headerBuilder = new StringBuilder();

            bool expectChapterTitle = false;
            bool expectSectionTitle = false;

            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;

            var docRelationships = BuildRelationshipMap(
                doc.MainDocumentPart.HyperlinkRelationships);

            var endnoteIdMap = BuildEndnoteIdMap(doc);

            foreach (var element in body.ChildElements) {

                // ── ТАБЛИЦА ──────────────────────────────────────────────────
                if (element is DocumentFormat.OpenXml.Wordprocessing.Table wordTable) {
                    if (mode == Mode.Chapters && lastContext != LastContext.None) {
                        var table = ReadTable(wordTable);
                        AssignTable(table, lastContext,
                            currentChapter, currentSection, currentArticle,
                            currentClause, currentSubClause);
                    }
                    continue;
                }

                // ── ПАРАГРАФ ─────────────────────────────────────────────────
                if (element is not Paragraph para)
                    continue;

                var line = GetParagraphText(para);

                if (string.IsNullOrWhiteSpace(line) && mode != Mode.Transitional)
                    continue;

                if (line.Contains("INCLUDEPICTURE") ||
                    line.Contains("MERGEFORMATINET") ||
                    line.Contains("userway.org") ||
                    line.Contains("\\*"))
                    continue;

                if (line.Contains("Keçİd müddəaları", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("KEÇİD MÜDDƏALARI", StringComparison.OrdinalIgnoreCase)) {
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
                var header = new Models.ChangableData.Header();
                if (mode == Mode.Header) {
                    if (IsChapterLine(line)) {
                        header.Id = 1;
                        header.FullText = headerBuilder.ToString().Trim();
                        headers.Add(header);
                        mode = Mode.Chapters;
                        expectChapterTitle = true;
                        continue;
                    }

                    if (Regex.IsMatch(line, @"^Maddə\s+[\d\.]+\.")) {
                        header.Id = 1;
                        header.FullText = headerBuilder.ToString().Trim();
                        headers.Add(header);
                        mode = Mode.Chapters;

                        currentChapter = new Chapter("AZƏRBAYCAN RESPUBLİKASININ QANUNU");
                        law.Chapters.Add(currentChapter);
                        currentSection = new Section("Qanunlar");
                        currentChapter.Sections.Add(currentSection);
                        lastContext = LastContext.Section;

                        // Не делаем continue — строка обрабатывается ниже как Article
                    }
                    else {
                        headerBuilder.AppendLine(line);
                        continue;
                    }
                }

                // --- ожидаем название Chapter ---
                if (expectChapterTitle) {
                    currentChapter = new Chapter(line);
                    law.Chapters.Add(currentChapter);
                    expectChapterTitle = false;
                    lastContext = LastContext.Chapter;
                    continue;
                }

                // --- ожидаем название Section ---
                if (expectSectionTitle) {
                    currentSection = new Section(line);
                    currentChapter?.Sections.Add(currentSection);
                    expectSectionTitle = false;
                    lastContext = LastContext.Section;
                    continue;
                }

                if (mode == Mode.Chapters) {
                    // BÖLMƏ
                    if (IsChapterLine(line)) {
                        expectChapterTitle = true;
                        currentSection = null;
                        currentArticle = null;
                        currentClause = null;
                        currentSubClause = null;
                        continue;
                    }

                    // I fəsil
                    if (Regex.IsMatch(line, @"^[IVX]+\s*fəsil", RegexOptions.IgnoreCase)) {
                        expectSectionTitle = true;
                        currentArticle = null;
                        currentClause = null;
                        currentSubClause = null;
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

                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);

                        currentArticle = new Article(id, title, endnoteRefId);
                        currentSection?.Articles.Add(currentArticle);
                        currentClause = null;
                        currentSubClause = null;
                        lastContext = LastContext.Article;
                        continue;
                    }

                    // CLAUSE (I. II. III.)
                    if (Regex.IsMatch(line, @"^[IVX]+\.\s")) {
                        string text = Regex.Replace(line, @"^[IVX]+\.\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);
                        var (linkText1, url1) = ExtractHyperlink(para);
                        currentClause = currentArticle?.AddClause(text, endnoteId: endnoteRefId, linkText: linkText1, url: url1);
                        currentSubClause = null;
                        lastContext = LastContext.Clause;
                        continue;
                    }

                    // SUBCLAUSE (1) (2) (3)
                    if (Regex.IsMatch(line, @"^\d+\)")) {
                        string text = Regex.Replace(line, @"^\d+\)\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);
                        currentSubClause = currentClause?.AddSubClause(text, endnoteId: endnoteRefId);
                        lastContext = LastContext.SubClause;
                        continue;
                    }

                    // SUBCLAUSE (1.1.1. / 1.2.3. — три+ сегмента)
                    if (Regex.IsMatch(line, @"^\d+\.\d+\.\d+\.\s")) {
                        string text = Regex.Replace(line, @"^\d+\.\d+\.\d+\.\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);
                        currentSubClause = currentClause?.AddSubClause(text, endnoteId: endnoteRefId);
                        lastContext = LastContext.SubClause;
                        continue;
                    }

                    // CLAUSE (1.1. / 1.2. — два сегмента)
                    if (Regex.IsMatch(line, @"^\d+\.\d+\.\s")) {
                        string text = Regex.Replace(line, @"^\d+\.\d+\.\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);
                        var (linkText2, url2) = ExtractHyperlink(para);
                        currentClause = currentArticle?.AddClause(text, endnoteId: endnoteRefId, linkText: linkText2, url: url2);
                        currentSubClause = null;
                        lastContext = LastContext.Clause;
                        continue;
                    }

                    // CLAUSE (1. / 2. / 3. — одиночная цифра с точкой)
                    if (Regex.IsMatch(line, @"^\d+\.\s")) {
                        string text = Regex.Replace(line, @"^\d+\.\s*", "");
                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);
                        var (linkText3, url3) = ExtractHyperlink(para);
                        currentClause = currentArticle?.AddClause(text, endnoteId: endnoteRefId, linkText: linkText3, url: url3);
                        currentSubClause = null;
                        lastContext = LastContext.Clause;
                        continue;
                    }

                    // обычный текст
                    if (currentArticle != null) {
                        string? endnoteRefId = ExtractEndnoteRefId(para, endnoteIdMap);
                        var (linkText4, url4) = ExtractHyperlink(para);
                        if (currentClause == null) {
                            currentClause = currentArticle.AddClause(line, endnoteId: endnoteRefId, linkText: linkText4, url: url4);
                            lastContext = LastContext.Clause;
                        }
                        else {
                            currentSubClause = currentClause.AddSubClause(line, endnoteId: endnoteRefId);
                            lastContext = LastContext.SubClause;
                        }
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
                            Id = id,
                        };
                        transitional.Add(currentTransitional);
                    }
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
                if (mode == Mode.Sources) {
                    var (linkText, url) = ExtractHyperlink(para);

                    string cleanedLine = Regex.Replace(line, @"^\d+[\.\)]\s*", "");

                    if (!string.IsNullOrWhiteSpace(cleanedLine))
                        sources.Add(new SourceDocumentsList(cleanedLine, linkText, url));
                    continue;
                }
            }

            if (headers[0].FullText == null)
                headers[0].FullText = headerBuilder.ToString().Trim();

            ReadAmendmentsFromEndnotes(doc, amendments);

            law.UpperObjects.Add(new UpperObject {
                Id = 1,
                ObjectName = "HEADER",
                Headers = headers,
            });

            law.SourcesData.Add(new SourceData {
                Id = 1,
                Type = "KEÇİD MÜDDƏALARI",
                Source = new ObservableCollection<object>(transitional.Cast<object>())
            });
            law.SourcesData[0].Source.Add(new TransitionalProvisionsDateNote());

            law.SourcesData.Add(new SourceData {
                Id = 2,
                Type = "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                Source = new ObservableCollection<object>(sources.Cast<object>())
            });

            law.SourcesData.Add(new SourceData {
                Id = 3,
                Type = "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                Source = new ObservableCollection<object>(amendments.Cast<object>())
            });

            return law;
        }

        // ── Читаем amendments из endnotes ─────────────────────────────────────
        private void ReadAmendmentsFromEndnotes(WordprocessingDocument doc, ObservableCollection<ConstitutionalAmendment> amendments) {
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

                var specialIdMatch = Regex.Match(content, @"^([A-Z]+\d+)\s+(.+)");

                if (specialIdMatch.Success) {
                    amendmentId = specialIdMatch.Groups[1].Value;
                    amendmentTitle = specialIdMatch.Groups[2].Value.Trim();
                }
                else {
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