using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using LawEditor.Services.Intefase;
using LawEditor.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Views
{
    public partial class LawEditorWindow : Window
    {

        private const int WM_MOUSEHWHEEL = 0x020E;
        private bool _isUpdatingRichText = false;

        public LawEditorWindow()
        {
            InitializeComponent();
            MainRichTextBox.IsEnabledChanged += (s, e) => UpdateLineNumbers();

            Loaded += (s, e) =>
            {
                IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
                hwndSource?.AddHook(HwndMessageHook);

                if (DataContext is LawEditorWindowViewModel vm)
                    vm.OnSelectedItemChanged = SetRichTextContent;
            };
        }
        private IntPtr HwndMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEHWHEEL)
            {
                int tilt = (short)((long)wParam >> 16);
                double scrollFactor = tilt / 3.0; // Ваша исходная скорость прокрутки

                ScrollViewer? targetScrollViewer = null;

                // 1. Проверяем левую панель (дерево) — ваша исходная рабочая логика
                if (TreeScrollViewer != null && TreeScrollViewer.IsMouseOver)
                {
                    targetScrollViewer = TreeScrollViewer;
                }
                // 2. Проверяем правый дисплей (текстовый редактор)
                else if (MainRichTextBox != null && MainRichTextBox.IsMouseOver)
                {
                    targetScrollViewer = FindVisualChild<ScrollViewer>(MainRichTextBox);
                }

                // 3. Если определили целевой скроллер — плавно двигаем его по горизонтали
                if (targetScrollViewer != null)
                {
                    double newOffset = Math.Clamp(
                        targetScrollViewer.HorizontalOffset + scrollFactor,
                        0,
                        targetScrollViewer.ScrollableWidth);

                    targetScrollViewer.ScrollToHorizontalOffset(newOffset);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
        private T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T viewer) return viewer;

                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is LawEditorWindowViewModel vm)
                vm.SelectedItem = e.NewValue;
        }

        private void TouchpadScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DependencyObject currentObj)
            {
                ScrollViewer? scrollViewer = currentObj as ScrollViewer;
                while (scrollViewer == null && currentObj != null)
                {
                    currentObj = VisualTreeHelper.GetParent(currentObj);
                    scrollViewer = currentObj as ScrollViewer;
                }

                if (scrollViewer != null)
                {
                    double newOffset = Math.Clamp(
                        scrollViewer.VerticalOffset - (e.Delta * 0.4),
                        0,
                        scrollViewer.ScrollableHeight);

                    scrollViewer.ScrollToVerticalOffset(newOffset);
                    e.Handled = true;
                }
            }
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
           
        }

        public void SetRichTextContent(object? selectedItem)
        {
            _isUpdatingRichText = true;

            string text = Models.TreeClasses.FullTextWrapper.GetFullText(selectedItem);

            Table? table = selectedItem switch
            {
                Article a => a.Table,
               // Clause cl => cl.Table,
               //SubClause sc => sc.Table,
                _ => null
            };

            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Consolas, Monaco, Courier New"),
                FontSize = 14,
                PagePadding = new Thickness(5),
                PageWidth = 10000
            };

            var paragraph = new Paragraph(new Run(text))
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A))
            };

            if (table != null)
            {
                var btn = BuildTableButton(table, selectedItem);
                paragraph.Inlines.Add(new InlineUIContainer(btn)
                {
                    BaselineAlignment = BaselineAlignment.Center
                });
            }

            doc.Blocks.Add(paragraph);
            MainRichTextBox.Document = doc;
            UpdateLineNumbers();

            _isUpdatingRichText = false;
        }

        private Button BuildTableButton(LawEditor.Models.SpecialElements.Table table, object? parent)
        {
            var btn = new Button
            {
                Content = $"  📋 Cədvəl #{table.Id}  ",
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(8, 3, 8, 3),
                Background = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };

            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                {
                    RelativeSource = new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            var content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);
            template.VisualTree = border;
            btn.Template = template;

            btn.PreviewMouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true;
                var win = new TableEditorWindow(table, () =>
                {
                    switch (parent)
                    {
                        case Article a: a.Table = null; break;
                    }
                    SetRichTextContent(parent);
                })
                { Owner = this };

                win.ShowDialog();
            };

            return btn;
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingRichText) return;

            UpdateLineNumbers();

            var text = new TextRange(
                MainRichTextBox.Document.ContentStart,
                MainRichTextBox.Document.ContentEnd
            ).Text.TrimEnd('\r', '\n');

            if (DataContext is LawEditorWindowViewModel vm && vm.SelectedText != text)
                vm.SelectedText = text;
        }

        private void UpdateLineNumbers()
        {
            if (LineNumbersBox == null || MainRichTextBox == null) return;

            var text = new TextRange(
                MainRichTextBox.Document.ContentStart,
                MainRichTextBox.Document.ContentEnd
            ).Text ?? string.Empty;

            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineCount = Math.Max(lines.Length, 1);

            var sb = new System.Text.StringBuilder();
            for (int i = 1; i <= lineCount; i++)
                sb.AppendLine(i.ToString());

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