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

        public Section() { }  // НУЖНО

        public Section(string title) {
            Id = counter++;
            Title = title;
        }

        public Article AddArticle(float id, string title) {
            var article = new Article(id, title);

            // Добавляем
            Articles.Add(article);

            // Сортируем
            Articles = Articles
                .OrderBy(a => a.Id)
                .ToList();

            return article;
        }
    }
}