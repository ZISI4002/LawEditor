using LawEditor.Models.RootClasses;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace LawEditor.Services
{
    public class LawXmlImportService
    {
        public Laws ImportFromXml(string folderPath,string fileName)
        {
            string filePath=Path.Combine(folderPath, fileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"XML файл не найден: {filePath}");

            var xdoc = XDocument.Load(filePath);
            var root = xdoc.Element("Laws");

            if (root == null)
                throw new InvalidDataException("Неверная структура XML: отсутствует корневой элемент <Laws>");

            var laws = new Laws
            {
                Header = root.Element("Header")?.Value ?? string.Empty
            };

            // Импорт Chapters
            var chaptersElement = root.Element("Chapters");
            if (chaptersElement != null)
            {
                foreach (var chapterEl in chaptersElement.Elements("Chapter"))
                {
                    var chapter = ParseChapter(chapterEl);
                    laws.Chapters.Add(chapter);
                }
            }

            // Импорт TransitionalProvisions
            var tpElement = root.Element("TransitionalProvisions");
            if (tpElement != null)
            {
                foreach (var tpEl in tpElement.Elements("TransitionalProvision"))
                {
                    laws.transitionalProvisions.Add(new TransitionalProvisions
                    {
                        Id = int.Parse(tpEl.Element("Id")?.Value ?? "0"),
                        Title = tpEl.Element("Title")?.Value ?? string.Empty,
                        Date = tpEl.Element("Date")?.Value ?? "12 noyabr 1995-ci il"
                    });
                }
            }

            // Импорт ConstitutionalAmendments
            var amendmentsElement = root.Element("ConstitutionalAmendments");
            if (amendmentsElement != null)
            {
                foreach (var amendmentEl in amendmentsElement.Elements("ConstitutionalAmendment"))
                {
                    laws.constitutionalAmendments.Add(new ConstitutionalAmendment
                    {
                        Id = int.Parse(amendmentEl.Element("Id")?.Value ?? "0"),
                        Title = amendmentEl.Element("Title")?.Value ?? string.Empty
                    });
                }
            }

            // Импорт SourceDocuments
            var sourcesElement = root.Element("SourceDocuments");
            if (sourcesElement != null)
            {
                foreach (var sourceEl in sourcesElement.Elements("SourceDocument"))
                {
                    laws.sourceDocumentsLists.Add(new SourceDocumentsList
                    {
                        Id = int.Parse(sourceEl.Element("Id")?.Value ?? "0"),
                        Title = sourceEl.Element("Title")?.Value ?? string.Empty
                    });
                }
            }

            return laws;
        }

        private Chapter ParseChapter(XElement chapterEl)
        {
            var chapter = new Chapter
            {
                Id = int.Parse(chapterEl.Element("Id")?.Value ?? "1"),
                Title = chapterEl.Element("Title")?.Value ?? string.Empty
            };

            var sectionsElement = chapterEl.Element("Sections");
            if (sectionsElement != null)
            {
                foreach (var sectionEl in sectionsElement.Elements("Section"))
                {
                    chapter.Sections.Add(ParseSection(sectionEl));
                }
            }

            return chapter;
        }

        private Section ParseSection(XElement sectionEl)
        {
            var section = new Section
            {
                Id = int.Parse(sectionEl.Element("Id")?.Value ?? "1"),
                Title = sectionEl.Element("Title")?.Value ?? string.Empty
            };

            var articlesElement = sectionEl.Element("Articles");
            if (articlesElement != null)
            {
                foreach (var articleEl in articlesElement.Elements("Article"))
                {
                    section.Articles.Add(ParseArticle(articleEl));
                }
            }

            return section;
        }

        private Article ParseArticle(XElement articleEl)
        {
            var article = new Article
            {
                Id = float.Parse(articleEl.Element("Id")?.Value ?? "1"),
                Title = articleEl.Element("Title")?.Value ?? string.Empty
            };

            var clausesElement = articleEl.Element("Clauses");
            if (clausesElement != null)
            {
                foreach (var clauseEl in clausesElement.Elements("Clause"))
                {
                    article.Clauses.Add(ParseClause(clauseEl));
                }
            }

            return article;
        }

        private Clause ParseClause(XElement clauseEl)
        {
            var clause = new Clause
            {
                Number = int.Parse(clauseEl.Element("Number")?.Value ?? "1"),
                Text = clauseEl.Element("Text")?.Value ?? string.Empty
            };

            var subClausesElement = clauseEl.Element("SubClauses");
            if (subClausesElement != null)
            {
                foreach (var subEl in subClausesElement.Elements("SubClause"))
                {
                    clause.SubClauses.Add(new SubClause
                    {
                        Number = int.Parse(subEl.Element("Number")?.Value ?? "1"),
                        Text = subEl.Element("Text")?.Value ?? string.Empty
                    });
                }
            }

            return clause;
        }
    }
}