using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.SpecialElements
{
    public class Row
    {
        string RowId { get; set; }
        ObservableCollection<Cell> Cells { get; set; }= new ObservableCollection<Cell>();
    }
}
