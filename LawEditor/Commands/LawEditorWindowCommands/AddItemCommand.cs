using DocumentFormat.OpenXml.Office2010.Excel;
using LawEditor.Models.ChangableSourse;
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
                // Bölüm əlavə etməklə bağlı əmrlər
                case "Yuxarıda bölmə əlavə et":

                    _viewModel.EditedLaws.AddChapter("Yeni Bölmə",anchor.Chapter.Id-1);
                    _viewModel.BuildTreeRoots();

                    break;
                case "Aşağıda bölmə əlavə et":

                        _viewModel.EditedLaws.AddChapter("Yeni Bölmə", anchor.Chapter.Id);
                    _viewModel.BuildTreeRoots();
                    break;
                // Fəsil əlavə etməklə bağlı əmrlər
                case "İçəridə fəsil əlavə et":
                    anchor.Chapter.AddSection("Yeni Fəsil");
                    break;
                case "Yuxarıda fəsil əlavə et":
                    anchor.Chapter.AddSection("Yeni Fəsil",anchor.Section.Id-1);
                    break;
                case "Aşağıda fəsil əlavə et":
                    anchor.Chapter.AddSection("Yeni Fəsil", anchor.Section.Id );
                    break;
                // Maddə əlavə etməklə bağlı əmrlər
                case "İçəridə maddə əlavə et":
                    anchor.Section.AddArticle("Yeni Maddə",_viewModel.EditedLaws);

                    break;

                case "Yuxarıda maddə əlavə et":


                    anchor.Section.AddArticle((anchor.Article.Id)-((anchor.Article.Id)%1),"Yeni Maddə", _viewModel.EditedLaws);

                    break;
                case "Aşağıda maddə əlavə et":
                    anchor.Section.AddArticle((anchor.Article.Id) - ((anchor.Article.Id) % 1)+1, "Yeni Maddə", _viewModel.EditedLaws);
                    break;
                // Hissə əlavə etməklə bağlı əmrlər
                case "Yuxarıda hissə əlavə et":
                    if(anchor.Article.Id % 1 != 0) { 
                    anchor.Section.AddArticle(anchor.Article.Id, "Yeni Maddə", _viewModel.EditedLaws);
                    }
                    else
                    {
                        decimal newUpPartID = anchor.Article.Id + 0.1m;
                        
                        anchor.Section.AddArticle(newUpPartID, "Yeni Maddə", _viewModel.EditedLaws);
                    }
                        break;
                case "Aşağıda hissə əlavə et":
                   
                        decimal newBelowPartID = anchor.Article.Id + 0.1m;
                        anchor.Section.AddArticle(newBelowPartID, "Yeni Maddə", _viewModel.EditedLaws);
                    break;

                case "Hissə əlavə et":
                    decimal cout = anchor.Section.GetMaxPartID(anchor.Article.Id)+0.1m;
                   
                    anchor.Section.AddArticle(cout, "Yeni Maddə", _viewModel.EditedLaws);

                    break;
                // Bənd əlavə etməklə bağlı əmrlər
                case "İçəridə bənd əlavə et":
                    anchor.Article.AddClause("Yeni Bənd");
                    break;
                case "Yuxarıda bənd əlavə et":
                        anchor.Article.AddClause("Yeni Bənd",anchor.Clause.Number-1);
                    break;
                case "Aşağıda bənd əlavə et":
                        anchor.Article.AddClause("Yeni Bənd", anchor.Clause.Number);
                    break;
                // Altbənd əlavə etməklə bağlı əmrlər
                case "İçəridə altbənd əlavə et":
                    anchor.Clause.AddSubClause("Yeni Altbənd");
                    break;
                case "Yuxarıda altbənd əlavə et":
                    anchor.Clause.AddSubClause("Yeni Altbənd", anchor.SubClause.Number - 1);
                    break;
                case "Aşağıda altbənd əlavə et":
                    anchor.Clause.AddSubClause("Yeni Altbənd", anchor.SubClause.Number);
                    break;
                // Keçid müddəası əlavə etməklə bağlı əmrlər
                case "İçəridə yeni Keçid Müddəası əlavə elə":
                    anchor.SourceData.AddTransitionalProvision("Yeni Keçid Müddəası");
                    break;
                case "Yuxarıda yeni Keçid Müddəası əlavə elə":
                    anchor.SourceData.AddTransitionalProvision("Yeni Keçid Müddəası", position: anchor.SourceData.Source.IndexOf(anchor.TransitionalProvision));
                    break;
                    case "Aşağıda yeni Keçid Müddəası əlavə elə":
                    anchor.SourceData.AddTransitionalProvision("Yeni Keçid Müddəası", position: anchor.SourceData.Source.IndexOf(anchor.TransitionalProvision)+1);
                        break;
                // Yeni Konstitusiya dəyişikliyi yada əlavəsi əlavə etməklə bağlı əmrlər
                case "İçəridə yeni Konstitusiyaya edilmiş dəyişiklik yada əlavəni əlavə elə":
                    anchor.SourceData.AddConstitutionalAmendment("Yeni Konstitusiya Dəyişikliyi",linkText:"New Link Text",url: "https://*");
                    break;
                case "Yuxarıda yeni Konstitusiyaya edilmiş dəyişiklik yada əlavəni əlavə elə":
                    {
                        var list = anchor.SourceData.Source.Cast<ConstitutionalAmendment>().ToList();
                        int pos = anchor.ConstitutionalAmendment.GetPositionInConstitutionalAmendmentsList(list);
                        string lastUpId = anchor.ConstitutionalAmendment.GetLastUpIntId(list, anchor.ConstitutionalAmendment);
                        anchor.SourceData.AddConstitutionalAmendment(
                            "Yeni Konstitusiya Dəyişikliyi",
                            id: lastUpId,
                            position: pos,
                             linkText: "New Link Text", url: "https://*");
                        break;
                    }

                case "Aşağıda yeni Konstitusiyaya edilmiş dəyişiklik yada əlavəni əlavə elə":
                    {
                        var list1 = anchor.SourceData.Source.Cast<ConstitutionalAmendment>().ToList();
                        int pos = anchor.ConstitutionalAmendment.GetPositionInConstitutionalAmendmentsList(list1);
                       string firstDownId = anchor.ConstitutionalAmendment.GetLastDownIntId(list1, anchor.ConstitutionalAmendment);
                        anchor.SourceData.AddConstitutionalAmendment(
                            "Yeni Konstitusiya Dəyişikliyi",
                            id: firstDownId,
                            position: pos + 1, linkText: "New Link Text", url: "https://*");
                        break;
                    }
                // Yeni İstifadə olunmuş mənbə əlavə etməklə bağlı əmrlər
                case "İçəridə yeni İstifadə olunmuş mənbə əlavə elə":
                    anchor.SourceData.AddSourceDocument("Yeni İstifadə Olunmuş Mənbə");
                    break;
                    case "Yuxarıda yeni İstifadə olunmuş mənbə əlavə elə":
                    anchor.SourceData.AddSourceDocument("Yeni İstifadə Olunmuş Mənbə", position: anchor.SourceData.Source.IndexOf(anchor.SourceDocument));
                        break;
                    case "Aşağıda yeni İstifadə olunmuş mənbə əlavə elə":
                    anchor.SourceData.AddSourceDocument("Yeni İstifadə Olunmuş Mənbə", position: anchor.SourceData.Source.IndexOf(anchor.SourceDocument)+1);
                    break;
                // Yeni qanun əlavə etməklə bağlı əmrlər
                case "Bölmə əlavə et":
                        _viewModel.EditedLaws.AddChapter("Yeni Bölmə");
                    _viewModel.BuildTreeRoots();
                    break;


            }
           _viewModel.HasUnsavedChanges = true;
            _viewModel.RefreshSelectedText();

        }
    }
}
