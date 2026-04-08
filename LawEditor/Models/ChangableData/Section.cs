using LawEditor.Models.RootClasses;
using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        public decimal GetMaxPartID(decimal id)
        {
            decimal maxId = id;
            for(decimal i = id + 0.1m; i < id + 1; i += 0.1m)
            {
                if (Articles.Any(a => a.Id == i))
                    maxId = i;
                else
                    break;
            }
            return maxId;

        }
        public Article AddArticle(decimal id, string title, Laws laws, string? endnoteId = null) {
            bool isSubArticle = id % 1 != 0;

            if (!isSubArticle) {
                //сдвигаем все целые
                foreach (var chapter in laws.Chapters)
                    foreach (var section in chapter.Sections)
                        foreach (var article in section.Articles)
                            if (article.Id >= id)
                                article.Id += 1;
            }
            else {
                int baseId = (int)Math.Floor(id); // например 110

                foreach (var chapter in laws.Chapters) {
                    foreach (var section in chapter.Sections) {
                        foreach (var article in section.Articles) {
                            int articleBase = (int)Math.Floor(article.Id);

                            // берём только подстатьи этого блока (110.x)
                            if (articleBase == baseId && article.Id >= id) {
                                article.Id += 0.1m;
                            }
                        }
                    }
                }
            }

            var newArticle = new Article(id, title);
            newArticle.EndnoteId = endnoteId;

            Articles.Add(newArticle);

            // сортировка
            var sortedList = Articles.OrderBy(a => a.Id).ToList();
            Articles.Clear();
            foreach (var a in sortedList)
                Articles.Add(a);

            return newArticle;
        }
        public Article AddArticle(string title, Laws laws, string? endnoteId = null) {
            decimal newId = Articles.Count > 0
                ? (decimal)Math.Floor(Articles.Max(a => a.Id)) + 1
                : 1;

            var newArticle = new Article(newId, title);
            newArticle.EndnoteId = endnoteId;
            Articles.Add(newArticle);
            return newArticle;
        }
        public void DeleteArticle(decimal id, Laws laws) {
            var article = Articles.FirstOrDefault(a => a.Id == id);
            if (article == null)
                return;

            Articles.Remove(article);

            bool isSubArticle = id % 1 != 0;

            if (!isSubArticle) {
                //для целых
                foreach (var chapter in laws.Chapters)
                    foreach (var section in chapter.Sections)
                        foreach (var a in section.Articles)
                            if (a.Id > id)
                                a.Id -= 1;
            }
            else {
                int baseId = (int)Math.Floor(id);

                foreach (var chapter in laws.Chapters) {
                    foreach (var section in chapter.Sections) {
                        foreach (var a in section.Articles) {
                            int articleBase = (int)Math.Floor(a.Id);

                            // только 110.x и только те, что после удаляемого
                            if (articleBase == baseId && a.Id > id) {
                                a.Id -= 0.1m;
                            }
                        }
                    }
                }
            }
        }

    }
}