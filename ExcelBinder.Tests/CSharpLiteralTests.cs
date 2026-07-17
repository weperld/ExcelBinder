using ExcelBinder.Services;
using Xunit;

namespace ExcelBinder.Tests
{
    public class CSharpLiteralTests
    {
        [Theory]
        [InlineData("hello", "\"hello\"")]
        [InlineData("He said \"hi\"", "\"He said \\\"hi\\\"\"")]
        [InlineData(@"C:\temp", "\"C:\\\\temp\"")]
        [InlineData("line1\nline2", "\"line1\\nline2\"")]
        [InlineData("tab\there", "\"tab\\there\"")]
        [InlineData("", "\"\"")]
        public void Escape_ProducesCompilableLiteral(string input, string expected)
        {
            Assert.Equal(expected, CSharpLiteral.Escape(input));
        }

        [Theory]
        [InlineData("MaxHp", true)]
        [InlineData("_private", true)]
        [InlineData("이름한글", true)]   // C#은 유니코드 문자 식별자 허용
        [InlineData("Value2", true)]
        [InlineData("2Value", false)]
        [InlineData("Max Hp", false)]
        [InlineData("Max-Hp", false)]
        [InlineData("", false)]
        public void IsValidIdentifier_Cases(string name, bool expected)
        {
            Assert.Equal(expected, CSharpLiteral.IsValidIdentifier(name));
        }
    }
}
