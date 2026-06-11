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
   public class DeleteRowCommand: ICommand
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
            if (parameter is TableRowData row)
            {
                _viewModel.Rows.Remove(row);
                _viewModel.WorkingCopy.Rows.Remove(row);
                _viewModel.Headers.RemoveAt(_viewModel.Headers.Count - 1);
                _viewModel.WorkingCopy.Headers.RemoveAt(_viewModel.WorkingCopy.Headers.Count - 1);
            }
        }
    
    }
}
