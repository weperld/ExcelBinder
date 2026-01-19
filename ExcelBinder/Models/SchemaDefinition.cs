using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExcelBinder.Models
{
    public class SchemaDefinition
    {
        [JsonProperty("className")]
        public string ClassName { get; set; } = string.Empty;

        [JsonProperty("key")]
        public string Key { get; set; } = ProjectConstants.Excel.DefaultSheetKey;

        [JsonProperty("fields")]
        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();
    }
}
