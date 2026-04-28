using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ExcelBinder.Models;
using ExcelBinder.ViewModels;

namespace ExcelBinder.Views
{
    public partial class GuideWindow : Window
    {
        private const double ScrollSpeedMultiplier = 1.5;
        private ScrollViewer? _docScrollViewer;

        public GuideWindow()
        {
            InitializeComponent();
        }

        private void TopicTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is not GuideViewModel vm) return;
            // 카테고리 노드를 선택해도 본문이 사라지지 않도록 GuideTopic 선택일 때만 반영
            if (e.NewValue is GuideTopic topic)
            {
                vm.SelectedTopic = topic;
            }
        }

        /// <summary>
        /// 본문 영역의 휠 스크롤 속도를 1.5배로 증가시킵니다.
        /// FlowDocumentScrollViewer의 내부 ScrollViewer를 직접 제어합니다.
        /// </summary>
        private void DocumentViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _docScrollViewer ??= FindVisualChild<ScrollViewer>(DocumentViewer);
            if (_docScrollViewer == null) return;

            int totalLines = Math.Max(1, (int)Math.Round(SystemParameters.WheelScrollLines * ScrollSpeedMultiplier));
            if (e.Delta > 0)
            {
                for (int i = 0; i < totalLines; i++) _docScrollViewer.LineUp();
            }
            else if (e.Delta < 0)
            {
                for (int i = 0; i < totalLines; i++) _docScrollViewer.LineDown();
            }
            e.Handled = true;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;
                var deeper = FindVisualChild<T>(child);
                if (deeper != null) return deeper;
            }
            return null;
        }

        /// <summary>
        /// 특정 토픽을 선택한 상태로 가이드 윈도우를 모달리스로 표시합니다.
        /// 진행 중인 페이지 작업을 중단하지 않고 가이드를 참조할 수 있습니다.
        /// </summary>
        public static void ShowForTopic(string topicId, Window? owner = null)
        {
            var vm = new GuideViewModel(topicId);
            var win = new GuideWindow
            {
                DataContext = vm,
                Owner = owner ?? Application.Current.MainWindow
            };
            vm.RequestClose += () => win.Close();
            win.Show();
        }
    }
}
