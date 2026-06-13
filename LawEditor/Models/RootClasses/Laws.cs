using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Serialization;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Models.RootClasses
{
    [XmlInclude(typeof(SourceData))]
    [XmlInclude(typeof(TransitionalProvisions))]
    [XmlInclude(typeof(SourceDocumentsList))]
    [XmlInclude(typeof(ConstitutionalAmendment))]
    [XmlInclude(typeof(TransitionalProvisionsDateNote))]
    public partial class Laws 
    {       
        public ObservableCollection<UpperObject> UpperObjects { get; set; } = new();
        public ObservableCollection<Chapter> Chapters { get; } = new();

        public ObservableCollection<SourceData> SourcesData { get; set; } = new();

        public Chapter AddChapter(string title, int? position = null)
        {
            var newChapter = new Chapter(title);
            if (position == null || position >= Chapters.Count)
            {
                Chapters.Add(newChapter);
                return newChapter;
            }
            int insertId = Chapters[position.Value].Id;
            foreach (var ch in Chapters.Where(c => c.Id >= insertId))
                ch.Id++;
            newChapter.Id = insertId;
            Chapters.Insert(position.Value, newChapter);
            return newChapter;
        }

        public void ResetCounter()
        {
            Chapter.ResetCounter();
            foreach (var chapter in Chapters)
                chapter.ResetSectionCounter();
        }

        public void DeleteChapter(int id)
        {
            var chapter = Chapters.FirstOrDefault(c => c.Id == id);
            if (chapter == null) return;
            Chapters.Remove(chapter);
            foreach (var ch in Chapters.Where(c => c.Id > id))
                ch.Id--;
            Chapter.DecreaseCounter();
        }

        public void DeleteTable(int tableId) {
            bool deleted = false;

            foreach (var chapter in Chapters) {
                // Удаление таблицы у Chapter
                if (chapter.Table?.Id == tableId) {
                    chapter.Table = null;
                    deleted = true;
                }
                else if (chapter.Table?.Id > tableId) {
                    chapter.Table.Id--;
                    chapter.Table.Title = $"Table {chapter.Table.Id}";
                }

                foreach (var section in chapter.Sections) {
                    // Удаление таблицы у Section
                    if (section.Table?.Id == tableId) {
                        section.Table = null;
                        deleted = true;
                    }
                    else if (section.Table?.Id > tableId) {
                        section.Table.Id--;
                        section.Table.Title = $"Table {section.Table.Id}";
                    }

                    foreach (var article in section.Articles) {
                        // Удаление таблицы у Article
                        if (article.Table?.Id == tableId) {
                            article.Table = null;
                            deleted = true;
                        }
                        else if (article.Table?.Id > tableId) {
                            article.Table.Id--;
                            article.Table.Title = $"Table {article.Table.Id}";
                        }

                        foreach (var clause in article.Clauses) {
                            // Удаление таблицы у Clause
                            if (clause.Table?.Id == tableId) {
                                clause.Table = null;
                                deleted = true;
                            }
                            else if (clause.Table?.Id > tableId) {
                                clause.Table.Id--;
                                clause.Table.Title = $"Table {clause.Table.Id}";
                            }

                            foreach (var subClause in clause.SubClauses) {
                                // Удаление таблицы у SubClause
                                if (subClause.Table?.Id == tableId) {
                                    subClause.Table = null;
                                    deleted = true;
                                }
                                else if (subClause.Table?.Id > tableId) {
                                    subClause.Table.Id--;
                                    subClause.Table.Title = $"Table {subClause.Table.Id}";
                                }
                            }
                        }
                    }
                }
            }

            if (deleted) {
                Table.DecreaseCounter();
            }
        }
    }
}