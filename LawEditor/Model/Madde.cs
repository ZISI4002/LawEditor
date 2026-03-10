using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Model {
    public class Madde {
        public float Id { get; }
        public string Title { get; set; }
        public List<Bend> Bendler { get; } = new();
        public Madde(float id, string title) {
            Id = id;
            Title = title;
        }
        public Bend AddBend(string text) {
            int number = Bendler.Count + 1;
            var bend = new Bend(number, text);
            Bendler.Add(bend);
            return bend;
        }
    }
}
