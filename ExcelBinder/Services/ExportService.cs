using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelBinder.Models;
using Newtonsoft.Json;

namespace ExcelBinder.Services
{
    public class ExportService
    {
        private readonly ExcelService _excelService = new();

        public void ExportToBinary(SchemaDefinition schema, IEnumerable<string[]> excelData, string outputPath, FeatureDefinition feature)
        {
            var dataList = excelData.ToList();
            if (dataList.Count < 1) return;

            var header = dataList[0];
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
                        var fieldName = field.Key;
                        var fieldType = field.Value;
                        WriteField(writer, fieldType, item.Data, header, fieldName, feature);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[Row {item.OriginalIndex}] Column '{field.Key}' error: {ex.Message}");
                    }
                }
            }
        }

        private void WriteField(BinaryWriter writer, string type, string[] row, string[] header, string fieldName, FeatureDefinition feature)
        {
            var info = TypeParser.ParseType(type, fieldName);
            if (info.IsList)
            {
                var indices = Enumerable.Range(0, header.Length).Where(i => header[i] == info.ColumnName).ToList();
                writer.Write(indices.Count);
                foreach (var idx in indices)
                {
                    WritePrimitive(writer, info.BaseType, row[idx]);
                }
            }
            else
            {
                int idx = Array.IndexOf(header, info.ColumnName);
                if (idx == -1) idx = Array.IndexOf(header, fieldName); // Fallback
                
                string value = idx != -1 && idx < row.Length ? row[idx] : "";
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
            var dataList = excelData.ToList();
            if (dataList.Count < 1) return;

            var header = dataList[0];
            var validRows = _excelService.GetFilteredData(dataList);

            var result = new List<Dictionary<string, object>>();

            foreach (var item in validRows)
            {
                var rowDict = new Dictionary<string, object>();
                foreach (var field in schema.Fields)
                {
                    try
                    {
                        rowDict[field.Key] = ParseField(field.Value, item.Data, header, field.Key, feature);
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

        private object ParseField(string type, string[] row, string[] header, string fieldName, FeatureDefinition feature)
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
                int idx = Array.IndexOf(header, info.ColumnName);
                if (idx == -1) idx = Array.IndexOf(header, fieldName);

                string value = idx != -1 && idx < row.Length ? row[idx] : "";
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
