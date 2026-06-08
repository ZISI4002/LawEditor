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
            _viewModel.Headers.Add($"Column {_viewModel.Headers.Count + 1}");
            foreach (var row in _viewModel.Rows)
                row.Cells.Add(string.Empty);
        }
    }
}
