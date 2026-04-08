using LawEditor.Models.ChangableData;
using LawEditor.Models.TreeClasses;
using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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


            if (!_viewModel.CurrentAnchor.IsValid()) {

                       MessageBox.Show(
                       "Silmək üçün heç bir element seçilməyib.", 
                       "Məlumat",                               
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );


                return; }


 MessageBoxResult result = MessageBox.Show("Deleting it will remove all other elements relying on it!", "Are you sure?", MessageBoxButton.YesNoCancel, MessageBoxImage
                 .Warning, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes) return;


            var anchor = _viewModel.CurrentAnchor;
            
            if (anchor.Chapter != null && anchor.Section == null)
            {
                _viewModel.EditedLaws.DeleteChapter(anchor.Chapter.Id);
                _viewModel.BuildTreeRoots();
            }
            else if (anchor.Section != null && anchor.Article == null)
            {
                if (anchor.Chapter != null)
                {
                    anchor.Chapter.DeleteSection(anchor.Section.Id);
                }
            }
            else if (anchor.Article != null && anchor.Clause == null)
            {
                if (anchor.Section != null)
                {
                    anchor.Section.DeleteArticle(anchor.Article.Id,_viewModel.EditedLaws);
                }
            }
            else if (anchor.Clause != null && anchor.SubClause == null)
            {
                if (anchor.Article != null)
                {
                    anchor.Article.DeleteClause(anchor.Clause.Number);
                }
            }
            else if (anchor.SubClause != null)
            {
                if (anchor.Clause != null)
                {
                    anchor.Clause.DeleteSubClause(anchor.SubClause.Number);
                }
            }
           
        }
    }
}
