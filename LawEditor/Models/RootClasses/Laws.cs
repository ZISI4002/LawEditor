using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;

namespace LawEditor.Models.RootClasses
{
    public partial class Laws : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string? _header;
        public string? Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(nameof(Header)); }
        }

        public ObservableCollection<Chapter> Chapters { get; } = new();
        public ObservableCollection<TransitionalProvisions> transitionalProvisions { get; set; } = new();
        public ObservableCollection<SourceDocumentsList> sourceDocumentsLists { get; set; } = new();
        public ObservableCollection<ConstitutionalAmendment> constitutionalAmendments { get; set; } = new();

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

        public TransitionalProvisions AddTransitionalProvision(string title, string? linkText = null, string? url = null, int? position = null)
        {
            var newItem = new TransitionalProvisions(title, linkText, url);
            if (position == null || position >= transitionalProvisions.Count)
            {
                transitionalProvisions.Add(newItem);
                return newItem;
            }
            int insertId = transitionalProvisions[position.Value].Id;
            foreach (var item in transitionalProvisions.Where(t => t.Id >= insertId))
                item.Id++;
            newItem.Id = insertId;
            transitionalProvisions.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteTransitionalProvision(int id)
        {
            var item = transitionalProvisions.FirstOrDefault(t => t.Id == id);
            if (item == null) return;
            transitionalProvisions.Remove(item);
            foreach (var t in transitionalProvisions.Where(t => t.Id > id))
                t.Id--;
            TransitionalProvisions.DecreaseCounter();
        }

        public SourceDocumentsList AddSourceDocument(string title, string? linkText = null, string? url = null, int? position = null)
        {
            var newItem = new SourceDocumentsList(title, linkText, url);
            if (position == null || position >= sourceDocumentsLists.Count)
            {
                sourceDocumentsLists.Add(newItem);
                return newItem;
            }
            int insertId = sourceDocumentsLists[position.Value].Id;
            foreach (var item in sourceDocumentsLists.Where(s => s.Id >= insertId))
                item.Id++;
            newItem.Id = insertId;
            sourceDocumentsLists.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteSourceDocument(int id)
        {
            var item = sourceDocumentsLists.FirstOrDefault(s => s.Id == id);
            if (item == null) return;
            sourceDocumentsLists.Remove(item);
            foreach (var s in sourceDocumentsLists.Where(s => s.Id > id))
                s.Id--;
            SourceDocumentsList.DecreaseCounter();
        }

        public ConstitutionalAmendment AddConstitutionalAmendment(string id, string title, string? linkText = null, string? url = null, int? position = null)
        {
            var newItem = new ConstitutionalAmendment(id, title, linkText, url);
            if (position == null || position >= constitutionalAmendments.Count)
            {
                constitutionalAmendments.Add(newItem);
                return newItem;
            }
            if (int.TryParse(id, out int numericId))
            {
                foreach (var item in constitutionalAmendments)
                    if (int.TryParse(item.Id, out int itemNumericId) && itemNumericId >= numericId)
                        item.Id = (itemNumericId + 1).ToString();
            }
            constitutionalAmendments.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteConstitutionalAmendment(string id)
        {
            var item = constitutionalAmendments.FirstOrDefault(c => c.Id == id);
            if (item == null) return;
            constitutionalAmendments.Remove(item);
            if (int.TryParse(id, out int numericId))
            {
                foreach (var c in constitutionalAmendments)
                    if (int.TryParse(c.Id, out int itemNumericId) && itemNumericId > numericId)
                        c.Id = (itemNumericId - 1).ToString();
            }
        }
    }
}