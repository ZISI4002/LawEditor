using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.LawEditorWindowCommands
{
    public class AddCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly LawEditorWindowViewModel _viewModel;

        public AddCommand(LawEditorWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=>true; 


        public void Execute(object? parameter)
        {
               _viewModel.IsAddMenuOpen = true;
        }
    }
}
