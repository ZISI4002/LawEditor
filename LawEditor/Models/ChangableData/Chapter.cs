using DocumentFormat.OpenXml.Bibliography;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Table = LawEditor.Models.SpecialElements.Table;

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

        public Section AddPhantomSection(string title, int id)
        {
            var newSection = new Section(id, title);

            Sections.Add(newSection);

            // Сортировка
            Sections.OrderBy(i => i.Id);

            return newSection;
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

        public void UpdateSection(int newId) {
            if (newId <= 0) {
                MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var list = Sections.ToList();

            int currentIndex = list.FindIndex(t => t.Id == newId);

            if (currentIndex >= 1) {
                if (newId <= list[currentIndex - 1].Id) {
                    MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({list[currentIndex - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var itemsToUpdate = list.Skip(currentIndex + 1).ToList();

            foreach (var item in itemsToUpdate) {
                item.Id = ++newId;
            }

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