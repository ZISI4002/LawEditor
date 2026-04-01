using LawEditor.Models;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace LawEditor.Models.RootClasses
{
    public class Laws {
        public string? Header { get; set; }
        public List<Chapter> Chapters { get; } = new();
        public List<TransitionalProvisions> transitionalProvisions { get; set; } = new();
        public List<SourceDocumentsList> sourceDocumentsLists { get; set; } = new();
        public List<ConstitutionalAmendment> constitutionalAmendments { get; set; } = new();


        //Добавить главу в указанную позицию(если позиция не указана, то в конец)
        public Chapter AddChapter(string title, int? position = null) {
            // создаём через конструктор → срабатывает counter
            var newChapter = new Chapter(title);

            // если добавляем в конец
            if (position == null || position >= Chapters.Count) {
                Chapters.Add(newChapter);
                return newChapter;
            }

            // получаем Id, куда вставляем
            int insertId = Chapters[position.Value].Id;

            // сдвигаем все элементы начиная с этой позиции
            foreach (var ch in Chapters.Where(c => c.Id >= insertId)) {
                ch.Id++;
            }

            // задаём правильный Id новому элементу
            newChapter.Id = insertId;

            // вставляем в список
            Chapters.Insert(position.Value, newChapter);

            return newChapter;
        }
        public void ResetCounter() {
            Chapter.ResetCounter();
            foreach (var chapter in Chapters) {
                chapter.ResetSectionCounter();
            }
        }

        // Удалить главу по Id
        public void DeleteChapter(int id) {
            var chapter = Chapters.FirstOrDefault(c => c.Id == id);

            if (chapter == null)
                return;

            // Удаляем
            Chapters.Remove(chapter);

            // Сдвигаем все последующие назад
            foreach (var ch in Chapters.Where(c => c.Id > id)) {
                ch.Id--;
            }

            // Уменьшаем counter
            Chapter.DecreaseCounter();
        }
        // Редактируем главу по Id (например, изменить название)
        public void UpdateChapter(int id, string? newTitle = null) {
            var chapter = Chapters.FirstOrDefault(c => c.Id == id);

            if (chapter == null)
                return;

            if (newTitle != null)
                chapter.Title = newTitle;
        }


        public TransitionalProvisions AddTransitionalProvision(string title, int? position = null) {
            // создаём → срабатывает counter
            var newItem = new TransitionalProvisions(title);

            // если добавляем в конец
            if (position == null || position >= transitionalProvisions.Count) {
                transitionalProvisions.Add(newItem);
                return newItem;
            }

            // получаем Id, куда вставляем
            int insertId = transitionalProvisions[position.Value].Id;

            // сдвигаем все элементы начиная с этой позиции
            foreach (var item in transitionalProvisions.Where(t => t.Id >= insertId)) {
                item.Id++;
            }

            // задаём правильный Id новому элементу
            newItem.Id = insertId;

            // вставляем в список
            transitionalProvisions.Insert(position.Value, newItem);

            return newItem;
        }

        public void DeleteTransitionalProvision(int id) {
            var item = transitionalProvisions.FirstOrDefault(t => t.Id == id);

            if (item == null)
                return;

            // Удаляем
            transitionalProvisions.Remove(item);

            // Сдвигаем все последующие назад
            foreach (var t in transitionalProvisions.Where(t => t.Id > id)) {
                t.Id--;
            }

            // Уменьшаем counter
            TransitionalProvisions.DecreaseCounter();
        }

        public void UpdateTransitionalProvision(int id, string? newTitle = null, string? newDate = null) {
            var item = transitionalProvisions.FirstOrDefault(t => t.Id == id);

            if (item == null)
                return;

            if (newTitle != null)
                item.Title = newTitle;

            if (newDate != null)
                item.Date = newDate;
        }


        public SourceDocumentsList AddSourceDocument(string title, int? position = null) {
            // создаём → срабатывает counter
            var newItem = new SourceDocumentsList(title);

            // если добавляем в конец
            if (position == null || position >= sourceDocumentsLists.Count) {
                sourceDocumentsLists.Add(newItem);
                return newItem;
            }

            // получаем Id, куда вставляем
            int insertId = sourceDocumentsLists[position.Value].Id;

            // сдвигаем все элементы начиная с этой позиции
            foreach (var item in sourceDocumentsLists.Where(s => s.Id >= insertId)) {
                item.Id++;
            }

            // задаём правильный Id новому элементу
            newItem.Id = insertId;

            // вставляем в список
            sourceDocumentsLists.Insert(position.Value, newItem);

            return newItem;
        }

        public void DeleteSourceDocument(int id) {
            var item = sourceDocumentsLists.FirstOrDefault(s => s.Id == id);

            if (item == null)
                return;

            // Удаляем
            sourceDocumentsLists.Remove(item);

            // Сдвигаем все последующие назад
            foreach (var s in sourceDocumentsLists.Where(s => s.Id > id)) {
                s.Id--;
            }

            // Уменьшаем counter
            SourceDocumentsList.DecreaseCounter();
        }

        public void UpdateSourceDocument(int id, string? newTitle = null) {
            var item = sourceDocumentsLists.FirstOrDefault(s => s.Id == id);

            if (item == null)
                return;

            if (newTitle != null)
                item.Title = newTitle;
        }

        public ConstitutionalAmendment AddConstitutionalAmendment(string id, string title, int? position = null) {
            var newItem = new ConstitutionalAmendment(id, title);

            if (position == null || position >= constitutionalAmendments.Count) {
                constitutionalAmendments.Add(newItem);
                return newItem;
            }

            // Если новый id числовой — сдвигаем все числовые id >= этого числа
            if (int.TryParse(id, out int numericId)) {
                foreach (var item in constitutionalAmendments) {
                    if (int.TryParse(item.Id, out int itemNumericId) && itemNumericId >= numericId)
                        item.Id = (itemNumericId + 1).ToString();
                }
            }

            constitutionalAmendments.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteConstitutionalAmendment(string id) {
            var item = constitutionalAmendments.FirstOrDefault(c => c.Id == id);
            if (item == null)
                return;

            constitutionalAmendments.Remove(item);

            // Если удалённый id числовой — сдвигаем все числовые id > этого числа на -1
            if (int.TryParse(id, out int numericId)) {
                foreach (var c in constitutionalAmendments) {
                    if (int.TryParse(c.Id, out int itemNumericId) && itemNumericId > numericId)
                        c.Id = (itemNumericId - 1).ToString();
                }
            }
        }

        public void UpdateConstitutionalAmendment(string id, string? newTitle = null) {
            var amendment = constitutionalAmendments.FirstOrDefault(c => c.Id == id);
            if (amendment == null)
                return;

            if (!string.IsNullOrWhiteSpace(newTitle))
                amendment.Title = newTitle;
        }
    }
}
