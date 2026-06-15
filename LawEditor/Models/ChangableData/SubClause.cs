using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Models.ChangableData
{
    public class SubClause //AltBend
    {
        public int Number { get; set; }
        public string? Text { get; set; }
        public string? EndnoteId { get; set; }
        public Table? Table { get; set; }
        public Image? Image { get; set; }
        public SubClause() { }
        public SubClause(int number, string text, string? endnoteId = null) {
            Number = number;
            Text = text;
            EndnoteId = endnoteId;
        }
        public void AddTable(Laws law) => law.AddTableFor(this);
    }
}