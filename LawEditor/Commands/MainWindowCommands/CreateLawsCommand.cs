using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.ViewModels;
using LawEditor.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            // Create a new law and add it to the collection
            // And add a chapter with a header to the law
            _viewModel.Laws.AddChapter("Bölmə 1");
            Models.ChangableData.Header header1 = new Models.ChangableData.Header 
            { 
                Id = 1,
                FullText= "Bu, konstitusiyanın başlığıdır.",

            };
            ObservableCollection<Models.ChangableData.Header> headers = new ObservableCollection<Models.ChangableData.Header>();
            headers.Add(header1);

            UpperObject upperObject1 = new UpperObject
            {
                Id = 1,
                ObjectName = "Header",
                Headers = headers
            };
            _viewModel.Laws.UpperObjects.Add(upperObject1);

            // Add source data for the law
            ObservableCollection<TransitionalProvisions> transitional = new ObservableCollection<TransitionalProvisions>();
            ObservableCollection<SourceDocumentsList> sources = new ObservableCollection<SourceDocumentsList>();
            ObservableCollection<ConstitutionalAmendment> amendments = new ObservableCollection<ConstitutionalAmendment>();

            _viewModel.Laws.SourceData.Add(new SourceData
            {
                Id = 1,
                Type = "KEÇİD MÜDDƏALARI",
                Source = new ObservableCollection<object>(transitional.Cast<object>())
            });
            

            _viewModel.Laws.SourceData.Add(new SourceData
            {
                Id = 2,
                Type = "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
                Source = new ObservableCollection<object>(sources.Cast<object>())
            });

            _viewModel.Laws.SourceData.Add(new SourceData
            {
                Id = 3,
                Type = "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
                Source = new ObservableCollection<object>(amendments.Cast<object>())
            });
            _viewModel.Laws.SourceData[0].AddTransitionalProvision("Keçid müddəası 1");
            _viewModel.Laws.SourceData[0].Source.Add(new TransitionalProvisionsDateNote());
            _viewModel.Laws.SourceData[1].AddSourceDocument("Mənbə sənədi 1");
            _viewModel.Laws.SourceData[2].AddConstitutionalAmendment("Konstitusiyaya edilmiş dəyişiklik 1");

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

