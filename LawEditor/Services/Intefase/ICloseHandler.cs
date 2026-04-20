using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawEditor.Services.Intefase
{
    public interface ICloseHandler
    {
        bool CanClose();
        void OnClosing();
    }
}
