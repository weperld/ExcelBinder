using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using ExcelBinder.Services;
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

        [JsonProperty("aiApiKey")]
        public string OpenAiApiKeyEncrypted
        {
            get => CryptoHelper.Encrypt(_openAiApiKey);
            set => _openAiApiKey = CryptoHelper.Decrypt(value);
        }

        [JsonIgnore]
        public string OpenAiApiKey
        {
            get => _openAiApiKey;
            set => SetProperty(ref _openAiApiKey, value);
        }

        [JsonProperty("claudeApiKey")]
        public string ClaudeApiKeyEncrypted
        {
            get => CryptoHelper.Encrypt(_claudeApiKey);
            set => _claudeApiKey = CryptoHelper.Decrypt(value);
        }

        [JsonIgnore]
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
