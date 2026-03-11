using LawEditor.Models.RootClasses;
using System.Collections.Generic;

namespace LawEditor.Models.ChangableData
{
    public class Section : LawElement
    {
        public List<Article> Articles { get; } = new();

        public Section(string title) : base(title) { }
    }
}