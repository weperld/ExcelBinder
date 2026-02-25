using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    public record ReleaseNoteEntry(string VersionTag, string Body, bool IsAlternate);

    /// <summary>
    /// GitHub Releases API를 사용하여 새 버전 출시 여부를 확인하는 서비스입니다.
    /// </summary>
    public class UpdateCheckService
    {
        private static readonly HttpClient _httpClient = CreateHttpClient();

        /// <summary>
        /// 새 버전이 있는지 확인합니다.
        /// </summary>
        /// <returns>새 버전이 있으면 VersionInfo, 최신 버전이거나 확인 실패 시 null</returns>
        public async Task<VersionInfo?> CheckForUpdateAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(ProjectConstants.Update.ApiUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var release = JsonConvert.DeserializeObject<VersionInfo>(json);
                if (release == null || string.IsNullOrEmpty(release.TagName))
                    return null;

                var currentVersion = GetCurrentVersion();
                if (currentVersion == null)
                    return null;

                if (Version.TryParse(release.VersionString, out var latestVersion))
                {
                    if (latestVersion > currentVersion)
                        return release;
                }

                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                LogService.Instance.Warning($"업데이트 확인 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 현재 버전 이후의 모든 릴리즈 노트를 조회합니다.
        /// </summary>
        /// <returns>현재 버전 초과 ~ 최신 버전 이하의 릴리즈 목록 (최신순)</returns>
        public async Task<List<VersionInfo>> GetReleaseNotesSinceCurrentAsync()
        {
            try
            {
                var currentVersion = GetCurrentVersion();
                if (currentVersion == null)
                    return new List<VersionInfo>();

                var response = await _httpClient.GetAsync(ProjectConstants.Update.ReleasesApiUrl);
                if (!response.IsSuccessStatusCode)
                    return new List<VersionInfo>();

                var json = await response.Content.ReadAsStringAsync();
                var releases = JsonConvert.DeserializeObject<List<VersionInfo>>(json);
                if (releases == null)
                    return new List<VersionInfo>();

                return releases
                    .Where(r => !string.IsNullOrEmpty(r.TagName)
                                && Version.TryParse(r.VersionString, out var v)
                                && v > currentVersion)
                    .OrderByDescending(r => Version.Parse(r.VersionString))
                    .ToList();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                LogService.Instance.Warning($"릴리즈 노트 조회 실패: {ex.Message}");
                return new List<VersionInfo>();
            }
        }

        /// <summary>
        /// 릴리즈 노트 목록을 간소화된 텍스트로 포맷합니다.
        /// </summary>
        public static string FormatReleaseNotes(List<VersionInfo> releases)
        {
            if (releases.Count == 0)
                return string.Empty;

            var lines = new List<string>();
            foreach (var release in releases)
            {
                lines.Add($"── {release.TagName} ──");
                lines.Add(StripMarkdown(release.Body));
                lines.Add("");
            }

            return string.Join("\n", lines).TrimEnd();
        }

        /// <summary>
        /// 릴리즈 목록을 버전 태그와 본문으로 분리된 엔트리 리스트로 변환합니다.
        /// </summary>
        public static List<ReleaseNoteEntry> ToReleaseNoteEntries(List<VersionInfo> releases)
        {
            return releases
                .Select((r, i) => new ReleaseNoteEntry(r.TagName, StripMarkdown(r.Body), i % 2 == 0))
                .ToList();
        }

        private static string StripMarkdown(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            var text = markdown;
            // Remove headers (## Header → Header)
            text = Regex.Replace(text, @"^#{1,6}\s+", "", RegexOptions.Multiline);
            // Remove bold/italic (**text** or *text* → text)
            text = Regex.Replace(text, @"\*{1,2}([^*]+)\*{1,2}", "$1");
            // Convert list markers (- item → • item)
            text = Regex.Replace(text, @"^[\-\*]\s+", "• ", RegexOptions.Multiline);
            // Remove links [text](url) → text
            text = Regex.Replace(text, @"\[([^\]]+)\]\([^)]+\)", "$1");
            // Remove inline code `code` → code
            text = Regex.Replace(text, @"`([^`]+)`", "$1");
            // Collapse multiple blank lines
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            return text.Trim();
        }

        /// <summary>
        /// 현재 앱 버전을 반환합니다.
        /// </summary>
        /// <returns>현재 앱의 Version 객체</returns>
        private static Version? GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(ProjectConstants.Update.TimeoutSeconds)
            };
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(ProjectConstants.Update.UserAgent, GetCurrentVersion()?.ToString() ?? "0.0.0"));
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            return client;
        }
    }
}
