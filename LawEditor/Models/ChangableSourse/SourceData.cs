using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class SourceData {
        public int Id { get; set; }
        public string Type { get; set; }

        public ObservableCollection<object> Source { get; set; } = new();

        // TransitionalProvision
        public TransitionalProvisions AddTransitionalProvision(
        string title, string? linkText = null, string? url = null, int? position = null) {
            var list = Source.Cast<TransitionalProvisions>().ToList();
            var newItem = new TransitionalProvisions(title, linkText, url);

            if (position == null || position >= list.Count) {
                Source.Add(newItem);
                return newItem;
            }

            int insertId = list[position.Value].Id;
            foreach (var item in list.Where(t => t.Id >= insertId))
                item.Id++;

            newItem.Id = insertId;
            Source.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteTransitionalProvision(int id) {
            var item = Source.Cast<TransitionalProvisions>().FirstOrDefault(t => t.Id == id);
            if (item == null) return;

            Source.Remove(item);
            foreach (var t in Source.Cast<TransitionalProvisions>().Where(t => t.Id > id))
                t.Id--;

            TransitionalProvisions.DecreaseCounter();
        }

        // SourceDocumentsList
        public SourceDocumentsList AddSourceDocument(
        string title, string? linkText = null, string? url = null, int? position = null) {
            var list = Source.Cast<SourceDocumentsList>().ToList();
            var newItem = new SourceDocumentsList(title, linkText, url);

            if (position == null || position >= list.Count) {
                Source.Add(newItem);
                return newItem;
            }

            int insertId = list[position.Value].Id;
            foreach (var item in list.Where(s => s.Id >= insertId))
                item.Id++;

            newItem.Id = insertId;
            Source.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteSourceDocument(int id) {
            var item = Source.Cast<SourceDocumentsList>().FirstOrDefault(s => s.Id == id);
            if (item == null) return;

            Source.Remove(item);
            foreach (var s in Source.Cast<SourceDocumentsList>().Where(s => s.Id > id))
                s.Id--;

            SourceDocumentsList.DecreaseCounter();
        }

        // ConstitutionalAmendment
        public ConstitutionalAmendment AddConstitutionalAmendment(
        string id, string title, string? linkText = null, string? url = null, int? position = null) {
            var list = Source.Cast<ConstitutionalAmendment>().ToList();
            var newItem = new ConstitutionalAmendment(id, title, linkText, url);

            if (position == null || position >= list.Count) {
                Source.Add(newItem);
                return newItem;
            }

            if (int.TryParse(id, out int numericId)) {
                foreach (var item in list) {
                    if (int.TryParse(item.Id, out int itemNumericId) && itemNumericId >= numericId)
                        item.Id = (itemNumericId + 1).ToString();
                }
            }

            Source.Insert(position.Value, newItem);
            return newItem;
        }

        public void DeleteConstitutionalAmendment(string id) {
            var item = Source.Cast<ConstitutionalAmendment>().FirstOrDefault(c => c.Id == id);
            if (item == null) return;

            Source.Remove(item);

            if (int.TryParse(id, out int numericId)) {
                foreach (var c in Source.Cast<ConstitutionalAmendment>()) {
                    if (int.TryParse(c.Id, out int itemNumericId) && itemNumericId > numericId)
                        c.Id = (itemNumericId - 1).ToString();
                }
            }
        }

    }
}
