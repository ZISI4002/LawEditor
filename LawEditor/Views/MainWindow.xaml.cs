using LawEditor.Models.RootClasses;
using LawEditor.Services.WordServises;
using LawEditor.Services.XMLSErvises;
using LawEditor.ViewModels;
using System.IO;
using System.Windows;

namespace LawEditor.Views
{
    public partial class MainWindow : Window
    {
        private readonly LawDisplayService _displayService = new LawDisplayService();
        private readonly XMLDisplayService _xmlDisplay = new XMLDisplayService();
        private readonly XMLTranslatorServise _xMLTranslator = new XMLTranslatorServise();
        private readonly XmlFileProcessingService xmlFileProcessingService = new XmlFileProcessingService();
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

        public void DisplayLaws(Laws laws)
        {
            RichTextLeft.Document = _displayService.BuildDocument(laws);
        }

        private void DisplayXML(MainWindowViewModel vm)
        {
            string folderPath = Path.GetDirectoryName(vm.FilePath);
            RichTextRight.Document = _xmlDisplay.BuildDocument(vm.XML, folderPath, vm.FileNameRight);
        }
        public void DisplayChangedXML(MainWindowViewModel vm)
        {
            string folderPath = Path.GetDirectoryName(vm.FilePath);
            _xMLTranslator.Translate(vm.Laws, folderPath, vm.FileNameRight);
            vm.XML = xmlFileProcessingService.ReadXmlFile(folderPath, vm.FileNameRight);
            RichTextRight.Document = _xmlDisplay.BuildDocument(vm.XML, folderPath, vm.FileNameRight);
        }
    }
}