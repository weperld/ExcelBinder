using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ExcelBinder.Models;
using ExcelBinder.Services;

namespace ExcelBinder.ViewModels
{
    public class GuideViewModel : ViewModelBase
    {
        private readonly GuideService _service = new();

        public ObservableCollection<GuideCategory> Categories { get; } = new();

        private GuideTopic? _selectedTopic;
        public GuideTopic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (SetProperty(ref _selectedTopic, value))
                {
                    LoadContent();
                }
            }
        }

        private FlowDocument? _currentDocument;
        public FlowDocument? CurrentDocument
        {
            get => _currentDocument;
            private set => SetProperty(ref _currentDocument, value);
        }

        private bool _isFirstRunMode;
        public bool IsFirstRunMode
        {
            get => _isFirstRunMode;
            set => SetProperty(ref _isFirstRunMode, value);
        }

        private bool _doNotShowAgain;
        public bool DoNotShowAgain
        {
            get => _doNotShowAgain;
            set => SetProperty(ref _doNotShowAgain, value);
        }

        public ICommand CloseCommand { get; }
        public event Action? RequestClose;

        public GuideViewModel(string? initialTopicId = null, bool isFirstRunMode = false)
        {
            IsFirstRunMode = isFirstRunMode;
            CloseCommand = new RelayCommand(() => RequestClose?.Invoke());

            var index = _service.LoadIndex();
            foreach (var cat in index.Categories)
            {
                Categories.Add(cat);
            }

            SelectedTopic = ResolveInitialTopic(initialTopicId);
        }

        private GuideTopic? ResolveInitialTopic(string? topicId)
        {
            if (!string.IsNullOrEmpty(topicId))
            {
                foreach (var c in Categories)
                {
                    var match = c.Topics.FirstOrDefault(t => t.Id == topicId);
                    if (match != null) return match;
                }
            }
            return Categories.FirstOrDefault()?.Topics.FirstOrDefault();
        }

        private void LoadContent()
        {
            if (_selectedTopic == null)
            {
                CurrentDocument = null;
                return;
            }

            var doc = _service.LoadTopicDocument(_selectedTopic.Resource)
                      ?? BuildFallbackDocument(_selectedTopic.Title);
            ApplyReadabilityDefaults(doc);
            ApplySectionAlternation(doc);
            CurrentDocument = doc;
        }

        /// <summary>
        /// 본문의 각 H2 섹션을 두 가지 옅은 배경색으로 교차 표시합니다.
        /// 섹션 경계는 FontWeight=SemiBold이며 FontSize 18~22인 Paragraph로 식별합니다.
        /// </summary>
        private static void ApplySectionAlternation(FlowDocument doc)
        {
            var bgA = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC));
            var bgB = new SolidColorBrush(Color.FromRgb(0xEE, 0xF2, 0xF8));

            var blocks = doc.Blocks.ToList();
            var preamble = new List<Block>();
            var sections = new List<List<Block>>();
            List<Block>? current = null;

            foreach (var b in blocks)
            {
                if (IsSectionHeader(b))
                {
                    if (current != null) sections.Add(current);
                    current = new List<Block> { b };
                }
                else if (current != null)
                {
                    current.Add(b);
                }
                else
                {
                    preamble.Add(b);
                }
            }
            if (current != null) sections.Add(current);

            if (sections.Count == 0) return;

            doc.Blocks.Clear();
            foreach (var b in preamble) doc.Blocks.Add(b);

            for (int i = 0; i < sections.Count; i++)
            {
                var section = new Section
                {
                    Background = (i % 2 == 0) ? bgA : bgB,
                    Padding = new Thickness(20, 8, 20, 12),
                    Margin = new Thickness(0, 4, 0, 0)
                };
                foreach (var b in sections[i]) section.Blocks.Add(b);
                doc.Blocks.Add(section);
            }
        }

        private static bool IsSectionHeader(Block b)
        {
            return b is Paragraph p
                && p.FontWeight == FontWeights.SemiBold
                && p.FontSize >= 18
                && p.FontSize <= 22;
        }

        /// <summary>
        /// 본문 가독성 강화: 폰트 크기, 행간, 가독성 최적화, 컬럼 폭 제한.
        /// 콘텐츠 파일에 명시된 개별 FontSize는 유지되며, 루트 기본값만 덮어씁니다.
        /// </summary>
        private static void ApplyReadabilityDefaults(FlowDocument doc)
        {
            doc.FontSize = 16;
            doc.LineHeight = 28;
            doc.ColumnWidth = 960;
            doc.PagePadding = new Thickness(32, 24, 32, 24);
            doc.IsOptimalParagraphEnabled = true;
            doc.IsHyphenationEnabled = false;
            doc.TextAlignment = TextAlignment.Left;
            doc.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x1F, 0x1F, 0x1F));
        }

        private static FlowDocument BuildFallbackDocument(string title)
        {
            var doc = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 14,
                PagePadding = new Thickness(0)
            };
            var heading = new Paragraph(new Run($"'{title}' {ProjectConstants.UI.MsgGuideMissing}"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 18
            };
            doc.Blocks.Add(heading);
            doc.Blocks.Add(new Paragraph(new Run("리소스 누락 또는 형식 오류일 수 있습니다.")));
            return doc;
        }
    }
}
