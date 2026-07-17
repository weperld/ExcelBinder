using System.Text;

namespace ExcelBinder.Services
{
    /// <summary>C# 코드 생성 시 문자열 리터럴/식별자 안전성을 보장하는 헬퍼.</summary>
    public static class CSharpLiteral
    {
        /// <summary>값을 따옴표 포함 C# 문자열 리터럴로 이스케이프합니다.</summary>
        public static string Escape(string value)
        {
            var sb = new StringBuilder(value.Length + 2);
            sb.Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\0': sb.Append("\\0"); break;
                    default:
                        if (char.IsControl(c)) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>C# 식별자(상수명/enum 멤버명)로 유효한지 검사합니다.</summary>
        public static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (name[0] != '_' && !char.IsLetter(name[0])) return false;
            foreach (char c in name)
            {
                if (c != '_' && !char.IsLetterOrDigit(c)) return false;
            }
            return true;
        }
    }
}
