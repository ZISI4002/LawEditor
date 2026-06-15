using DocumentFormat.OpenXml.Office2010.Excel;
using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Models.ChangableData
{
    public class Clause //Bend
    {
        public int Number { get; set; }
        public string? Text { get; set; }
        public Table? Table { get; set; }
        public string? EndnoteId { get; set; }
        public string? LinkText { get; set; }
        public string? Url { get; set; }
        public Image? Image { get; set; }
        public ObservableCollection<SubClause> SubClauses { get; set; } = new();
        public Clause() { }
        public Clause(int number, string text, string? endnoteId = null, string? linkText = null, string? url = null) {
            Number = number;
            Text = text;
            EndnoteId = endnoteId;
            LinkText = linkText;
            Url = url;
        }
        public Clause(string text, string? endnoteId = null, string? linkText = null, string? url = null) {
            Text = text;
            EndnoteId = endnoteId;
            LinkText = linkText;
            Url = url;
        }

        public SubClause AddPhantomSubClause(string text, int id, string? endnoteId = null)
        {
            var newSubClause = new SubClause(id, text, endnoteId);

            SubClauses.Add(newSubClause);

            // Сортировка
            SubClauses.OrderBy(i => i.Number);

            return newSubClause;
        }

        public SubClause AddSubClause(string text, int? position = null, string? endnoteId = null) {
            var newSub = new SubClause(0, text, endnoteId);

            if (position == null || position >= SubClauses.Count) {
                newSub.Number = SubClauses.Count + 1;
                SubClauses.Add(newSub);
                return newSub;
            }

            int insertNumber = SubClauses[position.Value].Number;
            foreach (var sub in SubClauses.Where(s => s.Number >= insertNumber)) {
                sub.Number++;
            }
            newSub.Number = insertNumber;
            SubClauses.Insert(position.Value, newSub);
            return newSub;
        }
        public void UpdateSubClause(int newNumber) {
            if (newNumber <= 0) {
                MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var list = SubClauses.ToList();

            int currentIndex = list.FindIndex(t => t.Number == newNumber);

            if (currentIndex >= 1) {
                if (newNumber <= list[currentIndex - 1].Number) {
                    MessageBox.Show($"Новый ID ({newNumber}) должен быть больше предыдущего ({list[currentIndex - 1].Number}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var itemsToUpdate = list.Skip(currentIndex + 1).ToList();

            foreach (var item in itemsToUpdate) {
                item.Number = ++newNumber;
            }
        }
        public void DeleteSubClause(int number) {
            var sub = SubClauses.FirstOrDefault(s => s.Number == number);
            if (sub == null)
                return;

            SubClauses.Remove(sub);
            foreach (var s in SubClauses.Where(s => s.Number > number)) {
                s.Number--;
            }
        }

        public void AddTable(Laws law) => law.AddTableFor(this);
    }
}