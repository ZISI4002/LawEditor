using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.Models.SpecialElements;
using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _viewModel.Rows.Add(row);
        }
    
    }
}
