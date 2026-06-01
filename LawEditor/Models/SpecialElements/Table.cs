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
        ObservableCollection<Column> Columns { get; set; } = new ObservableCollection<Column>();
    }
}
