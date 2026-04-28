using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExcelBinder.Models
{
    public class GuideTopic
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("resource")]
        public string Resource { get; set; } = string.Empty;
    }

    public class GuideCategory
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("topics")]
        public List<GuideTopic> Topics { get; set; } = new();
    }

    public class GuideIndex
    {
        [JsonProperty("categories")]
        public List<GuideCategory> Categories { get; set; } = new();
    }
}
