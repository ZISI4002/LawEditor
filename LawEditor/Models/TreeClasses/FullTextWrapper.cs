using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;

namespace LawEditor.Models.TreeClasses
{
    public class FullTextWrapper
    {
        public static bool CanRefresh { get; set; } = false;


        public static string GetFullText(object item)
        {

            return item switch
            {
                Header h => h.FullText ?? "",
                UpperObject u => GetFullUpperObject(u),
                Chapter c => GetFullChapter(c),
                Models.ChangableData.Section s => GetFullSection(s),
                Article a => GetFullArticle(a),
                Clause cl => GetFullClause(cl),
                SubClause sc => GetFullSubClause(sc),
                TransitionalProvisions tp => GetFullTransitionalProvisions(tp),
                TransitionalProvisionsDateNote tpDate => tpDate.DisplayText,
                SourceDocumentsList sd => GetFullSourseDocumentList(sd),
                ConstitutionalAmendment ca => GetFullConstitutionalAmendment(ca),
                SourceData sd => GetFullSourceData(sd),
                _ => ""
            };
        }

        private static int GetIndent(string line)
        {
            return line.TakeWhile(c => c == ' ').Count();
        }

        // ==================== GET ====================

        private static string GetFullUpperObject(UpperObject upperObject)
        {
            var sb = new StringBuilder();
            sb.AppendLine(upperObject.ObjectName);
            sb.AppendLine();
            if (upperObject.Headers != null)
                foreach (var header in upperObject.Headers)
                    sb.AppendLine(header.FullText);
            return sb.ToString();
        }

