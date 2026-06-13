// DeleteRowCommand.cs
using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.Models.SpecialElements;
using LawEditor.ViewModels;
using System;
using System.Windows.Input;

namespace LawEditor.Commands.TableEditorWindowCommands
{
    public class DeleteRowCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public readonly TableEditorViewModel _viewModel;
        public DeleteRowCommand(TableEditorViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is TableRowData row && _viewModel.Rows.Contains(row))
            {
                _viewModel.Rows.Remove(row);
            }
            else if (_viewModel.Rows.Count > 0)
            {
                _viewModel.Rows.RemoveAt(_viewModel.Rows.Count - 1);
            }

            _viewModel.WorkingCopy.Rows = _viewModel.Rows;
        }
    }
}