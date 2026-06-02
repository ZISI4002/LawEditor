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
        public List<string> Headers { get; set; } = new();
        public List<TableRowData> Rows { get; set; } = new();
        public Table(string title) {
            Id = counter++;
        }
    }
}
