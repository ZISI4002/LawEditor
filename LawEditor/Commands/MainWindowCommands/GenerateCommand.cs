using LawEditor.Services.XMLSErvises;
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

        public async void Execute(object? parameter)
{
    if (_viewModel.FileIsAdded == true || _viewModel.Laws != null)
    {
        _viewModel.IsDisplayRightVisible = true;
        _viewModel.IsLoading = true;

        try
        {
            var result = await Task.Run(() =>
            {
                XMLTranslatorServise xmlTranslator = new XMLTranslatorServise();
                XmlFileProcessingService xmlFileProcessingService = new XmlFileProcessingService();

                string folderPath = System.IO.Path.GetDirectoryName(_viewModel.FilePath);
                string fileName = _viewModel.FileNameRight;
                Task.Delay(10000).Wait(); 
                xmlTranslator.Translate(_viewModel.Laws, folderPath, fileName);
                var xmlLaw = xmlFileProcessingService.ReadXmlFile(folderPath, fileName);

                return xmlLaw;
            });

            _viewModel.XML = result;
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }
    else
    {
        _viewModel.IsDisplayRightVisible = false;

        WarningException warning = new WarningException("Fayl əlavə edilməyib");
        MessageBox.Show(warning.Message, "Xəbərdarlıq",
            MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
    }
}
