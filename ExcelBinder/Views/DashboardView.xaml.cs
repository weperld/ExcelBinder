using System.Windows;
using System.Windows.Controls;
using ExcelBinder.Models;
using ExcelBinder.ViewModels;

namespace ExcelBinder.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void FeatureContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is not ContextMenu menu) return;
            if (menu.Tag is not FeatureDefinition feature) return;
            if (DataContext is not MainViewModel vm) return;

            menu.Items.Clear();

            if (vm.CustomGroups.Count == 0)
            {
                menu.Items.Add(new MenuItem
                {
                    Header = "(그룹이 없습니다)",
                    IsEnabled = false
                });
                return;
            }

            var header = new MenuItem { Header = ProjectConstants.UI.MenuAddToGroup };
            foreach (var group in vm.CustomGroups)
            {
                bool isMember = group.FeatureIds.Contains(feature.Id);
                var item = new MenuItem
                {
                    Header = group.Name,
                    IsCheckable = true,
                    IsChecked = isMember,
                    StaysOpenOnClick = true,
                    Command = vm.ToggleFeatureGroupMembershipCommand,
                    CommandParameter = new MainViewModel.FeatureGroupToggleArgs
                    {
                        Group = group,
                        Feature = feature
                    }
                };
                header.Items.Add(item);
            }
            menu.Items.Add(header);
        }
    }
}
