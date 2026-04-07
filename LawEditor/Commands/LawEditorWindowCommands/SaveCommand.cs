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

            MessageBoxResult result = MessageBox.Show("All your changes will be saved permanently", "Are you sure?", MessageBoxButton.YesNoCancel, MessageBoxImage
                .Warning, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes) return;

            // Очищаем оригинальный Laws
            _viewModel.Laws.Chapters.Clear();
            _viewModel.Laws.SourceData.Clear();
            
          
            // Копируем EditedLaws обратно в Laws
           CopyLawsServise.CopyLawsData(_viewModel.EditedLaws, _viewModel.Laws);

            // Обновляем ссылку в MainWindowModel
            _viewModel.MainWindowModel.Laws = _viewModel.Laws;
            if (_viewModel.MainWindowModel.FileIsAdded)
            {
                var wordWriter = new WordFileWritingService();
                wordWriter.WriteWordFile(_viewModel.MainWindowModel.FilePath, _viewModel.MainWindowModel.Laws);


            }
            if (_viewModel.MainWindowModel.Window is MainWindow mainWindow)
            {
                mainWindow.DisplayLaws(_viewModel.Laws);
                if (_viewModel.MainWindowModel.FileIsAdded)
                {
                    mainWindow.DisplayChangedXML(_viewModel.MainWindowModel);
                }
            }

            // Закрываем окно
            _viewModel.Window.Close();

        }
    }
}
