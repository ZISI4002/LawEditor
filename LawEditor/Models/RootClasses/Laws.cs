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

        public ObservableCollection<object> SourceData { get; set; } = new()
        {
            new SourceData<TransitionalProvisions>
            {
                Id = 1,
                Type = "TransitionalProvisions"
            },

            new SourceData<SourceDocumentsList>
            {
                Id = 2,
                Type = "SourceDocumentsList"
            },

            new SourceData<ConstitutionalAmendment>
            {
                Id = 3,
                Type = "ConstitutionalAmendment"
            }
        };

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

        public TransitionalProvisions AddTransitionalProvision(
        string title, string? linkText = null, string? url = null, int? position = null) {
            var container = SourceData
                .OfType<SourceData<TransitionalProvisions>>()
                .First();

            var list = container.Source;

            var newItem = new TransitionalProvisions(title, linkText, url);

            if (position == null || position >= list.Count) {
                list.Add(newItem);
                return newItem;
            }

            int insertId = list[position.Value].Id;

            foreach (var item in list.Where(t => t.Id >= insertId))
                item.Id++;

            newItem.Id = insertId;
            list.Insert(position.Value, newItem);

            return newItem;
        }

        public void DeleteTransitionalProvision(int id) {
            var container = SourceData
                .OfType<SourceData<TransitionalProvisions>>()
                .First();

            var list = container.Source;

            var item = list.FirstOrDefault(t => t.Id == id);
            if (item == null) return;

            list.Remove(item);

            foreach (var t in list.Where(t => t.Id > id))
                t.Id--;

            TransitionalProvisions.DecreaseCounter();
        }

        //////////////////////////////////////////////////
        public SourceDocumentsList AddSourceDocument(
    string title, string? linkText = null, string? url = null, int? position = null) {
            var container = SourceData
                .OfType<SourceData<SourceDocumentsList>>()
                .First();

            var list = container.Source;

            var newItem = new SourceDocumentsList(title, linkText, url);

            if (position == null || position >= list.Count) {
                list.Add(newItem);
                return newItem;
            }

            int insertId = list[position.Value].Id;

            foreach (var item in list.Where(s => s.Id >= insertId))
                item.Id++;

            newItem.Id = insertId;
            list.Insert(position.Value, newItem);

            return newItem;
        }

        public void DeleteSourceDocument(int id) {
            var container = SourceData
                .OfType<SourceData<SourceDocumentsList>>()
                .First();

            var list = container.Source;

            var item = list.FirstOrDefault(s => s.Id == id);
            if (item == null) return;

            list.Remove(item);

            foreach (var s in list.Where(s => s.Id > id))
                s.Id--;

            SourceDocumentsList.DecreaseCounter();
        }

        public ConstitutionalAmendment AddConstitutionalAmendment(
    string id, string title, string? linkText = null, string? url = null, int? position = null) {
            var container = SourceData
                .OfType<SourceData<ConstitutionalAmendment>>()
                .First();

            var list = container.Source;

            var newItem = new ConstitutionalAmendment(id, title, linkText, url);

            if (position == null || position >= list.Count) {
                list.Add(newItem);
                return newItem;
            }

            if (int.TryParse(id, out int numericId)) {
                foreach (var item in list) {
                    if (int.TryParse(item.Id, out int itemNumericId) && itemNumericId >= numericId)
                        item.Id = (itemNumericId + 1).ToString();
                }
            }

            list.Insert(position.Value, newItem);

            return newItem;
        }

        public void DeleteConstitutionalAmendment(string id) {
            var container = SourceData
                .OfType<SourceData<ConstitutionalAmendment>>()
                .First();

            var list = container.Source;

            var item = list.FirstOrDefault(c => c.Id == id);
            if (item == null) return;

            list.Remove(item);

            if (int.TryParse(id, out int numericId)) {
                foreach (var c in list) {
                    if (int.TryParse(c.Id, out int itemNumericId) && itemNumericId > numericId)
                        c.Id = (itemNumericId - 1).ToString();
                }
            }
        }
    }
}