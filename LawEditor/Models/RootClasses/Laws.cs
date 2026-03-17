using LawEditor.Models;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace LawEditor.Models.RootClasses
{
    public class Laws {
        public string? Header { get; set; }
        public List<Chapter> Chapters { get; } = new();
        public List<TransitionalProvisions> transitionalProvisions { get; set; } = new();
        public List<ConstitutionalAmendment> constitutionalAmendments { get; set; } = new();
        public List<SourceDocumentsList> sourceDocumentsLists { get; set; } = new();

    }
}
