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

        //----------------------------------------------
        public TransitionalProvisions AddTransitionalProvision(string title, string? linkText = null, string? url = null, int? position = null) {
            var newItem = new TransitionalProvisions(title, linkText, url);

            if (position == null || position >= transitionalProvisions.Count) {
                transitionalProvisions.Add(newItem);
                return newItem;
            }

            int insertId = transitionalProvisions[position.Value].Id;
            foreach (var item in transitionalProvisions.Where(t => t.Id >= insertId)) {
                item.Id++;
            }
            newItem.Id = insertId;
            transitionalProvisions.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteTransitionalProvision(int id) {
            var item = transitionalProvisions.FirstOrDefault(t => t.Id == id);
            if (item == null)
                return;

            transitionalProvisions.Remove(item);
            foreach (var t in transitionalProvisions.Where(t => t.Id > id)) {
                t.Id--;
            }
            TransitionalProvisions.DecreaseCounter();
        }

        public void UpdateTransitionalProvision(int id, string? newTitle = null, string? newDate = null, string? newLinkText = null, string? newUrl = null) {
            var item = transitionalProvisions.FirstOrDefault(t => t.Id == id);
            if (item == null)
                return;

            if (newTitle != null)
                item.Title = newTitle;
            if (newDate != null)
                item.Date = newDate;
            if (newLinkText != null)
                item.LinkText = newLinkText;
            if (newUrl != null)
                item.Url = newUrl;
        }

        //----------------------------------------------

        public SourceDocumentsList AddSourceDocument(string title, string? linkText = null, string? url = null, int? position = null) {
            var newItem = new SourceDocumentsList(title, linkText, url);

            if (position == null || position >= sourceDocumentsLists.Count) {
                sourceDocumentsLists.Add(newItem);
                return newItem;
            }

            int insertId = sourceDocumentsLists[position.Value].Id;
            foreach (var item in sourceDocumentsLists.Where(s => s.Id >= insertId)) {
                item.Id++;
            }
            newItem.Id = insertId;
            sourceDocumentsLists.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteSourceDocument(int id) {
            var item = sourceDocumentsLists.FirstOrDefault(s => s.Id == id);
            if (item == null)
                return;

            sourceDocumentsLists.Remove(item);
            foreach (var s in sourceDocumentsLists.Where(s => s.Id > id)) {
                s.Id--;
            }
            SourceDocumentsList.DecreaseCounter();
        }

        public void UpdateSourceDocument(int id, string? newTitle = null, string? newLinkText = null, string? newUrl = null) {
            var item = sourceDocumentsLists.FirstOrDefault(s => s.Id == id);
            if (item == null)
                return;

            if (newTitle != null)
                item.Title = newTitle;
            if (newLinkText != null)
                item.LinkText = newLinkText;
            if (newUrl != null)
                item.Url = newUrl;
        }

        //----------------------------------------------

        public ConstitutionalAmendment AddConstitutionalAmendment(string id, string title, string? linkText = null, string? url = null, int? position = null) {
            var newItem = new ConstitutionalAmendment(id, title, linkText, url);

            if (position == null || position >= constitutionalAmendments.Count) {
                constitutionalAmendments.Add(newItem);
                return newItem;
            }

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
            if (int.TryParse(id, out int numericId)) {
                foreach (var c in constitutionalAmendments) {
                    if (int.TryParse(c.Id, out int itemNumericId) && itemNumericId > numericId)
                        c.Id = (itemNumericId - 1).ToString();
                }
            }
        }

        public void UpdateConstitutionalAmendment(string id, string? newTitle = null, string? newLinkText = null, string? newUrl = null) {
            var amendment = constitutionalAmendments.FirstOrDefault(c => c.Id == id);
            if (amendment == null)
                return;

            if (!string.IsNullOrWhiteSpace(newTitle))
                amendment.Title = newTitle;
            if (newLinkText != null)
                amendment.LinkText = newLinkText;
            if (newUrl != null)
                amendment.Url = newUrl;
        }
    }
}
