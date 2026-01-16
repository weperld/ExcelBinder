using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    public class FeatureService
    {
        private const string SettingsFile = "settings.json";

        public AppSettings LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings();
        }

        public void SaveSettings(AppSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }

        public List<FeatureDefinition> LoadFeatures(string directory)
        {
            var features = new List<FeatureDefinition>();
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return features;
            }

            foreach (var file in Directory.GetFiles(directory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var feature = JsonConvert.DeserializeObject<FeatureDefinition>(json);
                    if (feature != null)
                    {
                        features.Add(feature);
                    }
                }
                catch
                {
                    // Skip invalid feature files
                }
            }
            return features;
        }

        public FeatureDefinition? LoadFeatureFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<FeatureDefinition>(json);
            }
            catch
            {
                return null;
            }
        }

        public string? GetFeaturePath(string featureId, AppSettings settings)
        {
            // Check in directory
            if (Directory.Exists(settings.FeatureDefinitionsPath))
            {
                foreach (var file in Directory.GetFiles(settings.FeatureDefinitionsPath, "*.json"))
                {
                    var f = LoadFeatureFromFile(file);
                    if (f != null && f.Id == featureId) return file;
                }
            }

            // Check in bound features
            foreach (var path in settings.BoundFeatures)
            {
                var f = LoadFeatureFromFile(path);
                if (f != null && f.Id == featureId) return path;
            }

            return null;
        }
    }
}
