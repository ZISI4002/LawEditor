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


            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.XML) && vm.XML != null)
                    DisplayXML(vm.XML);
            };

            
        }

        private readonly LawDisplayService _displayService = new LawDisplayService();

        private void DisplayLaws(Laws laws)
        {
            RichTextLeft.Document = _displayService.BuildDocument(laws);
        }
        private void DisplayXML(Laws laws)
        {
            RichTextRight.Document = _displayService.BuildDocument(laws);
            FileNameLabelRight.Visibility = Visibility.Visible;
            FileNameLabelRight.Visibility = Visibility.Visible;
        }
       

       
    }
}