using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Chapter //Bolme
    {

        private static int counter = 1;

        public int Id { get; set; }
        public string? Title { get; set; }

        public List<Section> Sections { get; set; } = new();

        public Chapter() { }  

        public Chapter(string title) {
            Id = counter++;
            Title = title;
        }
        public static void DecreaseCounter() {
            if (counter > 1)
                counter--;
        }
        public static void ResetCounter() {
            counter = 1;
        }
        public void ResetSectionCounter() {
            
            Section.ResetCounter();
        }
        public Section AddSection(string title, int? position = null) {
            // создаём через конструктор → срабатывает counter
            var newSection = new Section(title);

            // если добавляем в конец
            if (position == null || position >= Sections.Count) {
                Sections.Add(newSection);
                return newSection;
            }

            // получаем Id, куда вставляем
            int insertId = Sections[position.Value].Id;

            // сдвигаем все элементы начиная с этой позиции
            foreach (var sec in Sections.Where(s => s.Id >= insertId)) {
                sec.Id++;
            }

            // задаём правильный Id новому элементу
            newSection.Id = insertId;

            // вставляем в список
            Sections.Insert(position.Value, newSection);

            return newSection;
        }

        public void DeleteSection(int id) {
            var section = Sections.FirstOrDefault(s => s.Id == id);

            if (section == null)
                return;

            Sections.Remove(section);

            foreach (var sec in Sections.Where(s => s.Id > id)) {
                sec.Id--;
            }

            Section.DecreaseCounter();
        }
        public void UpdateSection(int id, string? newTitle = null) {
            var section = Sections.FirstOrDefault(s => s.Id == id);

            if (section == null)
                return;

            if (newTitle != null)
                section.Title = newTitle;
        }

    }
}