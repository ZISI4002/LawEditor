using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.LawEditorWindowCommands
{
    public class DeleteCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private LawEditorWindowViewModel _viewModel;

        public DeleteCommand(LawEditorWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            int Level = _viewModel.CurrentAnchor.GetLevel();
            if (Level > 0)
            {
                switch (Level)
                {
                    case 1:
                        _viewModel.CurrentAnchor.Chapter = null;
                        break;
                    case 2:
                        _viewModel.CurrentAnchor.Section = null;
                        break;
                    case 3:
                        _viewModel.CurrentAnchor.Article = null;
                        break;
                    case 4:
                        _viewModel.CurrentAnchor.Clause = null;
                        break;
                    case 5:
                        _viewModel.CurrentAnchor.SubClause = null;
                        break;

                }
            }
        }
    }
}
