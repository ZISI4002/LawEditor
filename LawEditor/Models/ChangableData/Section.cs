using LawEditor.Models.RootClasses;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Section //Fesil
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string? Title { get; set; }

        public ObservableCollection<Article> Articles { get; set; } = new();

        public Section() { } 

        public Section(string title) {
            Id = counter++;
            Title = title;
        }
        public static void DecreaseCounter() {
            if (counter > 1)
                counter--;
        }
        public static void ResetCounter() {
            counter = 1;
        }
        
        public Article AddArticle(float id, string title, string? endnoteId = null) {
            var newArticle = new Article(id, title);
            newArticle.EndnoteId = endnoteId;
            bool isSubArticle = id % 1 != 0;
            if (!isSubArticle) {
                foreach (var article in Articles) {
                    if (article.Id >= id) {
                        article.Id += 1;
                    }
                }
            }
            Articles.Add(newArticle);
            var sortedList = Articles.OrderBy(a => a.Id).ToList();
            Articles.Clear();
            foreach (var article in sortedList) {
                Articles.Add(article);
            }
            return newArticle;
        }
        public void DeleteArticle(float id) {
            var article = Articles.FirstOrDefault(a => a.Id == id);
            if (article == null)
                return;
            Articles.Remove(article);
            foreach (var a in Articles) {
                if (a.Id > id) {
                    a.Id -= 1;
                }
            }
            var sortedList = Articles.OrderBy(a => a.Id).ToList();
            Articles.Clear();
            foreach (var articl in sortedList) {
                Articles.Add(articl);
            }
        }
        public void UpdateArticle(float id, string? newTitle = null, string? newEndnoteId = null) {
            var article = Articles.FirstOrDefault(a => a.Id == id);
            if (article == null)
                return;
            if (newTitle != null)
                article.Title = newTitle;
            if (newEndnoteId != null)
                article.EndnoteId = newEndnoteId;
        }
    }
}