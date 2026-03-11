namespace LawEditor.Models.ChangableData
{
    public class SubClause
    {
        public int Number { get; }
        public string Text { get; set; }

        public SubClause(int number, string text)
        {
            Number = number;
            Text = text;
        }
    }
}