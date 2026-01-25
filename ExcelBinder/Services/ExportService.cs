using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    /// <summary>
    /// 스키마 정의에 따라 엑셀 데이터를 바이너리 또는 JSON 형식으로 내보내는 서비스입니다.
    /// </summary>
    public class ExportService
    {
        private readonly ExcelService _excelService = new();

        /// <summary>
        /// 엑셀 데이터를 바이너리 형식으로 변환하여 저장합니다.
        /// </summary>
        public void ExportToBinary(SchemaDefinition schema, IEnumerable<string[]> excelData, string outputPath, FeatureDefinition feature)
        {
            var dataList = excelData as List<string[]> ?? excelData.ToList();
            if (dataList.Count < 1) return; // 최소 1행(헤더) 필요

            var header = dataList[ProjectConstants.Excel.HeaderRowIndex];
            var headerMap = CreateHeaderMap(header);

            var validRows = _excelService.GetFilteredData(dataList);

            using var stream = new FileStream(outputPath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            writer.Write(validRows.Count);

            foreach (var item in validRows)
            {
                foreach (var field in schema.Fields)
                {
                    try
                    {
                        WriteField(writer, field.Value, item.Data, header, headerMap, field.Key, feature);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[Row {item.OriginalIndex}] Column '{field.Key}' error: {ex.Message}");
                    }
                }
            }
        }

        private Dictionary<string, int> CreateHeaderMap(string[] header)
        {
            return header.Select((h, i) => new { h = h?.Trim(), i })
                         .Where(x => !string.IsNullOrEmpty(x.h))
                         .GroupBy(x => x.h, StringComparer.OrdinalIgnoreCase)
                         .ToDictionary(g => g.Key, g => g.First().i, StringComparer.OrdinalIgnoreCase);
        }

        private int GetColumnIndex(Dictionary<string, int> headerMap, string columnName, string fieldName)
        {
            if (headerMap.TryGetValue(columnName, out int idx)) return idx;

            var trimmedFieldName = fieldName.Trim();
            if (headerMap.TryGetValue(trimmedFieldName, out idx)) return idx;

            throw new Exception($"Column '{columnName}' (or '{trimmedFieldName}') not found in excel header. Please check your schema or excel file.");
        }

        private void WriteField(BinaryWriter writer, string type, string[] row, string[] header, Dictionary<string, int> headerMap, string fieldName, FeatureDefinition feature)
        {
            var info = TypeParser.ParseType(type, fieldName);
            if (info.IsList)
            {
                var indices = Enumerable.Range(0, header.Length)
                    .Where(i => string.Equals(header[i]?.Trim(), info.ColumnName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                writer.Write(indices.Count);
                foreach (var idx in indices)
                {
                    WritePrimitive(writer, info.BaseType, row[idx]);
                }
            }
            else
            {
                int idx = GetColumnIndex(headerMap, info.ColumnName, fieldName);
                string value = idx < row.Length ? row[idx] : "";
                WritePrimitive(writer, info.BaseType, value);
            }
        }

        private void WritePrimitive(BinaryWriter writer, string type, string value)
        {
            var parsed = ParsePrimitive(type, value);
            switch (type)
            {
                case ProjectConstants.Types.Int: writer.Write((int)parsed); break;
                case ProjectConstants.Types.Float: writer.Write((float)parsed); break;
                case ProjectConstants.Types.String: writer.Write((string)parsed); break;
                case ProjectConstants.Types.Bool: writer.Write((bool)parsed); break;
                case ProjectConstants.Types.Long: writer.Write((long)parsed); break;
                case ProjectConstants.Types.Double: writer.Write((double)parsed); break;
                case ProjectConstants.Types.UInt: writer.Write((uint)parsed); break;
                case ProjectConstants.Types.ULong: writer.Write((ulong)parsed); break;
                case ProjectConstants.Types.Short: writer.Write((short)parsed); break;
                case ProjectConstants.Types.Byte: writer.Write((byte)parsed); break;
                default: writer.Write(parsed?.ToString() ?? ""); break;
            }
        }

        public void ExportToJson(SchemaDefinition schema, IEnumerable<string[]> excelData, string outputPath, FeatureDefinition feature)
        {
            var dataList = excelData as List<string[]> ?? excelData.ToList();
            if (dataList.Count < 1) return;

            var header = dataList[ProjectConstants.Excel.HeaderRowIndex];
            var headerMap = CreateHeaderMap(header);

            var validRows = _excelService.GetFilteredData(dataList);

            var result = new List<Dictionary<string, object>>();

            foreach (var item in validRows)
            {
                var rowDict = new Dictionary<string, object>();
                foreach (var field in schema.Fields)
                {
                    try
                    {
                        rowDict[field.Key] = ParseField(field.Value, item.Data, header, headerMap, field.Key, feature);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[Row {item.OriginalIndex}] Column '{field.Key}' error: {ex.Message}");
                    }
                }
                result.Add(rowDict);
            }

            File.WriteAllText(outputPath, JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        private object ParseField(string type, string[] row, string[] header, Dictionary<string, int> headerMap, string fieldName, FeatureDefinition feature)
        {
            var info = TypeParser.ParseType(type, fieldName);
            if (info.IsList)
            {
                var indices = Enumerable.Range(0, header.Length)
                    .Where(i => string.Equals(header[i]?.Trim(), info.ColumnName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var list = new List<object>();
                foreach (var idx in indices)
                {
                    list.Add(ParsePrimitive(info.BaseType, row[idx]));
                }
                return list;
            }
            else
            {
                int idx = GetColumnIndex(headerMap, info.ColumnName, fieldName);
                string value = idx < row.Length ? row[idx] : "";
                return ParsePrimitive(info.BaseType, value);
            }
        }

        private object ParsePrimitive(string type, string value)
        {
            try
            {
                return type switch
                {
                    ProjectConstants.Types.Int => ParseInt(value),
                    ProjectConstants.Types.Float => float.Parse(value),
                    ProjectConstants.Types.Bool => bool.Parse(value),
                    ProjectConstants.Types.Long => long.Parse(value),
                    ProjectConstants.Types.Double => double.Parse(value),
                    ProjectConstants.Types.UInt => uint.Parse(value),
                    ProjectConstants.Types.ULong => ulong.Parse(value),
                    ProjectConstants.Types.Short => short.Parse(value),
                    ProjectConstants.Types.Byte => byte.Parse(value),
                    _ => value ?? ""
                };
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.Contains("Type Mismatch")))
            {
                throw new Exception($"Cannot parse '{value}' as {type}. {ex.Message}");
            }
        }

        private int ParseInt(string value)
        {
            if (float.TryParse(value, out float f) && f != (int)f)
                throw new Exception($"Type Mismatch: Expected int but got float-like value '{value}'");
            return int.Parse(value);
        }
    }
}
