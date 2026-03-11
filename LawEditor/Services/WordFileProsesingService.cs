using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.RegularExpressions;
using LawEditor.Models.ChangableData;
using DocumentFormat.OpenXml.Drawing.Diagrams;



namespace LawEditor.Services
{
   public class WordFileProsesingService
    {
        public Laws ReadWordFile(string filePath)
        {

            Laws law = new Laws();
            Chapter chapter = null;
            Section section = null;
            Article article = null;
            Clause clause = null;
            SubClause subClause = null;



            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;

            return law;

        }
    }
}
