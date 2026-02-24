using System.Windows;
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
    }
}
