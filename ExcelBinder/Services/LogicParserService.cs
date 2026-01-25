using System;
using System.Collections.Generic;
using System.Linq;
using ExcelBinder.Models;
using NPOI.SS.UserModel;

namespace ExcelBinder.Services
{
    public class LogicParserService
    {
        public object PrepareLogicContext(IEnumerable<string[]> excelData, FeatureDefinition feature, string nameSpace, string className)
        {
            var dataList = excelData.ToList();
            // 전역 규칙: 첫 번째 행 헤더, 두 번째 행 이후 데이터. 최소 1행 필요.
            if (dataList.Count < ProjectConstants.Excel.DataStartRowIndex) return new { };

            var header = dataList[ProjectConstants.Excel.HeaderRowIndex]; // 첫 번째 행을 헤더로 사용
            var rows = dataList.Skip(ProjectConstants.Excel.DataStartRowIndex); // 두 번째 행부터 실제 데이터

            int nameIdx = Array.IndexOf(header, ProjectConstants.Excel.LogicColumns.Name);
            int returnTypeIdx = Array.IndexOf(header, ProjectConstants.Excel.LogicColumns.ReturnType);
            int paramsIdx = Array.IndexOf(header, ProjectConstants.Excel.LogicColumns.Parameters);
            int formulaIdx = Array.IndexOf(header, ProjectConstants.Excel.LogicColumns.Formula);

            if (nameIdx == -1 || formulaIdx == -1)
                throw new Exception($"Logic sheet must contain '{ProjectConstants.Excel.LogicColumns.Name}' and '{ProjectConstants.Excel.LogicColumns.Formula}' columns.");

            var methods = rows.Select(row =>
            {
                string name = row[nameIdx];
                string returnType = returnTypeIdx != -1 && returnTypeIdx < row.Length ? row[returnTypeIdx] : ProjectConstants.Types.Void;
                string parameters = paramsIdx != -1 && paramsIdx < row.Length ? row[paramsIdx] : "";
                string formula = row[formulaIdx];

                // Simple excel formula to C# translation (very basic)
                if (formula.StartsWith("=")) formula = formula.Substring(1);

                return new
                {
                    Name = name,
                    ReturnType = returnType,
                    ParamsDecl = parameters,
                    Formula = formula
                };
            }).ToList();

            return new
            {
                Namespace = nameSpace,
                ClassName = className,
                Methods = methods
            };
        }
    }
}
