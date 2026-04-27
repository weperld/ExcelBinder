using System;
using System.Collections.Generic;
using System.Windows;
using ExcelBinder.Models;

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
        bool ShowUpdateInfoDialog(string message, List<ReleaseNoteEntry> entries, string title);
        string? ShowInput(string title, string prompt, string? defaultValue = null, Func<string, string?>? validator = null);
        IReadOnlyList<string>? ShowFeatureSelectionDialog(string title, string prompt, IEnumerable<FeatureDefinition> allFeatures, IEnumerable<string> selectedFeatureIds);
    }
}
