using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Models.ChangableData
{
    public class Article //Madde
    {
        public decimal Id { get; set; }
        public string? Title { get; set; }
        public string? EndnoteId { get; set; }
        public Table? Table { get; set; }
        public Image? Image { get; set; }
        public ObservableCollection<Clause> Clauses { get; set; } = new();
        public Article() { }
        public Article(decimal id, string title, string? endnoteId = null) {
            Id = id;
            Title = title;
            EndnoteId = endnoteId;
        }
        public Clause AddPhantomClause(string text, int id, string? endnoteId = null, string? linkText = null, string? url = null) {
            var newClause = new Clause(id, text, endnoteId, linkText, url);
            Clauses.Add(newClause);
            // Сортировка
            Clauses.OrderBy(i => i.Number);
            return newClause;
        }
        public Clause AddClause(string text, int? position = null, string? endnoteId = null, string? linkText = null, string? url = null) {
            var newClause = new Clause(text, endnoteId, linkText, url);
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
        public void UpdateClause(int newNumber) {
            if (newNumber <= 0) {
                MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var list = Clauses.ToList();

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
        public void DeleteClause(int number) {
            var clause = Clauses.FirstOrDefault(c => c.Number == number);
            if (clause == null)
                return;

            Clauses.Remove(clause);
            foreach (var c in Clauses.Where(c => c.Number > number)) {
                c.Number--;
            }
        }      
        
        public void AddTable(Laws law) => law.AddTableFor(this);
        public void AddImage(Laws law, string sourcePath) => law.AddImageFor(this, sourcePath);
    }
}