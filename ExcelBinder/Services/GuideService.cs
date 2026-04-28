using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    /// <summary>
    /// 인앱 사용자 가이드의 인덱스 및 콘텐츠 리소스를 로드하는 서비스입니다.
    /// </summary>
    public class GuideService
    {
        private const string IndexResourceName = "ExcelBinder.Resources.Guides._index.json";
        private const string ContentBaseUri = "pack://application:,,,/Resources/Guides/";

        public GuideIndex LoadIndex()
        {
            var asm = typeof(GuideService).Assembly;
            using var stream = asm.GetManifestResourceStream(IndexResourceName);
            if (stream == null)
            {
                LogService.Instance.Warning($"가이드 인덱스 리소스를 찾을 수 없습니다: {IndexResourceName}");
                return new GuideIndex();
            }
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<GuideIndex>(json) ?? new GuideIndex();
        }

        public FlowDocument? LoadTopicDocument(string resourceFileName)
        {
            if (string.IsNullOrWhiteSpace(resourceFileName)) return null;

            try
            {
                var uri = new Uri(ContentBaseUri + resourceFileName, UriKind.Absolute);
                var info = Application.GetResourceStream(uri);
                if (info == null)
                {
                    LogService.Instance.Warning($"가이드 콘텐츠 리소스 누락: {resourceFileName}");
                    return null;
                }
                using var stream = info.Stream;
                return XamlReader.Load(stream) as FlowDocument;
            }
            catch (Exception ex)
            {
                LogService.Instance.Warning($"가이드 콘텐츠 로드 실패 '{resourceFileName}': {ex.Message}");
                return null;
            }
        }
    }
}
