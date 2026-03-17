using LawEditor.Models.ChangableData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.TreeClasses
{
    public class LawAnchor
    {
        public Chapter? Chapter { get; set; }
        public Section? Section { get; set; }
        public Article? Article { get; set; }
        public Clause? Clause { get; set; }
        public SubClause? SubClause { get; set; }
    }
}
