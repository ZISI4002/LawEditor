using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Chapter //Bolme
    {

        private static int counter = 1;

        public int Id { get; set; }
        public string? Title { get; set; }

        public List<Section> Sections { get; set; } = new();

        public Chapter() { }  

        public Chapter(string title) {
            Id = counter++;
            Title = title;
        }

    }
}