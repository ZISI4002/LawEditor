using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Section : LawElement
    {
        public List<Chapter> Chapters { get; } = new();

        public Section(string title) : base(title) { }
    }
}