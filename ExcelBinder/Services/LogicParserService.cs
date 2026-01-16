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
            if (dataList.Count < 1) return new { };

            var header = dataList[0];
            var rows = dataList.Skip(1);

            int nameIdx = Array.IndexOf(header, "Name");
            int returnTypeIdx = Array.IndexOf(header, "ReturnType");
            int paramsIdx = Array.IndexOf(header, "Parameters");
            int formulaIdx = Array.IndexOf(header, "Formula");

            if (nameIdx == -1 || formulaIdx == -1)
                throw new Exception("Logic sheet must contain 'Name' and 'Formula' columns.");

            var methods = rows.Select(row =>
            {
                string name = row[nameIdx];
                string returnType = returnTypeIdx != -1 && returnTypeIdx < row.Length ? row[returnTypeIdx] : "void";
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
