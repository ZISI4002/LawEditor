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