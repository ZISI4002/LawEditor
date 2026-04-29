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
        public static string? Date { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
        public TransitionalProvisions() { }
        public TransitionalProvisions(string title) {
            Title = title;
           
        }
        
        public void UpdateTransitionalProvision(int previousId,int newId) {
            
        }
    }
}
