using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
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
