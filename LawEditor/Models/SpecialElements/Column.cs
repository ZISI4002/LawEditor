using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.SpecialElements
{
    public class Column
    {
        string ColumnId { get; set; }
        string ColumnName { get; set; }
        ObservableCollection<Cell> Cells { get; set; }= new ObservableCollection<Cell>();
    }
}
