using System.Collections.Generic;
using System.Linq;
using ExcelBinder.Services;
using Xunit;

namespace ExcelBinder.Tests
{
    public class ExcelFilterTests
    {
        private readonly ExcelService _service = new();

        [Fact]
        public void HeaderOnly_ReturnsEmpty()
        {
            var rows = _service.GetFilteredData(new List<string[]> { new[] { "Id", "Value" } });
            Assert.Empty(rows);
        }

        [Fact]
        public void CommentRow_IsExcluded()
        {
            var data = new List<string[]>
            {
                new[] { "Id", "Value" },
                new[] { "1", "a" },
                new[] { "#2", "b" },
                new[] { "3", "c" },
            };
            var rows = _service.GetFilteredData(data);
            Assert.Equal(new[] { "1", "3" }, rows.Select(r => r.Data[0]));
        }

        [Fact]
        public void FirstValidColumn_SkipsCommentHeader()
        {
            // 첫 열 헤더가 #이면 두 번째 열이 행 필터 기준
            var data = new List<string[]>
            {
                new[] { "#Memo", "Id" },
                new[] { "#note", "1" },   // 첫 유효 열(Id)=1 → 포함
                new[] { "x", "#2" },      // 첫 유효 열(Id)=#2 → 제외
            };
            var rows = _service.GetFilteredData(data);
            Assert.Single(rows);
            Assert.Equal("1", rows[0].Data[1]);
        }

        [Fact]
        public void OriginalIndex_IsExcelOneBasedRowNumber()
        {
            var data = new List<string[]>
            {
                new[] { "Id" },
                new[] { "10" }, // 엑셀 2행
                new[] { "#x" },
                new[] { "20" }, // 엑셀 4행
            };
            var rows = _service.GetFilteredData(data);
            Assert.Equal(2, rows[0].OriginalIndex);
            Assert.Equal(4, rows[1].OriginalIndex);
        }

        [Fact]
        public void ShortRow_IsIncluded()
        {
            // 첫 유효 열보다 짧은 행은 필터하지 않고 포함
            var data = new List<string[]>
            {
                new[] { "#Memo", "Id" },
                new string[] { "only" },
            };
            var rows = _service.GetFilteredData(data);
            Assert.Single(rows);
        }

        [Fact]
        public void EmptyFirstCell_IsIncluded()
        {
            var data = new List<string[]>
            {
                new[] { "Id", "Value" },
                new[] { "", "b" },
            };
            var rows = _service.GetFilteredData(data);
            Assert.Single(rows);
        }
    }
}
