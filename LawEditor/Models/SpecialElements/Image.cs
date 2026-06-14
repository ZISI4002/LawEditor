using System.IO;

namespace LawEditor.Models.SpecialElements
{
    public class Image
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public Image()
        {
            Id = counter++;
        }

        public void LoadFromFile(string path)
        {
            FilePath = path;
            Data = File.ReadAllBytes(path);
        }

        public static void ResetCounter() => counter = 1;
        public static void DecreaseCounter() { if (counter > 1) counter--; }
    }
}