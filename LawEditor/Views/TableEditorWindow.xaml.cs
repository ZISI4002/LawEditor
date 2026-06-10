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

        public TableEditorWindow(LawEditor.Models.SpecialElements.Table table, Action onDeleteTable)
        {
            InitializeComponent();
            _vm = new TableEditorViewModel(this, table, onDeleteTable);
            DataContext = _vm;
        }
        public TableEditorWindow()
        {
            InitializeComponent();
        }



        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
        }
    }
}
