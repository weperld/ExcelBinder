using ExcelBinder.Services;
using Xunit;

namespace ExcelBinder.Tests
{
    public class TypeParserTests
    {
        [Fact]
        public void Primitive_UsesFieldNameAsColumn()
        {
            var info = TypeParser.ParseType("int", "Hp");
            Assert.Equal("int", info.BaseType);
            Assert.False(info.IsList);
            Assert.False(info.IsEnum);
            Assert.False(info.IsReference);
            Assert.Equal("Hp", info.ColumnName);
        }

        [Fact]
        public void List_Primitive()
        {
            var info = TypeParser.ParseType("List<int>", "Items");
            Assert.True(info.IsList);
            Assert.Equal("int", info.BaseType);
            Assert.Equal("Items", info.ColumnName);
        }

        [Fact]
        public void Enum_WithType()
        {
            var info = TypeParser.ParseType("enum:ElementType", "Element");
            Assert.True(info.IsEnum);
            Assert.Equal("ElementType", info.EnumType);
        }

        [Fact]
        public void Enum_WithoutType_HasNullEnumType()
        {
            var info = TypeParser.ParseType("enum", "Element");
            Assert.True(info.IsEnum);
            Assert.Null(info.EnumType);
        }

        [Fact]
        public void List_Enum()
        {
            var info = TypeParser.ParseType("List<enum:ElementType>", "Elements");
            Assert.True(info.IsList);
            Assert.True(info.IsEnum);
            Assert.Equal("ElementType", info.EnumType);
        }

        [Fact]
        public void Reference()
        {
            var info = TypeParser.ParseType("int:ref:StatData", "stat");
            Assert.True(info.IsReference);
            Assert.Equal("StatData", info.RefType);
            Assert.Equal("int", info.BaseType);
            Assert.Equal("stat", info.ColumnName); // 마지막 토큰이 RefType이므로 컬럼명은 필드명 유지
        }

        [Fact]
        public void CustomColumnName_LastTokenOverrides()
        {
            var info = TypeParser.ParseType("int:HP", "HealthPoint");
            Assert.Equal("int", info.BaseType);
            Assert.Equal("HP", info.ColumnName);
        }

        [Fact]
        public void Whitespace_IsTrimmed()
        {
            var info = TypeParser.ParseType(" List< int > ", " Items ");
            Assert.True(info.IsList);
            Assert.Equal("int", info.BaseType);
            Assert.Equal("Items", info.ColumnName);
        }
    }
}
