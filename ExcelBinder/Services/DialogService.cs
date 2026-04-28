using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ExcelBinder.Models;
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

        public string? ShowInput(string title, string prompt, string? defaultValue = null, Func<string, string?>? validator = null)
        {
            var window = new Window
            {
                Title = title,
                Width = 520,
                MinWidth = 416,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
            };

            if (Application.Current.MainWindow != null)
                window.Owner = Application.Current.MainWindow;

            string? result = null;

            var root = new StackPanel { Margin = new Thickness(20) };

            var promptBlock = new TextBlock
            {
                Text = prompt,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            root.Children.Add(promptBlock);

            var input = new TextBox
            {
                Text = defaultValue ?? string.Empty,
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4),
                MinHeight = 28
            };
            root.Children.Add(input);

            var errorBlock = new TextBlock
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 0),
                Visibility = Visibility.Collapsed
            };
            root.Children.Add(errorBlock);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };

            var okBtn = new Button
            {
                Content = "확인",
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true,
                MinWidth = 80
            };
            okBtn.Click += (s, e) =>
            {
                string current = input.Text ?? string.Empty;
                if (validator != null)
                {
                    string? error = validator(current);
                    if (error != null)
                    {
                        errorBlock.Text = error;
                        errorBlock.Visibility = Visibility.Visible;
                        input.Focus();
                        return;
                    }
                }
                result = current;
                window.Close();
            };

            var cancelBtn = new Button
            {
                Content = "취소",
                Padding = new Thickness(16, 6, 16, 6),
                IsCancel = true,
                MinWidth = 80
            };
            cancelBtn.Click += (s, e) => window.Close();

            buttonPanel.Children.Add(okBtn);
            buttonPanel.Children.Add(cancelBtn);
            root.Children.Add(buttonPanel);

            window.Content = root;
            window.Loaded += (s, e) =>
            {
                input.Focus();
                input.SelectAll();
            };
            window.ShowDialog();

            return result;
        }

        public IReadOnlyList<string>? ShowFeatureSelectionDialog(string title, string prompt,
            IEnumerable<FeatureDefinition> allFeatures, IEnumerable<string> selectedFeatureIds)
        {
            var window = new Window
            {
                Title = title,
                Width = 624,
                Height = 676,
                MinWidth = 468,
                MinHeight = 468,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.CanResizeWithGrip,
            };

            if (Application.Current.MainWindow != null)
                window.Owner = Application.Current.MainWindow;

            IReadOnlyList<string>? result = null;
            var selectedSet = new HashSet<string>(selectedFeatureIds);
            var items = allFeatures
                .Select(f => new FeatureSelectionItem
                {
                    Id = f.Id,
                    DisplayName = string.IsNullOrEmpty(f.Name) ? f.Id : $"{f.Name}  ({f.Id})",
                    Category = f.Category,
                    IsChecked = selectedSet.Contains(f.Id)
                })
                .OrderBy(i => i.DisplayName)
                .ToList();

            var root = new DockPanel { Margin = new Thickness(20) };

            // 하단 버튼
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            DockPanel.SetDock(buttonPanel, Dock.Bottom);

            var okBtn = new Button
            {
                Content = "확인",
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true,
                MinWidth = 80
            };
            okBtn.Click += (s, e) =>
            {
                result = items.Where(i => i.IsChecked).Select(i => i.Id).ToList();
                window.Close();
            };

            var cancelBtn = new Button
            {
                Content = "취소",
                Padding = new Thickness(16, 6, 16, 6),
                IsCancel = true,
                MinWidth = 80
            };
            cancelBtn.Click += (s, e) => window.Close();

            buttonPanel.Children.Add(okBtn);
            buttonPanel.Children.Add(cancelBtn);
            root.Children.Add(buttonPanel);

            // 상단 안내
            var promptBlock = new TextBlock
            {
                Text = prompt,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };
            DockPanel.SetDock(promptBlock, Dock.Top);
            root.Children.Add(promptBlock);

            // 검색 박스
            var searchBox = new TextBox
            {
                FontSize = 13,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 8),
                Tag = "검색..."
            };
            DockPanel.SetDock(searchBox, Dock.Top);
            root.Children.Add(searchBox);

            // 가운데 - 체크박스 리스트
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(222, 226, 230)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4)
            };

            var listBox = new ListBox
            {
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                ItemTemplate = BuildFeatureSelectionTemplate()
            };
            ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
            listBox.ItemContainerStyle = BuildFeatureSelectionContainerStyle();
            listBox.ItemsSource = items;
            border.Child = listBox;
            root.Children.Add(border);

            searchBox.TextChanged += (s, e) =>
            {
                string query = (searchBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(query))
                {
                    listBox.ItemsSource = items;
                }
                else
                {
                    listBox.ItemsSource = items
                        .Where(i => i.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || (i.Category ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            };

            window.Content = root;
            window.ShowDialog();

            return result;
        }

        private static DataTemplate BuildFeatureSelectionTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding(nameof(FeatureSelectionItem.IsChecked)) { Mode = System.Windows.Data.BindingMode.TwoWay });
            factory.SetBinding(CheckBox.ContentProperty, new System.Windows.Data.Binding(nameof(FeatureSelectionItem.DisplayName)));
            factory.SetValue(CheckBox.MarginProperty, new Thickness(4, 2, 4, 2));
            var template = new DataTemplate { VisualTree = factory };
            template.DataType = typeof(FeatureSelectionItem);
            return template;
        }

        private static Style BuildFeatureSelectionContainerStyle()
        {
            var style = new Style(typeof(ListBoxItem));
            style.Setters.Add(new Setter(ListBoxItem.PaddingProperty, new Thickness(0)));
            style.Setters.Add(new Setter(ListBoxItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            return style;
        }

        private class FeatureSelectionItem : ViewModels.ViewModelBase
        {
            private bool _isChecked;
            public string Id { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string? Category { get; set; }
            public bool IsChecked
            {
                get => _isChecked;
                set => SetProperty(ref _isChecked, value);
            }
        }

        public bool ShowUpdateInfoDialog(string message, List<ReleaseNoteEntry> entries, string title)
        {
            var window = new Window
            {
                Title = title,
                Width = 624,
                Height = 546,
                MinWidth = 494,
                MinHeight = 390,
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
