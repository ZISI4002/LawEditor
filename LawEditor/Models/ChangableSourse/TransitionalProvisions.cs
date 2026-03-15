using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class TransitionalProvisions {
        private static int counter = 1;

        public int Id { get; set; }
        public string Title { get; set; }

        public string Date { get; set; } = "12 noyabr 1995-ci il\n№00.";

        public TransitionalProvisions() { }   // нужно для XmlSerializer

        public TransitionalProvisions(string title) {
            Id = counter++;
            Title = title;
        }
    }
}
