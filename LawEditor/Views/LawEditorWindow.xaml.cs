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
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is LawEditorWindowViewModel vm)
                vm.SelectedItem = e.NewValue;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LineNumbersBox == null || MainTextBox == null || LineNumbersPanel == null || SeparatorLine == null) return;

            // Проверяем реальное наличие текста. 
            // Если окно только открылось и текст NULL — скрываем панель номеров строк полностью.
            if (MainTextBox.Text == null)
            {
                LineNumbersPanel.Visibility = Visibility.Collapsed;
                SeparatorLine.Visibility = Visibility.Collapsed;
                LineNumbersBox.Text = string.Empty;
                return;
            }

            // Если элемент выбран (даже если текст внутри пустой "" при редактировании) — показываем ЧЕРНУЮ панель
            LineNumbersPanel.Visibility = Visibility.Visible;
            SeparatorLine.Visibility = Visibility.Visible;

            // Считаем перенос строк
            string[] lines = MainTextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
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
