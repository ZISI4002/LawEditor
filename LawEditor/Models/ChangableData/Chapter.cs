using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LawEditor.Models.ChangableData
{
    public class Chapter //Bolme
    {
        private static int counter = 1;

        public int Id { get; set; }
        private string? title;

        public string? Title {
            get { return title; }
            set { 
                title = value?.ToUpper(); 
            }
        }
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

        public void UpdateSection(int previousId, int newId) {
            if (newId <= 0)
                throw new ArgumentException("ID не может быть отрицательным или нулём.", nameof(newId));

            if (newId <= previousId)
                throw new ArgumentException($"Новый ID ({newId}) должен быть больше предыдущего ({previousId}).", nameof(newId));

            // Берём секцию с Id = previousId + 1 и все последующие
            var sectionsToUpdate = Sections
                .OrderBy(s => s.Id)
                .Where(s => s.Id > previousId)
                .ToList();

            decimal nextId = newId;
            foreach (var sec in sectionsToUpdate)
                sec.Id = (int)nextId++;

            var sorted = Sections.OrderBy(s => s.Id).ToList();
            Sections.Clear();
            foreach (var s in sorted)
                Sections.Add(s);
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
    }
}