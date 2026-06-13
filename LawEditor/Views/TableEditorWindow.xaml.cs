using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LawEditor.Views
{
    /// <summary>
    /// Логика взаимодействия для TableEditorWindow.xaml
    /// </summary>
    public partial class TableEditorWindow : Window
    {
        private readonly TableEditorViewModel _vm;

        public TableEditorWindow(LawEditor.Models.SpecialElements.Table table, LawEditor.ViewModels.LawEditorWindowViewModel parentViewModel)
        {
            InitializeComponent();
            _vm = new TableEditorViewModel(this, table, parentViewModel);
            DataContext = _vm;
        }
        public TableEditorWindow()
        {
            InitializeComponent();
        }



        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            if (DataContext is TableEditorViewModel vm)
            {
                GenerateColumns(grid, vm);
                vm.Headers.CollectionChanged += (s, args) => GenerateColumns(grid, vm);
            }
        }

        private void GenerateColumns(DataGrid grid, TableEditorViewModel vm)
        {
            grid.Columns.Clear();

            for (int i = 0; i < vm.Headers.Count; i++)
            {
                int colIndex = i;

                var column = new DataGridTextColumn
                {
                    Header = vm.Headers[i],
                    Binding = new Binding($"[{colIndex}]")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                };

                grid.Columns.Add(column);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }   

    }
}
