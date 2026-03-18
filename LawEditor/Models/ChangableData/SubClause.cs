namespace LawEditor.Models.ChangableData
{
    public class SubClause //AltBend
    {
        public int Number { get; set; }
        public string? Text { get; set; }

        public SubClause() { }  // ← НУЖНО

        public SubClause(int number, string text) {
            Number = number;
            Text = text;
        }       
    }
}