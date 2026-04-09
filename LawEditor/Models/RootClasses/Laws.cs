using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Serialization;

namespace LawEditor.Models.RootClasses
{
    [XmlInclude(typeof(SourceData))]
    [XmlInclude(typeof(TransitionalProvisions))]
    [XmlInclude(typeof(SourceDocumentsList))]
    [XmlInclude(typeof(ConstitutionalAmendment))]
    public partial class Laws 
    {       
        public ObservableCollection<UpperObject> UpperObjects { get; set; } = new();
        public ObservableCollection<Chapter> Chapters { get; } = new();

        public ObservableCollection<SourceData> SourceData { get; set; } = new();

        public Chapter AddChapter(string title, int? position = null)
        {
            var newChapter = new Chapter(title);
            if (position == null || position >= Chapters.Count)
            {
                Chapters.Add(newChapter);
                return newChapter;
            }
            int insertId = Chapters[position.Value].Id;
            foreach (var ch in Chapters.Where(c => c.Id >= insertId))
                ch.Id++;
            newChapter.Id = insertId;
            Chapters.Insert(position.Value, newChapter);
            return newChapter;
        }

        public void ResetCounter()
        {
            Chapter.ResetCounter();
            foreach (var chapter in Chapters)
                chapter.ResetSectionCounter();
        }

        public void DeleteChapter(int id)
        {
            var chapter = Chapters.FirstOrDefault(c => c.Id == id);
            if (chapter == null) return;
            Chapters.Remove(chapter);
            foreach (var ch in Chapters.Where(c => c.Id > id))
                ch.Id--;
            Chapter.DecreaseCounter();
        }       

    }
}