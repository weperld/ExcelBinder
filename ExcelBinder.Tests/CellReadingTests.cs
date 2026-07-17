using System;
using System.IO;
using ExcelBinder.Services;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Xunit;

namespace ExcelBinder.Tests
{
    /// <summary>
    /// NPOI CellType 기반 셀 읽기 실동작 검증: NPOI로 직접 만든 xlsx를
    /// ExcelService.ReadExcel로 왕복해 수식/날짜/숫자/불리언 처리를 확인한다.
    /// </summary>
    public class CellReadingTests
    {
        private static string CreateWorkbook(Action<ISheet> build)
        {
            string dir = Path.Combine(Path.GetTempPath(), "ExcelBinderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "cells.xlsx");

            var workbook = new XSSFWorkbook();
            try
            {
                var sheet = workbook.CreateSheet("Sheet1");
                build(sheet);

                // 수식 캐시 결과 생성 (엑셀이 저장 시 캐시를 남기는 것과 동일한 상태)
                XSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

                using var fs = new FileStream(path, FileMode.Create);
                workbook.Write(fs);
            }
            finally
            {
                workbook.Dispose();
            }
            return path;
        }

        [Fact]
        public void NumericCell_ReadsInvariantString()
        {
            string path = CreateWorkbook(sheet =>
            {
                var row = sheet.CreateRow(0);
                row.CreateCell(0).SetCellValue(42.0);
                row.CreateCell(1).SetCellValue(3.14);
                row.CreateCell(2).SetCellValue(-0.5);
            });

            var rows = new ExcelService().ReadExcel(path);
            Assert.Equal("42", rows[0][0]);    // 정수값 double은 소수점 없이
            Assert.Equal("3.14", rows[0][1]);  // 로케일 무관 '.' 소수점
            Assert.Equal("-0.5", rows[0][2]);
        }

        [Fact]
        public void FormulaCell_ReadsCachedResult_NotFormulaText()
        {
            string path = CreateWorkbook(sheet =>
            {
                var row = sheet.CreateRow(0);
                row.CreateCell(0).SetCellValue(10.0);
                row.CreateCell(1).SetCellValue(20.0);
                row.CreateCell(2).SetCellFormula("A1+B1");
                row.CreateCell(3).SetCellFormula("CONCATENATE(\"a\",\"b\")");
            });

            var rows = new ExcelService().ReadExcel(path);
            Assert.Equal("30", rows[0][2]);   // 수식 텍스트 "A1+B1"이 아닌 계산 값
            Assert.Equal("ab", rows[0][3]);
        }

        [Fact]
        public void BooleanCell_ReadsLowercase()
        {
            string path = CreateWorkbook(sheet =>
            {
                var row = sheet.CreateRow(0);
                row.CreateCell(0).SetCellValue(true);
                row.CreateCell(1).SetCellValue(false);
            });

            var rows = new ExcelService().ReadExcel(path);
            Assert.Equal("true", rows[0][0]);
            Assert.Equal("false", rows[0][1]);
        }

        [Fact]
        public void DateCell_ReadsInvariantFormat()
        {
            string path = CreateWorkbook(sheet =>
            {
                var row = sheet.CreateRow(0);
                var cell = row.CreateCell(0);
                cell.SetCellValue(new DateTime(2026, 7, 17, 14, 30, 0));
                var style = sheet.Workbook.CreateCellStyle();
                style.DataFormat = sheet.Workbook.CreateDataFormat().GetFormat("yyyy-mm-dd");
                cell.CellStyle = style;
            });

            var rows = new ExcelService().ReadExcel(path);
            Assert.Equal("2026-07-17 14:30:00", rows[0][0]);
        }

        [Fact]
        public void BlankAndMissingCells_ReadAsEmpty()
        {
            string path = CreateWorkbook(sheet =>
            {
                var row = sheet.CreateRow(0);
                row.CreateCell(0).SetCellValue("a");
                row.CreateCell(1); // Blank 셀
                row.CreateCell(2).SetCellValue("c");
            });

            var rows = new ExcelService().ReadExcel(path);
            Assert.Equal("a", rows[0][0]);
            Assert.Equal("", rows[0][1]);
            Assert.Equal("c", rows[0][2]);
        }

        [Fact]
        public void EmptyRow_DoesNotCrash()
        {
            string path = CreateWorkbook(sheet =>
            {
                sheet.CreateRow(0).CreateCell(0).SetCellValue("a");
                sheet.CreateRow(1); // 셀 없는 행: LastCellNum == -1
                sheet.CreateRow(2).CreateCell(0).SetCellValue("b");
            });

            var rows = new ExcelService().ReadExcel(path);
            Assert.Equal(3, rows.Count);
            Assert.Empty(rows[1]);
        }
    }
}
