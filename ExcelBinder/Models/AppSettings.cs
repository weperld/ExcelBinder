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
    }
}
