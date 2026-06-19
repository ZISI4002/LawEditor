using LawEditor.Models.ChangableData;
using LawEditor.Models.ChangableSourse;
using LawEditor.Models.RootClasses;
using LawEditor.Models.SpecialElements;
using LawEditor.Services.Intefase;
using LawEditor.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Section = LawEditor.Models.ChangableData.Section;
using Table = LawEditor.Models.SpecialElements.Table;

namespace LawEditor.Views
{
    public partial class LawEditorWindow : Window
    {
        private const int WM_MOUSEHWHEEL = 0x020E;
        private bool _isUpdatingRichText = false;
        private object? _currentSelectedItem;

        public LawEditorWindow()
        {
            InitializeComponent();
            MainRichTextBox.IsEnabledChanged += (s, e) => UpdateLineNumbers();
            MainRichTextBox.PreviewMouseRightButtonUp += MainRichTextBox_PreviewMouseRightButtonUp;
            MainRichTextBox.PreviewKeyDown += MainRichTextBox_PreviewKeyDown;

            Loaded += (s, e) =>
            {
                IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
                hwndSource?.AddHook(HwndMessageHook);

                if (DataContext is LawEditorWindowViewModel vm)
                {

                    vm.OnSelectedItemChanged = SetRichTextContent;
                }

            };
        }

