// AddRowCommand.cs
using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.Models.SpecialElements;
using LawEditor.ViewModels;
using System;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
    public class AddRowCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public readonly TableEditorViewModel _viewModel;
        public AddRowCommand(TableEditorViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            var row = new TableRowData();
            for (int i = 0; i < _viewModel.Headers.Count; i++)
                row.Cells.Add(string.Empty);

            if (parameter is TableRowData selected)
            {
                int index = _viewModel.Rows.IndexOf(selected);
                _viewModel.Rows.Insert(index + 1, row);
            }
            else
            {
                _viewModel.Rows.Add(row);
            }

            _viewModel.WorkingCopy.Rows = _viewModel.Rows;
        }
    }
}