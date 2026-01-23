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
            if (dataList.Count < 1) return;

            // 헤더 컬럼 위치를 캐싱하여 검색 성능을 최적화합니다.
            var header = dataList[0];
            var headerMap = header.Select((h, i) => new { h, i })
                                 .Where(x => !string.IsNullOrEmpty(x.h))
                                 .GroupBy(x => x.h)
                                 .ToDictionary(g => g.Key, g => g.First().i);

            var validRows = _excelService.GetFilteredData(dataList);

            using var stream = new FileStream(outputPath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            // 전체 행 개수를 먼저 기록합니다.
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

        /// <summary>
        /// 개별 필드 데이터를 타입에 맞춰 바이너리로 기록합니다.
        /// </summary>
        private void WriteField(BinaryWriter writer, string type, string[] row, string[] header, Dictionary<string, int> headerMap, string fieldName, FeatureDefinition feature)
        {
            var info = TypeParser.ParseType(type, fieldName);
            if (info.IsList)
            {
                // 리스트 타입인 경우 동일한 이름을 가진 모든 컬럼의 데이터를 수집합니다.
                var indices = Enumerable.Range(0, header.Length).Where(i => header[i] == info.ColumnName).ToList();
                writer.Write(indices.Count);
                foreach (var idx in indices)
                {
                    WritePrimitive(writer, info.BaseType, row[idx]);
                }
            }
            else
            {
                // 단일 타입인 경우 맵을 통해 빠르게 위치를 찾습니다.
                if (!headerMap.TryGetValue(info.ColumnName, out int idx))
                {
                    headerMap.TryGetValue(fieldName, out idx); // Fallback
                }
                
                string value = idx != 0 && idx < row.Length ? row[idx] : (idx == 0 && row.Length > 0 ? row[0] : "");
                if (idx >= row.Length) value = "";

                WritePrimitive(writer, info.BaseType, value);
            }
        }

        private void WritePrimitive(BinaryWriter writer, string type, string value)
        {
            try
            {
                switch (type)
                {
                    case ProjectConstants.Types.Int:
                        if (float.TryParse(value, out float f) && f != (int)f) 
                            throw new Exception($"Type Mismatch: Expected int but got float-like value '{value}'");
                        writer.Write(int.Parse(value));
                        break;
                    case ProjectConstants.Types.Float: writer.Write(float.Parse(value)); break;
                    case ProjectConstants.Types.String: writer.Write(value ?? ""); break;
                    case ProjectConstants.Types.Bool: writer.Write(bool.Parse(value)); break;
                    case ProjectConstants.Types.Long: writer.Write(long.Parse(value)); break;
                    case ProjectConstants.Types.Double: writer.Write(double.Parse(value)); break;
                    case ProjectConstants.Types.UInt: writer.Write(uint.Parse(value)); break;
                    case ProjectConstants.Types.ULong: writer.Write(ulong.Parse(value)); break;
                    case ProjectConstants.Types.Short: writer.Write(short.Parse(value)); break;
                    case ProjectConstants.Types.Byte: writer.Write(byte.Parse(value)); break;
                    default: writer.Write(value ?? ""); break;
                }
            }
            catch (Exception ex) when (!(ex is Exception && ex.Message.StartsWith("Type Mismatch")))
            {
                throw new Exception($"Cannot parse '{value}' as {type}. {ex.Message}");
            }
        }

        public void ExportToJson(SchemaDefinition schema, IEnumerable<string[]> excelData, string outputPath, FeatureDefinition feature)
        {
            var dataList = excelData as List<string[]> ?? excelData.ToList();
            if (dataList.Count < 1) return;

            var header = dataList[0];
            var headerMap = header.Select((h, i) => new { h, i })
                                 .Where(x => !string.IsNullOrEmpty(x.h))
                                 .GroupBy(x => x.h)
                                 .ToDictionary(g => g.Key, g => g.First().i);

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
                var indices = Enumerable.Range(0, header.Length).Where(i => header[i] == info.ColumnName).ToList();
                var list = new List<object>();
                foreach (var idx in indices)
                {
                    list.Add(ParsePrimitive(info.BaseType, row[idx]));
                }
                return list;
            }
            else
            {
                if (!headerMap.TryGetValue(info.ColumnName, out int idx))
                {
                    headerMap.TryGetValue(fieldName, out idx);
                }

                string value = idx != 0 && idx < row.Length ? row[idx] : (idx == 0 && row.Length > 0 ? row[0] : "");
                if (idx >= row.Length) value = "";

                return ParsePrimitive(info.BaseType, value);
            }
        }

        private object ParsePrimitive(string type, string value)
        {
            try
            {
                return type switch
                {
                    ProjectConstants.Types.Int => int.Parse(value),
                    ProjectConstants.Types.Float => float.Parse(value),
                    ProjectConstants.Types.Bool => bool.Parse(value),
                    ProjectConstants.Types.Long => long.Parse(value),
                    ProjectConstants.Types.Double => double.Parse(value),
                    ProjectConstants.Types.UInt => uint.Parse(value),
                    ProjectConstants.Types.ULong => ulong.Parse(value),
                    ProjectConstants.Types.Short => short.Parse(value),
                    ProjectConstants.Types.Byte => byte.Parse(value),
                    _ => value
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot parse '{value}' as {type}. {ex.Message}");
            }
        }
    }
}
