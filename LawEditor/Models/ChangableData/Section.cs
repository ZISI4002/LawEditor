using LawEditor.Models.RootClasses;
using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace LawEditor.Models.ChangableData
{
    public class Section //Fesil       
    {
        private static int counter = 1;

        public int Id { get; set; }

        private string? title;

        public string? Title {
            get { return title; }
            set {
                title = value?.ToUpper();
            }
        }

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

            var newArticle = new Article(id, title, endnoteId);

            Articles.Add(newArticle);

            // Сортировка
            foreach (var chapter in laws.Chapters) {
                foreach (var section in chapter.Sections) {
                    var sorted = section.Articles.OrderBy(a => a.Id).ToList();
                    section.Articles.Clear();
                    foreach (var a in sorted)
                        section.Articles.Add(a);
                }
            }

            return newArticle;
        }
        public Article AddArticle(string title, Laws laws, string? endnoteId = null) {
            decimal newId = Articles.Count > 0
                ? (decimal)Math.Floor(Articles.Max(a => a.Id)) + 1
                : 1;

            var newArticle = new Article(newId, title, endnoteId);
            Articles.Add(newArticle);
            return newArticle;
        }
        public void UpdateArticle(decimal currentId, decimal newId, Laws laws) {
            if (currentId == Math.Floor(currentId) && newId == Math.Floor(newId)) {
                // 1) Целое и целое

                if (newId <= 0) {
                    MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Собираем ВСЕ статьи из всех глав и секций
                var allArticles = laws.Chapters
                    .SelectMany(c => c.Sections)
                    .SelectMany(s => s.Articles)
                    .ToList();

                int currentIndex = allArticles.FindIndex(a => a.Id == newId);

                // Найти предыдущий целый элемент
                if (currentIndex >= 1) {
                    if (newId <= allArticles[currentIndex - 1].Id) {
                        MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({allArticles[currentIndex - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var itemsToUpdate = allArticles.Skip(currentIndex + 1).ToList();

                foreach (var item in itemsToUpdate) {
                    if(item.Id == Math.Floor(item.Id)) {
                        item.Id = ++newId;
                    }
                    else {
                        item.Id = newId + (item.Id%1);
                    }
                }


            }
            else if (currentId == Math.Floor(currentId) && newId != Math.Floor(newId)) {
                

            }
            else if (currentId != Math.Floor(currentId) && newId != Math.Floor(newId)) {
                // 3) Дробное и дробное

                if (newId <= 0) {
                    MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Собираем ВСЕ статьи из всех глав и секций
                var allArticles = laws.Chapters
                    .SelectMany(c => c.Sections)
                    .SelectMany(s => s.Articles)
                    .ToList();

                int currentIndex = allArticles.FindIndex(a => a.Id == newId);

                // Найти предыдущий целый элемент
                if (currentIndex >= 1) {
                    if (newId <= allArticles[currentIndex - 1].Id) {
                        MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({allArticles[currentIndex - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }


                var itemsToUpdate = allArticles.Skip(currentIndex + 1).ToList();

                foreach (var item in itemsToUpdate) {
                    if (item.Id != Math.Floor(item.Id)) {
                        item.Id = newId += 0.1m;
                    }
                    else {
                        break;
                    }
                }

            }
            else if (currentId != Math.Floor(currentId) && newId == Math.Floor(newId)) {
                // 4) Дробное и целое

                if (newId <= 0) {
                    MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Собираем ВСЕ статьи из всех глав и секций
                var allArticles = laws.Chapters
                    .SelectMany(c => c.Sections)
                    .SelectMany(s => s.Articles)
                    .ToList();

                int currentIndex = allArticles.FindIndex(a => a.Id == newId);

                // Найти предыдущий целый элемент
                if (currentIndex >= 1) {
                    if (newId <= allArticles[currentIndex - 1].Id) {
                        MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({allArticles[currentIndex - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                var itemsToUpdate = allArticles.Skip(currentIndex + 1).ToList();

                foreach (var item in itemsToUpdate) {
                    if (Math.Floor(item.Id) == currentId) {
                        item.Id -= 0.1m;
                    }
                    else {
                        if (item.Id == Math.Floor(item.Id)) {
                            item.Id = ++newId;
                        }
                        else {
                            item.Id = newId + (item.Id % 1);
                        }
                    }
                }

                // Сортировка
                foreach (var chapter in laws.Chapters) {
                    foreach (var section in chapter.Sections) {
                        var sorted = section.Articles.OrderBy(a => a.Id).ToList();
                        section.Articles.Clear();
                        foreach (var a in sorted)
                            section.Articles.Add(a);
                    }
                }

            }
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