using System;
using System.Collections.Generic;
using Scriban;

namespace ExcelBinder.Services
{
    /// <summary>
    /// Scriban 템플릿 엔진을 사용하여 코드를 생성하는 서비스입니다.
    /// </summary>
    public class TemplateEngineService
    {
        // 템플릿 파싱 결과를 캐싱하여 반복적인 렌더링 성능을 최적화합니다.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(int Length, int Hash), Template> _templateCache = new();

        /// <summary>
        /// 템플릿 내용과 컨텍스트를 받아 렌더링된 문자열을 반환합니다.
        /// </summary>
        /// <param name="templateContent">Scriban 템플릿 문자열</param>
        /// <param name="context">템플릿에 전달할 데이터 객체</param>
        /// <returns>렌더링된 결과 문자열</returns>
        public string Render(string templateContent, object context)
        {
            // 캐시에서 먼저 확인하고, 없으면 파싱하여 저장합니다.
            var cacheKey = (templateContent.Length, templateContent.GetHashCode());
            if (!_templateCache.TryGetValue(cacheKey, out var template))
            {
                template = Template.Parse(templateContent);
                if (template.HasErrors)
                {
                    throw new Exception($"Template Error: {string.Join(", ", template.Messages)}");
                }
                _templateCache.TryAdd(cacheKey, template);
            }
            
            return template.Render(context);
        }
    }
}
