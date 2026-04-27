using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LawEditor.Models.ChangableData
{
    public class Article //Madde
    {
        public decimal Id { get; set; }
        public string? Title { get; set; }
        public string? EndnoteId { get; set; }
        public ObservableCollection<Clause> Clauses { get; set; } = new();
        public Article() { }
        public Article(decimal id, string title, string? endnoteId = null) {
            Id = id;
            Title = title;
            EndnoteId = endnoteId;
        }
        public Clause AddClause(string text, int? position = null, string? endnoteId = null) {
            var newClause = new Clause(text, endnoteId);

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
        public void UpdateClause(int previousNumber, int newNumber) {
            if (newNumber <= 0)
                throw new ArgumentException("Номер не может быть отрицательным или нулём.", nameof(newNumber));

            if (newNumber <= previousNumber)
                throw new ArgumentException($"Новый номер ({newNumber}) должен быть больше предыдущего ({previousNumber}).", nameof(newNumber));

            // Берём все клаузы после previousNumber
            var clausesToUpdate = Clauses
                .OrderBy(c => c.Number)
                .Where(c => c.Number > previousNumber)
                .ToList();

            int diff = newNumber - (previousNumber + 1);

            foreach (var clause in clausesToUpdate) {
                clause.Number += diff;
            }

            var sorted = Clauses.OrderBy(c => c.Number).ToList();
            Clauses.Clear();
            foreach (var c in sorted)
                Clauses.Add(c);
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