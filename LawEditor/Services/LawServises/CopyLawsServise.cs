using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Services.LawServises
{
    public  class CopyLawsServise
    {
        public static void CopyLawsData(Laws source, Laws target)
        {
            target.Header = source.Header;

            // Копируем главы
            foreach (var ch in source.Chapters)
            {
                var newChapter = new Chapter
                {
                    Id = ch.Id,
                    Title = ch.Title
                };

                foreach (var sec in ch.Sections)
                {
                    var newSection = new Section
                    {
                        Id = sec.Id,
                        Title = sec.Title
                    };

                    foreach (var art in sec.Articles)
                    {
                        var newArticle = new Article
                        {
                            Id = art.Id,
                            Title = art.Title
                        };

                        foreach (var cl in art.Clauses)
                        {
                            var newClause = new Clause
                            {
                                Number = cl.Number,
                                Text = cl.Text
                            };

                            foreach (var sc in cl.SubClauses)
                            {
                                newClause.SubClauses.Add(new SubClause
                                {
                                    Number = sc.Number,
                                    Text = sc.Text
                                });
                            }

                            newArticle.Clauses.Add(newClause);
                        }

                        newSection.Articles.Add(newArticle);
                    }

                    newChapter.Sections.Add(newSection);
                }

                target.Chapters.Add(newChapter);
            }

            // Копируем TransitionalProvisions
            foreach (var tp in source.transitionalProvisions)
            {
                target.transitionalProvisions.Add(new TransitionalProvisions
                {
                    Id = tp.Id,
                    Title = tp.Title,
                    Date = tp.Date
                });
            }

            // Копируем ConstitutionalAmendment
            foreach (var ca in source.constitutionalAmendments)
            {
                target.constitutionalAmendments.Add(new ConstitutionalAmendment
                {
                    Id = ca.Id,
                    Title = ca.Title
                });
            }

            // Копируем SourceDocumentsList
            foreach (var sd in source.sourceDocumentsLists)
            {
                target.sourceDocumentsLists.Add(new SourceDocumentsList
                {
                    Id = sd.Id,
                    Title = sd.Title
                });
            }
        }
    }
}
