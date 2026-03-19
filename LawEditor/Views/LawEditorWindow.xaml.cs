using LawEditor.Models.RootClasses;
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
    /// Логика взаимодействия для LawEditorWindow.xaml
    /// </summary>
    public partial class LawEditorWindow : Window
    {
        public LawEditorWindow()
        {
            InitializeComponent();
            
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is LawEditorWindowViewModel vm)
                vm.SelectedItem = e.NewValue;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
