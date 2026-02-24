using Newtonsoft.Json;

namespace ExcelBinder.Models
{
    /// <summary>
    /// GitHub Releases API 응답에서 추출한 버전 정보를 나타냅니다.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// 릴리즈 태그명입니다. (예: "v1.3.0")
        /// </summary>
        [JsonProperty("tag_name")]
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// GitHub 릴리즈 페이지 URL입니다.
        /// </summary>
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        /// <summary>
        /// 릴리즈 노트 본문입니다.
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// 릴리즈의 버전 문자열입니다. TagName에서 "v" 접두사를 제거한 값입니다.
        /// </summary>
        [JsonIgnore]
        public string VersionString
        {
            get
            {
                var tag = TagName ?? string.Empty;
                return tag.StartsWith('v') || tag.StartsWith('V') ? tag[1..] : tag;
            }
        }
    }
}