        private IntPtr HwndMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEHWHEEL)
            {
                int tilt = (short)((long)wParam >> 16);
                double scrollFactor = tilt / 3.0;

                ScrollViewer? targetScrollViewer = null;

                // 1. Проверяем левую панель (дерево)
                if (TreeScrollViewer != null && TreeScrollViewer.IsMouseOver)
                {
                    targetScrollViewer = TreeScrollViewer;
                }
                // 2. Проверяем правый дисплей (текстовый редактор)
                else if (MainRichTextBox != null && MainRichTextBox.IsMouseOver)
                {
                    targetScrollViewer = FindVisualChild<ScrollViewer>(MainRichTextBox);
                }

                // 3. Выполняем горизонтальный скролл тачпада
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

        public void SetRichTextContent(object? selectedItem)
        {
            _isUpdatingRichText = true;
            _currentSelectedItem = selectedItem;

            string text = Models.TreeClasses.FullTextWrapper.GetFullText(selectedItem);

            Table? table = selectedItem switch
            {
                Chapter ch => ch.Table,
                Section section => section.Table,
                Article a => a.Table,
                Clause cl => cl.Table,
                SubClause sc => sc.Table,
                _ => null
            };

            Models.SpecialElements.Image? image = selectedItem switch
            {
                Chapter ch => ch.Image,
                Section section => section.Image,
                Article a => a.Image,
                Clause cl => cl.Image,
                SubClause sc => sc.Image,
                _ => null
            };

            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Consolas, Monaco, Courier New"),
                FontSize = 14,
                PagePadding = new Thickness(5)
            };

            doc.PageWidth = double.NaN; // позволяет авто-подстройку под контейнер

            var paragraph = new Paragraph
            {
                Foreground = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A))
            };

            Clause? clauseItem = selectedItem as Clause;

            if (clauseItem != null && !string.IsNullOrEmpty(clauseItem.LinkText))
            {
                int linkIndex = text.IndexOf(clauseItem.LinkText, StringComparison.Ordinal);

                if (linkIndex >= 0)
                {
                    // текст до ссылки
                    if (linkIndex > 0)
                    {
                        paragraph.Inlines.Add(new Run(text.Substring(0, linkIndex)));
                    }

                    var linkHyperlink = new Hyperlink(new Run(clauseItem.LinkText))
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
                        FontWeight = FontWeights.SemiBold,
                        Cursor = Cursors.Hand,
                        TextDecorations = TextDecorations.Underline
                    };

                    linkHyperlink.MouseLeftButtonDown += (s, e) =>
                    {
                        e.Handled = true;

                        var editor = new LinkEditorWindow(clauseItem.Url) { Owner = this };
                        if (editor.ShowDialog() == true)
                        {
                            clauseItem.Url = editor.Url;
                            SetRichTextContent(selectedItem);
                        }
                    };

                    paragraph.Inlines.Add(linkHyperlink);

                    // текст после ссылки
                    int afterStart = linkIndex + clauseItem.LinkText.Length;
                    if (afterStart < text.Length)
                    {
                        paragraph.Inlines.Add(new Run(text.Substring(afterStart)));
                    }
                }
                else
                {
                    // LinkText задан, но в тексте не найден — на всякий случай просто выводим текст целиком
                    paragraph.Inlines.Add(new Run(text));
                }
            }
            else
            {
                paragraph.Inlines.Add(new Run(text));
            }

            if (table != null)
            {
                var tableHyperlink = new Hyperlink(new Run($" [📋 Cədvəl #{table.Id}]"))
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
                    FontWeight = FontWeights.SemiBold,
                    Cursor = Cursors.Hand,
                    TextDecorations = TextDecorations.Underline
                };

                // ИСПОЛЬЗУЕМ MouseLeftButtonDown вместо Click
                tableHyperlink.MouseLeftButtonDown += (s, e) =>
                {
                    e.Handled = true; // Глушим событие, чтобы каретка RichTextBox не прыгала на ссылку

                    var win = new TableEditorWindow(table, DataContext as LawEditorWindowViewModel)
                    { Owner = this };

                    win.ShowDialog();
                };

                paragraph.Inlines.Add(tableHyperlink);
            }

            doc.Blocks.Add(paragraph);

            if (image != null)
            {
                AddImageBlock(doc, image);
            }

            MainRichTextBox.Document = doc;
            UpdateLineNumbers();

            _isUpdatingRichText = false;
        }

        private void AddImageBlock(FlowDocument doc, Models.SpecialElements.Image image)
        {
            System.Diagnostics.Debug.WriteLine($"[AddImageBlock] FilePath = {image.FilePath}");

            if (string.IsNullOrEmpty(image.FilePath) || !File.Exists(image.FilePath))
            {
                var missingParagraph = new Paragraph(new Run($"[Şəkil tapılmadı: {image.FileName}]"))
                {
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic
                };
                doc.Blocks.Add(missingParagraph);
                return;
            }

            BitmapImage? bitmap = null;

            try
            {
                byte[] fileBytes = File.ReadAllBytes(image.FilePath);

                bitmap = new BitmapImage();
                using (var stream = new MemoryStream(fileBytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }
                bitmap.Freeze();
            }
            catch (Exception ex)
            {
                var errorParagraph = new Paragraph(new Run($"[Şəkil yüklənmədi: {ex.Message}]"))
                {
                    Foreground = Brushes.Red,
                    FontStyle = FontStyles.Italic
                };
                doc.Blocks.Add(errorParagraph);
                return;
            }

            var imageControl = new System.Windows.Controls.Image
            {
                Source = bitmap,
                MaxWidth = 600,
                Stretch = Stretch.Uniform,
                ToolTip = image.Title ?? image.FileName
            };

            var container = new BlockUIContainer(imageControl)
            {
                Margin = new Thickness(0, 8, 0, 8)
            };

            doc.Blocks.Add(container);
            System.Diagnostics.Debug.WriteLine("[AddImageBlock] BlockUIContainer добавлен в doc.Blocks");
        }

        private void MainRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back)
                return;

            // Ищем BlockUIContainer с картинкой в текущем документе
            var doc = MainRichTextBox.Document;
            BlockUIContainer? imageBlock = doc.Blocks
                .OfType<BlockUIContainer>()
                .FirstOrDefault();

            if (imageBlock == null)
                return;

            // Проверяем: каретка стоит сразу после блока с картинкой?
            TextPointer caret = MainRichTextBox.CaretPosition;
            TextPointer blockEnd = imageBlock.ContentEnd.GetNextInsertionPosition(LogicalDirection.Forward)
                                   ?? imageBlock.ContentEnd;

            bool caretIsAfterImage = caret.CompareTo(blockEnd) <= 0
                                     && caret.CompareTo(imageBlock.ContentStart) >= 0;

            if (!caretIsAfterImage)
                return;

            e.Handled = true; // блокируем стандартный Backspace

            // Получаем Image из текущего selectedItem
            Models.SpecialElements.Image? image = _currentSelectedItem switch
            {
                Chapter ch => ch.Image,
                Section section => section.Image,
                Article a => a.Image,
                Clause cl => cl.Image,
                SubClause subClause => subClause.Image,
                _ => null
            };

            if (image == null)
                return;

            if (DataContext is LawEditorWindowViewModel vm)
            {
                vm.EditedLaws.DeleteImage(image.Id);
                SetRichTextContent(_currentSelectedItem); // перерисовываем
            }
        }

        private void MainRichTextBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_currentSelectedItem is not Clause clauseItem)
                return; // для других типов — стандартное меню остаётся

            string selectedText = MainRichTextBox.Selection.Text;

            if (string.IsNullOrWhiteSpace(selectedText))
                return; // ничего не выделено — стандартное меню остаётся

            e.Handled = true; // не даём появиться стандартному меню

            var result = MessageBox.Show(
                $"\"{selectedText}\" mətnini linkə çevirmək istəyirsiniz?",
                "Link yarat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var editor = new LinkEditorWindow(clauseItem.Url) { Owner = this };
            if (editor.ShowDialog() == true)
            {
                clauseItem.LinkText = selectedText;
                clauseItem.Url = editor.Url;
                SetRichTextContent(_currentSelectedItem);
            }
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