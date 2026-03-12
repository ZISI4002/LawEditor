using LawEditor.Services;
using LawEditor.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.MainWindowCommands
{
    public class AddWordCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly MainWindowViewModel _viewModel;
        public static string _fileName;
        public AddWordCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _viewModel.IsDisplayLeftVisible= true;
           


            // Open a file dialog to select a Word document
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Word Files (*.docx)|*.docx| (*.doc)|*.doc" ;
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                string fullPath = System.IO.Path.GetFullPath(filename);
                _fileName = System.IO.Path.GetFileName(filename);
                // Set the file name in the view model
                _viewModel.FileNameLeft = _fileName;
                if (_fileName.EndsWith(".docx"))
                    {
                    _viewModel.FileNameRight = _fileName.Replace(".docx", ".xml");
                }
                else if (_fileName.EndsWith(".doc"))
                {
                    _viewModel.FileNameRight = _fileName.Replace(".doc", ".xml");
                }
                //  Logic to read the Word file and process it 
                WordFileProsesingService wordService = new WordFileProsesingService();
                var laws = wordService.ReadWordFile(fullPath);
                _viewModel.Laws = laws;


            }
        }
    }
}
