using System.Windows;

namespace LawEditor.Views
{
    public partial class LinkEditorWindow : Window
    {
        public string Url { get; private set; } = string.Empty;

        public LinkEditorWindow(string? currentUrl)
        {
            InitializeComponent();
            UrlTextBox.Text = currentUrl ?? string.Empty;
            UrlTextBox.Focus();
            UrlTextBox.SelectAll();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Url = UrlTextBox.Text.Trim();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}