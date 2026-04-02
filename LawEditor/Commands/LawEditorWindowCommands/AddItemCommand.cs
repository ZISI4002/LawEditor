using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LawEditor.Commands.LawEditorWindowCommands
{
    public class AddItemCommand : ICommand
    {
        private readonly LawEditorWindowViewModel _viewModel;

        public AddItemCommand(LawEditorWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            _viewModel.IsAddMenuOpen=false;
            var anchor = _viewModel.CurrentAnchor;
            if (parameter is not string menuText) return;

            switch (menuText)
            {
                case "Yuxarıda bölmə əlavə et":

                    _viewModel.EditedLaws.AddChapter("Yeni Bölmə",anchor.Chapter.Id-1);
                    
                    break;
                case "Aşağıda bölmə əlavə et":

                        _viewModel.EditedLaws.AddChapter("Yeni Bölmə", anchor.Chapter.Id);
                    break;

                case "İçəridə fəsil əlavə et":
                    anchor.Chapter.AddSection("Yeni Fəsil");
                    break;
                case "Yuxarıda fəsil əlavə et":
                    anchor.Chapter.AddSection("Yeni Fəsil",anchor.Section.Id-1);
                    break;
                case "Aşağıda fəsil əlavə et":
                    anchor.Chapter.AddSection("Yeni Fəsil", anchor.Section.Id );
                    break;
                case "İçəridə maddə əlavə et":
                    var lastArticle = anchor.Section?.Articles.LastOrDefault();
                    float lastArticleId = lastArticle != null ? lastArticle.Id : 0;
                    anchor.Section.AddArticle(lastArticleId-lastArticleId%1+1, "Yeni Maddə");
                    
                    break;

                case "Yuxarıda maddə əlavə et":
                    break;
                case "Aşağıda maddə əlavə et":
                    break;
                case "Hissə əlavə et":
                    break;
                case "İçəridə bənd əlavə et":
                    break;
                case "Yuxarıda bənd əlavə et":
                    break;
                case "Aşağıda bənd əlavə et":
                    break;
                case "İçəridə altbənd əlavə et":
                    break;
                case "Yuxarıda altbənd əlavə et":
                    break;
                case "Aşağıda altbənd əlavə et":
                    break;
                case "Bölmə əlavə et":
                        _viewModel.EditedLaws.AddChapter("Yeni Bölmə");
                    break;


            }

            _viewModel.RefreshSelectedText();

        }
    }
}
