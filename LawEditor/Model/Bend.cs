using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Model {
    public class Bend {
        public int Number { get; }
        public string Text { get; set; }
        public List<AltBend> AltBendler { get; } = new();
        public Bend(int number, string text) {
            Number = number;
            Text = text;
        }
        public AltBend AddAltBend(string text) {
            int number = AltBendler.Count + 1;
            var alt = new AltBend(number, text);
            AltBendler.Add(alt);
            return alt;
        }
    }
}
