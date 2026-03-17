using LawEditor.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.Commands.MainWindowCommands
{
    public class GenerateCommand : ICommand
    {
        private readonly MainWindowViewModel _viewModel;

        public GenerateCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)=> true;

        public void Execute(object? parameter)
        {

           
            if(_viewModel.FileIsAdded == true|| _viewModel.Laws != null)
            {
                
                _viewModel.IsDisplayRightVisible = true;
             XMLTranslatorServise xmlTranslator = new XMLTranslatorServise();
                LawXmlImportService lawXmlImportService = new LawXmlImportService();
                string folderPath = System.IO.Path.GetDirectoryName(_viewModel.FilePath);
            string fileName = _viewModel.FileNameRight;
            xmlTranslator.Translate(_viewModel.Laws, folderPath, fileName);
              var lawXml=  lawXmlImportService.ImportFromXml(folderPath,fileName);
                _viewModel.XML= lawXml;
            }
            else {
                _viewModel.IsDisplayRightVisible = false;
                WarningException warning = new WarningException("Fayl əlavə edilməyib");
                MessageBox.Show(warning.Message, "Xəbərdarlıq", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
       
        
        }
    }
}
