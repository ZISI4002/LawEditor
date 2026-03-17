using System;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Article //Madde
    {
        public float Id { get; set; }
        public string? Title { get; set; }

        public List<Clause> Clauses { get; set; } = new();

        public Article() { }  // ← НУЖНО

        public Article(float id, string title) {
            Id = id;
            Title = title;
        }

        public Clause AddClause(string text)
        {
            int number = Clauses.Count + 1;
            var clause = new Clause(number, text);
            Clauses.Add(clause);
            return clause;
        }
    }
}