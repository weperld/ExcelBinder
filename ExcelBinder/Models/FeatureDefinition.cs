using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExcelBinder.Models
{
    public class FeatureDefinition
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("category")]
        public string Category { get; set; } = ProjectConstants.Categories.StaticData; // StaticData, Logic, SchemaGen

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("excelPath")]
        public string ExcelPath { get; set; } = string.Empty;

        [JsonProperty("schemaPath")]
        public string SchemaPath { get; set; } = string.Empty;

        [JsonProperty("exportPath")]
        public string ExportPath { get; set; } = string.Empty;

        [JsonProperty("scriptsPath")]
        public string ScriptsPath { get; set; } = string.Empty;

        [JsonProperty("defaultNamespace")]
        public string DefaultNamespace { get; set; } = ProjectConstants.Defaults.Namespace;

        [JsonProperty("typeMappings")]
        public Dictionary<string, string> TypeMappings { get; set; } = new();

        [JsonProperty("templates")]
        public FeatureTemplates Templates { get; set; } = new();

        [JsonProperty("outputOptions")]
        public OutputOptions OutputOptions { get; set; } = new();
    }

    public class FeatureTemplates
    {
        [JsonProperty("dataClass")]
        public string DataClass { get; set; } = string.Empty;
    }

    public class OutputOptions
    {
        [JsonProperty("extension")]
        public string Extension { get; set; } = ProjectConstants.Extensions.Binary;

        [JsonProperty("supportsBinary")]
        public bool SupportsBinary { get; set; } = true;

        [JsonProperty("supportsJson")]
        public bool SupportsJson { get; set; } = true;
    }
}
