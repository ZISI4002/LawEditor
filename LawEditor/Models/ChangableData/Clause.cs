using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LawEditor.Models.ChangableData
{
    public class Clause //Bend
    {
        public int Number { get; set; }
        public string? Text { get; set; }
        public string? EndnoteId { get; set; }
        public ObservableCollection<SubClause> SubClauses { get; set; } = new();
        public Clause() { }
        public Clause(int number, string text) {
            Number = number;
            Text = text;
        }
        public Clause(string text) {
            Text = text;
        }
        public SubClause AddSubClause(string text, int? position = null, string? endnoteId = null) {
            var newSub = new SubClause { Text = text, EndnoteId = endnoteId };

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
        public void DeleteSubClause(int number) {
            var sub = SubClauses.FirstOrDefault(s => s.Number == number);
            if (sub == null)
                return;

            SubClauses.Remove(sub);
            foreach (var s in SubClauses.Where(s => s.Number > number)) {
                s.Number--;
            }
        }
        public void UpdateSubClause(int number, string? newText = null, string? newEndnoteId = null) {
            var sub = SubClauses.FirstOrDefault(s => s.Number == number);
            if (sub == null)
                return;

            if (newText != null)
                sub.Text = newText;
            if (newEndnoteId != null)
                sub.EndnoteId = newEndnoteId;
        }
    }
}