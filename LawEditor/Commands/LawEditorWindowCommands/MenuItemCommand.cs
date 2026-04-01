using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.LawEditorWindowCommands
{
    public class MenuItemCommand : ICommand
    {
        private readonly LawEditorWindowViewModel _viewModel;

        public MenuItemCommand(LawEditorWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
