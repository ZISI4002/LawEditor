using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Model {
    public class Fesil : LawElement {
        public List<Madde> Maddeler { get; } = new();
        public Fesil(string title) : base(title) { }
    }
}