        private static string GetFullChapter(Chapter chapter)
        {
            var sb = new StringBuilder();
            sb.AppendLine(chapter.Title);
            sb.AppendLine();

            foreach (var section in chapter.Sections)
            {
                sb.AppendLine("  {" + section.Id + "} " + section.Title);
                foreach (var article in section.Articles)
                {
                    sb.AppendLine($"    [{article.Id.ToString(CultureInfo.InvariantCulture)}] {article.Title}");
                    foreach (var clause in article.Clauses)
                    {
                        sb.AppendLine($"      {clause.Number}. {clause.Text}");
                        foreach (var sub in clause.SubClauses)
                            sb.AppendLine($"        {sub.Number}) {sub.Text}");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetFullSection(Models.ChangableData.Section section)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{" + section.Id + "} " + section.Title);
            sb.AppendLine();

            foreach (var article in section.Articles)
            {
                sb.AppendLine($"  [{article.Id.ToString(CultureInfo.InvariantCulture)}] {article.Title}");
                foreach (var clause in article.Clauses)
                {
                    sb.AppendLine($"    {clause.Number}. {clause.Text}");
                    foreach (var sub in clause.SubClauses)
                        sb.AppendLine($"      {sub.Number}) {sub.Text}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetFullArticle(Article article)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{article.Id.ToString(CultureInfo.InvariantCulture)}] {article.Title}");
            if (!string.IsNullOrEmpty(article.EndnoteId))
                sb.AppendLine($"EndnoteId: {article.EndnoteId}");
            sb.AppendLine();

            foreach (var clause in article.Clauses)
            {
                sb.AppendLine($"  {clause.Number}. {clause.Text}");
                foreach (var sub in clause.SubClauses)
                    sb.AppendLine($"    {sub.Number}) {sub.Text}");
            }

            return sb.ToString();
        }

        private static string GetFullClause(Clause clause)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{clause.Number}. {clause.Text}");
            if (!string.IsNullOrEmpty(clause.EndnoteId))
                sb.AppendLine($"EndnoteId: {clause.EndnoteId}");
            sb.AppendLine();

            foreach (var sub in clause.SubClauses)
                sb.AppendLine($"  {sub.Number}) {sub.Text}");

            return sb.ToString();
        }

        private static string GetFullSubClause(SubClause sub)
        {
            string endnotePart = string.IsNullOrEmpty(sub.EndnoteId) ? "" : $"  (EndnoteId: {sub.EndnoteId})";
            return $"{sub.Number}) {sub.Text}  {endnotePart}";
        }

        private static string GetFullTransitionalProvisions(TransitionalProvisions tp)
        {
            return $"{tp.Id}) {tp.Title}";
        }
        private static string GetFullSourseDocumentList(SourceDocumentsList sd)
        {
            var sb = new StringBuilder();
            var titleWithoutLinkText = string.IsNullOrEmpty(sd.LinkText) ? sd.Title : sd.Title.Replace(sd.LinkText, "").Trim();
            sb.AppendLine($"  {sd.Id}) [{sd.LinkText}] {titleWithoutLinkText}");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(titleWithoutLinkText))
                sb.AppendLine($"🔗 Source URL: {sd.Url}");

            return sb.ToString();
        }

        private static string GetFullConstitutionalAmendment(ConstitutionalAmendment ca)
        {
            var sb = new StringBuilder();
            var titleWhitoutLinkText = string.IsNullOrEmpty(ca.LinkText) ? ca.Title : ca.Title.Replace(ca.LinkText, "").Trim();
            sb.AppendLine($"{ca.Id}) [{ca.LinkText}] {titleWhitoutLinkText}");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(ca.Url))
                sb.AppendLine($"🔗 Source URL: {ca.Url}");
            return sb.ToString();
        }

        private static string GetFullSourceData(SourceData sourceData)
        {
            var sb = new StringBuilder();
            sb.AppendLine(sourceData.Type);
            sb.AppendLine();

            foreach (var item in sourceData.Source)
            {
                switch (item)
                {
                    case TransitionalProvisions tp:
                        sb.AppendLine($"  {tp.Id}) {tp.Title}");
                        break;
                    case TransitionalProvisionsDateNote tpDate:
                        sb.AppendLine($"  {tpDate.DisplayText}");
                        break;
                    case SourceDocumentsList sd:
                        var titleWithoutLinkText = string.IsNullOrEmpty(sd.LinkText) ? sd.Title : sd.Title.Replace(sd.LinkText, "").Trim();
                        sb.AppendLine($"  {sd.Id}) [{sd.LinkText}] {titleWithoutLinkText}");
                        sb.AppendLine();
                        if (!string.IsNullOrEmpty(titleWithoutLinkText))
                        {
                            sb.AppendLine($"🔗 Source URL: {sd.Url}");
                            sb.AppendLine();
                            sb.AppendLine();
                        }
                        break;
                    case ConstitutionalAmendment ca:
                        var titleWhitoutLinkText = string.IsNullOrEmpty(ca.LinkText) ? ca.Title : ca.Title.Replace(ca.LinkText, "").Trim();
                        sb.AppendLine($"{ca.Id}) [{ca.LinkText}] {titleWhitoutLinkText}");
                        sb.AppendLine();
                        if (!string.IsNullOrEmpty(ca.Url))
                        {
                            sb.AppendLine($"🔗 Source URL: {ca.Url}");
                            sb.AppendLine();
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        // ==================== SET ====================

        public static void SetText(object item, string value)
        {
            switch (item)
            {
                case UpperObject u: ParseUpperObyect(u, value); break;
                case Chapter c: ParseChapter(c, value); break;
                case Models.ChangableData.Section s: ParseSection(s, value); break;
                case Article a: ParseArticle(a, value); break;
                case Clause cl: ParseClause(cl, value); break;
                case SubClause sc: ParseSubClause(sc, value); break;
                case Header h: h.FullText = value; break;
                case SourceData sd: ParseSourceData(sd, value); break;
                case TransitionalProvisions tp: ParseTransitionalProvisions(tp, value); break;
                case TransitionalProvisionsDateNote tpDate: tpDate.DisplayText = value; break;
                case SourceDocumentsList sd: ParseSourceDocumentsList(sd, value); break;
                case ConstitutionalAmendment ca: ParseConstitutionalAmendment(ca, value); break;
            }
        }

        // ==================== PARSE ====================

        private static void ParseUpperObyect(UpperObject upperObject, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            upperObject.ObjectName = lines[0].Trim();

            if (upperObject.Headers == null)
                upperObject.Headers = new ObservableCollection<Header>();

            upperObject.Headers.Clear();

            var rest = string.Join("\n", lines.Skip(1)).Trim();
            if (!string.IsNullOrEmpty(rest))
                upperObject.Headers.Add(new Header { FullText = rest });
        }

        private static void ParseChapter(Chapter chapter, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            chapter.Title = lines[0].Trim();
            chapter.Sections.Clear();

            Models.ChangableData.Section currentSection = null;
            Article currentArticle = null;
            Clause currentClause = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                int indent = GetIndent(rawLine);
                var line = rawLine.Trim();

                if (indent >= 2 && indent < 4)
                {
                    var match = Regex.Match(line, @"^\{([0-9]+)\}\s+(.*)$");
                    currentSection = match.Success
                        ? new Models.ChangableData.Section { Id = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture), Title = match.Groups[2].Value }
                        : new Models.ChangableData.Section { Title = line };
                    chapter.Sections.Add(currentSection);
                    currentArticle = null;
                    currentClause = null;
                }
                else if (indent >= 4 && indent < 6)
                {
                    var match = Regex.Match(line, @"^\[([0-9.]+)\]\s+(.*)$");
                    if (match.Success)
                    {
                        if (currentSection == null)
                        {
                            currentSection = new Models.ChangableData.Section { Title = "Auto Section" };
                            chapter.Sections.Add(currentSection);
                        }
                        decimal.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal artId);
                        currentArticle = new Article { Id = artId, Title = match.Groups[2].Value };
                        currentSection.Articles.Add(currentArticle);
                        currentClause = null;
                    }
                }
                else if (indent >= 6 && indent < 8)
                {
                    var match = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
                    if (match.Success && currentArticle != null)
                    {
                        currentClause = new Clause
                        {
                            Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            Text = match.Groups[2].Value
                        };
                        currentArticle.Clauses.Add(currentClause);
                    }
                }
                else if (indent >= 8)
                {
                    var match = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                    if (match.Success && currentClause != null)
                    {
                        currentClause.SubClauses.Add(new SubClause
                        {
                            Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            Text = match.Groups[2].Value
                        });
                    }
                }
            }
        }

        private static void ParseSection(Models.ChangableData.Section section, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;
            var titleMatch = Regex.Match(lines[0].Trim(), @"^\{([0-9]+)\}\s+(.*)$");
            int oldId = section.Id;
            if (titleMatch.Success)
            {
                int.TryParse(titleMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int secId);
                if (oldId != secId)
                {
                    MessageBox.Show(
                       "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                       "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                         );
                }
                section.Title = titleMatch.Groups[2].Value;
            }
            else
            {
                section.Title = lines[0].Trim();
            }

            section.Articles.Clear();
            Article currentArticle = null;
            Clause currentClause = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                int indent = GetIndent(rawLine);
                var line = rawLine.Trim();

                if (indent >= 2 && indent < 4)
                {
                    var match = Regex.Match(line, @"^\[([0-9.]+)\]\s+(.*)$");
                    if (match.Success)
                    {
                        decimal.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal artId);
                        currentArticle = new Article { Id = artId, Title = match.Groups[2].Value };
                        section.Articles.Add(currentArticle);
                        currentClause = null;
                    }
                }
                else if (indent >= 4 && indent < 6)
                {
                    var match = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
                    if (match.Success && currentArticle != null)
                    {
                        currentClause = new Clause
                        {
                            Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            Text = match.Groups[2].Value
                        };
                        currentArticle.Clauses.Add(currentClause);
                    }
                }
                else if (indent >= 6)
                {
                    var match = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                    if (match.Success && currentClause != null)
                    {
                        currentClause.SubClauses.Add(new SubClause
                        {
                            Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            Text = match.Groups[2].Value
                        });
                    }
                }
            }
        }

        private static void ParseArticle(Article article, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            var titleMatch = Regex.Match(lines[0].Trim(), @"^\[([0-9.]+)\]\s+(.*)$");
            decimal oldId = article.Id;
            if (titleMatch.Success)
            {
                decimal.TryParse(titleMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal artId);

                if (artId != oldId)
                {
                    MessageBox.Show(
                            "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                            "Xəbərdarlıq",
                             MessageBoxButton.OK,
                             MessageBoxImage.Information
                              );
                }
                article.Title = titleMatch.Groups[2].Value;
            }
            else
            {
                article.Title = lines[0].Trim();
            }

            article.Clauses.Clear();
            article.EndnoteId = null;
            Clause currentClause = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                int indent = GetIndent(rawLine);
                var line = rawLine.Trim();

                if (line.StartsWith("EndnoteId:"))
                {
                    article.EndnoteId = line.Replace("EndnoteId:", "").Trim();
                    continue;
                }

                if (indent >= 2 && indent < 4)
                {
                    var match = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
                    if (match.Success)
                    {
                        currentClause = new Clause
                        {
                            Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            Text = match.Groups[2].Value
                        };
                        article.Clauses.Add(currentClause);
                    }
                }
                else if (indent >= 4)
                {
                    var match = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                    if (match.Success && currentClause != null)
                    {
                        currentClause.SubClauses.Add(new SubClause
                        {
                            Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                            Text = match.Groups[2].Value
                        });
                    }
                }
            }
        }

