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
        public string GetLastUpIntId(List<ConstitutionalAmendment> amendmentsList, ConstitutionalAmendment amendment)
        {
            int startIndex = amendmentsList.FindIndex(a => a.Id == amendment.Id);
            
            for (int i = startIndex; i < amendmentsList.Count; i++)
            {
                if (int.TryParse(amendmentsList[i].Id, out int num))
                    return amendmentsList[i].Id;
            }

            return "1";
        }
        public string GetLastDownIntId(List<ConstitutionalAmendment> amendmentsList, ConstitutionalAmendment amendment)
        {
            int startIndex = amendmentsList.FindIndex(a => a.Id == amendment.Id);
            for (int i = startIndex + 1; i < amendmentsList.Count; i++)
            {
                if (int.TryParse(amendmentsList[i].Id, out int num))
                    return amendmentsList[i].Id;
            }

            return "2";
        }


        public int GetPositionInConstitutionalAmendmentsList(List<ConstitutionalAmendment> amendmentsList) {
            return amendmentsList.FindIndex(a => a.Id == Id);
        }
    }
}
