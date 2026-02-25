using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;

namespace ExcelBinder.Services
{
    public class DialogService : IDialogService
    {
        public void ShowLogWindow()
        {
            var logWin = new Views.LogWindow();
            if (Application.Current.MainWindow != null)
                logWin.Owner = Application.Current.MainWindow;
            logWin.ShowDialog();
        }

        public MessageBoxResult ShowMessage(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            return MessageBox.Show(message, title, button, icon);
        }

        public void ShowSettingsDialog(object viewModel)
        {
            var settingsWin = new Views.SettingsWindow { DataContext = viewModel };
            settingsWin.Owner = Application.Current.MainWindow;
            settingsWin.ShowDialog();
        }

        public string? BrowseFolder(string title = "")
        {
            var dialog = new OpenFolderDialog { Title = title };
            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public string? BrowseOpenFile(string filter, string title = "")
        {
            var dialog = new OpenFileDialog { Filter = filter, Title = title };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? BrowseSaveFile(string filter, string defaultFileName, string title = "")
        {
            var dialog = new SaveFileDialog { Filter = filter, FileName = defaultFileName, Title = title };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public bool ShowUpdateInfoDialog(string message, List<ReleaseNoteEntry> entries, string title)
        {
            var window = new Window
            {
                Title = title,
                Width = 480,
                Height = 420,
                MinWidth = 380,
                MinHeight = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.CanResizeWithGrip,
            };

            if (Application.Current.MainWindow != null)
                window.Owner = Application.Current.MainWindow;

            bool navigateToDownload = false;
            var mutedBrush = Application.Current.TryFindResource("MutedTextBrush") as Brush
                ?? new SolidColorBrush(Color.FromRgb(108, 117, 125));

            var root = new DockPanel { Margin = new Thickness(20) };

            // 하단 버튼
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            DockPanel.SetDock(buttonPanel, Dock.Bottom);

            var downloadBtn = new Button
            {
                Content = "다운로드 페이지로 이동",
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };
            downloadBtn.Click += (s, e) => { navigateToDownload = true; window.Close(); };

            var closeBtn = new Button
            {
                Content = "닫기",
                Padding = new Thickness(16, 6, 16, 6),
                IsCancel = true
            };
            closeBtn.Click += (s, e) => window.Close();

            buttonPanel.Children.Add(downloadBtn);
            buttonPanel.Children.Add(closeBtn);
            root.Children.Add(buttonPanel);

            // 상단 메시지
            var msgBlock = new TextBlock
            {
                Text = message,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };
            DockPanel.SetDock(msgBlock, Dock.Top);
            root.Children.Add(msgBlock);

            // 중앙 - 스크롤 가능한 릴리즈 노트
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10)
            };
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var accentBrush = Application.Current.TryFindResource("SecondaryBrush") as Brush
                ?? new SolidColorBrush(Color.FromRgb(52, 152, 219));
            var lightBgBrush = new SolidColorBrush(Color.FromRgb(232, 232, 232));

            var notesPanel = new StackPanel();
            foreach (var entry in entries)
            {
                var entryBorder = new Border
                {
                    Padding = new Thickness(8, 6, 8, 6),
                    CornerRadius = new CornerRadius(3),
                    Margin = new Thickness(0, 0, 0, 2),
                    Background = entry.IsAlternate ? lightBgBrush : Brushes.Transparent
                };

                var entryPanel = new StackPanel();
                entryPanel.Children.Add(new TextBlock
                {
                    Text = entry.VersionTag,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = accentBrush,
                    Margin = new Thickness(0, 0, 0, 2)
                });
                entryPanel.Children.Add(new TextBlock
                {
                    Text = entry.Body,
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20,
                    Foreground = mutedBrush
                });

                entryBorder.Child = entryPanel;
                notesPanel.Children.Add(entryBorder);
            }

            scrollViewer.Content = notesPanel;
            border.Child = scrollViewer;
            root.Children.Add(border);

            window.Content = root;
            window.ShowDialog();

            return navigateToDownload;
        }
    }
}