        private static void ParseClause(Clause clause, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            var match = Regex.Match(lines[0].Trim(), @"^(\d+)\.\s+(.*)$");
            int OldNumber = clause.Number;
            if (match.Success)
            {
                if (OldNumber != int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
                {
                    MessageBox.Show(
                        "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                        "Xəbərdarlıq",
                         MessageBoxButton.OK,
                         MessageBoxImage.Information
                          );

                }
                clause.Text = match.Groups[2].Value;
            }

            clause.SubClauses.Clear();
            clause.EndnoteId = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var line = rawLine.Trim();

                if (line.StartsWith("EndnoteId:"))
                {
                    clause.EndnoteId = line.Replace("EndnoteId:", "").Trim();
                    continue;
                }

                var subMatch = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                if (subMatch.Success)
                {
                    clause.SubClauses.Add(new SubClause
                    {
                        Number = int.Parse(subMatch.Groups[1].Value, CultureInfo.InvariantCulture),
                        Text = subMatch.Groups[2].Value
                    });
                }
            }
        }

        private static void ParseSubClause(SubClause sub, string text)
        {
            // Формат GET: "N) текст  (EndnoteId: xxx)"
            var trimmed = text.Trim();
            var endnoteMatch = Regex.Match(trimmed, @"\(EndnoteId:\s*([^)]+)\)\s*$");
            if (endnoteMatch.Success)
            {
                sub.EndnoteId = endnoteMatch.Groups[1].Value.Trim();
                trimmed = trimmed.Substring(0, endnoteMatch.Index).Trim();
            }
            else
            {
                sub.EndnoteId = null;
            }

            var match = Regex.Match(trimmed, @"^(\d+)\)\s+(.*)$");
            int OldNumber = sub.Number;
            if (match.Success)
            {
                if (OldNumber != int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
                {
                    MessageBox.Show(
                         "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                         "Xəbərdarlıq",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information
                           );

                }
                sub.Text = match.Groups[2].Value;
            }
            else
            {
                sub.Text = trimmed;
            }
        }

        private static void ParseTransitionalProvisions(TransitionalProvisions tp, string text)
        {
            int oldId = tp.Id;
            // Формат GET: "N) текст"
            var match = Regex.Match(text.Trim(), @"^(\d*)\)\s+(.*)$");
            if (match.Success)
            {
                if (oldId != int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
                {
                    MessageBox.Show(
                       "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                       "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                         );
                    CanRefresh = true;
                }
                tp.Title = match.Groups[2].Value;
            }
            else
            {

                tp.Title = text.Trim();
            }
        }
        private static void ParseSourceDocumentsList(SourceDocumentsList sd, string text)
        {
            // Формат GET:
            //   {sd.Id}) [{sd.LinkText}] titleWithoutLinkText
            //
            //   🔗 Source URL: url

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            sd.Url = null;
            sd.LinkText = null;

            var firstLine = lines[0].Trim();

            // Пробуем: N) [LinkText] RestOfTitle
            var fullMatch = Regex.Match(firstLine, @"^(\d*)\)\s+\[([^\]]*)\]\s+(.*)$");
            int oldId = sd.Id;
            if (fullMatch.Success)
            {
                if (oldId != int.Parse(fullMatch.Groups[1].Value, CultureInfo.InvariantCulture))
                {
                    MessageBox.Show(
                        "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                        "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    CanRefresh = true;
                }
                sd.LinkText = fullMatch.Groups[2].Value;
                var rest = fullMatch.Groups[3].Value.Trim();
                sd.Title = string.IsNullOrEmpty(rest)
                    ? sd.LinkText
                    : sd.LinkText + " " + rest;
            }
            else
            {
                // Нет LinkText
                var simpleMatch = Regex.Match(firstLine, @"^(\d+)\)\s+(.*)$");
                if (simpleMatch.Success)
                {
                    sd.Id = int.Parse(simpleMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                    sd.Title = simpleMatch.Groups[2].Value;
                }
                else
                {
                    sd.Title = firstLine;
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("🔗 Source URL:"))
                    sd.Url = line.Replace("🔗 Source URL:", "").Trim();
            }
        }
        private static void ParseConstitutionalAmendment(ConstitutionalAmendment ca, string text)
        {

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;
            ca.Url = null;
            ca.LinkText = null;
            var firstLine = lines[0].Trim();
            // Пробуем: N) [LinkText] RestOfTitle
            var fullMatch = Regex.Match(firstLine, @"^([^\s]*)\)\s+\[([^\]]*)\]\s+(.*)$");
            string oldId = ca.Id;
            if (fullMatch.Success)
            {
                if (oldId != fullMatch.Groups[1].Value)
                {
                    MessageBox.Show(
                        "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                        "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    CanRefresh = true;
                }

                ca.LinkText = fullMatch.Groups[2].Value;
                var rest = fullMatch.Groups[3].Value.Trim();
                ca.Title = string.IsNullOrEmpty(rest)
                    ? ca.LinkText
                    : ca.LinkText + " " + rest;
            }
            else
            {
                // Нет LinkText
                var simpleMatch = Regex.Match(firstLine, @"^([^\s]+)\)\s+(.*)$");
                if (simpleMatch.Success)
                {
                    ca.Id = simpleMatch.Groups[1].Value;
                    ca.Title = simpleMatch.Groups[2].Value;
                }
                else
                {
                    ca.Title = firstLine;
                }
            }
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("🔗 Source URL:"))
                    ca.Url = line.Replace("🔗 Source URL:", "").Trim();
            }
        }

        private static void ParseSourceData(SourceData sourceData, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            sourceData.Type = lines[0].Trim();

            List<int> TransitionalProvisionsIds = sourceData.Source
                .OfType<TransitionalProvisions>()
                .Select(tp => tp.Id)
                .ToList();
            List<int> SourceDocumentsIds = sourceData.Source
                .OfType<SourceDocumentsList>()
                .Select(sd => sd.Id)
                .ToList();
            List<string> ConstitutionalAmendmentIds = sourceData.Source
                .OfType<ConstitutionalAmendment>()
                .Select(ca => ca.Id)
                .ToList();

            sourceData.Source.Clear();

            bool trPending = false;
            var tr = new TransitionalProvisions();
            var sd = new SourceDocumentsList();
            var ca = new ConstitutionalAmendment();

            // Transitional counters
            int IdcounterofTransitionalProvisions = 0;
            var TransitionalProvisionsResult = MessageBoxResult.No;
            int IdofChangedTransitionalElement = 0;
            int PositionOfChangedTransitionalElement = 0;
            bool transitionalAsked = false;

            // SourceDocument counters
            int IdcounterofSourceDocuments = 0;
            var SourceDocumentsResult = MessageBoxResult.No;
            int IdofChangedSourceDocumentElement = 0;
            int PositionOfChangedSourceDocumentElement = 0;
            bool sourceDocumentAsked = false;
            bool sdUrlPending = false;
            // ConstitutionalAmendment counters
            int positionCounterofAmendments = 1;
            
            bool caUrlPending = false;
            bool constitutionalAmendmentAsked = false;
            var ConstitutionalAmendmentResult = MessageBoxResult.No;
            string IdofChangedConstitutionalAmendmentElement = "";
            int PositionOfChangedConstitutionalAmendmentElement = 0;
            int IdcounterofConstitutionalAmendments = 0;        

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var line = rawLine.Trim();

                switch (sourceData.Id)
                {
                    case 1:
                        {
                            var match = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                            if (match.Success)
                            {
                                if (trPending)
                                    sourceData.AddTransitionalProvision(title: tr.Title, id: tr.Id);

                                tr = new TransitionalProvisions();
                                tr.Id = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                                tr.Title = match.Groups[2].Value;
                                trPending = true;

                                if (TransitionalProvisionsResult == MessageBoxResult.No &&
                                    IdcounterofTransitionalProvisions < TransitionalProvisionsIds.Count &&
                                    TransitionalProvisionsIds[IdcounterofTransitionalProvisions] != tr.Id &&
                                    !transitionalAsked)
                                {
                                    transitionalAsked = true;
                                    TransitionalProvisionsResult = MessageBox.Show(
                                        "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                        "Təsdiqləmə",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

                                    if (TransitionalProvisionsResult == MessageBoxResult.Yes)
                                    {
                                        IdofChangedTransitionalElement = tr.Id;
                                        PositionOfChangedTransitionalElement = IdcounterofTransitionalProvisions;
                                    }
                                }

                                IdcounterofTransitionalProvisions++;
                                break;
                            }

                            var dateMatch = Regex.Match(line,
                                @"^\d{1,2}\s+(yanvar|fevral|mart|aprel|may|iyun|iyul|avqust|sentyabr|oktyabr|noyabr|dekabr)\s+\d{4}");
                            if (dateMatch.Success)
                            {
                                TransitionalProvisions.Date = line;
                                break;
                            }

                            if (Regex.IsMatch(line, @"^№"))
                            {
                                TransitionalProvisions.Date += "\n" + line;
                                break;
                            }

                            if (Regex.IsMatch(line, @"^\)\s+(.*)$"))
                            {
                                if (trPending)
                                {
                                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                                    tr.Title = tr.Title + "\n" + rest;
                                    CanRefresh = true;
                                }
                                break;
                            }

                            if (trPending)
                                tr.Title = tr.Title + "\n" + line;

                            break;
                        }

                  case 2:
{
    // Сначала проверяем новый элемент с [LinkText]
    var fullMatch = Regex.Match(line, @"^(\d+)\)\s+\[([^\]]*)\]\s+(.*)$");
    if (fullMatch.Success)
    {
        if (sd.Title != null)
        {
            sourceData.AddSourceDocument(sd.Title, sd.Id, sd.LinkText, sd.Url);
            sdUrlPending = false;
        }

        sd = new SourceDocumentsList();
        sd.Id = int.Parse(fullMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        sd.LinkText = fullMatch.Groups[2].Value;
        var rest = fullMatch.Groups[3].Value.Trim();
        sd.Title = string.IsNullOrEmpty(rest) ? sd.LinkText : sd.LinkText + " " + rest;

        if (SourceDocumentsResult == MessageBoxResult.No &&
            IdcounterofSourceDocuments < SourceDocumentsIds.Count &&
            SourceDocumentsIds[IdcounterofSourceDocuments] != sd.Id &&
            !sourceDocumentAsked)
        {
            sourceDocumentAsked = true;
            SourceDocumentsResult = MessageBox.Show(
                "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                "Təsdiqləmə",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (SourceDocumentsResult == MessageBoxResult.Yes)
            {
                IdofChangedSourceDocumentElement = sd.Id;
                PositionOfChangedSourceDocumentElement = IdcounterofSourceDocuments;
            }
        }

        IdcounterofSourceDocuments++;
        break;
    }

    // Потом новый элемент без [LinkText]
    var simpleMatch = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
    if (simpleMatch.Success)
    {
        if (sd.Title != null)
        {
            sourceData.AddSourceDocument(sd.Title, sd.Id, sd.LinkText, sd.Url);
            sdUrlPending = false;
        }

        sd = new SourceDocumentsList();
        sd.Id = int.Parse(simpleMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        sd.Title = simpleMatch.Groups[2].Value;

        if (SourceDocumentsResult == MessageBoxResult.No &&
            IdcounterofSourceDocuments < SourceDocumentsIds.Count &&
            SourceDocumentsIds[IdcounterofSourceDocuments] != sd.Id &&
            !sourceDocumentAsked)
        {
            sourceDocumentAsked = true;
            SourceDocumentsResult = MessageBox.Show(
                "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                "Təsdiqləmə",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (SourceDocumentsResult == MessageBoxResult.Yes)
            {
                IdofChangedSourceDocumentElement = sd.Id;
                PositionOfChangedSourceDocumentElement = IdcounterofSourceDocuments;
            }
        }

        IdcounterofSourceDocuments++;
        break;
    }

    // Потом URL
    if (line.StartsWith("🔗 Source URL:"))
    {
        if (sdUrlPending)
        {
            MessageBox.Show(
                "Ardıcıl olaraq iki 'Source URL' əlavə etmək olmaz. İkinci URL nəzərə alınmadı.",
                "Xəbərdarlıq",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
                                    CanRefresh = true;
                                    break;
        }

        sd.Url = line.Replace("🔗 Source URL:", "").Trim();
        sdUrlPending = true;
        break;
    }

    // Строка вида ") текст" без номера — склеиваем к предыдущему
    if (Regex.IsMatch(line, @"^\)\s+\[([^\]]*)\]\s+(.*)$"))
        {
                                if (sd.Title != null)
                                {
                                    sourceData.AddSourceDocument(sd.Title, sd.Id, sd.LinkText, sd.Url);
                                    var rest = Regex.Match(line, @"^\)\s+\[([^\]]*)\]\s+(.*)$").Groups[2].Value;
                                    var linkTextMatch = Regex.Match(line, @"^\)\s+\[([^\]]*)\]\s+(.*)$").Groups[1].Value;
                                    sd.Title = rest;
                                    sd.LinkText = linkTextMatch;
                                    sd.Id = SourceDocumentsIds[IdcounterofSourceDocuments];
                                    sdUrlPending = false;
                                    sourceDocumentAsked = true;
                                    IdcounterofSourceDocuments++;
                                }
                                break;
                            }
                                if (Regex.IsMatch(line, @"^\)\s+(.*)$"))
                               {
                                if (sd.Title != null)
                                  {
                                      sourceData.AddSourceDocument(sd.Title, sd.Id, sd.LinkText, sd.Url);
                                      var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                                      sd.Title = rest;
                                      sd.Id = SourceDocumentsIds[IdcounterofSourceDocuments];

                                      sdUrlPending = false; 
                                      sourceDocumentAsked = true; 
                                        IdcounterofSourceDocuments++;
                                }
                                        break;
                                   }

                              // Обычная continuation строка
                               if (sd.Title != null)
                                     sd.Title = sd.Title + "\n" + line;
 
                                     break;
                                   }

                    case 3:
                        {
                            var fullMatch = Regex.Match(line, @"^([^\s]+)\)\s+\[([^\]]*)\]\s+(.*)$");
                            if (fullMatch.Success)
                            {
                                if (ca.Title != null)
                                {
                                    sourceData.AddConstitutionalAmendment(ca.Title, ca.Id, ca.LinkText, ca.Url, positionCounterofAmendments);
                                    positionCounterofAmendments++;
                                    caUrlPending = false;
                                }

                                ca = new ConstitutionalAmendment();
                                ca.Id = fullMatch.Groups[1].Value;
                                ca.LinkText = fullMatch.Groups[2].Value;
                                var rest = fullMatch.Groups[3].Value.Trim();
                                ca.Title = string.IsNullOrEmpty(rest) ? ca.LinkText : ca.LinkText + " " + rest;

                                if (ConstitutionalAmendmentResult == MessageBoxResult.No &&
                                    IdcounterofConstitutionalAmendments < ConstitutionalAmendmentIds.Count &&
                                    ConstitutionalAmendmentIds[IdcounterofConstitutionalAmendments] != ca.Id &&
                                    !constitutionalAmendmentAsked)
                                {
                                    constitutionalAmendmentAsked = true;
                                    ConstitutionalAmendmentResult = MessageBox.Show(
                                        "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                        "Təsdiqləmə",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

                                    if (ConstitutionalAmendmentResult == MessageBoxResult.Yes)
                                    {
                                        IdofChangedConstitutionalAmendmentElement = ca.Id;
                                        PositionOfChangedConstitutionalAmendmentElement = IdcounterofConstitutionalAmendments;
                                    }
                                }

                                IdcounterofConstitutionalAmendments++;
                                break;
                            }

                            var simpleMatch = Regex.Match(line, @"^([^\s]+)\)\s+(.*)$");
                            if (simpleMatch.Success)
                            {
                                if (ca.Title != null)
                                {
                                    sourceData.AddConstitutionalAmendment(ca.Title, ca.Id, ca.LinkText, ca.Url, positionCounterofAmendments);
                                    positionCounterofAmendments++;
                                    caUrlPending = false;
                                }

                                ca = new ConstitutionalAmendment();
                                ca.Id = simpleMatch.Groups[1].Value;
                                ca.Title = simpleMatch.Groups[2].Value;

                                if (ConstitutionalAmendmentResult == MessageBoxResult.No &&
                                    IdcounterofConstitutionalAmendments < ConstitutionalAmendmentIds.Count &&
                                    ConstitutionalAmendmentIds[IdcounterofConstitutionalAmendments] != ca.Id &&
                                    !constitutionalAmendmentAsked)
                                {
                                    constitutionalAmendmentAsked = true;
                                    ConstitutionalAmendmentResult = MessageBox.Show(
                                        "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                        "Təsdiqləmə",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

                                    if (ConstitutionalAmendmentResult == MessageBoxResult.Yes)
                                    {
                                        IdofChangedConstitutionalAmendmentElement = ca.Id;
                                        PositionOfChangedConstitutionalAmendmentElement = IdcounterofConstitutionalAmendments;
                                    }
                                }

                                IdcounterofConstitutionalAmendments++;
                                break;
                            }

                            if (line.StartsWith("🔗 Source URL:"))
                            {
                                if (caUrlPending)
                                {
                                    MessageBox.Show(
                                        "Ardıcıl olaraq iki 'Source URL' əlavə etmək olmaz. İkinci URL nəzərə alınmadı.",
                                        "Xəbərdarlıq",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                    CanRefresh = true;
                                    break;
                                }

                                ca.Url = line.Replace("🔗 Source URL:", "").Trim();
                                caUrlPending = true;
                                break;
                            }

                            // Строка вида ") [LinkText] текст" без номера
                            if (Regex.IsMatch(line, @"^\)\s+\[([^\]]*)\]\s+(.*)$"))
                            {
                                if (ca.Title != null)
                                {
                                    sourceData.AddConstitutionalAmendment(ca.Title, ca.Id, ca.LinkText, ca.Url, positionCounterofAmendments);
                                    positionCounterofAmendments++;
                                    var linkTextMatch = Regex.Match(line, @"^\)\s+\[([^\]]*)\]\s+(.*)$");
                                    ca = new ConstitutionalAmendment();
                                    ca.LinkText = linkTextMatch.Groups[1].Value;
                                    ca.Title = linkTextMatch.Groups[2].Value;
                                    ca.Id = ConstitutionalAmendmentIds[IdcounterofConstitutionalAmendments];
                                    caUrlPending = false;
                                    constitutionalAmendmentAsked = true;
                                    IdcounterofConstitutionalAmendments++;
                                }
                                break;
                            }

                            // Строка вида ") текст" без номера
                            if (Regex.IsMatch(line, @"^\)\s+(.*)$"))
                            {
                                if (ca.Title != null)
                                {
                                    sourceData.AddConstitutionalAmendment(ca.Title, ca.Id, ca.LinkText, ca.Url, positionCounterofAmendments);
                                    positionCounterofAmendments++;
                                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                                    ca = new ConstitutionalAmendment();
                                    ca.Title = rest;
                                    ca.Id = ConstitutionalAmendmentIds[IdcounterofConstitutionalAmendments];
                                    caUrlPending = false;
                                    constitutionalAmendmentAsked = true;
                                    IdcounterofConstitutionalAmendments++;
                                }
                                break;
                            }

                            // Обычная continuation строка
                            if (ca.Title != null)
                                ca.Title = ca.Title + "\n" + line;

                            break;
                        }
                }
            }

            // Добавляем последний незакрытый TransitionalProvision
            if (sourceData.Id == 1 && trPending)
                sourceData.AddTransitionalProvision(title: tr.Title, id: tr.Id);

            // Добавляем последний незакрытый SourceDocument
            if (sourceData.Id == 2 && sd.Title != null)
                sourceData.AddSourceDocument(sd.Title, sd.Id, sd.LinkText, sd.Url);
            
            // Добавляем последний незакрытый ConstitutionalAmendment
            if (sourceData.Id == 3 && ca.Title != null)
            {
                sourceData.AddConstitutionalAmendment(ca.Title, ca.Id, ca.LinkText, ca.Url, positionCounterofAmendments);
            }
            
            // Обработка изменения ID для TransitionalProvisions
            if (TransitionalProvisionsResult == MessageBoxResult.Yes)
            {
                var currentList = sourceData.Source.OfType<TransitionalProvisions>().ToList();

                if (currentList.Count >= TransitionalProvisionsIds.Count)
                {
                    sourceData.UpdateTransitionalProvision(IdofChangedTransitionalElement);
                }
                else
                {
                    sourceData.AddTransitionalProvision(
                        title: "",
                        id: TransitionalProvisionsIds[PositionOfChangedTransitionalElement]);
                    sourceData.DeleteTransitionalProvision(
                        TransitionalProvisionsIds[PositionOfChangedTransitionalElement]);
                }

                CanRefresh = true;
            }

            // Обработка изменения ID для SourceDocuments
            if (SourceDocumentsResult == MessageBoxResult.Yes)
            {
                var currentList = sourceData.Source.OfType<SourceDocumentsList>().ToList();

                if (currentList.Count >= SourceDocumentsIds.Count)
                {
                    sourceData.UpdateSourceDocument(
                        IdofChangedSourceDocumentElement);
                }
                else
                {
                    sourceData.AddSourceDocument(
                        title: "",
                        id: SourceDocumentsIds[PositionOfChangedSourceDocumentElement]);
                    sourceData.DeleteSourceDocument(
                        SourceDocumentsIds[PositionOfChangedSourceDocumentElement]);
                }

                CanRefresh = true;
            }

            // Обработка изменения ID для ConstitutionalAmendments
            if (ConstitutionalAmendmentResult == MessageBoxResult.Yes)
            {
                var currentList = sourceData.Source.OfType<ConstitutionalAmendment>().ToList();

                if (currentList.Count >= ConstitutionalAmendmentIds.Count)
                {
                    sourceData.UpdateConstitutionalAmendment(
                        ConstitutionalAmendmentIds[PositionOfChangedConstitutionalAmendmentElement],
                        IdofChangedConstitutionalAmendmentElement);
                }
                else
                {
                    sourceData.AddConstitutionalAmendment(
                        title: "",
                        id: ConstitutionalAmendmentIds[PositionOfChangedConstitutionalAmendmentElement]);
                    sourceData.DeleteConstitutionalAmendment(
                        ConstitutionalAmendmentIds[PositionOfChangedConstitutionalAmendmentElement]);
                }

                CanRefresh = true;
            }


            if (sourceData.Id == 1)
                sourceData.Source.Add(new TransitionalProvisionsDateNote());
        }
    }
}