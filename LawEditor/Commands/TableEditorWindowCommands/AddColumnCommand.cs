// AddColumnCommand.cs
using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
    public class AddColumnCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly TableEditorViewModel _viewModel;

        public AddColumnCommand(TableEditorViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            int insertIndex = _viewModel.Headers.Count;

            if (parameter is DataGridColumn col)
                insertIndex = Math.Min(col.DisplayIndex + 1, _viewModel.Headers.Count);

            _viewModel.Headers.Insert(insertIndex, $"Column {_viewModel.Headers.Count + 1}");

            foreach (var row in _viewModel.Rows)
                row.Cells.Insert(insertIndex, string.Empty);

            _viewModel.WorkingCopy.Headers = _viewModel.Headers;
            _viewModel.WorkingCopy.Rows = _viewModel.Rows;
        }
    }
}