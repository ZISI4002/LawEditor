using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace LawEditor.Models.SpecialElements
{
    public class TableRowData : INotifyPropertyChanged
    {
        public ObservableCollection<string> Cells { get; set; } = new ObservableCollection<string>();

        public string this[int index]
        {
            get => index >= 0 && index < Cells.Count ? Cells[index] : string.Empty;
            set
            {
                if (index >= 0 && index < Cells.Count && Cells[index] != value)
                {
                    Cells[index] = value;
                    OnPropertyChanged(Binding.IndexerName); // "Item[]"
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}