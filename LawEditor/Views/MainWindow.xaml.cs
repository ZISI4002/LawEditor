using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LawEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void BtnAddWord_Click(object sender, RoutedEventArgs e)
        {
            DisplayLeft.Visibility = Visibility.Visible;
            BtnAdd.Visibility = Visibility.Visible;
            BtnUpdate.Visibility = Visibility.Visible;
            BtnDelete.Visibility = Visibility.Visible;
            BtnAddWord.Visibility = Visibility.Collapsed;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DisplayRight.Visibility = Visibility.Visible;
        }
        
    }
}