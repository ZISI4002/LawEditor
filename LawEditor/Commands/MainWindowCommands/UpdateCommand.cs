using LawEditor.ViewModels;
using LawEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.Commands.MainWindowCommands
{
    public class UpdateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly MainWindowViewModel _viewModel;

        public UpdateCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=> true;
        public void Execute(object? parameter)
        {
            LawEditorWindow editorWindow = new LawEditorWindow();
            LawEditorWindowViewModel editorViewModel = new LawEditorWindowViewModel(editorWindow,_viewModel);
            editorWindow.DataContext = editorViewModel;
            editorWindow.ShowDialog();
        }
    }
}
