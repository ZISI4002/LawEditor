using LawEditor.Models.RootClasses;
using LawEditor.Services;
using LawEditor.ViewModels;
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
            var vm = new MainWindowViewModel(this);
            DataContext = vm;
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.Laws) && vm.Laws != null)
                    DisplayLaws(vm.Laws);
            };
        }

        private readonly LawDisplayService _displayService = new LawDisplayService();

        private void DisplayLaws(Laws laws)
        {
            RichTextLeft.Document = _displayService.BuildDocument(laws);
            DisplayLeft.Visibility = Visibility.Visible;
            FileNameLabelLeft.Visibility = Visibility.Visible;
        }
        private void BtnAddWord_Click(object sender, RoutedEventArgs e)
        {
           
            BtnAdd.Visibility = Visibility.Visible;
            BtnUpdate.Visibility = Visibility.Visible;
            BtnDelete.Visibility = Visibility.Visible;
            FileNameLabelLeft.Visibility = Visibility.Visible;
            BtnAddWord.Visibility = Visibility.Collapsed;
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            DisplayRight.Visibility = Visibility.Visible;
            FileNameLabelRight.Visibility = Visibility.Visible;
        }
        
    }
}