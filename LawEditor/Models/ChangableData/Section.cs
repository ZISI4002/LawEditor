using LawEditor.Models.RootClasses;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using LawEditor.ViewModels;

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
        public Article AddArticle(float id, string title, Laws laws, string? endnoteId = null) {
            bool isSubArticle = id % 1 != 0;
            if (!isSubArticle) {
                foreach (var chapter in laws.Chapters)
                    foreach (var section in chapter.Sections)
                        foreach (var article in section.Articles)
                            if (article.Id >= id)
                                article.Id += 1;
            }

            var newArticle = new Article(id, title);
            newArticle.EndnoteId = endnoteId;

            Articles.Add(newArticle);

            var sortedList = Articles.OrderBy(a => a.Id).ToList();
            Articles.Clear();
            foreach (var a in sortedList)
                Articles.Add(a);

            return newArticle;
        }
        public void DeleteArticle(float id, Laws laws) {
            var article = Articles.FirstOrDefault(a => a.Id == id);
            if (article == null)
                return;

            Articles.Remove(article);

            bool isSubArticle = id % 1 != 0;
            if (!isSubArticle) {
                foreach (var chapter in laws.Chapters)
                    foreach (var section in chapter.Sections)
                        foreach (var a in section.Articles)
                            if (a.Id > id)
                                a.Id -= 1;
            }
        }

    }
}