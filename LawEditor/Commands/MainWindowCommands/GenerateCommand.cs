using LawEditor.Services.XMLServises;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public bool CanExecute(object? parameter) => true;

        public async void Execute(object? parameter)
        {
            if (_viewModel.FileIsAdded == true ||
                (_viewModel.Laws.Chapters.Count > 0 &&
                 _viewModel.Laws.UpperObjects.Count > 0 &&
                 _viewModel.Laws.SourcesData.Count > 0))
            {
                _viewModel.IsDisplayRightVisible = true;
                _viewModel.IsLoading = true; // ← В UI-потоке, ДО Task.Run

                try
                {
                    var result = await Task.Run(() =>
                    {
                        // IsLoading = true отсюда УДАЛЕНО
                        XMLTranslatorServise xmlTranslator = new XMLTranslatorServise();
                        XmlFileProcessingService xmlFileProcessingService = new XmlFileProcessingService();

                        string folderPath = System.IO.Path.GetDirectoryName(_viewModel.FilePath);
                        string fileName = _viewModel.FileNameRight;
                        xmlTranslator.Translate(_viewModel.Laws, folderPath, fileName);
                        var xmlLaw = xmlFileProcessingService.ReadXmlFile(Path.Combine(folderPath, "CreatedXML"), fileName);

                        return xmlLaw;
                    });

                    await Task.Delay(4000); // ← Не .Wait() — UI не блокируется

                    _viewModel.XML = result;
                    _viewModel.XMLIsGenerated = true;
                }
                finally
                {
                    _viewModel.IsLoading = false; // ← После await, в UI-потоке
                }
            }
                else
              {
                _viewModel.IsDisplayRightVisible = false;
                MessageBox.Show("Fayl əlavə edilməyib", "Xəbərdarlıq",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
              }
        }
    }
 }
