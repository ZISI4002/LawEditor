using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;

namespace LawEditor.Models.TreeClasses
{
    public class LawAnchor
    {
        // Корень документа
        public Laws? Laws { get; set; }

        //Хидр
        public UpperObject? UpperObject { get; set; }

        // Иерархия
        public Chapter? Chapter { get; set; }
        public Section? Section { get; set; }
        public Article? Article { get; set; }
        public Clause? Clause { get; set; }
        public SubClause? SubClause { get; set; }

        // Дополнительные разделы Laws
        public SourceData? SourceData { get; set; }
        public TransitionalProvisions? TransitionalProvision { get; set; }
        public TransitionalProvisionsDateNote? TransitionalProvisionsDateNote { get; set; }
        public SourceDocumentsList? SourceDocument { get; set; }
        public ConstitutionalAmendment? ConstitutionalAmendment { get; set; }

        public object? GetDeepestItem()
        {
            if (SubClause != null) return SubClause;
            if (Clause != null) return Clause;
            if (Article != null) return Article;
            if (Section != null) return Section;
            if (Chapter != null) return Chapter;
            if(SourceData != null) return SourceData;
            if (TransitionalProvision != null) return TransitionalProvision;
            if (SourceDocument != null) return SourceDocument;
            if (ConstitutionalAmendment != null) return ConstitutionalAmendment;
            if (Laws != null) return Laws;
            return null;
        }

        public int GetLevel()
        {
            if (SubClause != null) return 5;
            if (Clause != null) return 4;
            if (Article != null) return 3;
            if (Section != null) return 2;
            if (Chapter != null) return 1;
            if (SourceData != null) return 1;
            if (TransitionalProvision != null) return 2;
            if (SourceDocument != null) return 2;
            if (ConstitutionalAmendment != null) return 2;
            if (Laws != null) return 0;
            return -1;
        }

        public bool IsValid()
        {
            if (Laws != null) return true;
            if (SourceData != null) return true;
            if (SourceData==null && TransitionalProvision != null) return false;
            if (SourceData==null && SourceDocument != null) return true;
            if (SourceData == null && ConstitutionalAmendment != null) return true;
            if (Chapter == null) return false;
            if (Article != null && (Section == null || Chapter == null)) return false;
            if (Clause != null && (Article == null || Section == null || Chapter == null)) return false;
            if (SubClause != null && (Clause == null || Article == null || Section == null || Chapter == null)) return false;
            return true;
        }
    }
}