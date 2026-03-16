using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.DisplayCommand
{
    public class RemoveCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public readonly MainWindowViewModel _viewModel;

        public RemoveCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=> true;

        public void Execute(object? parameter)
        {
                _viewModel.IsDisplayLeftVisible = false;
                _viewModel.IsAddWordVisible = true;
                _viewModel.IsDisplayRightVisible = false;
                _viewModel.FileIsAdded = false;
                _viewModel.FileNameLeft = string.Empty;
                _viewModel.FileNameRight = string.Empty;
                _viewModel.Laws = null;
                _viewModel.XML = null;
        }
    }
}
