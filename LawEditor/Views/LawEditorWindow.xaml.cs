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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LawEditor.Views
{
    public partial class LawEditorWindow : Window
    {
        private const int WM_MOUSEHWHEEL = 0x020E;

        public LawEditorWindow()
        {
            InitializeComponent();
            MainTextBox.IsEnabledChanged += (s, e) => TextBox_TextChanged(null, null);

            // Безопасно подмешиваемся к сообщениям окна после его загрузки
            Loaded += (s, e) =>
            {
                IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
                hwndSource?.AddHook(HwndMessageHook);
            };
        }

        // Этот хук теперь работает без HitTest и без Mouse.DirectlyOver, ничего не блокируя
        private IntPtr HwndMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEHWHEEL)
            {
                int tilt = (short)((long)wParam >> 16);
                // Инвертируем дельту, так как тачпад возвращает направление сдвига пальца, 
                // а ScrollToHorizontalOffset требует смещения самой каретки скролла
                double scrollFactor = tilt / 3.0;

                ScrollViewer targetScrollViewer = null;

                // Проверяем физическое нахождение мыши над панелями через IsMouseOver. 
                // Это встроенное WPF свойство, оно работает мгновенно и со 100% точностью.
                if (TreeScrollViewer != null && TreeScrollViewer.IsMouseOver)
                {
                    targetScrollViewer = TreeScrollViewer;
                }
                else if (MainScrollViewer != null && MainScrollViewer.IsMouseOver)
                {
                    targetScrollViewer = MainScrollViewer;
                }

                // Если мышь находится над одним из наших скроллеров — двигаем его по горизонтали
                if (targetScrollViewer != null && targetScrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    double newOffset = targetScrollViewer.HorizontalOffset + scrollFactor;

                    if (newOffset < 0) newOffset = 0;
                    if (newOffset > targetScrollViewer.ScrollableWidth) newOffset = targetScrollViewer.ScrollableWidth;

                    targetScrollViewer.ScrollToHorizontalOffset(newOffset);
                    handled = true; // Указываем ОС, что сообщение горизонтального скролла обработано
                }
            }
            return IntPtr.Zero;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is LawEditorWindowViewModel vm)
                vm.SelectedItem = e.NewValue;
        }

        // Вертикальный плавный скролл (двумя пальцами вверх-вниз)
        private void TouchpadScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DependencyObject currentObj)
            {
                ScrollViewer scrollViewer = currentObj as ScrollViewer;
                while (scrollViewer == null && currentObj != null)
                {
                    currentObj = VisualTreeHelper.GetParent(currentObj);
                    scrollViewer = currentObj as ScrollViewer;
                }

                if (scrollViewer != null)
                {
                    double newOffset = scrollViewer.VerticalOffset - (e.Delta * 0.4);

                    if (newOffset < 0) newOffset = 0;
                    if (newOffset > scrollViewer.ScrollableHeight) newOffset = scrollViewer.ScrollableHeight;

                    scrollViewer.ScrollToVerticalOffset(newOffset);
                    e.Handled = true;
                }
            }
        }

        // Жесткая синхронизация номеров строк и текстового поля по вертикали
        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (LineNumbersScrollViewer != null && MainScrollViewer != null)
            {
                LineNumbersScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset);
            }
        }

        // Генератор номеров строк
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