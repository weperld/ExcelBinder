using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExcelBinder.Models
{
    public class FeatureGroup
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("featureIds")]
        public List<string> FeatureIds { get; set; } = new();

        [JsonIgnore]
        public bool IsAllGroup => Id == ProjectConstants.Defaults.AllGroupId;
    }

    public class FeatureGroupCollection
    {
        [JsonProperty("groups")]
        public List<FeatureGroup> Groups { get; set; } = new();
    }
}
