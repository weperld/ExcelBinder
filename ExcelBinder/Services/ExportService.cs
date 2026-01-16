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
        public void ExportToBinary(SchemaDefinition schema, IEnumerable<string[]> excelData, string outputPath, FeatureDefinition feature)
        {
            var dataList = excelData.ToList();
            if (dataList.Count < 1) return;

            var header = dataList[0];
            var rows = dataList.Skip(1);

            using var stream = new FileStream(outputPath, FileMode.Create);
            using var writer = new BinaryWriter(stream);

            writer.Write(rows.Count());

            int rowIndex = 2; // Excel row starts from 1, header is 1, so data starts from 2
            foreach (var row in rows)
            {
                foreach (var field in schema.Fields)
                {
                    try
                    {
                        var fieldName = field.Key;
                        var fieldType = field.Value;
                        WriteField(writer, fieldType, row, header, fieldName, feature);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[Row {rowIndex}] Column '{field.Key}' error: {ex.Message}");
                    }
                }
                rowIndex++;
            }
        }

        private void WriteField(BinaryWriter writer, string type, string[] row, string[] header, string fieldName, FeatureDefinition feature)
        {
            var info = ParseType(type, fieldName);
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

        private struct TypeInfo
        {
            public string BaseType;
            public bool IsList;
            public bool IsReference;
            public string? RefType;
            public string ColumnName;
        }

        private TypeInfo ParseType(string schemaType, string fieldName)
        {
            var info = new TypeInfo { ColumnName = fieldName };
            string current = schemaType;

            if (current.StartsWith("List<"))
            {
                info.IsList = true;
                current = current.Substring(5, current.Length - 6);
            }

            var parts = current.Split(':');
            info.BaseType = parts[0];

            if (parts.Contains("ref"))
            {
                info.IsReference = true;
                int refIdx = Array.IndexOf(parts, "ref");
                if (refIdx + 1 < parts.Length)
                {
                    info.RefType = parts[refIdx + 1];
                }
            }

            string last = parts.Last();
            if (last != info.BaseType && last != "ref" && last != info.RefType)
            {
                info.ColumnName = last;
            }

            return info;
        }

        private void WritePrimitive(BinaryWriter writer, string type, string value)
        {
            try
            {
                switch (type)
                {
                    case "int":
                        if (float.TryParse(value, out float f) && f != (int)f) 
                            throw new Exception($"Type Mismatch: Expected int but got float-like value '{value}'");
                        writer.Write(int.Parse(value));
                        break;
                    case "float": writer.Write(float.Parse(value)); break;
                    case "string": writer.Write(value ?? ""); break;
                    case "bool": writer.Write(bool.Parse(value)); break;
                    case "long": writer.Write(long.Parse(value)); break;
                    case "double": writer.Write(double.Parse(value)); break;
                    case "uint": writer.Write(uint.Parse(value)); break;
                    case "ulong": writer.Write(ulong.Parse(value)); break;
                    case "short": writer.Write(short.Parse(value)); break;
                    case "byte": writer.Write(byte.Parse(value)); break;
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
            var rows = dataList.Skip(1);

            var result = new List<Dictionary<string, object>>();

            int rowIndex = 2;
            foreach (var row in rows)
            {
                var rowDict = new Dictionary<string, object>();
                foreach (var field in schema.Fields)
                {
                    try
                    {
                        rowDict[field.Key] = ParseField(field.Value, row, header, field.Key, feature);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[Row {rowIndex}] Column '{field.Key}' error: {ex.Message}");
                    }
                }
                result.Add(rowDict);
                rowIndex++;
            }

            File.WriteAllText(outputPath, JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        private object ParseField(string type, string[] row, string[] header, string fieldName, FeatureDefinition feature)
        {
            var info = ParseType(type, fieldName);
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
                    "int" => int.Parse(value),
                    "float" => float.Parse(value),
                    "bool" => bool.Parse(value),
                    "long" => long.Parse(value),
                    "double" => double.Parse(value),
                    "uint" => uint.Parse(value),
                    "ulong" => ulong.Parse(value),
                    "short" => short.Parse(value),
                    "byte" => byte.Parse(value),
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
