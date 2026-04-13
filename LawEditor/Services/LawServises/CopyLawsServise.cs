using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Services.LawServises
{
    public  class CopyLawsServise
    {
        public static void CopyLawsData(Laws source, Laws target)
        {
            if (source == null) return;

            // Копируем UpperObjects
            foreach (var upperObj in source.UpperObjects)
            {
                var newUpperObject = new UpperObject 
                { 
                    Id = upperObj.Id, 
                    ObjectName = upperObj.ObjectName,
                    Headers = new ObservableCollection<Header>()
                };
                
                foreach (var header in upperObj.Headers)
                {
                    newUpperObject.Headers.Add(new Header 
                    { 
                        Id = header.Id, 
                        FullText = header.FullText 
                    });
                }
                
                target.UpperObjects.Add(newUpperObject);
            }

            // Копируем главы
            foreach (var ch in source.Chapters)
            {
                var newChapter = new Chapter { Id = ch.Id, Title = ch.Title };
                foreach (var sec in ch.Sections)
                {
                    var newSection = new Section { Id = sec.Id, Title = sec.Title };
                    foreach (var art in sec.Articles)
                    {
                        var newArticle = new Article { Id = art.Id, Title = art.Title };
                        foreach (var cl in art.Clauses)
                        {
                            var newClause = new Clause { Number = cl.Number, Text = cl.Text };
                            foreach (var sc in cl.SubClauses)
                                newClause.SubClauses.Add(new SubClause { Number = sc.Number, Text = sc.Text });
                            newArticle.Clauses.Add(newClause);
                        }
                        newSection.Articles.Add(newArticle);
                    }
                    newChapter.Sections.Add(newSection);
                }
                target.Chapters.Add(newChapter);
            }

            // Копируем SourceData
            target.SourceData.Clear();

            foreach (var sourceContainer in source.SourceData)
            {
                if (sourceContainer is not SourceData src) continue;

                var newContainer = new SourceData
                {
                    Id = src.Id,
                    Type = src.Type,
                    Source = new ObservableCollection<object>()
                };

                foreach (var item in src.Source)
                {
                    object? copy = item switch
                    {
                        TransitionalProvisions tp => new TransitionalProvisions
                        {
                            Id = tp.Id,
                            Title = tp.Title,
                           
                            LinkText= tp.LinkText,
                            Url = tp.Url
                        },
                        ConstitutionalAmendment ca => new ConstitutionalAmendment
                        {
                            Id = ca.Id,
                            Title = ca.Title,
                            LinkText= ca.LinkText,
                            Url = ca.Url
                        },
                        SourceDocumentsList sd => new SourceDocumentsList
                        {
                            Id = sd.Id,
                            Title = sd.Title,
                            LinkText= sd.LinkText,
                            Url = sd.Url
                        },
                        TransitionalProvisionsDateNote => new TransitionalProvisionsDateNote(),
                        _ => null
                    };

                    if (copy != null)
                        newContainer.Source.Add(copy);
                }

                target.SourceData.Add(newContainer);
            }
        }
    }
}
