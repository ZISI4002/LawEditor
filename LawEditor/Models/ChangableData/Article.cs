using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LawEditor.Models.ChangableData
{
    public class Article //Madde
    {
        public float Id { get; set; }
        public string? Title { get; set; }
        public string? EndnoteId { get; set; }
        public ObservableCollection<Clause> Clauses { get; set; } = new();
        public Article() { }
        public Article(float id, string title) {
            Id = id;
            Title = title;
        }
        public Clause AddClause(string text, int? position = null, string? endnoteId = null) {
            var newClause = new Clause(text);
            newClause.EndnoteId = endnoteId;

            if (position == null || position >= Clauses.Count) {
                newClause.Number = Clauses.Count + 1;
                Clauses.Add(newClause);
                return newClause;
            }

            int insertNumber = Clauses[position.Value].Number;
            foreach (var clause in Clauses.Where(c => c.Number >= insertNumber)) {
                clause.Number++;
            }
            newClause.Number = insertNumber;
            Clauses.Insert(position.Value, newClause);
            return newClause;
        }
        public void DeleteClause(int number) {
            var clause = Clauses.FirstOrDefault(c => c.Number == number);
            if (clause == null)
                return;

            Clauses.Remove(clause);
            foreach (var c in Clauses.Where(c => c.Number > number)) {
                c.Number--;
            }
        }

    }
}