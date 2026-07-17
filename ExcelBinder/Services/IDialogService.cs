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
        string? BrowseSaveFile(string filter, string defaultFileName, string title = "", string? initialDirectory = null);
        bool ShowUpdateInfoDialog(string message, List<ReleaseNoteEntry> entries, string title);
        string? ShowInput(string title, string prompt, string? defaultValue = null, Func<string, string?>? validator = null);
        IReadOnlyList<string>? ShowFeatureSelectionDialog(string title, string prompt, IEnumerable<FeatureDefinition> allFeatures, IEnumerable<string> selectedFeatureIds);
        /// <summary>
        /// 가이드 윈도우를 모달로 표시합니다. "다시 보지 않기"를 체크하고 첫 실행 모드에서 닫으면 true를 반환합니다.
        /// </summary>
        bool ShowGuideWindow(string? topicId = null, bool isFirstRunMode = false);
    }
}
