using LawEditor.ViewModels;
using LawEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.MainWindowCommands
{
    public class CreateLawsCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly MainWindowViewModel _viewModel;

        public CreateLawsCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=> true;
        
        

        public void Execute(object? parameter)
        {
            _viewModel.IsDisplayLeftVisible = true;
            _viewModel.IsDisplayButtonsVisible = false;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DateTime now = DateTime.Now;
            string dateNow = now.ToString("dd-MM-yyyy_HH-mm-ss-fff");
            string fileName = $"Generated_Law_{dateNow}.xml";
            _viewModel.FileNameRight = fileName;
            _viewModel.FileNameLeft = fileName.Replace(".xml", ".doc");
            _viewModel.FilePath = System.IO.Path.Combine(folderPath, fileName);


            _viewModel.Laws.AddChapter("Bölmə 1");
            if (_viewModel.Window is MainWindow mainWindow)
            {
                mainWindow.DisplayLaws(_viewModel.Laws);
            }
            LawEditorWindow editorWindow = new LawEditorWindow();
            LawEditorWindowViewModel editorViewModel = new LawEditorWindowViewModel(editorWindow, _viewModel);
            editorWindow.DataContext = editorViewModel;
            editorWindow.ShowDialog();
            
        }
    }
}

