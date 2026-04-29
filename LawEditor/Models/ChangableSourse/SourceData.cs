using DocumentFormat.OpenXml.Drawing;
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
 string title, string? linkText = null, string? url = null, int? position = null)
        { 
            var list = Source.OfType<TransitionalProvisions>().OrderBy(x => x.Id).ToList();
            var newItem = new TransitionalProvisions(title);
            var dateNode = Source.OfType<TransitionalProvisionsDateNote>().FirstOrDefault();

            if (position == null || position >= list.Count)
            {
                newItem.Id = list.Any() ? list.Max(x => x.Id) + 1 : 1;
                if (dateNode != null)
                    Source.Insert(Source.IndexOf(dateNode), newItem);
                else
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
        public void UpdateTransitionalProvision(int previousId, int newId) {
            if (newId <= 0)
                throw new ArgumentException("ID не может быть отрицательным или нулём.", nameof(newId));

            if (newId <= previousId)
                throw new ArgumentException($"Новый ID ({newId}) должен быть больше предыдущего ({previousId}).", nameof(newId));

            var itemsToUpdate = Source.OfType<TransitionalProvisions>()
                .OrderBy(t => t.Id)
                .Where(t => t.Id > previousId)
                .ToList();

            int diff = newId - (previousId + 1);

            foreach (var item in itemsToUpdate)
                item.Id += diff;
        }
        public void DeleteTransitionalProvision(int id)
        {
            var list = Source.OfType<TransitionalProvisions>().ToList();

            var item = list.FirstOrDefault(t => t.Id == id);
            if (item == null) return;

            Source.Remove(item);

            foreach (var t in Source.OfType<TransitionalProvisions>().Where(t => t.Id > id))
                t.Id--;
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
        public void UpdateSourceDocument(int previousId, int newId) {
            if (newId <= 0)
                throw new ArgumentException("ID не может быть отрицательным или нулём.", nameof(newId));

            if (newId <= previousId)
                throw new ArgumentException($"Новый ID ({newId}) должен быть больше предыдущего ({previousId}).", nameof(newId));

            var itemsToUpdate = Source.OfType<SourceDocumentsList>()
                .OrderBy(s => s.Id)
                .Where(s => s.Id > previousId)
                .ToList();

            int diff = newId - (previousId + 1);

            foreach (var item in itemsToUpdate)
                item.Id += diff;
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
        public ConstitutionalAmendment AddConstitutionalAmendment(string title, string id = null, string? linkText = null, string? url = null, int? position = null) {
            var list = Source.Cast<ConstitutionalAmendment>().ToList();
            var newItem = new ConstitutionalAmendment(id, title, linkText, url);

            if ((position == null && id == null) || position >= list.Count) {
                newItem.Id = id ?? (list.Any() ? list.Max(c => int.TryParse(c.Id, out int numId) ? numId : 0) + 1 : 1).ToString();
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
        public void UpdateConstitutionalAmendment(int previousId, int newId) {
            if (newId <= 0)
                throw new ArgumentException("ID не может быть отрицательным или нулём.", nameof(newId));

            if (newId <= previousId)
                throw new ArgumentException($"Новый ID ({newId}) должен быть больше предыдущего ({previousId}).", nameof(newId));

            var itemsToUpdate = Source.OfType<ConstitutionalAmendment>()
                .Where(c => int.TryParse(c.Id, out int id) && id > previousId)
                .OrderBy(c => int.Parse(c.Id))
                .ToList();

            int diff = newId - (previousId + 1);

            foreach (var item in itemsToUpdate)
                item.Id = (int.Parse(item.Id) + diff).ToString();
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
