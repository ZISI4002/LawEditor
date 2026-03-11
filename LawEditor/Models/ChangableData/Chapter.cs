using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Chapter : LawElement
    {
        public List<Section> Sections { get; } = new();

        public Chapter(string title) : base(title) { }
    }
}