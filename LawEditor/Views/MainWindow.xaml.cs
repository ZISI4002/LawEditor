using LawEditor.Models.RootClasses;
using LawEditor.Services;
using LawEditor.ViewModels;
using System.IO;
using System.Windows;

namespace LawEditor.Views
{
    public partial class MainWindow : Window
    {
        private readonly LawDisplayService _displayService = new LawDisplayService();
        private readonly XMLDisplayService _xmlDisplay = new XMLDisplayService();

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
                    DisplayXML(vm);
            };
        }

        private void DisplayLaws(Laws laws)
        {
            RichTextLeft.Document = _displayService.BuildDocument(laws);
        }

        private void DisplayXML(MainWindowViewModel vm)
        {
            string folderPath = Path.GetDirectoryName(vm.FilePath);
            RichTextRight.Document = _xmlDisplay.BuildDocument(vm.XML, folderPath, vm.FileNameRight);
        }
    }
}