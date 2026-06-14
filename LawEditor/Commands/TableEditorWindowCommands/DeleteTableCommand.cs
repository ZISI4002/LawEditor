using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
    public class DeleteTableCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public readonly TableEditorViewModel _viewModel;

        public DeleteTableCommand(TableEditorViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=> true;

        public void Execute(object? parameter)
        {
            MessageBoxResult result = MessageBox.Show("Deleting it will remove all other elements relying on it!", "Are you sure?", MessageBoxButton.YesNoCancel, MessageBoxImage
                 .Warning, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes) return;

            _viewModel.ParentViewModel.EditedLaws.DeleteTable(_viewModel.OriginalTable.Id);
            _viewModel.ParentViewModel.RefreshSelectedText();
            _viewModel.Window.Close();
        }
    }
}
