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
            _viewModel.Laws.AddChapter("Birinci Bölmə");
            LawEditorWindow editorWindow = new LawEditorWindow();
            LawEditorWindowViewModel editorViewModel = new LawEditorWindowViewModel(editorWindow, _viewModel);
            editorWindow.DataContext = editorViewModel;
            editorWindow.ShowDialog();
            
        }
    }
}
