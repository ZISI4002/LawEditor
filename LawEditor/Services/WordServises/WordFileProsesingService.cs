using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
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


namespace LawEditor.Services.WordServises
{
   public class WordFileProsesingService
   {
        private bool IsChapterLine(string line) {
            
            if (string.IsNullOrWhiteSpace(line))
                return false;

            string[] chapterPrefixes =
                { "birinci", "ikinci", "üçüncü", "dördüncü", "beşinci",
          "altıncı", "yeddinci", "səkkizinci", "doqquzuncu" };
            

            // приводим к нижнему регистру и убираем лишние пробелы
            string lower = line.ToLower().Trim();

            foreach (var prefix in chapterPrefixes) {
                // проверяем по шаблону: prefix + пробелы + bölmə
                // ^ — начало строки
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
            Amendments,
            Sources
        }
        public Laws ReadWordFile(string filePath)
        {
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



            foreach (var para in body.Elements<Paragraph>()) {
                var raw = para.InnerText ?? "";
                var line = raw.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Пропускаем строки с INCLUDEPICTURE и другим мусором
                if (line.Contains("INCLUDEPICTURE") || 
                    line.Contains("MERGEFORMATINET") ||
                    line.Contains("userway.org") ||
                    line.Contains("\\*"))
                    continue;

                // --- переключатели списков ---
                if (line.Contains("Keçİd müddəaları", StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Transitional;
                    continue;
                }

                if (line.Contains("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI", StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Sources;
                    continue;
                }

                if (line.Contains("KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI", StringComparison.OrdinalIgnoreCase)) {
                    mode = Mode.Amendments;
                    continue;
                }

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
                    if (IsChapterLine(line)){
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

                        if (idStr.Length == 4 && idStr.All(char.IsDigit)) {
                            id = int.Parse(idStr) / 10f;
                        }
                        else {
                            float.TryParse(idStr, out id);
                        }

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

                    // текст без римских пунктов
                    if (currentArticle != null) {
                        if (currentClause == null) {
                            currentClause = currentArticle.AddClause(line);
                        }
                        else {
                            currentClause.Text += " " + line;
                        }

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

                // AMENDMENTS
                if (mode == Mode.Amendments) {
                    law.constitutionalAmendments.Add(new ConstitutionalAmendment(line));
                    continue;
                }
            }

            if (law.Header == null)
                law.Header = headerBuilder.ToString().Trim();
            
            return law;
        }
    }
}