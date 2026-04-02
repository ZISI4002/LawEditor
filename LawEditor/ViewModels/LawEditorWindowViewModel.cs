using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Models.TreeClasses;
using LawEditor.Services.LawServises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.ViewModels
{
    public class LawEditorWindowViewModel : BaseViewModel
    {
        
        
        public LawEditorWindowViewModel(Window window,MainWindowViewModel mainWindowViewModel) : base(window)
        {
            this.MainWindowModel = mainWindowViewModel;
           Laws= mainWindowViewModel.Laws;

            EditedLaws = new Laws();
            CopyLawsServise.CopyLawsData(Laws, EditedLaws);
            this.SaveCommand = new Commands.LawEditorWindowCommands.SaveCommand(this);
            this.DeleteCommand = new Commands.LawEditorWindowCommands.DeleteCommand(this);
           
            this.AddItemCommand = new Commands.LawEditorWindowCommands.AddItemCommand(this);

        }
        public MainWindowViewModel MainWindowModel { get; set; }

        public Laws Laws { get; set; }
        public Laws EditedLaws { get; set; }
        public LawAnchor CurrentAnchor { get; set; } = new();
        private bool _isMenuOpen;
        public bool IsAddMenuOpen
        {
            get => _isMenuOpen;
            set => Set(ref _isMenuOpen, value);
        }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
       
        public ICommand AddItemCommand { get; set; }
       

        public string MenuItem1Text => GetMenuItems().Item1;
        public string MenuItem2Text => GetMenuItems().Item2;
        public string MenuItem3Text => GetMenuItems().Item3;
        public string MenuItem4Text => GetMenuItems().Item4;
       

        // Видимость кнопок
        public bool MenuItem1Visible => GetMenuItems().Item1 != null;
        public bool MenuItem2Visible => GetMenuItems().Item2 != null;
        public bool MenuItem3Visible => GetMenuItems().Item3 != null;
        public bool MenuItem4Visible => GetMenuItems().Item4 != null;


        private (string Item1, string Item2, string Item3, string Item4) GetMenuItems()
        {
            return _selectedItem switch
            {
                Chapter => ("Yuxarıda bölmə əlavə et", "Aşağıda bölmə əlavə et", "İçəridə fəsil əlavə et",null),
                Section => ("Yuxarıda fəsil əlavə et", "Aşağıda fəsil əlavə et", "İçəridə maddə əlavə et", null),
                Article => ("Yuxarıda maddə əlavə et", "Aşağıda maddə əlavə et", "Hissə əlavə et","İçəridə bənd əlavə et"),
                Clause => ("Yuxarıda bənd əlavə et", "Aşağıda bənd əlavə et", "İçəridə altbənd əlavə et", null),
                SubClause => ("Yuxarıda altbənd əlavə et", "Aşağıda altbənd əlavə et", null, null),
                _ => ("Bölmə əlavə et", null, null,null),
            };
        }

        private object? _selectedItem;
        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                Set(ref _selectedItem, value);
                OnPropertyChanged(nameof(SelectedText));
                OnPropertyChanged(nameof(SelectedTypeName));
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(MenuItem1Text));
                OnPropertyChanged(nameof(MenuItem2Text));
                OnPropertyChanged(nameof(MenuItem3Text));
                OnPropertyChanged(nameof(MenuItem1Visible));
                OnPropertyChanged(nameof(MenuItem2Visible));
                OnPropertyChanged(nameof(MenuItem3Visible));
                UpdateAnchor(value);
            }
        }

        private void UpdateAnchor(object? item)
        {
            CurrentAnchor = new LawAnchor();

            switch (item)
            {
                case Chapter c:
                    CurrentAnchor.Chapter = c;
                    break;

                case Section s:
                    CurrentAnchor.Section = s;
                    CurrentAnchor.Chapter = EditedLaws.Chapters
                        .FirstOrDefault(ch => ch.Sections.Contains(s));
                    break;

                case Article a:
                    foreach (var ch in EditedLaws.Chapters)
                        foreach (var sec in ch.Sections)
                            if (sec.Articles.Contains(a))
                            {
                                CurrentAnchor.Chapter = ch;
                                CurrentAnchor.Section = sec;
                                CurrentAnchor.Article = a;
                            }
                    break;

                case Clause cl:
                    foreach (var ch in EditedLaws.Chapters)
                        foreach (var sec in ch.Sections)
                            foreach (var art in sec.Articles)
                                if (art.Clauses.Contains(cl))
                                {
                                    CurrentAnchor.Chapter = ch;
                                    CurrentAnchor.Section = sec;
                                    CurrentAnchor.Article = art;
                                    CurrentAnchor.Clause = cl;
                                }
                    break;

                case SubClause sc:
                    foreach (var ch in EditedLaws.Chapters)
                        foreach (var sec in ch.Sections)
                            foreach (var art in sec.Articles)
                                foreach (var cl in art.Clauses)
                                    if (cl.SubClauses.Contains(sc))
                                    {
                                        CurrentAnchor.Chapter = ch;
                                        CurrentAnchor.Section = sec;
                                        CurrentAnchor.Article = art;
                                        CurrentAnchor.Clause = cl;
                                        CurrentAnchor.SubClause = sc;
                                    }
                    break;
            }
        }
        public void RefreshSelectedText()
        {
            OnPropertyChanged(nameof(SelectedText));
        }
        public string SelectedText
        {
            get => FullTextWrapper.GetFullText(_selectedItem);
            set => FullTextWrapper.SetText(_selectedItem, value);
        }

        public string SelectedTypeName => _selectedItem switch
        {
            Chapter => "Bölmə (Chapter)",
            Section => "Fəsil (Section)",
            Article => "Maddə (Article)",
            Clause => "Bənd (Clause)",
            SubClause => "Alt Bənd (SubClause)",
            _ => ""
        };

        public bool HasSelection => _selectedItem != null;

        
    }
}
