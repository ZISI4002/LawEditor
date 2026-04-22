using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.MainWindowCommands
{
    public class RemoveCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public readonly MainWindowViewModel _viewModel;

        public RemoveCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter)=> true;

        public void Execute(object? parameter)
        {

            // Очищаем оригинальный Laws
            _viewModel.Laws.ResetCounter();
            _viewModel.Laws.Chapters.Clear();
            _viewModel.Laws.SourcesData.Clear();
            SourceDocumentsList.DecreaseCounterToOne();
            TransitionalProvisions.Date = string.Empty;
            TransitionalProvisions.Date=DateOnly.FromDateTime(DateTime.Now).ToString("dd.MM.yyyy");
            // Сбрасываем все свойства, связанные с отображением и данными
            _viewModel.IsDisplayLeftVisible = false;
                _viewModel.IsDisplayButtonsVisible = true;
                _viewModel.IsDisplayRightVisible = false;
                _viewModel.FileIsAdded = false;
            _viewModel.XMLIsGenerated = false;
            _viewModel.FileNameLeft = string.Empty;
                _viewModel.FileNameRight = string.Empty;
                _viewModel.Laws = new Laws() ;
                _viewModel.XML = null;
        }
    }
}
