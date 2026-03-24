using LawEditor.Models.ChangableData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LawEditor.Models.TreeClasses
{
   
        public class FullTextWrapper
        {
            public static string GetFullText(object item)
            {
                return item switch
                {
                    Chapter c => GetFullChapter(c),
                    Section s => GetFullSection(s),
                    Article a => GetFullArticle(a),
                    Clause cl => GetFullClause(cl),
                    SubClause sc => sc.Text ?? "",
                    _ => ""
                };
            }

        private static string GetFullChapter(Chapter chapter)
        {
            var sb = new StringBuilder();
            sb.AppendLine(chapter.Title);
            sb.AppendLine();

            foreach (var section in chapter.Sections)
            {
                sb.AppendLine($"  {section.Title}");
                foreach (var article in section.Articles)
                {
                    sb.AppendLine($"    [{article.Id}] {article.Title}");  // ← добавили Id
                    foreach (var clause in article.Clauses)
                    {
                        sb.AppendLine($"      {clause.Number}. {clause.Text}");
                        foreach (var sub in clause.SubClauses)
                        {
                            sb.AppendLine($"        {sub.Number}) {sub.Text}");
                        }
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetFullSection(Section section)
        {
            var sb = new StringBuilder();
            sb.AppendLine(section.Title);
            sb.AppendLine();

            foreach (var article in section.Articles)
            {
                sb.AppendLine($"  [{article.Id}] {article.Title}");  // ← добавили Id
                foreach (var clause in article.Clauses)
                {
                    sb.AppendLine($"    {clause.Number}. {clause.Text}");
                    foreach (var sub in clause.SubClauses)
                    {
                        sb.AppendLine($"      {sub.Number}) {sub.Text}");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetFullArticle(Article article)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{article.Id}] {article.Title}");  // ← добавили Id
            sb.AppendLine();

            foreach (var clause in article.Clauses)
            {
                sb.AppendLine($"  {clause.Number}. {clause.Text}");
                foreach (var sub in clause.SubClauses)
                {
                    sb.AppendLine($"    {sub.Number}) {sub.Text}");
                }
            }

            return sb.ToString();
        }
        private static string GetFullClause(Clause clause)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{clause.Number}. {clause.Text}");

                foreach (var sub in clause.SubClauses)
                {
                    sb.AppendLine($"  {sub.Number}) {sub.Text}");
                }

                return sb.ToString();
            }

            public static void SetText(object item, string value)
            {
                switch (item)
                {
                    case Chapter c: ParseChapter(c, value); break;
                    case Section s: ParseSection(s, value); break;
                    case Article a: ParseArticle(a, value); break;
                    case Clause cl: ParseClause(cl, value); break;
                    case SubClause sc: sc.Text = value; break;
                }
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
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Секция (2 пробела)
                if (line.StartsWith("  ") && !line.StartsWith("    "))
                {
                    currentSection = new Section { Title = line.Trim() };
                    chapter.Sections.Add(currentSection);
                    currentArticle = null;
                    currentClause = null;
                }
                // Статья (4 пробела + "[ID]")
                else if (line.StartsWith("    ") && !line.StartsWith("      "))
                {
                    if (currentSection != null)
                    {
                        // Парсим "[123.45] Название статьи"
                        var articleMatch = Regex.Match(line.Trim(), @"^\[([0-9.]+)\]\s*(.*)$");
                        if (articleMatch.Success)
                        {
                            float id = float.Parse(articleMatch.Groups[1].Value);
                            string title = articleMatch.Groups[2].Value;
                            currentArticle = new Article { Id = id, Title = title };
                            currentSection.Articles.Add(currentArticle);
                            currentClause = null;
                        }
                    }
                }
                // Бенд (6 пробелов)
                else if (line.StartsWith("      ") && !line.StartsWith("        "))
                {
                    var match = Regex.Match(line.Trim(), @"^(\d+)\.\s*(.*)$");
                    if (match.Success && currentArticle != null)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string clauseText = match.Groups[2].Value;
                        currentClause = new Clause { Number = number, Text = clauseText };
                        currentArticle.Clauses.Add(currentClause);
                    }
                }
                // Суббенд (8 пробелов)
                else if (line.StartsWith("        "))
                {
                    var match = Regex.Match(line.Trim(), @"^(\d+)\)\s*(.*)$");
                    if (match.Success && currentClause != null)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string subText = match.Groups[2].Value;
                        currentClause.SubClauses.Add(new SubClause { Number = number, Text = subText });
                    }
                }
            }
        }

        private static void ParseSection(Section section, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            section.Title = lines[0].Trim();
            section.Articles.Clear();

            Article currentArticle = null;
            Clause currentClause = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Статья (2 пробела + "[ID]")
                if (line.StartsWith("  ") && !line.StartsWith("    "))
                {
                    var articleMatch = Regex.Match(line.Trim(), @"^\[([0-9.]+)\]\s*(.*)$");
                    if (articleMatch.Success)
                    {
                        float id = float.Parse(articleMatch.Groups[1].Value);
                        string title = articleMatch.Groups[2].Value;
                        currentArticle = new Article { Id = id, Title = title };
                        section.Articles.Add(currentArticle);
                        currentClause = null;
                    }
                }
                // Бенд (4 пробела)
                else if (line.StartsWith("    ") && !line.StartsWith("      "))
                {
                    var match = Regex.Match(line.Trim(), @"^(\d+)\.\s*(.*)$");
                    if (match.Success && currentArticle != null)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string clauseText = match.Groups[2].Value;
                        currentClause = new Clause { Number = number, Text = clauseText };
                        currentArticle.Clauses.Add(currentClause);
                    }
                }
                // Суббенд (6 пробелов)
                else if (line.StartsWith("      "))
                {
                    var match = Regex.Match(line.Trim(), @"^(\d+)\)\s*(.*)$");
                    if (match.Success && currentClause != null)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string subText = match.Groups[2].Value;
                        currentClause.SubClauses.Add(new SubClause { Number = number, Text = subText });
                    }
                }
            }
        }

        private static void ParseArticle(Article article, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            // Первая строка - "[123.45] Название"
            var titleMatch = Regex.Match(lines[0].Trim(), @"^\[([0-9.]+)\]\s*(.*)$");
            if (titleMatch.Success)
            {
                article.Id = float.Parse(titleMatch.Groups[1].Value);
                article.Title = titleMatch.Groups[2].Value;
            }

            article.Clauses.Clear();
            Clause currentClause = null;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Бенд (2 пробела)
                if (line.StartsWith("  ") && !line.StartsWith("    "))
                {
                    var match = Regex.Match(line.Trim(), @"^(\d+)\.\s*(.*)$");
                    if (match.Success)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string clauseText = match.Groups[2].Value;
                        currentClause = new Clause { Number = number, Text = clauseText };
                        article.Clauses.Add(currentClause);
                    }
                }
                // Суббенд (4 пробела)
                else if (line.StartsWith("    "))
                {
                    var match = Regex.Match(line.Trim(), @"^(\d+)\)\s*(.*)$");
                    if (match.Success && currentClause != null)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string subText = match.Groups[2].Value;
                        currentClause.SubClauses.Add(new SubClause { Number = number, Text = subText });
                    }
                }
            }
        }
        private static void ParseClause(Clause clause, string text)
            {
                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lines.Length == 0) return;

                // Первая строка - "N. текст"
                var firstMatch = Regex.Match(lines[0].Trim(), @"^(\d+)\.\s*(.*)$");
                if (firstMatch.Success)
                {
                    clause.Number = int.Parse(firstMatch.Groups[1].Value);
                    clause.Text = firstMatch.Groups[2].Value;
                }

                clause.SubClauses.Clear();

                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Суббенд (2 пробела + "N)")
                    var match = Regex.Match(line.Trim(), @"^(\d+)\)\s*(.*)$");
                    if (match.Success)
                    {
                        int number = int.Parse(match.Groups[1].Value);
                        string subText = match.Groups[2].Value;
                        clause.SubClauses.Add(new SubClause { Number = number, Text = subText });
                    }
                }
            }
        }
    }

