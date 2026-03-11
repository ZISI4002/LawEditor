using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class TransitionalProvisions : LawElement {
        public string Date { get; } = "12 noyabr 1995-ci il\n№00.";
        public TransitionalProvisions (string title) : base(title) { }
    }
}
