using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using Table = LawEditor.Models.SpecialElements.Table;
using Image = LawEditor.Models.SpecialElements.Image;

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

            return $"{sub.Number}) {sub.Text} \n {endnotePart}";
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
        public static void SetText(object item, string value, Laws laws)
        {
            var lines = value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            bool isArchitectureCorrect = item switch
            {
                Chapter => VerifyChapter(lines, out int el1, out string em1) || ShowError(em1),
                Models.ChangableData.Section => VerifySection(lines, out int el2, out string em2) || ShowError(em2),
                Article => VerifyArticle(lines, out int el3, out string em3) || ShowError(em3),
                Clause => VerifyClause(lines, out int el4, out string em4) || ShowError(em4),
                SubClause => VerifySubClause(lines, out int el5, out string em5) || ShowError(em5),
                _ => true
            };

            if (isArchitectureCorrect)
            {
                switch (item)
                {
                    case UpperObject u: ParseUpperObyect(u, value); break;
                    case Chapter c: ParseChapter(c, value, laws); break;
                    case Models.ChangableData.Section s: ParseSection(s, value, laws); break;
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
          
        }

        private static bool ShowError(string message)
        {
            MessageBox.Show(message, "Arxitektura xətası", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        // ==================== Architecture Correctness Verifier ====================

        private static bool VerifyChapter(string[] lines, out int errorLine, out string errorMessage)
        {
            errorLine = -1; errorMessage = "";
            bool insideSection = false, insideArticle = false, insideClause = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (Regex.IsMatch(line, @"^\{[0-9]+\}\s+.*$"))
                {
                    insideSection = true;
                    insideArticle = false;
                    insideClause = false;
                }
                else if (Regex.IsMatch(line, @"^\[[0-9.]+\]\s+.*$"))
                {
                    if (!insideSection)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: maddə fəsil olmadan tapıldı — '{line}'";
                        return false;
                    }
                    insideArticle = true;
                    insideClause = false;
                }
                else if (Regex.IsMatch(line, @"^\d+\.\s+.*$"))
                {
                    if (!insideArticle)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: bənd maddə olmadan tapıldı — '{line}'";
                        return false;
                    }
                    insideClause = true;
                }
                else if (Regex.IsMatch(line, @"^\d+\)\s+.*$"))
                {
                    if (!insideClause)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: alt bənd bənd olmadan tapıldı — '{line}'";
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool VerifySection(string[] lines, out int errorLine, out string errorMessage)
        {
            errorLine = -1; errorMessage = "";
            bool insideArticle = false, insideClause = false;
            int countSection = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (Regex.IsMatch(line, @"^\{[0-9]+\}\s+.*$"))
                {
                    countSection++;
                }
                if (countSection >= 2)
                {
                    errorLine = i + 1;
                    errorMessage = $"Sətir {i + 1}: Be element fəsil daxilində ola bilməz — '{line}'";
                    return false;
                }

                else if (Regex.IsMatch(line, @"^\[[0-9.]+\]\s+.*$"))
                {
                    insideArticle = true;
                    insideClause = false;
                }
                else if (Regex.IsMatch(line, @"^\d+\.\s+.*$"))
                {
                    if (!insideArticle)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: bənd maddə olmadan tapıldı — '{line}'";
                        return false;
                    }
                    insideClause = true;
                }
                else if (Regex.IsMatch(line, @"^\d+\)\s+.*$"))
                {
                    if (!insideClause)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: alt bənd bənd olmadan tapıldı — '{line}'";
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool VerifyArticle(string[] lines, out int errorLine, out string errorMessage)
        {
            errorLine = -1; errorMessage = "";
            bool insideClause = false;
            int coutArticle = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (Regex.IsMatch(line, @"^\[[0-9.]+\]\s+.*$"))
                {
                    coutArticle++;
                }
                    if (coutArticle >= 2)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: Bu element maddə  daxilində ola bilməz — '{line}'";
                        return false;
                    } 
                    else 
                if (Regex.IsMatch(line, @"^\{[0-9]+\}\s+.*$") )
                {
                    errorLine = i + 1;
                    errorMessage = $"Sətir {i + 1}: fəsil maddə  daxilində ola bilməz — '{line}'";
                    return false;
                }
                else if (Regex.IsMatch(line, @"^\d+\.\s+.*$"))
                {
                    insideClause = true;
                }
                else if (Regex.IsMatch(line, @"^\d+\)\s+.*$"))
                {
                    if (!insideClause)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: alt bənd bənd olmadan tapıldı — '{line}'";
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool VerifyClause(string[] lines, out int errorLine, out string errorMessage)
        {
            errorLine = -1; errorMessage = "";
            int coutClause = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;


                if(Regex.IsMatch(line, @"^\d+\.\s+.*$") ){ 
                    coutClause++;
                }
                 else if (Regex.IsMatch(line, @"^\d+\)\s+.*$"))
                {
                    if (coutClause == 0)
                    {
                        errorLine = i + 1;
                        errorMessage = $"Sətir {i + 1}: alt bənd bənd olmadan tapıldı — '{line}'";
                        return false;
                    }
                }
                 

                if (Regex.IsMatch(line, @"^\{[0-9]+\}\s+.*$") ||
                    Regex.IsMatch(line, @"^\[[0-9.]+\]\s+.*$") ||
                    coutClause >= 2
                    )
                {
                    errorLine = i + 1;
                    errorMessage = $"Sətir {i + 1}: bu element bənd daxilində ola bilməz — '{line}'";
                    return false;
                }


             }
            return true;
        }

        private static bool VerifySubClause(string[] lines, out int errorLine, out string errorMessage)
        {
            errorLine = -1; errorMessage = "";
            int coutSubClause = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (Regex.IsMatch(line, @"^\d+\)\s+.*$"))
                {
                    coutSubClause++;
                }
                
                    if (Regex.IsMatch(line, @"^\{[0-9]+\}\s+.*$") ||
                        Regex.IsMatch(line, @"^\[[0-9.]+\]\s+.*$")||
                        Regex.IsMatch(line, @"^\d+\.\s+.*$") ||
                        coutSubClause >= 2
                        )
                    {
                    errorLine = i + 1;
                    errorMessage = $"Sətir {i + 1}:  bu element altbənd daxilində ola bilməz — '{line}'";
                    return false;
                }
                else
                    {
                        continue;
                    }
            }
            return true;
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

        private static void ParseChapter(Chapter chapter, string text, Laws laws)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            chapter.Title = lines[0].Trim();

            List<int> IdesofSections = chapter.Sections.Select(s => s.Id).ToList();

            // Сохраняем старые Article IDs для каждой Section
            var oldArticleIds = chapter.Sections
                .ToDictionary(s => s.Id, s => s.Articles.Select(a => a.Id).ToList());

            // Сохраняем старые Clause IDs для каждой Article
            var oldClauseIds = chapter.Sections
                .ToDictionary(
                    s => s.Id,
                    s => s.Articles.ToDictionary(a => a.Id, a => a.Clauses.Select(c => c.Number).ToList())
                );

            // Сохраняем старые SubClause IDs для каждой Clause
            var oldSubClauseIds = chapter.Sections
                .ToDictionary(
                    s => s.Id,
                    s => s.Articles.ToDictionary(
                        a => a.Id,
                        a => a.Clauses.ToDictionary(
                            c => c.Number,
                            c => c.SubClauses.Select(sub => sub.Number).ToList())
                    )
                );

            chapter.Sections.Clear();

            // Section переменные
            int IdcounterofSections = 0;
            var SectionsResult = MessageBoxResult.No;
            bool sectionsAsked = false;
            int IdofChangedSectionElement = 0;
            int PositionOfChangedSectionElement = 0;
            Models.ChangableData.Section currentSection = null;

            // Article переменные
            List<decimal> idsOfArticles = new List<decimal>();
            List<decimal> idsOfArticlesSnapshot = new List<decimal>();
            int idCounterOfArticles = 0;
            var articlesFinalResult = MessageBoxResult.No;
            Models.ChangableData.Section articlesTargetSection = null;
            bool articlesAsked = false;
            decimal idOfChangedArticleElement = 0;
            decimal oldIdOfChangedArticleElement = 0;
            int positionOfChangedArticleElement = 0;
            Article currentArticle = null;

            // Clause переменные
            List<int> idsOfClauses = new List<int>();
            List<int> idsOfClausesSnapshot = new List<int>();
            int idCounterOfClauses = 0;
            var clausesFinalResult = MessageBoxResult.No;
            Article clausesTargetArticle = null;
            bool clausesAsked = false;
            int idOfChangedClauseElement = 0;
            int positionOfChangedClauseElement = 0;
            Clause currentClause = null;

            // SubClause переменные
            List<int> idsOfSubClauses = new List<int>();
            List<int> idsOfSubClausesSnapshot = new List<int>();
            int idCounterOfSubClauses = 0;
            var subClausesFinalResult = MessageBoxResult.No;
            Clause subClausesTargetClause = null;
            bool subClausesAsked = false;
            int idOfChangedSubClauseElement = 0;
            int positionOfChangedSubClauseElement = 0;
            SubClause currentSubClause = null;

            // ── Применяем SubClause изменения ──
            void ApplySubClauseChanges()
            {
                if (subClausesFinalResult != MessageBoxResult.Yes || subClausesTargetClause == null)
                    return;

                var currentSubList = subClausesTargetClause.SubClauses.ToList();

                if (currentSubList.Count >= idsOfSubClausesSnapshot.Count)
                    subClausesTargetClause.UpdateSubClause(idOfChangedSubClauseElement);
                else
                {
                    subClausesTargetClause.AddPhantomSubClause("", idsOfSubClausesSnapshot[positionOfChangedSubClauseElement]);
                    subClausesTargetClause.DeleteSubClause(
                        idsOfSubClausesSnapshot[positionOfChangedSubClauseElement]);
                }

                subClausesFinalResult = MessageBoxResult.No;
                subClausesTargetClause = null;
                CanRefresh = true;
            }

            // ── Применяем Clause изменения ──
            void ApplyClauseChanges()
            {
                ApplySubClauseChanges();

                if (clausesFinalResult != MessageBoxResult.Yes || clausesTargetArticle == null)
                    return;

                var currentClauseList = clausesTargetArticle.Clauses.ToList();

                if (currentClauseList.Count >= idsOfClausesSnapshot.Count)
                    clausesTargetArticle.UpdateClause(idOfChangedClauseElement);
                else
                {
                    clausesTargetArticle.AddPhantomClause("", idsOfClausesSnapshot[positionOfChangedClauseElement]);
                    clausesTargetArticle.DeleteClause(
                        idsOfClausesSnapshot[positionOfChangedClauseElement]);
                }

                clausesFinalResult = MessageBoxResult.No;
                clausesTargetArticle = null;
                CanRefresh = true;
            }

            // ── Применяем Article изменения ──
            void ApplyArticleChanges()
            {
                ApplyClauseChanges();

                if (articlesFinalResult != MessageBoxResult.Yes || articlesTargetSection == null)
                    return;

                // ── Проверяем следующий после изменённого element ──
                int nextPos = positionOfChangedArticleElement + 1;

                bool nextIsFractional =
                    nextPos < articlesTargetSection.Articles.Count &&
                    articlesTargetSection.Articles[nextPos].Id !=
                    Math.Floor(articlesTargetSection.Articles[nextPos].Id);

                if (!nextIsFractional)
                    articlesFinalResult = MessageBoxResult.No;

                if (articlesFinalResult != MessageBoxResult.Yes)
                    return;

                var currentArticleList = articlesTargetSection.Articles.ToList();

                if (currentArticleList.Count >= idsOfArticlesSnapshot.Count)
                {
                    articlesTargetSection.UpdateArticle(
                        oldIdOfChangedArticleElement,
                        idOfChangedArticleElement,
                        laws);
                }
                else
                {
                    articlesTargetSection.AddArticle(
                        idsOfArticlesSnapshot[positionOfChangedArticleElement],
                        "",
                        laws);

                    articlesTargetSection.DeleteArticle(
                        idsOfArticlesSnapshot[positionOfChangedArticleElement],
                        laws);
                }

                articlesFinalResult = MessageBoxResult.No;
                articlesTargetSection = null;
                CanRefresh = true;
            }

            // ── Сброс SubClause счётчиков ──
            void ResetSubClauseCounters(int secId, decimal artId, int clauseNumber)
            {
                if (oldSubClauseIds.TryGetValue(secId, out var artDict) &&
                    artDict.TryGetValue(artId, out var subDict) &&
                    subDict.TryGetValue(clauseNumber, out var oldSubs))
                    idsOfSubClauses = oldSubs;
                else
                    idsOfSubClauses = new List<int>();

                idCounterOfSubClauses = 0;
                subClausesAsked = false;
                idOfChangedSubClauseElement = 0;
                positionOfChangedSubClauseElement = 0;
                currentSubClause = null;
            }

            // ── Сброс Clause счётчиков ──
            void ResetClauseCounters(int secId, decimal artId)
            {
                if (oldClauseIds.TryGetValue(secId, out var artDict) &&
                    artDict.TryGetValue(artId, out var oldCls))
                    idsOfClauses = oldCls;
                else
                    idsOfClauses = new List<int>();

                idCounterOfClauses = 0;
                clausesAsked = false;
                idOfChangedClauseElement = 0;
                positionOfChangedClauseElement = 0;
                currentClause = null;
                currentSubClause = null;
            }

            // ── Сброс Article счётчиков ──
            void ResetArticleCounters(int secId)
            {
                idsOfArticles = oldArticleIds.TryGetValue(secId, out var oldArts)
                    ? oldArts
                    : new List<decimal>();

                idCounterOfArticles = 0;
                articlesAsked = false;
                idOfChangedArticleElement = 0;
                oldIdOfChangedArticleElement = 0;
                positionOfChangedArticleElement = 0;
                currentArticle = null;
                currentClause = null;
                currentSubClause = null;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var line = rawLine.Trim();

                // ── Section: "{1} текст" ──
                var sectionMatch = Regex.Match(line, @"^\{([0-9]+)\}\s+(.*)$");
                if (sectionMatch.Success)
                {
                    ApplyArticleChanges();

                    int secId = int.Parse(sectionMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    if (SectionsResult == MessageBoxResult.No &&
                               !sectionsAsked &&
                               (IdcounterofSections >= IdesofSections.Count ||
                               IdesofSections[IdcounterofSections] != secId))
                    {
                        sectionsAsked = true;
                        SectionsResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (SectionsResult == MessageBoxResult.Yes)
                        {
                            IdofChangedSectionElement = secId;
                            PositionOfChangedSectionElement = IdcounterofSections;
                        }
                    }

                    // Используем AddSection
                    currentSection = chapter.AddSection(sectionMatch.Groups[2].Value);
                    currentSection.Id = secId;
                    IdcounterofSections++;

                    ResetArticleCounters(secId);
                    continue;
                }

                // ── Section без номера: "{} текст" ──
                if (Regex.IsMatch(line, @"^\{\}\s+(.*)$") && IdcounterofSections < IdesofSections.Count)
                {
                    ApplyArticleChanges();

                    var rest = Regex.Match(line, @"^\{\}\s+(.*)$").Groups[1].Value;
                    int restoredSecId = IdesofSections[IdcounterofSections];

                    currentSection = chapter.AddSection(rest);
                    currentSection.Id = restoredSecId;
                    sectionsAsked = true;
                    IdcounterofSections++;

                    ResetArticleCounters(restoredSecId);
                    continue;
                }

                // ── Article: "[1] текст" или "[10.2] текст" ──
                var articleMatch = Regex.Match(line, @"^\[([0-9.]+)\]\s+(.*)$");
                if (articleMatch.Success)
                {
                    ApplyClauseChanges();

                    if (currentSection == null)
                    {
                        currentSection = chapter.AddSection("Auto Section");
                        ResetArticleCounters(currentSection.Id);
                    }

                    decimal.TryParse(articleMatch.Groups[1].Value, NumberStyles.Float,
                        CultureInfo.InvariantCulture, out decimal artId);

                    if (articlesFinalResult == MessageBoxResult.No && !articlesAsked)
                    {
                        if (articlesFinalResult == MessageBoxResult.No && !articlesAsked)
                        {
                            decimal prevArtId = idCounterOfArticles > 0
                                ? idsOfArticles[idCounterOfArticles - 1]
                                : -1;

                            // ── Проверка корректности дробного ID ──
                            if (artId != Math.Floor(artId))
                            {
                                decimal expectedBase = Math.Floor(artId);
                                decimal expectedPrev = Math.Round(artId - 0.1m, 1);

                                bool isFirstFractional =
                                    prevArtId == expectedBase &&
                                    Math.Round(artId - expectedBase, 1) == 0.1m;

                                bool isContinuation =
                                    Math.Round(prevArtId, 1) == expectedPrev;

                                if (!isFirstFractional && !isContinuation)
                                {
                                    MessageBox.Show(
                                        "Bu rəqəmi yazmaq uyğun deyil.",
                                        "Xəbərdarlıq",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                                    artId = Math.Round(prevArtId + 0.1m, 1);
                                    CanRefresh = true;

                                    goto skipArticleIdCheck;
                                }
                            }

                            if (idCounterOfArticles < idsOfArticles.Count)
                            {
                                decimal oldArtId = idsOfArticles[idCounterOfArticles];

                                if (oldArtId != artId)
                                {
                                    articlesAsked = true;

                                    var articlesResult = MessageBox.Show(
                                        "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                        "Təsdiqləmə",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

                                    if (articlesResult == MessageBoxResult.Yes)
                                    {
                                        oldIdOfChangedArticleElement = oldArtId;
                                        idOfChangedArticleElement = artId;
                                        positionOfChangedArticleElement = idCounterOfArticles;
                                        articlesFinalResult = MessageBoxResult.Yes;
                                        articlesTargetSection = currentSection;
                                        idsOfArticlesSnapshot = new List<decimal>(idsOfArticles);
                                    }
                                }
                            }
                            else
                            {
                                articlesAsked = true;

                                var articlesResult = MessageBox.Show(
                                    "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                    "Təsdiqləmə",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (articlesResult == MessageBoxResult.Yes)
                                {
                                    oldIdOfChangedArticleElement = idsOfArticles.Count > 0
                                        ? idsOfArticles[idsOfArticles.Count - 1]
                                        : 0;

                                    idOfChangedArticleElement = artId;
                                    positionOfChangedArticleElement = idCounterOfArticles;
                                    articlesFinalResult = MessageBoxResult.Yes;
                                    articlesTargetSection = currentSection;
                                    idsOfArticlesSnapshot = new List<decimal>(idsOfArticles);
                                }
                            }
                        }

                    skipArticleIdCheck:;
                    }

                    currentArticle = new Article { Id = artId, Title = articleMatch.Groups[2].Value };
                    currentSection.Articles.Add(currentArticle);
                    idCounterOfArticles++;

                    ResetClauseCounters(currentSection.Id, artId);
                    continue;
                }

                // ── Article без номера: "[] текст" ──
                if (Regex.IsMatch(line, @"^\[\]\s+(.*)$") &&
                    currentSection != null &&
                    idCounterOfArticles < idsOfArticles.Count)
                {
                    ApplyClauseChanges();

                    var rest = Regex.Match(line, @"^\[\]\s+(.*)$").Groups[1].Value;
                    decimal restoredArtId = idsOfArticles[idCounterOfArticles];

                    currentArticle = new Article { Id = restoredArtId, Title = rest };
                    currentSection.Articles.Add(currentArticle);
                    articlesAsked = true;
                    idCounterOfArticles++;

                    ResetClauseCounters(currentSection.Id, restoredArtId);
                    continue;
                }

                // ── Clause: "1. текст" ──
                var clauseMatch = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
                if (clauseMatch.Success && currentArticle != null)
                {
                    ApplySubClauseChanges();

                    int newClauseNumber = int.Parse(clauseMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    if (clausesFinalResult == MessageBoxResult.No &&
                          !clausesAsked &&
                          (idCounterOfClauses >= idsOfClauses.Count ||
                           idsOfClauses[idCounterOfClauses] != newClauseNumber))
                    {
                        clausesAsked = true;
                        var clausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (clausesResult == MessageBoxResult.Yes)
                        {
                            idOfChangedClauseElement = newClauseNumber;
                            positionOfChangedClauseElement = idCounterOfClauses;
                            clausesFinalResult = MessageBoxResult.Yes;
                            clausesTargetArticle = currentArticle;
                            idsOfClausesSnapshot = new List<int>(idsOfClauses);
                        }
                    }

                    currentClause = currentArticle.AddClause(clauseMatch.Groups[2].Value);
                    currentClause.Number = newClauseNumber;
                    currentSubClause = null;
                    idCounterOfClauses++;

                    ResetSubClauseCounters(currentSection.Id, currentArticle.Id, newClauseNumber);
                    continue;
                }

                // ── Clause без номера: ". текст" ──
                if (Regex.IsMatch(line, @"^\.\s+(.*)$") && currentArticle != null)
                {
                    ApplySubClauseChanges();

                    if (idCounterOfClauses < idsOfClauses.Count)
                    {
                        var rest = Regex.Match(line, @"^\.\s+(.*)$").Groups[1].Value;
                        int restoredClauseNum = idsOfClauses[idCounterOfClauses];

                        currentClause = currentArticle.AddClause(rest);
                        currentClause.Number = restoredClauseNum;
                        clausesAsked = true;
                        currentSubClause = null;
                        idCounterOfClauses++;

                        ResetSubClauseCounters(currentSection.Id, currentArticle.Id, restoredClauseNum);
                    }
                    continue;
                }

                // ── SubClause: "1) текст" ──
                var subMatch = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                if (subMatch.Success && currentClause != null)
                {
                    int newSubNumber = int.Parse(subMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    if (subClausesFinalResult == MessageBoxResult.No &&
                          !subClausesAsked &&
                          (idCounterOfSubClauses >= idsOfSubClauses.Count ||
                          idsOfSubClauses[idCounterOfSubClauses] != newSubNumber))
                    {
                        subClausesAsked = true;
                        var subClausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (subClausesResult == MessageBoxResult.Yes)
                        {
                            idOfChangedSubClauseElement = newSubNumber;
                            positionOfChangedSubClauseElement = idCounterOfSubClauses;
                            subClausesFinalResult = MessageBoxResult.Yes;
                            subClausesTargetClause = currentClause;
                            idsOfSubClausesSnapshot = new List<int>(idsOfSubClauses);
                        }
                    }

                    currentSubClause = currentClause.AddSubClause(subMatch.Groups[2].Value);
                    currentSubClause.Number = newSubNumber;
                    idCounterOfSubClauses++;
                    continue;
                }

                // ── SubClause без номера: ") текст" ──
                if (Regex.IsMatch(line, @"^\)\s+(.*)$") && currentSubClause != null)
                {
                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                    currentSubClause.Text += "\n" + rest;
                    continue;
                }

                if (Regex.IsMatch(line, @"^\)\s+(.*)$") && currentSubClause == null && currentClause != null)
                {
                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                    currentClause.Text += "\n" + rest;
                    continue;
                }

                // ── Continuation ──
                if (currentSubClause != null)
                    currentSubClause.Text += "\n" + line;
                else if (currentClause != null)
                    currentClause.Text += "\n" + line;
                else if (currentArticle != null)
                    currentArticle.Title += "\n" + line;
                else if (currentSection != null)
                    currentSection.Title += "\n" + line;
            }

            // Применяем Article изменения для последней Section (включая Clause и SubClause)
            ApplyArticleChanges();

            // Обработка изменения Section ID
            if (SectionsResult == MessageBoxResult.Yes)
            {
                var currentList = chapter.Sections.ToList();

                if (currentList.Count >= IdesofSections.Count)
                {
                    chapter.UpdateSection(IdofChangedSectionElement);
                }
                else
                {
                    chapter.AddPhantomSection("", IdesofSections[PositionOfChangedSectionElement]);
                    chapter.DeleteSection(IdesofSections[PositionOfChangedSectionElement]);
                }

                CanRefresh = true;
            }
        }

        private static void ParseSection(Models.ChangableData.Section section, string text, Laws laws)
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
                        MessageBoxImage.Information);
                    CanRefresh = true;
                }
                section.Title = titleMatch.Groups[2].Value;
            }
            else
            {
                section.Title = lines[0].Trim();
            }

            List<decimal> IdesofArticles = section.Articles.Select(a => a.Id).ToList();

            var oldClauseIds = section.Articles
                .ToDictionary(a => a.Id, a => a.Clauses.Select(c => c.Number).ToList());

            var oldSubClauseIds = section.Articles
                .ToDictionary(
                    a => a.Id,
                    a => a.Clauses.ToDictionary(
                        c => c.Number,
                        c => c.SubClauses.Select(s => s.Number).ToList())
                );

            // ── Special Elements of Articles ──
            List<Article> ArticlesWithEndnoteId = section.Articles
                .Where(a => !string.IsNullOrEmpty(a.EndnoteId))
                .ToList();
            List<Article> ArticlesWithImage = section.Articles
                .Where(a => a.Image != null)
                .ToList();
            List<Article> ArticlesWithTable = section.Articles
                .Where(a => a.Table != null)
                .ToList();

            // ── Special Elements of Clauses ──
            List<(decimal IdofArticle, Clause clause)> ClausesWithEndnoteId = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (IdofArticle: a.Id, clause: c))
                .Where(x => !string.IsNullOrEmpty(x.clause.EndnoteId))
                .ToList();
            List<(decimal IdofArticle, Clause clause)> ClausesWithImage = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (IdofArticle: a.Id, clause: c))
                .Where(x => x.clause.Image != null)
                .ToList();
            List<(decimal IdofArticle, Clause clause)> ClausesWithTable = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (IdofArticle: a.Id, clause: c))
                .Where(x => x.clause.Table != null)
                .ToList();
            List<(decimal IdofArticle, Clause clause)> ClausesWithLink = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (IdofArticle: a.Id, clause: c))
                .Where(x => !string.IsNullOrEmpty(x.clause.LinkText))
                .ToList();

            // ── Special Elements of SubClauses ──
            List<(decimal IdofArticle, int IdofClause, SubClause sub)> SubClausesWithEndnoteId = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (a, c))
                .SelectMany(x => x.c.SubClauses, (x, s) => (IdofArticle: x.a.Id, IdofClause: x.c.Number, sub: s))
                .Where(x => !string.IsNullOrEmpty(x.sub.EndnoteId))
                .ToList();
            List<(decimal IdofArticle, int IdofClause, SubClause sub)> SubClausesWithImage = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (a, c))
                .SelectMany(x => x.c.SubClauses, (x, s) => (IdofArticle: x.a.Id, IdofClause: x.c.Number, sub: s))
                .Where(x => x.sub.Image != null)
                .ToList();
            List<(decimal IdofArticle, int IdofClause, SubClause sub)> SubClausesWithTable = section.Articles
                .SelectMany(a => a.Clauses, (a, c) => (a, c))
                .SelectMany(x => x.c.SubClauses, (x, s) => (IdofArticle: x.a.Id, IdofClause: x.c.Number, sub: s))
                .Where(x => x.sub.Table != null)
                .ToList();

            section.Articles.Clear();

            // Article переменные
            int IdcounterofArticles = 0;
            var ArticlesResult = MessageBoxResult.No;
            bool articlesAsked = false;
            decimal IdofChangedArticleElement = 0;
            decimal OldIdofChangedArticleElement = 0;
            int PositionOfChangedArticleElement = 0;
            Article currentArticle = null;

            string ArticleEndnoteToRestore = null;
            Image ArticleImageToRestore = null;
            Table ArticleTableToRestore = null;

            // Clause переменные
            List<int> idsOfClauses = new List<int>();
            List<int> idsOfClausesSnapshot = new List<int>();
            int idCounterOfClauses = 0;
            var clausesFinalResult = MessageBoxResult.No;
            Article clausesTargetArticle = null;
            bool clausesAsked = false;
            int idOfChangedClauseElement = 0;
            int positionOfChangedClauseElement = 0;
            Clause currentClause = null;

            string ClauseEndnoteToRestore = null;
            Image ClauseImageToRestore = null;
            Table ClauseTableToRestore = null;
            string ClauseLinkTextToRestore = null;
            string ClauseUrlToRestore = null;

            // SubClause переменные
            List<int> idsOfSubClauses = new List<int>();
            List<int> idsOfSubClausesSnapshot = new List<int>();
            int idCounterOfSubClauses = 0;
            var subClausesFinalResult = MessageBoxResult.No;
            Clause subClausesTargetClause = null;
            bool subClausesAsked = false;
            int idOfChangedSubClauseElement = 0;
            int positionOfChangedSubClauseElement = 0;
            SubClause currentSubClause = null;

            string SubClauseEndnoteToRestore = null;
            Image SubClauseImageToRestore = null;
            Table SubClauseTableToRestore = null;

            void ApplySubClauseChanges()
            {
                if (subClausesFinalResult != MessageBoxResult.Yes || subClausesTargetClause == null)
                    return;

                var currentSubList = subClausesTargetClause.SubClauses.ToList();

                if (currentSubList.Count >= idsOfSubClausesSnapshot.Count)
                    subClausesTargetClause.UpdateSubClause(idOfChangedSubClauseElement);
                else
                {
                    subClausesTargetClause.AddPhantomSubClause("", idsOfSubClausesSnapshot[positionOfChangedSubClauseElement]);
                    subClausesTargetClause.DeleteSubClause(idsOfSubClausesSnapshot[positionOfChangedSubClauseElement]);
                }

                subClausesFinalResult = MessageBoxResult.No;
                subClausesTargetClause = null;
                CanRefresh = true;
            }

            void ApplyClauseChanges()
            {
                ApplySubClauseChanges();

                if (clausesFinalResult != MessageBoxResult.Yes || clausesTargetArticle == null)
                    return;

                var currentClauseList = clausesTargetArticle.Clauses.ToList();

                if (currentClauseList.Count >= idsOfClausesSnapshot.Count)
                    clausesTargetArticle.UpdateClause(idOfChangedClauseElement);
                else
                {
                    clausesTargetArticle.AddPhantomClause("", idsOfClausesSnapshot[positionOfChangedClauseElement]);
                    clausesTargetArticle.DeleteClause(idsOfClausesSnapshot[positionOfChangedClauseElement]);
                }

                clausesFinalResult = MessageBoxResult.No;
                clausesTargetArticle = null;
                CanRefresh = true;
            }

            void ResetSubClauseCounters(decimal artId, int clauseNumber)
            {
                if (oldSubClauseIds.TryGetValue(artId, out var subDict) &&
                    subDict.TryGetValue(clauseNumber, out var oldSubs))
                    idsOfSubClauses = oldSubs;
                else
                    idsOfSubClauses = new List<int>();

                idCounterOfSubClauses = 0;
                subClausesAsked = false;
                idOfChangedSubClauseElement = 0;
                positionOfChangedSubClauseElement = 0;
                currentSubClause = null;

                SubClauseEndnoteToRestore = null;
                SubClauseImageToRestore = null;
                SubClauseTableToRestore = null;
            }

            void ResetClauseCounters(decimal artId)
            {
                idsOfClauses = oldClauseIds.TryGetValue(artId, out var oldCls)
                    ? oldCls
                    : new List<int>();

                idCounterOfClauses = 0;
                clausesAsked = false;
                idOfChangedClauseElement = 0;
                positionOfChangedClauseElement = 0;
                currentClause = null;
                currentSubClause = null;

                ClauseEndnoteToRestore = null;
                ClauseImageToRestore = null;
                ClauseTableToRestore = null;
                ClauseLinkTextToRestore = null;
                ClauseUrlToRestore = null;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var line = rawLine.Trim();

                // ── Article: "[1] текст" или "[10.2] текст" ──
                var articleMatch = Regex.Match(line, @"^\[([0-9.]+)\]\s+(.*)$");
                if (articleMatch.Success)
                {
                    ApplyClauseChanges();

                    decimal.TryParse(articleMatch.Groups[1].Value, NumberStyles.Float,
                        CultureInfo.InvariantCulture, out decimal artId);

                    if (ArticlesResult == MessageBoxResult.No && !articlesAsked)
                    {
                        decimal prevArtId = IdcounterofArticles > 0
                            ? IdesofArticles[IdcounterofArticles - 1]
                            : -1;

                        if (artId != Math.Floor(artId))
                        {
                            decimal expectedBase = Math.Floor(artId);
                            decimal expectedPrev = Math.Round(artId - 0.1m, 1);

                            bool isFirstFractional = prevArtId == expectedBase &&
                                                     Math.Round(artId - expectedBase, 1) == 0.1m;
                            bool isContinuation = Math.Round(prevArtId, 1) == expectedPrev;

                            if (!isFirstFractional && !isContinuation)
                            {
                                MessageBox.Show(
                                    "Bu rəqəmi yazmaq uyğun deyil.",
                                    "Xəbərdarlıq",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                                artId = Math.Round(prevArtId + 0.1m, 1);
                                CanRefresh = true;
                                goto skipArticleIdCheck;
                            }
                        }

                        if (IdcounterofArticles < IdesofArticles.Count)
                        {
                            decimal oldArtId = IdesofArticles[IdcounterofArticles];
                            if (oldArtId != artId)
                            {
                                articlesAsked = true;
                                ArticlesResult = MessageBox.Show(
                                    "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                    "Təsdiqləmə",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

                                if (ArticlesResult == MessageBoxResult.Yes)
                                {
                                    OldIdofChangedArticleElement = oldArtId;
                                    IdofChangedArticleElement = artId;
                                    PositionOfChangedArticleElement = IdcounterofArticles;
                                }
                            }
                        }
                        else
                        {
                            articlesAsked = true;
                            ArticlesResult = MessageBox.Show(
                                "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                                "Təsdiqləmə",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (ArticlesResult == MessageBoxResult.Yes)
                            {
                                OldIdofChangedArticleElement = IdesofArticles.Count > 0
                                    ? IdesofArticles[IdesofArticles.Count - 1]
                                    : 0;
                                IdofChangedArticleElement = artId;
                                PositionOfChangedArticleElement = IdcounterofArticles;
                            }
                        }
                    }
                skipArticleIdCheck:;

                    // ── Реставрация Article ──
                    foreach (var a in ArticlesWithEndnoteId)
                    {
                        if (a.Id == artId || a.Title == articleMatch.Groups[2].Value)
                        {
                            ArticleEndnoteToRestore = a.EndnoteId;
                            break;
                        }
                    }
                    foreach (var a in ArticlesWithImage)
                    {
                        if (a.Id == artId || a.Title == articleMatch.Groups[2].Value)
                        {
                            ArticleImageToRestore = a.Image;
                            break;
                        }
                    }
                    foreach (var a in ArticlesWithTable)
                    {
                        if (a.Id == artId || a.Title == articleMatch.Groups[2].Value)
                        {
                            ArticleTableToRestore = a.Table;
                            break;
                        }
                    }

                    currentArticle = new Article
                    {
                        Id = artId,
                        Title = articleMatch.Groups[2].Value,
                        EndnoteId = ArticleEndnoteToRestore,
                        Image = ArticleImageToRestore,
                        Table = ArticleTableToRestore
                    };
                    section.Articles.Add(currentArticle);

                    ArticleEndnoteToRestore = null;
                    ArticleImageToRestore = null;
                    ArticleTableToRestore = null;

                    IdcounterofArticles++;

                    ResetClauseCounters(artId);
                    continue;
                }

                // ── Article без номера: "[] текст" ──
                if (Regex.IsMatch(line, @"^\[\]\s+(.*)$") && IdcounterofArticles < IdesofArticles.Count)
                {
                    ApplyClauseChanges();

                    var rest = Regex.Match(line, @"^\[\]\s+(.*)$").Groups[1].Value;
                    decimal restoredId = IdesofArticles[IdcounterofArticles];

                    // ── Реставрация Article (unnumbered) ──
                    foreach (var a in ArticlesWithEndnoteId)
                    {
                        if (a.Id == restoredId || a.Title == rest)
                        {
                            ArticleEndnoteToRestore = a.EndnoteId;
                            break;
                        }
                    }
                    foreach (var a in ArticlesWithImage)
                    {
                        if (a.Id == restoredId || a.Title == rest)
                        {
                            ArticleImageToRestore = a.Image;
                            break;
                        }
                    }
                    foreach (var a in ArticlesWithTable)
                    {
                        if (a.Id == restoredId || a.Title == rest)
                        {
                            ArticleTableToRestore = a.Table;
                            break;
                        }
                    }

                    currentArticle = new Article
                    {
                        Id = restoredId,
                        Title = rest,
                        EndnoteId = ArticleEndnoteToRestore,
                        Image = ArticleImageToRestore,
                        Table = ArticleTableToRestore
                    };
                    section.Articles.Add(currentArticle);

                    ArticleEndnoteToRestore = null;
                    ArticleImageToRestore = null;
                    ArticleTableToRestore = null;

                    articlesAsked = true;
                    IdcounterofArticles++;

                    ResetClauseCounters(restoredId);
                    continue;
                }

                // ── Clause: "1. текст" ──
                var clauseMatch = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
                if (clauseMatch.Success && currentArticle != null)
                {
                    ApplySubClauseChanges();

                    int newClauseNumber = int.Parse(clauseMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    if (clausesFinalResult == MessageBoxResult.No &&
                        !clausesAsked &&
                        (idCounterOfClauses >= idsOfClauses.Count ||
                         idsOfClauses[idCounterOfClauses] != newClauseNumber))
                    {
                        clausesAsked = true;
                        var clausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (clausesResult == MessageBoxResult.Yes)
                        {
                            idOfChangedClauseElement = newClauseNumber;
                            positionOfChangedClauseElement = idCounterOfClauses;
                            clausesFinalResult = MessageBoxResult.Yes;
                            clausesTargetArticle = currentArticle;
                            idsOfClausesSnapshot = new List<int>(idsOfClauses);
                        }
                    }

                    // ── Реставрация Clause ──
                    foreach (var x in ClausesWithEndnoteId)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            (x.clause.Number == newClauseNumber || x.clause.Text == clauseMatch.Groups[2].Value))
                        {
                            ClauseEndnoteToRestore = x.clause.EndnoteId;
                            break;
                        }
                    }
                    foreach (var x in ClausesWithImage)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            (x.clause.Number == newClauseNumber || x.clause.Text == clauseMatch.Groups[2].Value))
                        {
                            ClauseImageToRestore = x.clause.Image;
                            break;
                        }
                    }
                    foreach (var x in ClausesWithTable)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            (x.clause.Number == newClauseNumber || x.clause.Text == clauseMatch.Groups[2].Value))
                        {
                            ClauseTableToRestore = x.clause.Table;
                            break;
                        }
                    }
                    foreach (var x in ClausesWithLink)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            (x.clause.Number == newClauseNumber || x.clause.Text == clauseMatch.Groups[2].Value))
                        {
                            ClauseLinkTextToRestore = x.clause.LinkText;
                            ClauseUrlToRestore = x.clause.Url;
                            break;
                        }
                    }

                    currentClause = currentArticle.AddClause(clauseMatch.Groups[2].Value);
                    currentClause.Number = newClauseNumber;
                    currentClause.EndnoteId = ClauseEndnoteToRestore;
                    currentClause.Image = ClauseImageToRestore;
                    currentClause.Table = ClauseTableToRestore;
                    currentClause.LinkText = ClauseLinkTextToRestore;
                    currentClause.Url = ClauseUrlToRestore;

                    ClauseEndnoteToRestore = null;
                    ClauseImageToRestore = null;
                    ClauseTableToRestore = null;
                    ClauseLinkTextToRestore = null;
                    ClauseUrlToRestore = null;

                    idCounterOfClauses++;

                    ResetSubClauseCounters(currentArticle.Id, newClauseNumber);
                    continue;
                }

                // ── Clause без номера: ". текст" ──
                if (Regex.IsMatch(line, @"^\.\s+(.*)$") && currentArticle != null)
                {
                    ApplySubClauseChanges();

                    if (idCounterOfClauses < idsOfClauses.Count)
                    {
                        var rest = Regex.Match(line, @"^\.\s+(.*)$").Groups[1].Value;
                        int restoredClauseNum = idsOfClauses[idCounterOfClauses];

                        // ── Реставрация Clause (unnumbered) ──
                        foreach (var x in ClausesWithEndnoteId)
                        {
                            if (x.IdofArticle == currentArticle.Id &&
                                (x.clause.Number == restoredClauseNum || x.clause.Text == rest))
                            {
                                ClauseEndnoteToRestore = x.clause.EndnoteId;
                                break;
                            }
                        }
                        foreach (var x in ClausesWithImage)
                        {
                            if (x.IdofArticle == currentArticle.Id &&
                                (x.clause.Number == restoredClauseNum || x.clause.Text == rest))
                            {
                                ClauseImageToRestore = x.clause.Image;
                                break;
                            }
                        }
                        foreach (var x in ClausesWithTable)
                        {
                            if (x.IdofArticle == currentArticle.Id &&
                                (x.clause.Number == restoredClauseNum || x.clause.Text == rest))
                            {
                                ClauseTableToRestore = x.clause.Table;
                                break;
                            }
                        }
                        foreach (var x in ClausesWithLink)
                        {
                            if (x.IdofArticle == currentArticle.Id &&
                                (x.clause.Number == restoredClauseNum || x.clause.Text == rest))
                            {
                                ClauseLinkTextToRestore = x.clause.LinkText;
                                ClauseUrlToRestore = x.clause.Url;
                                break;
                            }
                        }

                        currentClause = currentArticle.AddClause(rest);
                        currentClause.Number = restoredClauseNum;
                        currentClause.EndnoteId = ClauseEndnoteToRestore;
                        currentClause.Image = ClauseImageToRestore;
                        currentClause.Table = ClauseTableToRestore;
                        currentClause.LinkText = ClauseLinkTextToRestore;
                        currentClause.Url = ClauseUrlToRestore;

                        ClauseEndnoteToRestore = null;
                        ClauseImageToRestore = null;
                        ClauseTableToRestore = null;
                        ClauseLinkTextToRestore = null;
                        ClauseUrlToRestore = null;

                        clausesAsked = true;
                        idCounterOfClauses++;

                        ResetSubClauseCounters(currentArticle.Id, restoredClauseNum);
                    }
                    continue;
                }

                // ── SubClause: "1) текст" ──
                var subMatch = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                if (subMatch.Success && currentClause != null)
                {
                    int newSubNumber = int.Parse(subMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    if (subClausesFinalResult == MessageBoxResult.No &&
                        !subClausesAsked &&
                        (idCounterOfSubClauses >= idsOfSubClauses.Count ||
                         idsOfSubClauses[idCounterOfSubClauses] != newSubNumber))
                    {
                        subClausesAsked = true;
                        var subClausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (subClausesResult == MessageBoxResult.Yes)
                        {
                            idOfChangedSubClauseElement = newSubNumber;
                            positionOfChangedSubClauseElement = idCounterOfSubClauses;
                            subClausesFinalResult = MessageBoxResult.Yes;
                            subClausesTargetClause = currentClause;
                            idsOfSubClausesSnapshot = new List<int>(idsOfSubClauses);
                        }
                    }

                    // ── Реставрация SubClause ──
                    foreach (var x in SubClausesWithEndnoteId)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            x.IdofClause == currentClause.Number &&
                            (x.sub.Number == newSubNumber || x.sub.Text == subMatch.Groups[2].Value))
                        {
                            SubClauseEndnoteToRestore = x.sub.EndnoteId;
                            break;
                        }
                    }
                    foreach (var x in SubClausesWithImage)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            x.IdofClause == currentClause.Number &&
                            (x.sub.Number == newSubNumber || x.sub.Text == subMatch.Groups[2].Value))
                        {
                            SubClauseImageToRestore = x.sub.Image;
                            break;
                        }
                    }
                    foreach (var x in SubClausesWithTable)
                    {
                        if (x.IdofArticle == currentArticle.Id &&
                            x.IdofClause == currentClause.Number &&
                            (x.sub.Number == newSubNumber || x.sub.Text == subMatch.Groups[2].Value))
                        {
                            SubClauseTableToRestore = x.sub.Table;
                            break;
                        }
                    }

                    currentSubClause = currentClause.AddSubClause(subMatch.Groups[2].Value);
                    currentSubClause.Number = newSubNumber;
                    currentSubClause.EndnoteId = SubClauseEndnoteToRestore;
                    currentSubClause.Image = SubClauseImageToRestore;
                    currentSubClause.Table = SubClauseTableToRestore;

                    SubClauseEndnoteToRestore = null;
                    SubClauseImageToRestore = null;
                    SubClauseTableToRestore = null;

                    idCounterOfSubClauses++;
                    continue;
                }

                // ── SubClause без номера: ") текст" ──
                if (Regex.IsMatch(line, @"^\)\s+(.*)$") && currentSubClause != null)
                {
                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                    currentSubClause.Text += "\n" + rest;
                    continue;
                }

                if (Regex.IsMatch(line, @"^\)\s+(.*)$") && currentSubClause == null && currentClause != null)
                {
                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                    currentClause.Text += "\n" + rest;
                    continue;
                }

                // ── Continuation ──
                if (currentSubClause != null)
                    currentSubClause.Text += "\n" + line;
                else if (currentClause != null)
                    currentClause.Text += "\n" + line;
                else if (currentArticle != null)
                    currentArticle.Title += "\n" + line;
            }

            // Применяем Clause изменения для последней Article
            ApplyClauseChanges();

            if (ArticlesResult == MessageBoxResult.Yes)
            {
                int nextPos = PositionOfChangedArticleElement + 1;
                bool nextIsFractional = nextPos < section.Articles.Count &&
                                        section.Articles[nextPos].Id != Math.Floor(section.Articles[nextPos].Id);

                if (!nextIsFractional)
                    ArticlesResult = MessageBoxResult.No;
            }

            if (ArticlesResult == MessageBoxResult.Yes)
            {
                var currentList = section.Articles.ToList();

                if (currentList.Count >= IdesofArticles.Count)
                {
                    section.UpdateArticle(
                        OldIdofChangedArticleElement,
                        IdofChangedArticleElement,
                        laws);
                }
                else
                {
                    section.AddPhantomArticle(
                        IdesofArticles[PositionOfChangedArticleElement],
                        "",
                        laws);
                    section.DeleteArticle(
                        IdesofArticles[PositionOfChangedArticleElement],
                        laws);
                }

                CanRefresh = true;
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
                    CanRefresh = true;
                }
                article.Title = titleMatch.Groups[2].Value;
            }
            else
            {
                article.Title = lines[0].Trim();
            }

            article.EndnoteId = null;

            List<int> IdesofClauses = article.Clauses.Select(c => c.Number).ToList();
            var oldSubClauseIds = article.Clauses
                .ToDictionary(c => c.Number, c => c.SubClauses.Select(s => s.Number).ToList());







            //Speshial Elements of SubClauses
            List<(int IdofClause, SubClause sub)> SubclausesWithEndnoteId = article.Clauses
                .SelectMany(c => c.SubClauses, (c, s) => (IdofClause: c.Number, sub: s))
                .Where(x => !string.IsNullOrEmpty(x.sub.EndnoteId))
                .ToList();

            List < (int IdofClause, SubClause sub) > SubclausesWithImage = article.Clauses
                .SelectMany(c => c.SubClauses, (c, s) => (IdofClause: c.Number, sub: s))
                .Where(x => x.sub.Image != null)
                .ToList();
            List<(int IdofClause, SubClause sub)> SubclausesWithTable = article.Clauses
                .SelectMany(c => c.SubClauses, (c, s) => (IdofClause: c.Number, sub: s))
                .Where(x => x.sub.Table != null)
                .ToList();





            // Speshial Elements of Clauses
            List<Clause> ClausesWithEndnoteId = article.Clauses
                .Where(c => !string.IsNullOrEmpty(c.EndnoteId))
                .ToList();
            List<Clause> ClausesWithImage = article.Clauses
                .Where(c => c.Image != null)
                .ToList();
            List<Clause> ClausesWithTable = article.Clauses
                .Where(c => c.Table != null)
                .ToList(); 
            List<Clause> ClausesWithLink = article.Clauses
                .Where(c => !string.IsNullOrEmpty(c.LinkText))
                .ToList();

            article.Clauses.Clear();

            // Clause переменные
            int IdcounterofClauses = 0;
            var ClausesResult = MessageBoxResult.No;
            bool clausesAsked = false;
            int IdofChangedClauseElement = 0;
            int PositionOfChangedClauseElement = 0;
            Clause currentClause = null;
            Clause subClausesTargetClause = null;
           
            Table ClauseTabletoRestore = null;
            Image ClauseImagetoRestore = null;
            string ClauseEndnotetoRestore = null;
            string ClauseLinkTexttoRestore = null;
            string ClauseUrltoRestore = null;


            // SubClause переменные
            List<int> idsOfSubClauses = new List<int>();
            List<int> idsOfSubClausesSnapshot = new List<int>();
            int idCounterOfSubClauses = 0;
            var subClausesResult = MessageBoxResult.No;
            var subClausesFinalResult = MessageBoxResult.No;
            bool subClausesAsked = false;
            int idOfChangedSubClauseElement = 0;
            int positionOfChangedSubClauseElement = 0;
            SubClause currentSubClause = null;

            Table subClauseTableToRestore = null;
            Image subClauseImagetoRestore = null;
            string subClauseEndnotetoRestore = null;

            void ApplySubClauseChanges()
            {
                if (subClausesFinalResult != MessageBoxResult.Yes || subClausesTargetClause == null)
                    return;

                var currentSubList = subClausesTargetClause.SubClauses.ToList();

                if (currentSubList.Count >= idsOfSubClausesSnapshot.Count)
                {
                    subClausesTargetClause.UpdateSubClause(idOfChangedSubClauseElement);
                }
                else
                {
                    subClausesTargetClause.AddPhantomSubClause("", idsOfSubClausesSnapshot[positionOfChangedSubClauseElement]);
                    subClausesTargetClause.DeleteSubClause(idsOfSubClausesSnapshot[positionOfChangedSubClauseElement]);
                }

                subClausesFinalResult = MessageBoxResult.No;
                subClausesTargetClause = null;
                CanRefresh = true;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                if (string.IsNullOrWhiteSpace(rawLine)) continue;

                var line = rawLine.Trim();

                if (line.StartsWith("EndnoteId:"))
                {
                    article.EndnoteId = line.Replace("EndnoteId:", "").Trim();
                    continue;
                }

                // Clause: "1. текст"
                var clauseMatch = Regex.Match(line, @"^(\d+)\.\s+(.*)$");
                if (clauseMatch.Success)
                {
                    ApplySubClauseChanges();

                    int newNumber = int.Parse(clauseMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    // ── ФИКС: срабатывает и когда счётчик вышел за пределы (новый элемент) ──
                    if (ClausesResult == MessageBoxResult.No &&
                        !clausesAsked &&
                        (IdcounterofClauses >= IdesofClauses.Count ||
                         IdesofClauses[IdcounterofClauses] != newNumber))
                    {
                        clausesAsked = true;
                        ClausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (ClausesResult == MessageBoxResult.Yes)
                        {
                            IdofChangedClauseElement = newNumber;
                            PositionOfChangedClauseElement = IdcounterofClauses;
                        }
                    }


                    foreach (var clause in ClausesWithEndnoteId)
                    {
                        if (clause.Number == newNumber || clause.Text == clauseMatch.Groups[2].Value)
                        {
                            ClauseEndnotetoRestore = clause.EndnoteId;
                            break;
                        }
                    }
                    foreach (var clause in ClausesWithImage)
                    {
                        if (clause.Number == newNumber || clause.Text == clauseMatch.Groups[2].Value)
                        {
                            ClauseImagetoRestore = clause.Image;
                            break;
                        }
                    }
                    foreach (var clause in ClausesWithTable)
                    {
                        if (clause.Number == newNumber || clause.Text == clauseMatch.Groups[2].Value)
                        {
                            ClauseTabletoRestore = clause.Table;
                            break;
                        }
                    }
                    foreach (var clause in ClausesWithLink)
                    {
                        if (clause.Number == newNumber || clause.Text == clauseMatch.Groups[2].Value)
                        {
                            ClauseLinkTexttoRestore = clause.LinkText;
                            ClauseUrltoRestore = clause.Url;
                            break;
                        }
                    }

                    currentClause = new Clause
                    {
                        Number = newNumber,
                        Text = clauseMatch.Groups[2].Value,
                        EndnoteId = ClauseEndnotetoRestore,
                        Image = ClauseImagetoRestore,
                        Table = ClauseTabletoRestore,
                        LinkText = ClauseLinkTexttoRestore,
                        Url = ClauseUrltoRestore
                    };

                    article.Clauses.Add(currentClause);


                    ClauseEndnotetoRestore = null;
                    ClauseImagetoRestore = null;
                    ClauseTabletoRestore = null;
                    ClauseLinkTexttoRestore = null;
                    ClauseUrltoRestore = null; 


                    IdcounterofClauses++;

                    idsOfSubClauses = oldSubClauseIds.TryGetValue(newNumber, out var oldSubs)
                        ? oldSubs
                        : new List<int>();
                    idCounterOfSubClauses = 0;
                    subClausesResult = MessageBoxResult.No;
                    subClausesAsked = false;
                    idOfChangedSubClauseElement = 0;
                    positionOfChangedSubClauseElement = 0;
                    currentSubClause = null;

                    continue;
                }

                // Clause без номера: ". текст"
                if (Regex.IsMatch(line, @"^\.\s+(.*)$"))
                {
                    ApplySubClauseChanges();

                    if (IdcounterofClauses < IdesofClauses.Count)
                    {
                        var rest = Regex.Match(line, @"^\.\s+(.*)$").Groups[1].Value;
                        foreach (var clause in ClausesWithEndnoteId)
                        {
                            if (clause.Number == IdesofClauses[IdcounterofClauses] || clause.Text == rest)
                            {
                                ClauseEndnotetoRestore = clause.EndnoteId;
                                break;
                            }
                        }
                        foreach (var clause in ClausesWithImage)
                        {
                            if (clause.Number == IdesofClauses[IdcounterofClauses] || clause.Text == rest)
                            {
                                ClauseImagetoRestore = clause.Image;
                                break;
                            }
                        }
                        foreach (var clause in ClausesWithTable)
                        {
                            if (clause.Number == IdesofClauses[IdcounterofClauses] || clause.Text == rest)
                            {
                                ClauseTabletoRestore = clause.Table;
                                break;
                            }
                        }
                        foreach (var clause in ClausesWithLink)
                        {
                            if (clause.Number == IdesofClauses[IdcounterofClauses] || clause.Text == rest)
                            {
                                ClauseLinkTexttoRestore = clause.LinkText;
                                ClauseUrltoRestore = clause.Url;
                                break;
                            }
                        }

                        currentClause = new Clause
                        {
                            Number = IdesofClauses[IdcounterofClauses],
                            Text = rest,
                            EndnoteId = ClauseEndnotetoRestore,
                            Table = ClauseTabletoRestore,
                            Image = ClauseImagetoRestore,
                            LinkText = ClauseLinkTexttoRestore,
                            Url = ClauseUrltoRestore,

                        };
                        article.Clauses.Add(currentClause);

                        ClauseEndnotetoRestore = null;
                        ClauseImagetoRestore = null;
                        ClauseTabletoRestore = null;
                        ClauseLinkTexttoRestore = null;
                        ClauseUrltoRestore = null;

                        clausesAsked = true;
                        IdcounterofClauses++;

                        idsOfSubClauses = oldSubClauseIds.TryGetValue(currentClause.Number, out var oldSubs2)
                            ? oldSubs2
                            : new List<int>();
                        idCounterOfSubClauses = 0;
                        subClausesResult = MessageBoxResult.No;
                        subClausesAsked = false;
                        currentSubClause = null;
                    }
                    continue;
                }

                // SubClause: "1) текст"
                var subMatch = Regex.Match(line, @"^(\d+)\)\s+(.*)$");
                if (subMatch.Success && currentClause != null)
                {
                    int newSubNumber = int.Parse(subMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    // ── ФИКС: то же самое для SubClause ──
                    if (subClausesResult == MessageBoxResult.No &&
                        !subClausesAsked &&
                        (idCounterOfSubClauses >= idsOfSubClauses.Count ||
                         idsOfSubClauses[idCounterOfSubClauses] != newSubNumber))
                    {
                        subClausesAsked = true;
                        subClausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (subClausesResult == MessageBoxResult.Yes)
                        {
                            idOfChangedSubClauseElement = newSubNumber;
                            positionOfChangedSubClauseElement = idCounterOfSubClauses;
                            subClausesFinalResult = MessageBoxResult.Yes;
                            subClausesTargetClause = currentClause;
                            idsOfSubClausesSnapshot = new List<int>(idsOfSubClauses);
                        }
                    }
                    foreach (var sub in SubclausesWithEndnoteId)
                    {
                        if (sub.IdofClause == currentClause.Number &&
                            (sub.sub.Number == newSubNumber || sub.sub.Text == subMatch.Groups[2].Value))
                        {
                            subClauseEndnotetoRestore = sub.sub.EndnoteId;
                            break;
                        }
                    }
                    foreach (var sub in SubclausesWithImage)
                    {
                        if (sub.IdofClause == currentClause.Number &&
                            (sub.sub.Number == newSubNumber || sub.sub.Text == subMatch.Groups[2].Value))
                        {
                            subClauseImagetoRestore = sub.sub.Image;
                            break;
                        }
                    }
                    foreach (var sub in SubclausesWithTable)
                    {
                        if (sub.IdofClause == currentClause.Number &&
                            (sub.sub.Number == newSubNumber || sub.sub.Text == subMatch.Groups[2].Value))
                        {
                            subClauseTableToRestore = sub.sub.Table;
                            break;
                        }
                    }   

                    currentSubClause = new SubClause
                    {
                        Number = newSubNumber,
                        Text = subMatch.Groups[2].Value,
                        EndnoteId=subClauseEndnotetoRestore,
                        Image = subClauseImagetoRestore,
                        Table = subClauseTableToRestore
                    };
                    currentClause.SubClauses.Add(currentSubClause);

                    subClauseEndnotetoRestore = null;
                    subClauseImagetoRestore = null;
                    subClauseTableToRestore = null;

                    idCounterOfSubClauses++;
                    continue;
                }

                // SubClause без номера: ") текст"
                if (Regex.IsMatch(line, @"^\)\s+(.*)$") && currentSubClause != null)
                {
                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                    currentSubClause.Text += "\n" + rest;
                    continue;
                }

                if (Regex.IsMatch(line, @"^\)\s+(.*)$") && currentSubClause == null)
                {
                    var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                    if (currentClause != null)
                        currentClause.Text += "\n" + rest;
                    continue;
                }

                // Continuation
                if (currentSubClause != null)
                    currentSubClause.Text += "\n" + line;
                else if (currentClause != null)
                    currentClause.Text += "\n" + line;
            }

            // Обработка изменения Clause ID
            if (ClausesResult == MessageBoxResult.Yes)
            {
                var currentList = article.Clauses.ToList();

                if (currentList.Count >= IdesofClauses.Count)
                {
                    article.UpdateClause(IdofChangedClauseElement);
                }
                else
                {
                    article.AddPhantomClause("", IdesofClauses[PositionOfChangedClauseElement]);
                    article.DeleteClause(IdesofClauses[PositionOfChangedClauseElement]);
                }

                CanRefresh = true;
            }

            // Применяем SubClause изменения для последнего Clause
            ApplySubClauseChanges();
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
                    CanRefresh = true;

                }
                clause.Text = match.Groups[2].Value;
            }
           
            // About SubClauses: 
            List<int> IdesofSubclauses = clause.SubClauses.Select(sc => sc.Number).ToList();
            List<SubClause> SubclausesWithTables = clause.SubClauses.Where(sc => sc.Table != null).ToList();
            List<SubClause> SubclausesWithImages = clause.SubClauses.Where(sc => sc.Image != null).ToList();
            List<SubClause> SubClausesWithEndnoteIds = clause.SubClauses.Where(sc => sc.EndnoteId != null).ToList();

            int IdcounterofSubclauses = 0;
            var SubclausesResult = MessageBoxResult.No;
            bool subclausesAsked = false;
            int IdofChangedSubclauseElement = 0;
            int PositionOfChangedSubclauseElement = 0;
            Table tableToRestore = null;
            Image imageToRestore = null;
            string endnoteIdToRestore = null;
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
                    int newNumber = int.Parse(subMatch.Groups[1].Value, CultureInfo.InvariantCulture);

                    if (SubclausesResult == MessageBoxResult.No &&
                           !subclausesAsked &&
                       (IdcounterofSubclauses >= IdesofSubclauses.Count ||
                          IdesofSubclauses[IdcounterofSubclauses] != newNumber))
                    {
                        subclausesAsked = true;
                        SubclausesResult = MessageBox.Show(
                            "Dəyişdirilmiş elementdən aşağıdakı bütün rəqəmsal ID-ləri yeniləmək istəyirsiniz?",
                            "Təsdiqləmə",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (SubclausesResult == MessageBoxResult.Yes)
                        {
                            IdofChangedSubclauseElement = newNumber;
                            PositionOfChangedSubclauseElement = IdcounterofSubclauses;
                        }
                    }


                    foreach (var sub in SubclausesWithTables)
                    {
                        if (sub.Number == newNumber || sub.Text == subMatch.Groups[2].Value)
                        {
                            tableToRestore = sub.Table;
                            break;
                        }
                    }
                    foreach (var sub in SubclausesWithImages)
                    {
                        if (sub.Number == newNumber || sub.Text == subMatch.Groups[2].Value)
                        {
                            imageToRestore = sub.Image;
                            break;
                        }
                    }
                    foreach (var sub in SubClausesWithEndnoteIds)
                    {
                        if (sub.Number == newNumber || sub.Text == subMatch.Groups[2].Value)
                        {
                            endnoteIdToRestore = sub.EndnoteId;
                            break;
                        }
                    }

                        clause.SubClauses.Add(new SubClause
                    {
                        Number = newNumber,
                        Text = subMatch.Groups[2].Value,
                        Table = tableToRestore,
                        Image = imageToRestore,
                        EndnoteId = endnoteIdToRestore
                    });
                    endnoteIdToRestore = null;
                    imageToRestore = null;
                    tableToRestore = null;

                    IdcounterofSubclauses++;
                    continue;
                }

                // Строка вида ") текст" без номера — восстанавливаем старый номер
                if (Regex.IsMatch(line, @"^\)\s+(.*)$"))
                {
                    if (IdcounterofSubclauses < IdesofSubclauses.Count)
                    {
                        var rest = Regex.Match(line, @"^\)\s+(.*)$").Groups[1].Value;
                        clause.SubClauses.Add(new SubClause
                        {
                            Number = IdesofSubclauses[IdcounterofSubclauses],
                            Text = rest
                        });
                        subclausesAsked = true;
                        IdcounterofSubclauses++;
                    }
                    continue;
                }

                // Continuation — дописываем к последнему SubClause
                if (clause.SubClauses.Count > 0)
                {
                    var last = clause.SubClauses.Last();
                    last.Text = last.Text + "\n" + line;
                }
            }

            // Обработка изменения ID
            if (SubclausesResult == MessageBoxResult.Yes)
            {
                var currentList = clause.SubClauses.ToList();

                if (currentList.Count >= IdesofSubclauses.Count)
                {
                    // Элемент добавлен — сдвигаем через UpdateSubClause
                    clause.UpdateSubClause(IdofChangedSubclauseElement);
                }
                else
                {
                    // Элемент удалён — добавляем пустышку и удаляем через DeleteSubClause
                    clause.AddPhantomSubClause("", IdesofSubclauses[PositionOfChangedSubclauseElement]);
                    clause.DeleteSubClause(IdesofSubclauses[PositionOfChangedSubclauseElement]);
                }

                CanRefresh = true;
            }
        }

        private static void ParseSubClause(SubClause sub, string text)
        {
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .Select(l => l.Trim())
                            .ToArray();

            if (lines.Length == 0) return;

            // EndnoteId из последней строки
            var lastLine = lines[^1];
            var endnoteMatch = Regex.Match(lastLine, @"\(EndnoteId:\s*([^)]+)\)\s*$");
            if (endnoteMatch.Success)
            {
                sub.EndnoteId = endnoteMatch.Groups[1].Value.Trim();
                // убираем последнюю строку если она только EndnoteId
                lines = lines[..^1];
            }
            else
            {
                sub.EndnoteId = null;
            }

            if (lines.Length == 0) return;

            var firstLine = lines[0];
            var match = Regex.Match(firstLine, @"^(\d+)\)\s+(.*)$");
            int oldNumber = sub.Number;

            if (match.Success)
            {
                if (oldNumber != int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
                {
                    MessageBox.Show(
                        "Bu səviyyədə nömrə dəyişdirmək olmaz. Zəhmət olmasa səviyənizi artırın",
                        "Xəbərdarlıq",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    CanRefresh = true;
                }

                var textAfterNumber = match.Groups[2].Value;
                sub.Text = lines.Length > 1
                    ? textAfterNumber + "\n" + string.Join("\n", lines.Skip(1))
                    : textAfterNumber;
            }
            else
            {
                sub.Text = string.Join("\n", lines);
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
            // List
            List<int> TransitionalProvisionsIds = new List<int>();
            List<int> SourceDocumentsIds=new List<int>();
            List<string> ConstitutionalAmendmentIds=new List<string>();
            // Objects
            var tr = new TransitionalProvisions();
            var sd = new SourceDocumentsList();
            var ca = new ConstitutionalAmendment();




            if (sourceData.Id == 1 && sourceData.Source.Count > 1)
            {
                TransitionalProvisionsIds = sourceData.Source
                   .OfType<TransitionalProvisions>()
                   .Select(tp => tp.Id)
                   .ToList();
                tr.Id = TransitionalProvisionsIds[0];

            }
            else if (sourceData.Id == 2)
            {
                SourceDocumentsIds = sourceData.Source
                    .OfType<SourceDocumentsList>()
                    .Select(sd => sd.Id)
                    .ToList();
                sd.Id = SourceDocumentsIds[0];

            }
            else if (sourceData.Id == 3)
            {
                ConstitutionalAmendmentIds = sourceData.Source
                .OfType<ConstitutionalAmendment>()
                .Select(ca => ca.Id)
                .ToList();
                ca.Id = ConstitutionalAmendmentIds[0];

            }
            sourceData.Source.Clear();

            bool trPending = false;

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
                                                  !transitionalAsked &&
                                       (IdcounterofTransitionalProvisions >= TransitionalProvisionsIds.Count ||
                                       TransitionalProvisionsIds[IdcounterofTransitionalProvisions] != tr.Id))
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
                                       !sourceDocumentAsked &&
                                       (IdcounterofSourceDocuments >= SourceDocumentsIds.Count ||
                                       SourceDocumentsIds[IdcounterofSourceDocuments] != sd.Id))
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
                                       !constitutionalAmendmentAsked &&
                                      (IdcounterofConstitutionalAmendments >= ConstitutionalAmendmentIds.Count ||
                                       ConstitutionalAmendmentIds[IdcounterofConstitutionalAmendments] != ca.Id))
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
                                    !constitutionalAmendmentAsked &&
                                    (IdcounterofConstitutionalAmendments >= ConstitutionalAmendmentIds.Count ||
                                    ConstitutionalAmendmentIds[IdcounterofConstitutionalAmendments] != ca.Id))
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
                    sourceData.AddPhantomTransitionalProvision(
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
                    sourceData.AddPhantomSourceDocument(
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
                    sourceData.AddPhantomConstitutionalAmendment(
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