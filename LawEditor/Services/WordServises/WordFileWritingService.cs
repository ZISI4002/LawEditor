using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Linq;

namespace LawEditor.Services.WordServises
{
    public class WordFileWritingService
    {
        public void WriteWordFile(string filePath, Laws laws)
        {
            using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Header
            if (laws.UpperObjects.Count > 0 && laws.UpperObjects[0].Headers.Count > 0)
            {
                var headerText = laws.UpperObjects[0].Headers[0].FullText;
                if (!string.IsNullOrWhiteSpace(headerText))
                {
                    var headerLines = headerText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in headerLines)
                        body.AppendChild(CreateParagraph(line));
                }
            }

            // Chapters
            foreach (var chapter in laws.Chapters)
            {
                string chapterOrdinal = ToAzerbaijaniOrdinal(chapter.Id);
                body.AppendChild(CreateParagraph($"{chapterOrdinal} BÖLMƏ", true));
                body.AppendChild(CreateParagraph(chapter.Title, true));

                foreach (var section in chapter.Sections)
                {
                    string sectionRoman = ToRoman(section.Id);
                    body.AppendChild(CreateParagraph($"{sectionRoman} fəsil"));
                    body.AppendChild(CreateParagraph(section.Title));

                    foreach (var article in section.Articles)
                    {
                        string articleId = FormatArticleId(article.Id);
                        body.AppendChild(CreateParagraph($"Maddə {articleId}. {article.Title}", true));

                        foreach (var clause in article.Clauses)
                        {
                            if (clause.Number > 0)
                            {
                                string clauseRoman = ToRoman(clause.Number);
                                body.AppendChild(CreateParagraph($"{clauseRoman}. {clause.Text}"));
                            }
                            else
                            {
                                body.AppendChild(CreateParagraph(clause.Text));
                            }

                            foreach (var subClause in clause.SubClauses)
                                body.AppendChild(CreateParagraph($"{subClause.Number}) {subClause.Text}"));
                        }
                    }
                }
            }

            // Transitional Provisions
            var transitionalData = laws.SourceData.FirstOrDefault(s => s.Id == 1);
            if (transitionalData?.Source.Count > 0)
            {
                body.AppendChild(CreateParagraph("Keçİd müddəaları", true));
                foreach (var item in transitionalData.Source)
                {
                    if (item is TransitionalProvisions tp)
                        body.AppendChild(CreateParagraph(tp.Title ?? ""));
                }
                //#   Добавь пустую строку после пунктов(перед датой) (от Алсу)
            }

            // Source Documents
            var sourceData = laws.SourceData.FirstOrDefault(s => s.Id == 2);
            if (sourceData?.Source.Count > 0)
            {
                body.AppendChild(CreateParagraph("İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI", true));
                foreach (var item in sourceData.Source)
                {
                    if (item is SourceDocumentsList sd)
                        body.AppendChild(CreateParagraph(sd.Title ?? ""));
                }
            }

            // Constitutional Amendments
            // Reader пропускает эту строку (continue), amendments читаются из endnotes
            // Поэтому просто пишем заголовок — amendments не пишем в body
            var amendmentsData = laws.SourceData.FirstOrDefault(s => s.Id == 3);
            if (amendmentsData?.Source.Count > 0)
            {
                body.AppendChild(CreateParagraph("KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI", true));
                // Reader читает amendments из endnotes, не из body
                // Поэтому здесь ничего не пишем
            }

            mainPart.Document.Save();
        }

        private Paragraph CreateParagraph(string text, bool bold = false)
        {
            var para = new Paragraph();
            var run = new Run();

            if (bold)
                run.RunProperties = new RunProperties(new Bold());

            run.AppendChild(new Text(text));
            para.AppendChild(run);

            return para;
        }

        private string FormatArticleId(decimal id)
        {
            if (id == (int)id)
                return ((int)id).ToString();
            return id.ToString("0.0");
        }

        private string ToAzerbaijaniOrdinal(int number)
        {
            string[] ordinals = {
                "", "Birinci", "İkinci", "Üçüncü", "Dördüncü",
                "Beşinci", "Altıncı", "Yeddinci", "Səkkizinci",
                "Doqquzuncu", "Onuncu", "On birinci", "On ikinci",
                "On üçüncü", "On dördüncü", "On beşinci"
            };

            if (number > 0 && number < ordinals.Length)
                return ordinals[number].ToUpper();

            return number.ToString();
        }

        private string ToRoman(int number)
        {
            if (number < 1) return "";

            string[] romanNumerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

            var result = "";
            for (int i = 0; i < values.Length; i++)
            {
                while (number >= values[i])
                {
                    number -= values[i];
                    result += romanNumerals[i];
                }
            }

            return result;
        }
    }
}