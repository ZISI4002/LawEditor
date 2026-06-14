using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Models.TreeClasses;
using LawEditor.Services.Intefase;
using LawEditor.Services.LawServises;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace LawEditor.ViewModels
{
    public class LawEditorWindowViewModel : BaseViewModel, ICloseHandler
    {
        public LawEditorWindowViewModel(Window window, MainWindowViewModel mainWindowViewModel) : base(window)
        {
            this.MainWindowModel = mainWindowViewModel;
            this.Laws = mainWindowViewModel.Laws;

            EditedLaws = new Laws();
            if (EditedLaws.UpperObjects == null)
                EditedLaws.UpperObjects = new ObservableCollection<UpperObject>();

            CopyLawsServise.CopyLawsData(Laws, EditedLaws);

            BuildTreeRoots();

            this.SaveCommand = new Commands.LawEditorWindowCommands.SaveCommand(this);
            this.DeleteCommand = new Commands.LawEditorWindowCommands.DeleteCommand(this);
            this.AddItemCommand = new Commands.LawEditorWindowCommands.AddItemCommand(this);
        }

        public Action<object?>? OnSelectedItemChanged { get; set; }

        public MainWindowViewModel MainWindowModel { get; set; }
        public Laws Laws { get; set; }
        public Laws EditedLaws { get; set; }
        public LawAnchor CurrentAnchor { get; set; } = new();

        public ObservableCollection<object> TreeRoots { get; } = new();

        public void BuildTreeRoots()
        {
            TreeRoots.Clear();

            foreach (var upperObj in EditedLaws.UpperObjects)
                TreeRoots.Add(upperObj);

            foreach (var ch in EditedLaws.Chapters)
                TreeRoots.Add(ch);

            foreach (var container in EditedLaws.SourcesData)
                TreeRoots.Add(container);
        }

        private bool _isMenuOpen;
        public bool IsAddMenuOpen
        {
            get => _isMenuOpen;
            set => Set(ref _isMenuOpen, value);
        }

        public bool HasUnsavedChanges { get; set; } = false;
        public bool IsMesageSaving { get; set; } = false;

        public bool CanClose()
        {
            if (!HasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "Du you want to save changes?",
                "LawEditor",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.Yes)
            {
                IsMesageSaving = true;
                SaveCommand.Execute(null);
            }

            return true;
        }

        public void OnClosing() { }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddItemCommand { get; set; }

        public string MenuItem1Text => GetMenuItems().Item1;
        public string MenuItem2Text => GetMenuItems().Item2;
        public string MenuItem3Text => GetMenuItems().Item3;
        public string MenuItem4Text => GetMenuItems().Item4;
        public string MenuItem5Text => GetMenuItems().Item5;
        public string MenuItem6Text => GetMenuItems().Item6;
        public string MenuItem7Text => GetMenuItems().Item7;
        public bool MenuItem1Visible => GetMenuItems().Item1 != null;
        public bool MenuItem2Visible => GetMenuItems().Item2 != null;
        public bool MenuItem3Visible => GetMenuItems().Item3 != null;
        public bool MenuItem4Visible => GetMenuItems().Item4 != null;
        public bool MenuItem5Visible => GetMenuItems().Item5 != null;
        public bool MenuItem6Visible => GetMenuItems().Item6 != null;
        public bool MenuItem7Visible => GetMenuItems().Item7 != null;

        private (string Item1, string Item2, string Item3, string Item4, string Item5, string Item6, string Item7) GetMenuItems()
        {
            return _selectedItem switch
            {
                Chapter => ("Yuxarıda bölmə əlavə et", "Aşağıda bölmə əlavə et", "İçəridə fəsil əlavə et", "Bölməyə cədvəl əlavə edin", null, null, null),
                Section => ("Yuxarıda fəsil əlavə et", "Aşağıda fəsil əlavə et", "İçəridə maddə əlavə et", "Fəsilə cədvəl əlavə edin", null, null, null),
                Article => ("Yuxarıda maddə əlavə et", "Aşağıda maddə əlavə et", "Hissə əlavə et", "İçəridə bənd əlavə et", "Yuxarıda hissə əlavə et", "Aşağıda hissə əlavə et","Maddəyə cədvəl əlavə edin"),
                Clause => ("Yuxarıda bənd әlavә et", "Aşağıda bәnd әlavә et", "İçәridә altbәnd әlavә et", "Bəndə cədvəl əlavə edin", null, null, null),
                SubClause => ("Yuxarıda altbәnd әlavә et", "Aşağıda altbәnd әlavә et", "Altbəndə cədvəl əlavə edin", null, null, null, null),
                SourceData sr when sr.Type == "KEÇİD MÜDDƏALARI" => ("İçәridә yeni Keçid Müddәası әlavә elә",null, null, null, null, null, null),
                SourceData sr when sr.Type == "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI" => ("İçәridә yeni İstifadе olunmuş mӘnbе әlavе elе",null, null, null, null, null, null),
                SourceData sr when sr.Type == "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI" => ("İçəridə yeni Konstitusiyaya edilmiş dəyişiklik yada əlavəni əlavə elə", null, null, null, null, null, null),
                TransitionalProvisions => ("Yuxarıda yeni Keçid Müddəası əlavə elə", "Aşağıda yeni Keçid Müddəası əlavə elə", null, null, null, null, null),
                TransitionalProvisionsDateNote => (null, null, null, null, null, null, null),
                SourceDocumentsList => ("Yuxarıda yeni İstifadə olunmuş mənbə əlavə elə", "Aşağıda yeni İstifadə olunmuş mənbə əlavə elə", null, null, null, null, null),
                ConstitutionalAmendment => ("Yuxarıda yeni Konstitusiyaya edilmiş dəyişiklik yada əlavəni əlavə elə", "Aşağıda yeni Konstitusiyaya edilmiş dəyişiklik yada əlavəni әlavә elә", null, null, null, null, null),
                Models.RootClasses.Laws => ("Bölmә әlavә et", null, null, null, null, null, null),
                _ => ("Bölmә әlavә et", null, null, null, null, null, null),
            };
        }

        private object? _selectedItem;
        private string _selectedText = "";

        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                Set(ref _selectedItem, value);
                _selectedText = FullTextWrapper.GetFullText(value);
                OnSelectedItemChanged?.Invoke(value);
                OnPropertyChanged(nameof(SelectedTypeName));
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(MenuItem1Text));
                OnPropertyChanged(nameof(MenuItem2Text));
                OnPropertyChanged(nameof(MenuItem3Text));
                OnPropertyChanged(nameof(MenuItem4Text));
                OnPropertyChanged(nameof(MenuItem5Text));
                OnPropertyChanged(nameof(MenuItem6Text));
                OnPropertyChanged(nameof(MenuItem7Text));
                OnPropertyChanged(nameof(MenuItem1Visible));
                OnPropertyChanged(nameof(MenuItem2Visible));
                OnPropertyChanged(nameof(MenuItem3Visible));
                OnPropertyChanged(nameof(MenuItem4Visible));
                OnPropertyChanged(nameof(MenuItem5Visible));
                OnPropertyChanged(nameof(MenuItem6Visible));
                OnPropertyChanged(nameof(MenuItem7Visible));
                UpdateAnchor(value);
            }
        }

        public string SelectedText
        {
            get => _selectedText;
            set
            {
                if (_selectedText == value) return;
                _selectedText = value;
                FullTextWrapper.SetText(_selectedItem, value, EditedLaws);
                HasUnsavedChanges = true;
                if (FullTextWrapper.CanRefresh)
                {
                    RefreshSelectedText();
                    FullTextWrapper.CanRefresh = false;
                }
            }
        }

        public void RefreshSelectedText()
        {
            _selectedText = FullTextWrapper.GetFullText(_selectedItem);
            OnSelectedItemChanged?.Invoke(_selectedItem);
        }

        private void UpdateAnchor(object? item)
        {
            CurrentAnchor = new LawAnchor();

            switch (item)
            {
                case UpperObject uo:
                    CurrentAnchor.UpperObject = uo;
                    break;

                case Header h:
                    var upperObj = EditedLaws.UpperObjects.FirstOrDefault(u => u.Headers.Contains(h));
                    if (upperObj != null)
                        CurrentAnchor.UpperObject = upperObj;
                    break;

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

                case SourceData sd:
                    CurrentAnchor.SourceData = sd;
                    break;

                case TransitionalProvisions tp:
                    CurrentAnchor.TransitionalProvision = tp;
                    CurrentAnchor.SourceData = EditedLaws.SourcesData.FirstOrDefault(s => s.Source.Contains(tp));
                    break;

                case SourceDocumentsList sd:
                    CurrentAnchor.SourceDocument = sd;
                    CurrentAnchor.SourceData = EditedLaws.SourcesData.FirstOrDefault(s => s.Source.Contains(sd));
                    break;

                case ConstitutionalAmendment ca:
                    CurrentAnchor.ConstitutionalAmendment = ca;
                    CurrentAnchor.SourceData = EditedLaws.SourcesData.FirstOrDefault(s => s.Source.Contains(ca));
                    break;
            }
        }

        public string SelectedTypeName => _selectedItem switch
        {
            UpperObject => "Qanunun başlığı (Header)",
            Header => "Başlıq mətn (Header Text)",
            Chapter => "Bölmə (Chapter)",
            Section => "Fəsil (Section)",
            Article => "Maddə (Article)",
            Clause => "Bənd (Clause)",
            SubClause => "Alt Bənd (SubClause)",
            TransitionalProvisions => "KEÇİD MÜDDƏALARI",
            SourceDocumentsList => "İSTİFADƏ OLUNMUŞ MƏNBƏ SƏNƏDLƏRİNİN SİYAHISI",
            ConstitutionalAmendment => "KONSTİTUSİYAYA EDİLMİŞ DƏYİŞİKLİK VƏ ƏLAVƏLƏRİN SİYAHISI",
            _ => ""
        };

        public bool HasSelection => _selectedItem != null;
    }
}