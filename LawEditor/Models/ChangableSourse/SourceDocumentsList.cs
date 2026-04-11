using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LawEditor.Models.ChangableSourse {
    public class SourceDocumentsList{
        private static int counter = 1;

        public int Id { get; set; }
        public string Title { get; set; }
        public string? LinkText { get; set; } //содержится в тайтл
        public string? Url { get; set; }

        public SourceDocumentsList() { }

        public SourceDocumentsList(string title, string? linkText = null, string? url = null) {
            Id = counter++;
            Title = title;
            LinkText = linkText;
            Url = url;
        }
        public static void DecreaseCounter() {
            if (counter > 1)
                counter--;
        }
        public static void DecreaseCounterToOne()
        {
           while (counter > 1)
                counter--;
        }
    }
}
