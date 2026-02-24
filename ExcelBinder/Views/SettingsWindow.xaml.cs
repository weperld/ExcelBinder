using System.Diagnostics;
using System.Windows;
using ExcelBinder.Models;

namespace ExcelBinder.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnGitHubClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(ProjectConstants.Update.ReleasesPageUrl) { UseShellExecute = true });
        }
    }
}
