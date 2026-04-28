using System.Windows;
using System.Windows.Controls;

namespace ExcelBinder.Views
{
    public partial class FeatureBuilderView : UserControl
    {
        public FeatureBuilderView()
        {
            InitializeComponent();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string topicId) return;
            GuideWindow.ShowForTopic(topicId, Window.GetWindow(this));
        }
    }
}
