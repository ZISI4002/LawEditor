using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.SpecialElements
{
    public class Table
    {
        private static int counter = 1;

        public int Id { get; set; }
        public string Title { get; set; }
        public ObservableCollection<string> Headers { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<TableRowData> Rows { get; set; } = new ObservableCollection<TableRowData>();
        public Table() {
            Id = counter++;
            Title = $"Table {Id}";
        }
        public static void ResetCounter() {
            counter = 1;
        }
    }
}
