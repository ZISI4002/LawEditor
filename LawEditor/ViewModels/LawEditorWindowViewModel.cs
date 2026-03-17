using DocumentFormat.OpenXml.Wordprocessing;
using LawEditor.Models.ChangableData;
using LawEditor.Models.RootClasses;
using LawEditor.Models.TreeClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LawEditor.ViewModels
{
    public class LawEditorWindowViewModel : BaseViewModel
    {
        public LawEditorWindowViewModel(Window window,MainWindowViewModel mainWindowViewModel) : base(window)
        {
            this.MainWindowModel = mainWindowViewModel;
           Laws= mainWindowViewModel.Laws;
        }
        public MainWindowViewModel MainWindowModel { get; set; }

        public Laws Laws { get; set; }
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
                    CurrentAnchor.Chapter = Laws.Chapters
                        .FirstOrDefault(ch => ch.Sections.Contains(s));
                    break;

                case Article a:
                    foreach (var ch in Laws.Chapters)
                        foreach (var sec in ch.Sections)
                            if (sec.Articles.Contains(a))
                            {
                                CurrentAnchor.Chapter = ch;
                                CurrentAnchor.Section = sec;
                                CurrentAnchor.Article = a;
                            }
                    break;

                case Clause cl:
                    foreach (var ch in Laws.Chapters)
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
                    foreach (var ch in Laws.Chapters)
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
            get => _selectedItem switch
            {
                Chapter c => c.Title,
                Section s => s.Title,
                Article a => a.Title,
                Clause cl => cl.Text,
                SubClause sc => sc.Text,
                _ => string.Empty
            };
            set
            {
                switch (_selectedItem)
                {
                    case Chapter c: c.Title = value; break;
                    case Section s: s.Title = value; break;
                    case Article a: a.Title = value; break;
                    case Clause cl: cl.Text = value; break;
                    case SubClause sc: sc.Text = value; break;
                }
            }
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
    }
}
