using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Chapter
    {

        private static int counter = 1;
        public int Id { get; }
        public string Title { get; protected set; } //protected на случай изменения заголовка в будущем
        public Chapter(string title)
        { //автосчетчик
            Id = counter++;
            Title = title;
        }

        public List<Section> Sections { get; } = new();

    }
}