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
        public Clause AddClause(string text, int? position = null) {
            var newClause = new Clause(text);

            // если добавляем в конец
            if (position == null || position >= Clauses.Count) {
                newClause.Number = Clauses.Count + 1;
                Clauses.Add(newClause);
                return newClause;
            }

            // номер, куда вставляем
            int insertNumber = Clauses[position.Value].Number;

            // сдвигаем все начиная с этой позиции
            foreach (var clause in Clauses.Where(c => c.Number >= insertNumber)) {
                clause.Number++;
            }

            // задаём номер новому
            newClause.Number = insertNumber;

            // вставляем в список
            Clauses.Insert(position.Value, newClause);

            return newClause;
        }
        public void DeleteClause(int number) {
            var clause = Clauses.FirstOrDefault(c => c.Number == number);

            if (clause == null)
                return;

            // Удаляем
            Clauses.Remove(clause);

            // Сдвигаем все последующие назад
            foreach (var c in Clauses.Where(c => c.Number > number)) {
                c.Number--;
            }
        }
        public void UpdateClause(int number, string? newText = null) {
            var clause = Clauses.FirstOrDefault(c => c.Number == number);

            if (clause == null)
                return;

            if (newText != null)
                clause.Text = newText;
        }
    }
}