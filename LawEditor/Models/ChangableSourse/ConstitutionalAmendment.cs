using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse {
    public class ConstitutionalAmendment{

        public string Id { get; set; }
        public string Title { get; set; }

        public ConstitutionalAmendment() { }  // нужно

        public ConstitutionalAmendment(string id, string title) {
            Id = id;
            Title = title;
        }
    }
}
