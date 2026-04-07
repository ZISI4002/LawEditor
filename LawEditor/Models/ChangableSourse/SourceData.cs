using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class SourceData<T> {
        public int Id { get; set; }
        public string Type { get; set; }

        public ObservableCollection<T> Source { get; set; } = new();
    }
}
