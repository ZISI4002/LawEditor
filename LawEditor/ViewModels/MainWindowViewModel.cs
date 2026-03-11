using LawEditor.Commands.MainWindowCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel(Window window) : base(window)
        {
            this.AddWordCommand = new AddWordCommand(this);
        }

       public ICommand AddWordCommand { get; set; }
        private string _fileNameLeft;
        public string FileNameLeft
        {
            get => _fileNameLeft;
            set => Set(ref _fileNameLeft, value);
        }

        private string _fileNameRight;
        public string FileNameRight
        {
            get => _fileNameRight;
            set => Set(ref _fileNameRight, value);
        }
    }
}
