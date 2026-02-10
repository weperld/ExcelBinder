using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    /// <summary>
    /// 애플리케이션 설정 및 특징(Feature) 정의 파일을 관리하는 서비스입니다.
    /// </summary>
    public class FeatureService
    {
        private const string SettingsFile = ProjectConstants.Files.AppSettings;
        
        // 특징 정의 파일의 역직렬화 결과를 캐싱하여 파일 I/O를 줄입니다.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, FeatureDefinition> _featureCache = new();

        /// <summary>
        /// 로컬 설정 파일을 로드합니다.
        /// </summary>
        public AppSettings LoadSettings()
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings();
        }

        /// <summary>
        /// 설정을 로컬 파일로 저장합니다.
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsFile, json);
        }

        /// <summary>
        /// 파일 경로로부터 특징 정의를 로드하며, 파일의 수정 시간을 확인하여 캐싱을 적용합니다.
        /// </summary>
        public FeatureDefinition? LoadFeatureFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            
            // 파일 경로와 마지막 수정 시간을 조합하여 캐시 키를 생성합니다.
            string key = $"{filePath}_{File.GetLastWriteTime(filePath).Ticks}";
            if (_featureCache.TryGetValue(key, out var cached)) return cached;

            try
            {
                var json = File.ReadAllText(filePath);
                var feature = JsonConvert.DeserializeObject<FeatureDefinition>(json);
                if (feature != null)
                {
                    _featureCache.TryAdd(key, feature);
                }
                return feature;
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"Failed to load feature from {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 특정 디렉토리 내의 모든 특징 정의 파일(.json)을 로드합니다.
        /// </summary>
        public IEnumerable<FeatureDefinition> LoadFeatures(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return Enumerable.Empty<FeatureDefinition>();

            var features = new List<FeatureDefinition>();
            foreach (var file in Directory.GetFiles(directoryPath, "*.json"))
            {
                var feature = LoadFeatureFromFile(file);
                if (feature != null)
                {
                    features.Add(feature);
                }
            }
            return features;
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
