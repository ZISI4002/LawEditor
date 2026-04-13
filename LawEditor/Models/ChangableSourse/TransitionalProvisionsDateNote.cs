using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Models.ChangableSourse
{
    public class TransitionalProvisionsDateNote
    {
        public string DisplayText
        {
            get => TransitionalProvisions.Date;
            set => TransitionalProvisions.Date = value;
        }
    }
}
