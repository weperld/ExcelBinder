using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using ExcelBinder.ViewModels;

namespace ExcelBinder.Models
{
    public class AppSettings : ViewModelBase
    {
        private string _featureDefinitionsPath = ProjectConstants.Defaults.FeatureDefinitionsPath;
        private string _lastFeatureId = string.Empty;
        private ObservableCollection<string> _boundFeatures = new();
        private bool _isBinaryChecked = true;
        private bool _isJsonChecked = false;
        private bool _checkForUpdatesOnStartup = true;
        private string _openAiApiKey = string.Empty;
        private string _claudeApiKey = string.Empty;
        private string _aiModel = ProjectConstants.AI.DefaultModel;

        [JsonProperty("featureDefinitionsPath")]
        public string FeatureDefinitionsPath 
        { 
            get => _featureDefinitionsPath; 
            set => SetProperty(ref _featureDefinitionsPath, value); 
        }

        [JsonProperty("lastFeatureId")]
        public string LastFeatureId 
        { 
            get => _lastFeatureId; 
            set => SetProperty(ref _lastFeatureId, value); 
        }
        
        [JsonProperty("boundFeatures")]
        public ObservableCollection<string> BoundFeatures 
        { 
            get => _boundFeatures; 
            set => SetProperty(ref _boundFeatures, value); 
        }

        [JsonProperty("checkForUpdatesOnStartup")]
        public bool CheckForUpdatesOnStartup
        {
            get => _checkForUpdatesOnStartup;
            set => SetProperty(ref _checkForUpdatesOnStartup, value);
        }

        [JsonProperty("isBinaryChecked")]
        public bool IsBinaryChecked
        {
            get => _isBinaryChecked;
            set => SetProperty(ref _isBinaryChecked, value);
        }

        [JsonProperty("isJsonChecked")]
        public bool IsJsonChecked
        {
            get => _isJsonChecked;
            set => SetProperty(ref _isJsonChecked, value);
        }

        // TODO: API 키가 settings.json에 평문으로 저장됩니다.
        // 향후 Windows DPAPI(ProtectedData)를 사용한 암호화 저장으로 전환을 권장합니다.
        [JsonProperty("aiApiKey")]
        public string OpenAiApiKey
        {
            get => _openAiApiKey;
            set => SetProperty(ref _openAiApiKey, value);
        }

        [JsonProperty("claudeApiKey")]
        public string ClaudeApiKey
        {
            get => _claudeApiKey;
            set => SetProperty(ref _claudeApiKey, value);
        }

        [JsonProperty("aiModel")]
        public string AiModel
        {
            get => _aiModel;
            set => SetProperty(ref _aiModel, value);
        }
    }
}
