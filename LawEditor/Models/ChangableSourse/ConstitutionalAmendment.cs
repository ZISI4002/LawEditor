using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class ConstitutionalAmendment{
        private static int counter = 1;

        public int Id { get; set; }
        public string Title { get; set; }

        public ConstitutionalAmendment() { }  // нужно

        public ConstitutionalAmendment(string title) {
            Id = counter++;
            Title = title;
        }
        public static void DecreaseCounter() {
            if (counter > 1)
                counter--;
        }
    }
}
