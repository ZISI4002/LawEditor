using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Models.TreeClasses;
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
            CopyLawsData(mainWindowViewModel.Laws, EditedLaws);
            this.SaveCommand = new Commands.LawEditorWindowCommands.SaveCommand(this);
        }
        public MainWindowViewModel MainWindowModel { get; set; }

        public Laws Laws { get; set; }
        public Laws EditedLaws { get; set; }
        
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public LawAnchor CurrentAnchor { get; set; } = new();

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
            _ => "Элемент не выбран"
        };

        public bool HasSelection => _selectedItem != null;

        public void CopyLawsData(Laws source, Laws target)
        {
            target.Header = source.Header;

            // Копируем главы
            foreach (var ch in source.Chapters)
            {
                var newChapter = new Chapter
                {
                    Id = ch.Id,
                    Title = ch.Title
                };

                foreach (var sec in ch.Sections)
                {
                    var newSection = new Section
                    {
                        Id = sec.Id,
                        Title = sec.Title
                    };

                    foreach (var art in sec.Articles)
                    {
                        var newArticle = new Article
                        {
                            Id = art.Id,
                            Title = art.Title
                        };

                        foreach (var cl in art.Clauses)
                        {
                            var newClause = new Clause
                            {
                                Number = cl.Number,
                                Text = cl.Text
                            };

                            foreach (var sc in cl.SubClauses)
                            {
                                newClause.SubClauses.Add(new SubClause
                                {
                                    Number = sc.Number,
                                    Text = sc.Text
                                });
                            }

                            newArticle.Clauses.Add(newClause);
                        }

                        newSection.Articles.Add(newArticle);
                    }

                    newChapter.Sections.Add(newSection);
                }

                target.Chapters.Add(newChapter);
            }

            // Копируем TransitionalProvisions
            foreach (var tp in source.transitionalProvisions)
            {
                target.transitionalProvisions.Add(new TransitionalProvisions
                {
                    Id = tp.Id,
                    Title = tp.Title,
                    Date = tp.Date
                });
            }

            // Копируем ConstitutionalAmendment
            foreach (var ca in source.constitutionalAmendments)
            {
                target.constitutionalAmendments.Add(new ConstitutionalAmendment
                {
                    Id = ca.Id,
                    Title = ca.Title
                });
            }

            // Копируем SourceDocumentsList
            foreach (var sd in source.sourceDocumentsLists)
            {
                target.sourceDocumentsLists.Add(new SourceDocumentsList
                {
                    Id = sd.Id,
                    Title = sd.Title
                });
            }
        }
    }
}
