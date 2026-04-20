using LawEditor.Models.RootClasses;
using LawEditor.Services;
using LawEditor.Services.LawServises;
using LawEditor.Services.WordServises;
using LawEditor.ViewModels;
using LawEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.Commands.LawEditorWindowCommands
{
    public class SaveCommand : ICommand
    {
        private readonly LawEditorWindowViewModel _viewModel;

        public SaveCommand(LawEditorWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)=> true;

        public void Execute(object? parameter)
        {
            if (_viewModel.IsMesageSaving==false && _viewModel.HasUnsavedChanges==true)
            {
                MessageBoxResult result = MessageBox.Show("All your changes will be saved permanently", "Are you sure?", MessageBoxButton.YesNoCancel, MessageBoxImage
                    .Warning, MessageBoxResult.No);

                if (result != MessageBoxResult.Yes) return;

            }
            if(_viewModel.HasUnsavedChanges==false)
            {
                MessageBox.Show("There are no changes to save.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                // Очищаем оригинальный Laws
                _viewModel.Laws.Chapters.Clear();
                _viewModel.Laws.SourcesData.Clear();
                _viewModel.Laws.UpperObjects.Clear();
                _viewModel.HasUnsavedChanges = false;
                

                // Копируем EditedLaws обратно в Laws
                CopyLawsServise.CopyLawsData(_viewModel.EditedLaws, _viewModel.Laws);

                // Обновляем ссылку в MainWindowModel
                _viewModel.MainWindowModel.Laws = _viewModel.Laws;
                if (_viewModel.MainWindowModel.FileIsAdded)
                {
                    var wordWriter = new WordFileWritingService();
                    var dateTime = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
                    var fileNameExtension= System.IO.Path.GetExtension(_viewModel.MainWindowModel.FilePath);
                    var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(_viewModel.MainWindowModel.FilePath);
                    var directory = System.IO.Path.GetDirectoryName(_viewModel.MainWindowModel.FilePath);
                    var clonpath = System.IO.Path.Combine(directory, fileNameWithoutExtension + "_editedIn_" + dateTime + fileNameExtension);
                    wordWriter.WriteWordFile(clonpath, _viewModel.MainWindowModel.Laws);


                }
                if (_viewModel.MainWindowModel.Window is MainWindow mainWindow)
                {
                    mainWindow.DisplayLaws(_viewModel.Laws);
                    if (_viewModel.MainWindowModel.FileIsAdded)
                    {
                        mainWindow.DisplayChangedXML(_viewModel.MainWindowModel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Закрываем окно
            if (_viewModel.IsMesageSaving == false)
            { 
                _viewModel.Window.Close();
            }


            _viewModel.IsMesageSaving = false;
        }
    }
}
