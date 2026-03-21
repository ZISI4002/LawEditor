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

        public Clause(string text)
        {
            Text = text;
        }

        public SubClause AddSubClause(string text)
        {
            int number = SubClauses.Count + 1;
            var sub = new SubClause(number, text);
            SubClauses.Add(sub);
            return sub;
        }
        public SubClause AddSubClause(string text, int? position = null) {
            var newSub = new SubClause {
                Text = text
            };

            // если добавляем в конец
            if (position == null || position >= SubClauses.Count) {
                newSub.Number = SubClauses.Count + 1;
                SubClauses.Add(newSub);
                return newSub;
            }

            // номер, куда вставляем
            int insertNumber = SubClauses[position.Value].Number;

            // сдвигаем все начиная с этой позиции
            foreach (var sub in SubClauses.Where(s => s.Number >= insertNumber)) {
                sub.Number++;
            }

            // задаём номер новому
            newSub.Number = insertNumber;

            // вставляем в список
            SubClauses.Insert(position.Value, newSub);

            return newSub;
        }
        public void DeleteSubClause(int number) {
            var sub = SubClauses.FirstOrDefault(s => s.Number == number);

            if (sub == null)
                return;

            // Удаляем
            SubClauses.Remove(sub);

            // Сдвигаем все последующие назад
            foreach (var s in SubClauses.Where(s => s.Number > number)) {
                s.Number--;
            }
        }
        public void UpdateSubClause(int number, string? newText = null) {
            var sub = SubClauses.FirstOrDefault(s => s.Number == number);

            if (sub == null)
                return;

            if (newText != null)
                sub.Text = newText;
        }
    }
}