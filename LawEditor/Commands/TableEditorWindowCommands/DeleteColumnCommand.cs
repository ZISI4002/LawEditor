// DeleteColumnCommand.cs
using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
    public class DeleteColumnCommand : ICommand
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
            int idx = -1;

            if (parameter is DataGridColumn col)
                idx = col.DisplayIndex;
            else
                idx = _viewModel.Headers.Count - 1; // fallback: последняя колонка

            if (idx < 0 || idx >= _viewModel.Headers.Count) return;
            if (_viewModel.Headers.Count <= 1) return; // не даём удалить последнюю

            _viewModel.Headers.RemoveAt(idx);

            foreach (var row in _viewModel.Rows)
                if (idx < row.Cells.Count)
                    row.Cells.RemoveAt(idx);

            _viewModel.WorkingCopy.Headers = _viewModel.Headers;
            _viewModel.WorkingCopy.Rows = _viewModel.Rows;
        }
    }
}