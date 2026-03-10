using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Model {
    public class KecidMuddeaları : LawElement {
        public string Date { get; } = "12 noyabr 1995-ci il\n№00.";
        public KecidMuddeaları(string title) : base(title) { }
    }
}
