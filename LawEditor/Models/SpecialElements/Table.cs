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
        int Id;
        ObservableCollection<Row> Rows { get; set; } = new ObservableCollection<Row>();
    }
}
