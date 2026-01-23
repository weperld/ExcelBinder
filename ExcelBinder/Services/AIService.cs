using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    /// <summary>
    /// LLM(OpenAI, Claude)을 사용하여 템플릿 코드를 자동 생성하는 서비스입니다.
    /// </summary>
    public class AIService
    {
        // 소켓 고갈 방지를 위해 HttpClient를 정적 멤버로 공유합니다.
        private static readonly HttpClient _httpClient = new();
        private const string OpenAIEndpoint = "https://api.openai.com/v1/chat/completions";
        private const string AnthropicEndpoint = "https://api.anthropic.com/v1/messages";

        /// <summary>
        /// AI 모델을 사용하여 템플릿 내용을 생성합니다.
        /// </summary>
        public async Task<string> GenerateTemplateAsync(string apiKey, string model, string systemPrompt, string userPrompt)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception("API Key가 설정되지 않았습니다.");

            if (model.StartsWith("claude-"))
            {
                return await GenerateClaudeTemplateAsync(apiKey, model, systemPrompt, userPrompt);
            }
            else
            {
                return await GenerateOpenAiTemplateAsync(apiKey, model, systemPrompt, userPrompt);
            }
        }

        private async Task<string> GenerateOpenAiTemplateAsync(string apiKey, string model, string systemPrompt, string userPrompt)
        {
            bool isReasoningModel = model.StartsWith("o1") || model.StartsWith("o3") || model.StartsWith("gpt-5");
            
            var messages = new List<object>();
            if (isReasoningModel)
            {
                messages.Add(new { role = "developer", content = systemPrompt });
            }
            else
            {
                messages.Add(new { role = "system", content = systemPrompt });
            }
            messages.Add(new { role = "user", content = userPrompt });

            object requestBody;
            if (isReasoningModel)
            {
                requestBody = new
                {
                    model = model,
                    messages = messages
                };
            }
            else
            {
                requestBody = new
                {
                    model = model,
                    messages = messages,
                    temperature = 0.3
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, OpenAIEndpoint);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"OpenAI API 오류: {response.StatusCode}\n{responseString}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
            return result.choices[0].message.content.ToString();
        }

        private async Task<string> GenerateClaudeTemplateAsync(string apiKey, string model, string systemPrompt, string userPrompt)
        {
            var requestBody = new
            {
                model = model,
                max_tokens = 4096,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.3
            };

            var request = new HttpRequestMessage(HttpMethod.Post, AnthropicEndpoint);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Claude API 오류: {response.StatusCode}\n{responseString}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
            return result.content[0].text.ToString();
        }

        public string GetSystemPromptForTemplate(string category, string schemaJson)
        {
            var sb = new StringBuilder();
            sb.AppendLine("너는 Scriban(.liquid) 템플릿 엔진 전문가이자 숙련된 소프트웨어 엔지니어다.");
            sb.AppendLine("사용자의 요청에 따라 엑셀 데이터를 코드로 변환하기 위한 Scriban 템플릿을 작성하라.");
            sb.AppendLine();
            sb.AppendLine("### 사용 가능한 공통 변수:");
            sb.AppendLine("- {{ namespace }}: 출력 파일의 네임스페이스");
            sb.AppendLine("- {{ class_name }}: 클래스 이름");
            sb.AppendLine();

            if (category == "StaticData")
            {
                sb.AppendLine("### StaticData 전용 변수:");
                sb.AppendLine("- {{ key }}: 테이블의 기본 키(Key) 이름");
                sb.AppendLine("- {{ key_type }}: 기본 키의 타입");
                sb.AppendLine("- {{ fields }}: 필드 리스트. 각 필드는 다음 속성을 가짐:");
                sb.AppendLine("  - name: 필드명");
                sb.AppendLine("  - type: 데이터 타입");
                sb.AppendLine("  - is_list: 리스트 여부 (bool)");
                sb.AppendLine("  - is_reference: 참조 타입 여부 (bool)");
                sb.AppendLine("  - read_method: BinaryReader에서 사용할 읽기 메서드 (예: reader.ReadInt32())");
            }
            else if (category == "Logic")
            {
                sb.AppendLine("### Logic 전용 변수:");
                sb.AppendLine("- {{ methods }}: 메서드 리스트. 각 메서드는 다음 속성을 가짐:");
                sb.AppendLine("  - name: 메서드 이름");
                sb.AppendLine("  - return_type: 반환 타입");
                sb.AppendLine("  - params_decl: 매개변수 선언문 (예: \"int id, string name\")");
                sb.AppendLine("  - formula: 변환된 C# 수식 (예: \"id + 100\")");
                sb.AppendLine();
                sb.AppendLine("### Logic 템플릿 예시 (Scriban 문법):");
                sb.AppendLine("{% for method in methods %} public {{ method.return_type }} {{ method.name }}({{ method.params_decl }}) { return {{ method.formula }}; } {% end %}");
            }

            sb.AppendLine();
            sb.AppendLine("### 대상 스키마 구조 (JSON):");
            sb.AppendLine(schemaJson);
            sb.AppendLine();
            sb.AppendLine("### 규칙:");
            sb.AppendLine("1. 출력은 오직 Scriban 템플릿 코드만 포함해야 하며, 설명이나 마크다운 코드 블록(```)은 제거하라.");
            sb.AppendLine("2. 제공된 변수명을 정확히 사용하라.");
            sb.AppendLine("3. 사용자가 특정 플랫폼(예: Unity)이나 스타일을 요청하면 그에 맞춰 작성하라.");

            return sb.ToString();
        }
    }
}
