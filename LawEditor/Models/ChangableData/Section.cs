using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Section //Fesil
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string? Title { get; set; }

        public List<Article> Articles { get; set; } = new();

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
        //Добавляем Article(Madde)
        public Article AddArticle(float id, string title) {
            var newArticle = new Article(id, title);

            bool isSubArticle = id % 1 != 0;

            // если это не субстатья, то нужно сдвинуть все статьи с id >= id на 1
            if (!isSubArticle) {
                foreach (var article in Articles) {
                    if (article.Id >= id) {
                        article.Id += 1;
                    }
                }
            }
            // добавляем
            Articles.Add(newArticle);

            // сортируем
            Articles = Articles
                .OrderBy(a => a.Id)
                .ToList();

            return newArticle;
        }
        //Удаляем Article(Madde)
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

            // Сортируем (на всякий случай)
            Articles = Articles
                .OrderBy(a => a.Id)
                .ToList();
        }

        // Редактируем Article(Madde)
        public void UpdateArticle(float id, string? newTitle = null) {
            var article = Articles.FirstOrDefault(a => a.Id == id);

            if (article == null)
                return;

            if (newTitle != null)
                article.Title = newTitle;
        }
    }
}