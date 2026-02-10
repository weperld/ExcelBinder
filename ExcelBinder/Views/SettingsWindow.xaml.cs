using System.Windows;
using ExcelBinder.Services;

namespace ExcelBinder.Views
{
    public partial class SettingsWindow : Window, IPasswordProvider
    {
        public string OpenAiApiKey => ApiKeyBox.Password;
        public string ClaudeApiKey => ClaudeApiKeyBox.Password;

        public SettingsWindow()
        {
            InitializeComponent();
        }
    }
}
