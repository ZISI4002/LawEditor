using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Section
    {
        private static int counter = 1;
        public int Id { get; }
        public string Title { get; protected set; } //protected на случай изменения заголовка в будущем
        public Section(string title)
        { //автосчетчик
            Id = counter++;
            Title = title;
        }
    
        public List<Article> Articles { get; } = new();

    }
}