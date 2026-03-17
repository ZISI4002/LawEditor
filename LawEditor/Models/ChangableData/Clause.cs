using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Clause //Bend
    {
        public int Number { get; set; }
        public string? Text { get; set; }

        public List<SubClause> SubClauses { get; set; } = new();

        public Clause() { }  // ← НУЖНО

        public Clause(int number, string text) {
            Number = number;
            Text = text;
        }

        public SubClause AddSubClause(string text)
        {
            int number = SubClauses.Count + 1;
            var sub = new SubClause(number, text);
            SubClauses.Add(sub);
            return sub;
        }
    }
}