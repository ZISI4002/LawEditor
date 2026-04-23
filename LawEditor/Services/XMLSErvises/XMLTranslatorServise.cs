using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LawEditor.Services.XMLServises {
    public class XMLTranslatorServise {
        public void Translate(Laws law, string folderPath, string fileName)
        {
            folderPath = folderPath.Replace("EditedFiles", "");
            var createdXmlFolder = Path.Combine(folderPath, "CreatedXML");

            if (!Directory.Exists(createdXmlFolder))
            {
                Directory.CreateDirectory(createdXmlFolder);
            }

            string filePath = Path.Combine(createdXmlFolder, fileName);
            var serializer = new XmlSerializer(typeof(Laws));
            using var stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, law);
        }
    }
}
