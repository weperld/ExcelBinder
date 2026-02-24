using System;
using System.Windows;

namespace ExcelBinder.Services
{
    public interface IDialogService
    {
        void ShowLogWindow();
        MessageBoxResult ShowMessage(string message, string title = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information);
        void ShowSettingsDialog(object viewModel);
        string? BrowseFolder(string title = "");
        string? BrowseOpenFile(string filter, string title = "");
        string? BrowseSaveFile(string filter, string defaultFileName, string title = "");
    }
}
