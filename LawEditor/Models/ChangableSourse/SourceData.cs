using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LawEditor.Models.ChangableSourse {
    public class SourceData {
        public int Id { get; set; }
        public string Type { get; set; }
       
        public ObservableCollection<object> Source { get; set; } = new();

        // TransitionalProvision

        public TransitionalProvisions AddPhantomTransitionalProvision(
 string title, int? id = null, int? position = null)
        {
            var list = Source.OfType<TransitionalProvisions>().OrderBy(x => x.Id).ToList();
            var newItem = new TransitionalProvisions(title);
            var dateNode = Source.OfType<TransitionalProvisionsDateNote>().FirstOrDefault();

            if (position == null && id != null) {
                newItem.Id = id.Value;
                Source.Add(newItem);
                return newItem;
            }
            if (position == null || position >= list.Count) {
                newItem.Id = list.Any() ? list.Max(x => x.Id) + 1 : 1;
                if (dateNode != null)
                    Source.Insert(Source.IndexOf(dateNode), newItem);
                else
                    Source.Add(newItem);
                return newItem;
            }

            int insertId = list[position.Value].Id;

            newItem.Id = insertId;
            Source.Insert(position.Value, newItem);
            return newItem;
        }

        public TransitionalProvisions AddTransitionalProvision(
 string title, int? id = null,  int? position = null)
        { 
            var list = Source.OfType<TransitionalProvisions>().OrderBy(x => x.Id).ToList();
            var newItem = new TransitionalProvisions(title);
            var dateNode = Source.OfType<TransitionalProvisionsDateNote>().FirstOrDefault();

            if(position == null && id != null)
            {
                newItem.Id = id.Value;
                Source.Add(newItem);
                return newItem;
            }
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
        public void UpdateTransitionalProvision( int newId) {
            if (newId <= 0) {
                MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var list = Source.OfType<TransitionalProvisions>().ToList();

            int currentIndex = list.FindIndex(t => t.Id == newId);

            if (currentIndex>=1) {
                if (newId <= list[currentIndex - 1].Id) {
                    MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({list[currentIndex - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var itemsToUpdate = list.Skip(currentIndex + 1).ToList();

            foreach (var item in itemsToUpdate) {
                item.Id = ++newId;
            }
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
        public SourceDocumentsList AddPhantomSourceDocument(
       string title, int? id = null, string? linkText = null, string? url = null, int? position = null)
        {
            var list = Source.Cast<SourceDocumentsList>().ToList();
            var newItem = new SourceDocumentsList(title, linkText, url);

            if (position == null && id != null) {
                newItem.Id = id.Value;
                Source.Add(newItem);
                return newItem;
            }

            if (position == null || position >= list.Count) {
                Source.Add(newItem);
                return newItem;
            }

            int insertId = list[position.Value].Id;

            newItem.Id = insertId;
            Source.Insert(position.Value, newItem);
            return newItem;
        }
        public SourceDocumentsList AddSourceDocument(
        string title, int? id = null, string? linkText = null, string? url = null, int? position = null) {
            var list = Source.Cast<SourceDocumentsList>().ToList();
            var newItem = new SourceDocumentsList(title, linkText, url);

            if (position == null && id != null) {
                newItem.Id = id.Value;
                Source.Add(newItem);
                return newItem;
            }

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
        public void UpdateSourceDocument(int newId) {

            if (newId <= 0) {
                MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var list = Source.OfType<SourceDocumentsList>().ToList();

            int currentIndex = list.FindIndex(t => t.Id == newId);

            if (currentIndex >= 1) {
                if (newId <= list[currentIndex - 1].Id) {
                    MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({list[currentIndex - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var itemsToUpdate = list.Skip(currentIndex + 1).ToList();

            foreach (var item in itemsToUpdate) {
                item.Id = ++newId;
            }
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
        public ConstitutionalAmendment AddPhantomConstitutionalAmendment(string title, string id = null, string? linkText = null, string? url = null, int? position = null)
        {
            var list = Source.Cast<ConstitutionalAmendment>().ToList();
            var newItem = new ConstitutionalAmendment(id, title, linkText, url);

            if (position == null && id != null) {
                newItem.Id = id;
                Source.Add(newItem);
                return newItem;
            }

            if ((position == null && id == null) || position >= list.Count) {
                newItem.Id = id ?? (list.Any() ? list.Max(c => int.TryParse(c.Id, out int numId) ? numId : 0) + 1 : 1).ToString();
                Source.Add(newItem);
                return newItem;
            }

            Source.Insert(position.Value, newItem);
            return newItem;
        }

        public ConstitutionalAmendment AddConstitutionalAmendment(string title, string id = null, string? linkText = null, string? url = null, int? position = null) {
            var list = Source.Cast<ConstitutionalAmendment>().ToList();
            var newItem = new ConstitutionalAmendment(id, title, linkText, url);

            if (position == null && id != null) {
                newItem.Id = id;
                Source.Add(newItem);
                return newItem;
            }

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
        public void UpdateConstitutionalAmendment(string currentId, string newId) {

            if (int.TryParse(currentId, out int currentInt) && !int.TryParse(newId, out _)) {
                // 1) Цифра и строка

                var list = Source.OfType<ConstitutionalAmendment>().ToList();
                int currentIndex = list.FindIndex(t => t.Id == newId);

                var itemsToUpdate = list.Skip(currentIndex + 1).ToList();

                foreach (var item in itemsToUpdate) {
                    if (int.TryParse(item.Id, out int itemInt)) {
                        item.Id = (itemInt - 1).ToString();
                    }
                        
                }
            }
            else if ((int.TryParse(currentId, out currentInt) && int.TryParse(newId, out int newInt))
                || (!int.TryParse(currentId, out _) && int.TryParse(newId, out newInt))) {
                // 2) Цифра и Цифра
                // 3) строка и Цифра

                if (newInt <= 0) {
                    MessageBox.Show("ID не может быть отрицательным или нулём.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var list = Source.OfType<ConstitutionalAmendment>().ToList();

                int currentIndex = list.FindIndex(t => t.Id == newId);
                
                if (currentIndex >= 1) {
                    // Ищу первую цифру до новой цифры
                    for (int i = currentIndex; i == 0; i--) {
                        if (int.TryParse(list[i - 1].Id, out int compare)) {
                            if (compare < newInt) {
                                break;
                            }
                            else {
                                MessageBox.Show($"Новый ID ({newId}) должен быть больше предыдущего ({list[i - 1].Id}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                }
                
                var itemsToUpdate = list.Skip(currentIndex + 1).ToList();

                foreach (var item in itemsToUpdate) {
                    if (int.TryParse(item.Id, out int itemInt)) {
                        item.Id = (++newInt).ToString();
                    }
                }

            }            

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
