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

        public void DeleteTable(int id) {
            // Собираем все объекты с таблицами в порядке дерева
            var allWithTables = new List<(Action<Table?> setTable, Func<Table?> getTable)>();

            foreach (var chapter in Chapters) {
                allWithTables.Add((t => chapter.Table = t, () => chapter.Table));

                foreach (var section in chapter.Sections) {
                    allWithTables.Add((t => section.Table = t, () => section.Table));

                    foreach (var article in section.Articles) {
                        allWithTables.Add((t => article.Table = t, () => article.Table));

                        foreach (var clause in article.Clauses) {
                            allWithTables.Add((t => clause.Table = t, () => clause.Table));

                            foreach (var subClause in clause.SubClauses) {
                                allWithTables.Add((t => subClause.Table = t, () => subClause.Table));
                            }
                        }
                    }
                }
            }

            // Находим объект с искомой таблицей
            var target = allWithTables.FirstOrDefault(x => x.getTable()?.Id == id);
            if (target.setTable == null) return;

            // Удаляем
            target.setTable(null);

            // Сдвигаем Id у всех таблиц с Id > удалённого
            foreach (var (_, getTable) in allWithTables) {
                var table = getTable();
                if (table != null && table.Id > id) {
                    table.Id--;
                    table.Title = $"Table {table.Id}";
                }
            }

            Table.DecreaseCounter();
        }
    }
}