using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExcelBinder.Models
{
    public class ProjectProfile
    {
        [JsonProperty("projectName")]
        public string ProjectName { get; set; } = "DefaultProject";

        [JsonProperty("defaultNamespace")]
        public string DefaultNamespace { get; set; } = "GameData";

        [JsonProperty("typeMappings")]
        public Dictionary<string, string> TypeMappings { get; set; } = new();

        [JsonProperty("templates")]
        public TemplatePaths Templates { get; set; } = new();

        [JsonProperty("coreFiles")]
        public List<string> CoreFiles { get; set; } = new();

        public class TemplatePaths
        {
            [JsonProperty("dataClass")]
            public string DataClass { get; set; } = "Templates/DataClass.liquid";

            [JsonProperty("managerClass")]
            public string ManagerClass { get; set; } = "Templates/StaticDataManager.liquid";
        }
    }
}
