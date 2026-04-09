using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LawEditor.Models.TreeClasses
{
    public class FullTextWrapper
    {
        public static string GetFullText(object item)
        {
            return item switch
            {
                Header h => h.FullText ?? "",
                UpperObject u => GetFullUpperObject(u),
                Chapter c => GetFullChapter(c),
                Section s => GetFullSection(s),
                Article a => GetFullArticle(a),
                Clause cl => GetFullClause(cl),
                SubClause sc => sc.Text ?? "",
                TransitionalProvisions tp => tp.Title ?? "",
                SourceDocumentsList sd => sd.Title ?? "",
                ConstitutionalAmendment ca => ca.Title ?? "",
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

        private static string GetFullSection(Section section)
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

            foreach (var sub in clause.SubClauses)
                sb.AppendLine($"  {sub.Number}) {sub.Text}");

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
                    case SourceDocumentsList sd:
                        sb.AppendLine($"  {sd.Id}) {sd.Title}");
                        break;
                    case ConstitutionalAmendment ca:
                        sb.AppendLine($"  {ca.Id}) {ca.Title}");
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
                case UpperObject u: ParseUpperObyect(u,value); break;
                case Chapter c: ParseChapter(c, value); break;
                case Section s: ParseSection(s, value); break;
                case Article a: ParseArticle(a, value); break;
                case Clause cl: ParseClause(cl, value); break;
                case SubClause sc: sc.Text = value; break;
                case Header h: h.FullText = value; break;
                case SourceData sd: ParseSourceData(sd, value); break;
                case TransitionalProvisions tp: tp.Title = value; break;
                case SourceDocumentsList sd: sd.Title = value; break;
                case ConstitutionalAmendment ca: ca.Title = value; break;
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

            // всё остальное — один Header
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

            Section currentSection = null;
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
                        ? new Section { Id = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture), Title = match.Groups[2].Value }
                        : new Section { Title = line };
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
                            currentSection = new Section { Title = "Auto Section" };
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

        private static void ParseSection(Section section, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            var titleMatch = Regex.Match(lines[0].Trim(), @"^\{([0-9]+)\}\s+(.*)$");
            if (titleMatch.Success)
            {
                int.TryParse(titleMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int secId);
                section.Id = secId;
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
            if (titleMatch.Success)
            {
                decimal.TryParse(titleMatch.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal artId);
                article.Id = artId;
                article.Title = titleMatch.Groups[2].Value;
            }
            else
            {
                article.Title = lines[0].Trim();
            }

            article.Clauses.Clear();
            Clause currentClause = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                int indent = GetIndent(rawLine);
                var line = rawLine.Trim();

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
            if (match.Success)
            {
                clause.Number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                clause.Text = match.Groups[2].Value;
            }

            clause.SubClauses.Clear();

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var subMatch = Regex.Match(rawLine.Trim(), @"^(\d+)\)\s+(.*)$");
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


        private static void ParseSourceData(SourceData sourceData, string text)
{
    var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    if (lines.Length == 0) return;

    sourceData.Type = lines[0].Trim();
    sourceData.Source.Clear();

    for (int i = 1; i < lines.Length; i++)
    {
        var rawLine = lines[i];
        if (string.IsNullOrWhiteSpace(rawLine)) continue;

        var line = rawLine.Trim();

        switch (sourceData.Id)
        {
            case 1: // KEÇİD MÜDDƏALARI
            {
                var match = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                if (match.Success)
                    sourceData.AddTransitionalProvision(match.Groups[2].Value);
                else
                    sourceData.AddTransitionalProvision(line);
                break;
            }
            case 2: // İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI
            {
                var match = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                if (match.Success)
                    sourceData.AddSourceDocument(match.Groups[2].Value);
                else
                    sourceData.AddSourceDocument(line);
                break;
            }
            case 3: // KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI
            {
                var match = Regex.Match(line, @"^([^\s]+)\)\s+(.*)$");
                if (match.Success)
                    sourceData.AddConstitutionalAmendment(match.Groups[1].Value, match.Groups[2].Value);
                else
                    sourceData.AddConstitutionalAmendment(i.ToString(), line);
                break;
            }
        }
    }
}
    }
}