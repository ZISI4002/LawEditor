using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.SpecialElements;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

        private List<(object owner, Func<Table?> getTable, Action<Table?> setTable)> CollectAllWithTables() {
            var list = new List<(object owner, Func<Table?> getTable, Action<Table?> setTable)>();

            foreach (var chapter in Chapters) {
                list.Add((chapter, () => chapter.Table, t => chapter.Table = t));

                foreach (var section in chapter.Sections) {
                    list.Add((section, () => section.Table, t => section.Table = t));

                    foreach (var article in section.Articles) {
                        list.Add((article, () => article.Table, t => article.Table = t));

                        foreach (var clause in article.Clauses) {
                            list.Add((clause, () => clause.Table, t => clause.Table = t));

                            foreach (var subClause in clause.SubClauses) {
                                list.Add((subClause, () => subClause.Table, t => subClause.Table = t));
                            }
                        }
                    }
                }
            }

            return list;
        }

        // ── DELETE: удаляет таблицу по Id, сдвигает все последующие Id на -1 ───
        public void DeleteTable(int id) {
            var allWithTables = CollectAllWithTables();

            // Находим объект с искомой таблицей
            var target = allWithTables.FirstOrDefault(x => x.getTable()?.Id == id);
            if (target.setTable == null) return;

            // Удаляем
            target.setTable(null);

            // Сдвигаем Id у всех таблиц с Id > удалённого
            foreach (var (_, getTable, _) in allWithTables) {
                var table = getTable();
                if (table != null && table.Id > id) {
                    table.Id--;
                    table.Title = $"Table {table.Id}";
                }
            }

            Table.DecreaseCounter();
        }

        // ── ADD: создаёт новую таблицу для указанного объекта (owner) ──────────
        public void AddTableFor(object owner) {
            var allWithTables = CollectAllWithTables();

            int myIndex = allWithTables.FindIndex(x => ReferenceEquals(x.owner, owner));
            if (myIndex == -1) return;

            // Если у объекта уже есть таблица — ничего не делаем
            if (allWithTables[myIndex].getTable() != null) return;

            // Ищем первую таблицу СРЕДИ ЭЛЕМЕНТОВ ПОСЛЕ текущего
            int newId = 1;
            bool foundAfter = false;
            for (int i = myIndex + 1; i < allWithTables.Count; i++) {
                var t = allWithTables[i].getTable();
                if (t != null) {
                    newId = t.Id;
                    foundAfter = true;
                    break;
                }
            }

            // Если после текущего объекта нет таблиц — новый Id = максимальный + 1
            if (!foundAfter) {
                int max = 0;
                foreach (var (_, getTable, _) in allWithTables) {
                    var t = getTable();
                    if (t != null && t.Id > max) max = t.Id;
                }
                newId = max + 1;
            }

            // Сдвигаем Id у всех таблиц с Id >= newId
            foreach (var (_, getTable, _) in allWithTables) {
                var t = getTable();
                if (t != null && t.Id >= newId) {
                    t.Id++;
                    t.Title = $"Table {t.Id}";
                }
            }

            // Создаём новую таблицу
            var table = Table.CreateManual();
            table.Id = newId;
            table.Title = $"Table {newId}";
            table.Headers.Add("Column 1");
            table.Rows.Add(new TableRowData { Cells = { "" } });

            allWithTables[myIndex].setTable(table);
            Table.IncreaseCounter();
        }

        private List<(object owner, Func<Image?> getImage, Action<Image?> setImage)> CollectAllWithImages() {
            var list = new List<(object owner, Func<Image?> getImage, Action<Image?> setImage)>();
            foreach (var chapter in Chapters) {
                list.Add((chapter, () => chapter.Image, i => chapter.Image = i));
                foreach (var section in chapter.Sections) {
                    list.Add((section, () => section.Image, i => section.Image = i));
                    foreach (var article in section.Articles) {
                        list.Add((article, () => article.Image, i => article.Image = i));
                        foreach (var clause in article.Clauses) {
                            list.Add((clause, () => clause.Image, i => clause.Image = i));
                            foreach (var subClause in clause.SubClauses) {
                                list.Add((subClause, () => subClause.Image, i => subClause.Image = i));
                            }
                        }
                    }
                }
            }
            return list;
        }

        // ── DELETE ────────────────────────────────────────────────────────────────
        public void DeleteImage(int id) {
            var all = CollectAllWithImages();

            var target = all.FirstOrDefault(x => x.getImage()?.Id == id);
            if (target.setImage == null) return;

            var targetImage = target.getImage();
            if (targetImage != null && File.Exists(targetImage.FilePath))
                File.Delete(targetImage.FilePath);

            target.setImage(null);

            foreach (var (_, getImage, _) in all) {
                var img = getImage();
                if (img == null || img.Id <= id) continue;

                string oldPath = img.FilePath;
                img.Id--;
                string newPath = Path.Combine(Path.GetDirectoryName(oldPath)!, img.FileName);

                if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);

                img.FilePath = newPath;
            }

            Image.DecreaseCounter();
        }

        // ── ADD ───────────────────────────────────────────────────────────────────
        public void AddImageFor(object owner, string sourcePath) {
            var all = CollectAllWithImages();

            int myIndex = all.FindIndex(x => ReferenceEquals(x.owner, owner));
            if (myIndex == -1) return;

            if (all[myIndex].getImage() != null) return;

            int newId = 1;
            bool foundAfter = false;
            for (int i = myIndex + 1; i < all.Count; i++) {
                var img = all[i].getImage();
                if (img != null) {
                    newId = img.Id;
                    foundAfter = true;
                    break;
                }
            }

            if (!foundAfter) {
                int max = 0;
                foreach (var (_, getImage, _) in all) {
                    var img = getImage();
                    if (img != null && img.Id > max) max = img.Id;
                }
                newId = max + 1;
            }

            foreach (var (_, getImage, _) in all) {
                var img = getImage();
                if (img == null || img.Id < newId) continue;

                string oldPath = img.FilePath;
                img.Id++;
                string newPath = Path.Combine(Path.GetDirectoryName(oldPath)!, img.FileName);

                if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);

                img.FilePath = newPath;
            }

            string extension = Path.GetExtension(sourcePath);
            var newImage = new Image { Extension = extension };
            newImage.Id = newId;

            Directory.CreateDirectory(Image.SpecialImagesFolder);
            string destPath = Path.Combine(Image.SpecialImagesFolder, newImage.FileName);
            File.Copy(sourcePath, destPath, overwrite: true);
            newImage.FilePath = destPath;

            all[myIndex].setImage(newImage);
            Image.IncreaseCounter();
        }
    }
}