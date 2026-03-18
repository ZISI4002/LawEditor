using LawEditor.Models.RootClasses;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace LawEditor.Services
{
    public class XMLDisplayService
    {
        public FlowDocument BuildDocument(Laws laws, string folderPath, string fileName)
        {
            var doc = new FlowDocument();
            doc.PagePadding = new Thickness(12);

            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                string xmlContent = File.ReadAllText(filePath);

                doc.Blocks.Add(new Paragraph(new Run(xmlContent))
                {
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 11,
                    Foreground = Brushes.Black,
                    TextAlignment = TextAlignment.Left
                });
            }
            else
            {
                doc.Blocks.Add(new Paragraph(new Run("XML faylı tapılmadı"))
                {
                    FontSize = 14,
                    Foreground = Brushes.Red,
                    TextAlignment = TextAlignment.Center
                });
            }

            return doc;
        }
    }
}