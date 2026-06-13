using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
    public class SaveTableCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public readonly TableEditorViewModel _viewModel;

        public SaveTableCommand(TableEditorViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=>true;


        public void Execute(object? parameter)
        {

            _viewModel.WorkingCopy.Title = _viewModel.Title;
            _viewModel.OriginalTable.ApplyFrom(_viewModel.WorkingCopy);
            _viewModel.ParentViewModel.HasUnsavedChanges = true;
            _viewModel.Window.Close();

        }
    }
}
