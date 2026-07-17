using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExcelBinder.Services;
using ExcelBinder.Models;

namespace ExcelBinder.ViewModels
{
    /// <summary>
    /// 업데이트 확인/다운로드를 담당하는 ViewModel입니다. MainViewModel.Update로 노출됩니다.
    /// </summary>
    public class UpdateViewModel : ViewModelBase
    {
        private readonly UpdateCheckService _updateCheckService = new();

        private VersionInfo? _latestVersionInfo;
        private bool _isUpdateBannerVisible;
        private string _releaseNotesText = string.Empty;
        private List<ReleaseNoteEntry> _releaseNoteEntries = new();
        private bool _isDownloadingUpdate;
        private int _downloadProgress;

        /// <summary>
        /// 최신 버전 정보입니다. null이면 업데이트 없음 또는 확인 전 상태입니다.
        /// </summary>
        public VersionInfo? LatestVersionInfo
        {
            get => _latestVersionInfo;
            set
            {
                if (SetProperty(ref _latestVersionInfo, value))
                {
                    IsUpdateBannerVisible = value != null;
                    OnPropertyChanged(nameof(UpdateMessage));
                    OnPropertyChanged(nameof(IsUpdateIndicatorVisible));
                }
            }
        }

        /// <summary>
        /// 업데이트 알림 배너의 표시 여부입니다.
        /// </summary>
        public bool IsUpdateBannerVisible
        {
            get => _isUpdateBannerVisible;
            set
            {
                if (SetProperty(ref _isUpdateBannerVisible, value))
                {
                    OnPropertyChanged(nameof(IsUpdateIndicatorVisible));
                }
            }
        }

        /// <summary>
        /// 업데이트가 존재하지만 토스트가 닫혀 있을 때 인디케이터 버튼을 표시합니다.
        /// </summary>
        public bool IsUpdateIndicatorVisible => LatestVersionInfo != null && !IsUpdateBannerVisible;

        /// <summary>
        /// 업데이트 다운로드가 진행 중인지 여부입니다.
        /// </summary>
        public bool IsDownloadingUpdate
        {
            get => _isDownloadingUpdate;
            set
            {
                if (SetProperty(ref _isDownloadingUpdate, value))
                {
                    OnPropertyChanged(nameof(CanDownloadUpdate));
                }
            }
        }

        /// <summary>
        /// 업데이트 다운로드 진행률입니다 (0~100).
        /// </summary>
        public int DownloadProgress
        {
            get => _downloadProgress;
            set => SetProperty(ref _downloadProgress, value);
        }

        /// <summary>
        /// 다운로드 버튼 활성화 여부 — 다운로드 중이 아니어야 활성화됩니다.
        /// </summary>
        public bool CanDownloadUpdate => !IsDownloadingUpdate;

        /// <summary>
        /// 업데이트 알림 배너에 표시할 메시지입니다.
        /// </summary>
        public string UpdateMessage => LatestVersionInfo != null
            ? string.Format(ProjectConstants.Update.MsgNewVersion, LatestVersionInfo.VersionString)
            : string.Empty;

        /// <summary>
        /// 현재 버전 이후의 릴리즈 노트 텍스트입니다.
        /// </summary>
        public string ReleaseNotesText
        {
            get => _releaseNotesText;
            set => SetProperty(ref _releaseNotesText, value);
        }

        /// <summary>
        /// 릴리즈 노트를 버전 태그와 본문으로 분리한 엔트리 목록입니다.
        /// </summary>
        public List<ReleaseNoteEntry> ReleaseNoteEntries
        {
            get => _releaseNoteEntries;
            set => SetProperty(ref _releaseNoteEntries, value);
        }

        public ICommand OpenUpdatePageCommand { get; }
        public ICommand CheckForUpdateCommand { get; }
        public ICommand DismissUpdateBannerCommand { get; }
        public ICommand ShowUpdateBannerCommand { get; }
        public ICommand DownloadUpdateCommand { get; }

        public UpdateViewModel()
        {
            OpenUpdatePageCommand = new RelayCommand(ExecuteOpenUpdatePage);
            CheckForUpdateCommand = new RelayCommand(ExecuteCheckForUpdate);
            DismissUpdateBannerCommand = new RelayCommand(() => IsUpdateBannerVisible = false);
            ShowUpdateBannerCommand = new RelayCommand(() => IsUpdateBannerVisible = true);
            DownloadUpdateCommand = new RelayCommand(ExecuteDownloadUpdate);
        }

