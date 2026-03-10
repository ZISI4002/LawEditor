using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace LawEditor.Model {
    public class Law {
        public string Evvel { get; set; } //тута над модицикатором подумаю
        public List<Bolme> Bolmeler { get; } = new();
        public List<KecidMuddeaları> kecidMuddeaları { get; set; } = new();
        public List<MenbeSenedlerininSiyahisi> menbeSenedlerininSiyahisi { get; set; } = new();
        public List<KonstitusiyayaEdilmisDeyislik> konstitusiyayaEdilmisDeyislik { get; set; } = new();

    }
}
