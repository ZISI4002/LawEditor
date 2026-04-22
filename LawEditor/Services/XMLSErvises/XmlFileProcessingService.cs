using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LawEditor.Services.XMLServises
{
    public class XmlFileProcessingService
    {
        public Laws ReadXmlFile(string folderPath, string fileName)
        {
            string filePath = Path.Combine(folderPath, fileName);

            var doc = XDocument.Load(filePath);
            var root = doc.Root;

            if (root == null)
                throw new InvalidOperationException("XML faylı boşdur");

            var law = new Laws();

            // Initialize SourceData containers
            law.SourcesData.Add(new SourceData { Type = "TransitionalProvisions", Source = new() });
            law.SourcesData.Add(new SourceData { Type = "ConstitutionalAmendment", Source = new() });
            law.SourcesData.Add(new SourceData { Type = "SourceDocumentsList", Source = new() });

            // Initialize UpperObjects if needed
            if (law.UpperObjects.Count == 0)
            {
                law.UpperObjects.Add(new UpperObject { ObjectName = "Default" });
                if (law.UpperObjects[0].Headers == null)
                    law.UpperObjects[0].Headers = new();
                law.UpperObjects[0].Headers.Add(new Header());
            }

            // Header
            var headerElement = root.Element("Header");
            if (headerElement != null)
                law.UpperObjects[0].Headers[0].FullText = headerElement.Value;

            // Chapters
            var chaptersElement = root.Element("Chapters");
            if (chaptersElement != null)
            {
                foreach (var chapterElement in chaptersElement.Elements("Chapter"))
                {
                    var chapterTitle = chapterElement.Element("Title")?.Value ?? "";
                    var chapter = new Chapter(chapterTitle);

                    var sectionsElement = chapterElement.Element("Sections");
                    if (sectionsElement != null)
                    {
                        foreach (var sectionElement in sectionsElement.Elements("Section"))
                        {
                            var sectionTitle = sectionElement.Element("Title")?.Value ?? "";
                            var section = new Section(sectionTitle);

                            var articlesElement = sectionElement.Element("Articles");
                            if (articlesElement != null)
                            {
                                foreach (var articleElement in articlesElement.Elements("Article"))
                                {
                                    decimal id = 0;
                                    var idElement = articleElement.Element("Id");
                                    if (idElement != null)
                                        decimal.TryParse(idElement.Value, out id);

                                    var articleTitle = articleElement.Element("Title")?.Value ?? "";
                                    var article = new Article(id, articleTitle);

                                    var clausesElement = articleElement.Element("Clauses");
                                    if (clausesElement != null)
                                    {
                                        int clauseNumber = 1;
                                        foreach (var clauseElement in clausesElement.Elements("Clause"))
                                        {
                                            var clauseText = clauseElement.Element("Text")?.Value ?? "";
                                            var clause = new Clause(clauseNumber, clauseText);

                                            var subClausesElement = clauseElement.Element("SubClauses");
                                            if (subClausesElement != null)
                                            {
                                                int subClauseNumber = 1;
                                                foreach (var subClauseElement in subClausesElement.Elements("SubClause"))
                                                {
                                                    var subClauseText = subClauseElement.Element("Text")?.Value ?? "";
                                                    var subClause = new SubClause(subClauseNumber, subClauseText);
                                                    clause.SubClauses.Add(subClause);
                                                    subClauseNumber++;
                                                }
                                            }

                                            article.Clauses.Add(clause);
                                            clauseNumber++;
                                        }
                                    }

                                    section.Articles.Add(article);
                                }
                            }

                            chapter.Sections.Add(section);
                        }
                    }

                    law.Chapters.Add(chapter);
                }
            }

            // TransitionalProvisions
            var transitionalProvisions = law.SourcesData[0] as dynamic;
            var transitionalElement = root.Element("transitionalProvisions");
            if (transitionalElement != null)
            {
                foreach (var item in transitionalElement.Elements("TransitionalProvisions"))
                {
                    var title = item.Element("Title")?.Value ?? "";
                    transitionalProvisions.Add(new TransitionalProvisions(title));
                }
            }

            // ConstitutionalAmendments
            var constitutionalAmendments = law.SourcesData[1] as dynamic;
            var amendmentsElement = root.Element("constitutionalAmendments");
            if (amendmentsElement != null)
            {
                foreach (var item in amendmentsElement.Elements("ConstitutionalAmendment"))
                {
                    var title = item.Element("Title")?.Value ?? "";
                    var Id = item.Element("Id")?.Value ?? "";
                    constitutionalAmendments.Add(new ConstitutionalAmendment(Id, title));
                }
            }

            // SourceDocuments
            var sourceDocumentsLists = law.SourcesData[2] as dynamic;
            var sourcesElement = root.Element("sourceDocumentsLists");
            if (sourcesElement != null)
            {
                foreach (var item in sourcesElement.Elements("SourceDocumentsList"))
                {
                    var title = item.Element("Title")?.Value ?? "";
                    sourceDocumentsLists.Add(new SourceDocumentsList(title));
                }
            }

            return law;
        }
    }
}