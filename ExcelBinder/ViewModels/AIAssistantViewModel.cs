using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class AIAssistantViewModel : ViewModelBase
    {
        private readonly AIService _aiService = new();
        private readonly FeatureDefinition _feature;
        private readonly AppSettings _settings;

        private string _userPrompt = string.Empty;
        private string _generatedTemplate = string.Empty;
        private bool _isBusy;
        private string _statusMessage = "무엇을 도와드릴까요?";

        public string UserPrompt
        {
            get => _userPrompt;
            set => SetProperty(ref _userPrompt, value);
        }

        public string GeneratedTemplate
        {
            get => _generatedTemplate;
            set => SetProperty(ref _generatedTemplate, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

        public ICommand SendCommand { get; }
        public ICommand ApplyCommand { get; }

        public event Action<string> OnTemplateApplied;

        public AIAssistantViewModel(FeatureDefinition feature, AppSettings settings)
        {
            _feature = feature;
            _settings = settings;

            SendCommand = new RelayCommand(async () => await ExecuteSend());
            ApplyCommand = new RelayCommand(ExecuteApply);

            ChatHistory.Add(new ChatMessage { Role = "Assistant", Content = $"안녕하세요! '{_feature.Name}' 기능을 위한 템플릿 생성을 도와드릴게요. 원하는 스타일이나 요구사항을 말씀해 주세요. (예: '유니티 전용 데이터 클래스로 짜줘')" });
        }

        private async Task ExecuteSend()
        {
            if (string.IsNullOrWhiteSpace(UserPrompt) || IsBusy) return;

            string prompt = UserPrompt;
            UserPrompt = string.Empty;
            ChatHistory.Add(new ChatMessage { Role = "User", Content = prompt });

            IsBusy = true;
            StatusMessage = "AI가 템플릿을 생성 중입니다...";

            try
            {
                // Prepare schema context
                string schemaContext = "현재 설정된 정보가 없습니다. (스키마 경로를 확인해 주세요)";
                
                // Try to find schema files in the feature's schema path
                if (!string.IsNullOrEmpty(_feature.SchemaPath) && Directory.Exists(_feature.SchemaPath))
                {
                    var schemaFiles = Directory.GetFiles(_feature.SchemaPath, "*.json");
                    if (schemaFiles.Length > 0)
                    {
                        // Prefer schema file that matches feature ID if exists
                        string? targetSchema = schemaFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(_feature.Id, StringComparison.OrdinalIgnoreCase)) 
                                               ?? schemaFiles[0];

                        schemaContext = File.ReadAllText(targetSchema);
                    }
                }

                string systemPrompt = _aiService.GetSystemPromptForTemplate(_feature.Category, schemaContext);
                
                // Add chat history to user prompt for context
                string combinedUserPrompt = prompt;
                if (ChatHistory.Count > 2) // More than just welcome and current user prompt
                {
                   // Logic to include history could be added here
                }

                string apiKey = _settings.AiModel.StartsWith("claude-") ? _settings.ClaudeApiKey : _settings.OpenAiApiKey;
                string result = await _aiService.GenerateTemplateAsync(apiKey, _settings.AiModel, systemPrompt, combinedUserPrompt);
                
                GeneratedTemplate = result;
                ChatHistory.Add(new ChatMessage { Role = "Assistant", Content = "템플릿 생성이 완료되었습니다! 오른쪽 미리보기를 확인해 보세요." });
                StatusMessage = "완료되었습니다.";
            }
            catch (Exception ex)
            {
                ChatHistory.Add(new ChatMessage { Role = "Assistant", Content = $"오류가 발생했습니다: {ex.Message}" });
                StatusMessage = "오류 발생";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteApply()
        {
            if (string.IsNullOrEmpty(GeneratedTemplate)) return;
            OnTemplateApplied?.Invoke(GeneratedTemplate);
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public HorizontalAlignment Alignment => Role == "User" ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        public System.Windows.Media.Brush Background => Role == "User" ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.LightGray;
    }
}
