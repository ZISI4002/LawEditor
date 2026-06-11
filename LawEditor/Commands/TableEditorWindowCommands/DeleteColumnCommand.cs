using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
   public class DeleteColumnCommand: ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly TableEditorViewModel _viewModel;
        public DeleteColumnCommand(TableEditorViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            if (_viewModel.SelectedColumnIndex < 0 || _viewModel.SelectedColumnIndex >= _viewModel.Headers.Count) return;

            int idx = _viewModel.SelectedColumnIndex;
            _viewModel.Headers.RemoveAt(idx);

            foreach (var row in _viewModel.Rows)
                if (idx < row.Cells.Count)
                    row.Cells.RemoveAt(idx);

            _viewModel.SelectedColumnIndex = -1;
            _viewModel.WorkingCopy.Headers = _viewModel.Headers;
            _viewModel.WorkingCopy.Rows = _viewModel.Rows;
        }
    }
}
