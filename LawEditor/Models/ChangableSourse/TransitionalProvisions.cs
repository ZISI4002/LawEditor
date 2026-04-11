using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class TransitionalProvisions {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? LinkText { get; set; }
        public string? Url { get; set; }
        public static string Date { get; set; }
        public TransitionalProvisions() { }
        public TransitionalProvisions(string title, string? linkText = null, string? url = null) {
            Title = title;
            LinkText = linkText;
            Url = url;
        }
    }
}
