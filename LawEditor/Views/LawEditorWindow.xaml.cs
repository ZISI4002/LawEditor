using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Services.Intefase;
using LawEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            MainTextBox.IsEnabledChanged += (s, e) => TextBox_TextChanged(null, null);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is LawEditorWindowViewModel vm)
                vm.SelectedItem = e.NewValue;
        }

        // 1. Этот метод намертво связывает вертикальную прокрутку номеров и текста
        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (LineNumbersScrollViewer != null && MainScrollViewer != null)
            {
                // Передаем вертикальное смещение от текста к номерам строк
                LineNumbersScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset);
            }
        }

        // 2. Твой стандартный подсчет строк
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LineNumbersBox == null || MainTextBox == null) return;

            string text = MainTextBox.Text ?? string.Empty;
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineCount = lines.Length;

            if (lineCount < 1) lineCount = 1;

            var sb = new System.Text.StringBuilder();
            for (int i = 1; i <= lineCount; i++)
            {
                sb.AppendLine(i.ToString());
            }

            LineNumbersBox.Text = sb.ToString();
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is ICloseHandler vm)
            {
                if (!vm.CanClose())
                {
                    e.Cancel = true;
                    return;
                }

                vm.OnClosing();
            }
        }
    }
}
