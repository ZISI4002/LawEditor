using LawEditor.Models.RootClasses;
using LawEditor.Services.WordServises;
using LawEditor.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Word Files (*.doc)|*.doc| (*.docx)|*.docx";

            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                string fullPath = System.IO.Path.GetFullPath(filename);
                _fileName = System.IO.Path.GetFileName(filename);

                try
                {
                    // Проверяем доступность файла
                    using (FileStream stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Файл доступен
                    }

                    _viewModel.IsDisplayLeftVisible = true;
                    _viewModel.IsDisplayButtonsVisible = false;
                    _viewModel.FilePath = fullPath;
                    _viewModel.FileNameLeft = _fileName;

                    if (_fileName.EndsWith(".doc"))
                    {
                        _viewModel.FileNameRight = _fileName.Replace(".doc", ".xml");

                    }
                    else if (_fileName.EndsWith(".docx"))
                    {
                        _viewModel.FileNameRight = _fileName.Replace(".docx", ".xml");
                    }

                    WordFileProsesingService wordService = new WordFileProsesingService();
                    var laws = wordService.ReadWordFile(fullPath);
                    _viewModel.Laws = laws;
                    _viewModel.FileIsAdded = true;
                }
                catch (IOException)
                {
                    MessageBox.Show("Fayl hal-hazırda açıqdır. Zəhmət olmasa faylı bağlayıb yenidən cəhd edin.",
                                  "Fayl açıqdır",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Xəta baş verdi: {ex.Message}",
                                  "Xəta",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
    }
}
