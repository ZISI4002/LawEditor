using LawEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LawEditor.Views
{
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
                vm.Headers.CollectionChanged += (s, args) =>
                {
                    // Перестраиваем только если изменилось количество колонок
                    if (args.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                        GenerateColumns(grid, vm);
                };
            }
        }

        private void GenerateColumns(DataGrid grid, TableEditorViewModel vm)
        {
            grid.Columns.Clear();

            for (int i = 0; i < vm.Headers.Count; i++)
            {
                int colIndex = i;

                var headerTemplate = new DataTemplate();
                var tbxFactory = new FrameworkElementFactory(typeof(TextBox));

                tbxFactory.SetBinding(TextBox.TextProperty,
                    new Binding($"Headers[{colIndex}]")
                    {
                        Source = vm,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });

                tbxFactory.SetValue(TextBox.BackgroundProperty, Brushes.Transparent);
                tbxFactory.SetValue(TextBox.ForegroundProperty,
                    (Brush)new BrushConverter().ConvertFrom("#38BDF8"));
                tbxFactory.SetValue(TextBox.BorderThicknessProperty, new Thickness(0, 0, 0, 1));
                tbxFactory.SetValue(TextBox.BorderBrushProperty, Brushes.SteelBlue);
                tbxFactory.SetValue(TextBox.FontWeightProperty, FontWeights.Bold);
                tbxFactory.SetValue(TextBox.FontSizeProperty, 13.0);
                tbxFactory.SetValue(TextBox.PaddingProperty, new Thickness(4, 2, 4, 2));

                headerTemplate.VisualTree = tbxFactory;

                var column = new DataGridTextColumn
                {
                    HeaderTemplate = headerTemplate,
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