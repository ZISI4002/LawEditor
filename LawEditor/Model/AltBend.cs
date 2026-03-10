using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Model {
    public class AltBend {
        public int Number { get; }
        public string Text { get; set; }
        public AltBend(int number, string text) {
            Number = number;
            Text = text;
        }
    }
}
