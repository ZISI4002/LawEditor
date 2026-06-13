// TableEditorViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using LawEditor.Commands.TableEditorWindowCommands;
using LawEditor.Models.SpecialElements;

namespace LawEditor.ViewModels
{
    public class TableEditorViewModel : BaseViewModel
    {
        public TableEditorViewModel(Window window, LawEditor.Models.SpecialElements.Table original,LawEditorWindowViewModel parentViewModel)
            : base(window)
        {
            OriginalTable = original;
            WorkingCopy = original.Clone();
            Title = WorkingCopy.Title;
            ParentViewModel = parentViewModel;

            SaveTableCommand = new SaveTableCommand(this);
            AddRowCommand = new AddRowCommand(this);
            DeleteRowCommand = new DeleteRowCommand(this);
            AddColumnCommand = new AddColumnCommand(this);
            DeleteColumnCommand = new DeleteColumnCommand(this);
        }

        public Table OriginalTable { get; set; }
        public Table WorkingCopy { get; set; }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public ObservableCollection<string> Headers => WorkingCopy.Headers;
        public ObservableCollection<TableRowData> Rows => WorkingCopy.Rows;

        public LawEditorWindowViewModel ParentViewModel { get; set; }
        public ICommand SaveTableCommand { get; }
        public ICommand AddRowCommand { get; }
        public ICommand DeleteRowCommand { get; }
        public ICommand AddColumnCommand { get; }
        public ICommand DeleteColumnCommand { get; }
        public ICommand DeleteTableCommand { get; }
    }
}