        /// <summary>
        /// 앱 시작 시 fire-and-forget으로 업데이트를 확인합니다.
        /// async void는 생성자 이후 호출을 위한 의도적 사용입니다.
        /// </summary>
        public async void StartUpdateCheckOnStartup()
        {
            await CheckForUpdateAsync();
        }

        private async Task CheckForUpdateAsync()
        {
            try
            {
                var versionInfo = await _updateCheckService.CheckForUpdateAsync();
                LatestVersionInfo = versionInfo;

                if (versionInfo != null)
                {
                    await LoadReleaseNotesAsync();
                }
                else
                {
                    ReleaseNotesText = string.Empty;
                    ReleaseNoteEntries = new();
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"업데이트 확인 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 릴리즈 노트 조회+포맷. CheckForUpdateAsync/ExecuteCheckForUpdate 양쪽에서 공유합니다.
        /// </summary>
        private async Task LoadReleaseNotesAsync()
        {
            var releases = await _updateCheckService.GetReleaseNotesSinceCurrentAsync();
            ReleaseNotesText = UpdateCheckService.FormatReleaseNotes(releases);
            ReleaseNoteEntries = UpdateCheckService.ToReleaseNoteEntries(releases);
        }

        private async void ExecuteDownloadUpdate()
        {
            if (LatestVersionInfo == null || IsDownloadingUpdate) return;

            if (LatestVersionInfo.Assets.Count == 0)
            {
                AppServices.Dialog.ShowMessage(
                    ProjectConstants.Update.MsgNoAsset,
                    ProjectConstants.Update.TitleDownload,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string defaultName = string.Format(
                ProjectConstants.Update.DefaultDownloadFileName,
                LatestVersionInfo.TagName);

            string? targetPath = AppServices.Dialog.BrowseSaveFile(
                ProjectConstants.Update.ZipFilter,
                defaultName,
                ProjectConstants.Update.TitleDownload);
            if (targetPath == null) return;

            try
            {
                IsDownloadingUpdate = true;
                DownloadProgress = 0;
                var progress = new Progress<int>(p => DownloadProgress = p);
                bool ok = await _updateCheckService.DownloadAssetAsync(
                    LatestVersionInfo, targetPath, progress);

                if (ok)
                {
                    var openFolder = AppServices.Dialog.ShowMessage(
                        string.Format(ProjectConstants.Update.MsgDownloadSuccess, targetPath),
                        ProjectConstants.Update.TitleDownload,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    if (openFolder == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{targetPath}\"")
                            {
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            LogService.Instance.Warning($"폴더 열기 실패: {ex.Message}");
                        }
                    }
                }
                else
                {
                    AppServices.Dialog.ShowMessage(
                        ProjectConstants.Update.MsgDownloadFailed,
                        ProjectConstants.Update.TitleDownload,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"업데이트 다운로드 예외: {ex.Message}");
                AppServices.Dialog.ShowMessage(
                    string.Format(ProjectConstants.Update.MsgDownloadError, ex.Message),
                    ProjectConstants.Update.TitleDownload,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsDownloadingUpdate = false;
            }
        }

        private void ExecuteOpenUpdatePage()
        {
            var url = LatestVersionInfo?.HtmlUrl ?? ProjectConstants.Update.ReleasesPageUrl;
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                LogService.Instance.Error($"브라우저 열기 실패: {ex.Message}");
            }
        }

        private async void ExecuteCheckForUpdate()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                var versionInfo = await _updateCheckService.CheckForUpdateAsync();
                LatestVersionInfo = versionInfo;
                if (versionInfo != null)
                {
                    await LoadReleaseNotesAsync();

                    var navigate = AppServices.Dialog.ShowUpdateInfoDialog(
                        string.Format(ProjectConstants.Update.MsgNewVersion, versionInfo.VersionString),
                        ReleaseNoteEntries,
                        ProjectConstants.Update.TitleUpdateCheck);
                    if (navigate)
                    {
                        ExecuteOpenUpdatePage();
                    }
                }
                else
                {
                    AppServices.Dialog.ShowMessage(
                        ProjectConstants.Update.MsgUpToDate,
                        ProjectConstants.Update.TitleUpdateCheck, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                AppServices.Dialog.ShowMessage(
                    $"{ProjectConstants.Update.MsgCheckFailed}: {ex.Message}",
                    ProjectConstants.Update.TitleUpdateCheck, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
