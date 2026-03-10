using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Model {
    public class Bolme : LawElement {
        public List<Fesil> Fesiller { get; } = new();
        public Bolme(string title) : base(title) { }
    }
}
