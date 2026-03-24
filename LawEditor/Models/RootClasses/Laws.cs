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
        public List<ConstitutionalAmendment> constitutionalAmendments { get; set; } = new();
        public List<SourceDocumentsList> sourceDocumentsLists { get; set; } = new();

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
    }
}
