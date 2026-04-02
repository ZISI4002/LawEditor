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
        public string? Title { get; set; }
        public string Date { get; set; } = "12 noyabr 1995-ci il\n№00.";
        public string? LinkText { get; set; }
        public string? Url { get; set; }
        public TransitionalProvisions() { }
        public TransitionalProvisions(string title, string? linkText = null, string? url = null) {
            Id = counter++;
            Title = title;
            LinkText = linkText;
            Url = url;
        }
        public static void DecreaseCounter() {
            if (counter > 1)
                counter--;
        }
    }
}
