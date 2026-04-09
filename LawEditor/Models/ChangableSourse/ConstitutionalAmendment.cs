using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class ConstitutionalAmendment {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? LinkText { get; set; } // тоже содержится в тайтле
        public string? Url { get; set; }
        public ConstitutionalAmendment() { }
        public ConstitutionalAmendment(string id, string title, string? linkText = null, string? url = null) {
            Id = id;
            Title = title;
            LinkText = linkText;
            Url = url;
        }
    }
}
