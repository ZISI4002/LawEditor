using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LawEditor.Models.ChangableSourse {
    public class SourceDocumentsList{
        private static int counter = 1;
        public int Id { get; }
        public string Title { get; protected set; } //protected на случай изменения заголовка в будущем
        public SourceDocumentsList(string title)
        { //автосчетчик
            Id = counter++;
            Title = title;
        }
    }
}
