using System.IO;

namespace LawEditor.Models.SpecialElements
{
    public class Image
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string? Title { get; set; }
        public string Extension { get; set; } = string.Empty;
        public string FileName => $"Image {Id}{Extension}";
        public string FilePath { get; set; } = string.Empty;

        public Image() {
            Id = counter++;
        }
        public static void IncreaseCounter() => counter++;
        public static void DecreaseCounter() { if (counter > 1) counter--; }
        public static void ResetCounter() => counter = 1;
    }
}