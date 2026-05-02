using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace LawEditor.Models.ChangableData
{
    public class Clause //Bend
    {
        public int Number { get; set; }
        public string? Text { get; set; }
        public string? EndnoteId { get; set; }
        public ObservableCollection<SubClause> SubClauses { get; set; } = new();
        public Clause() { }
        public Clause(int number, string text, string? endnoteId = null) {
            Number = number;
            Text = text;
            EndnoteId = endnoteId;
        }
        public Clause(string text, string? endnoteId = null) {
            Text = text;
            EndnoteId = endnoteId;
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
        public void UpdateSubClause(int currentNumber, int newNumber) {
            if (newNumber <= 0)
                MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            var list = SubClauses.ToList();

            int currentIndex = list.FindIndex(t => t.Number == newNumber);

            if (newNumber <= list[currentIndex - 1].Number)
                MessageBox.Show($"Новый ID ({newNumber}) должен быть больше предыдущего ({list[currentIndex - 1].Number}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

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

    }
}