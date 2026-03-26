using LawEditor.Models.RootClasses;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Chapter //Bolme
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string? Title { get; set; }

        public ObservableCollection<Section> Sections { get; set; } = new();

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
            var newSection = new Section(title);

            if (position == null || position >= Sections.Count) {
                Sections.Add(newSection);
                return newSection;
            }

            int insertId = Sections[position.Value].Id;

            foreach (var sec in Sections.Where(s => s.Id >= insertId)) {
                sec.Id++;
            }

            newSection.Id = insertId;
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