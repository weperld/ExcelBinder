using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    public class CodeGeneratorService
    {
        private readonly TemplateEngineService _templateEngine = new();

        public string? GenerateDataCode(SchemaDefinition schema, FeatureDefinition feature, string nameSpace)
        {
            string? templateContent = GetTemplate(feature.Templates.DataClass);
            if (string.IsNullOrEmpty(templateContent)) return null;

            var context = PrepareDataContext(schema, feature, nameSpace);
            return _templateEngine.Render(templateContent, context);
        }


        public string? GenerateLogicCode(object context, FeatureDefinition feature)
        {
            string? templateContent = GetTemplate(feature.Templates.DataClass);
            if (string.IsNullOrEmpty(templateContent)) return null;

            return _templateEngine.Render(templateContent, context);
        }

        private string? GetTemplate(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) return File.ReadAllText(path);
            return null;
        }

        private object PrepareDataContext(SchemaDefinition schema, FeatureDefinition feature, string nameSpace)
        {
            var fields = schema.Fields.Select(f =>
            {
                var info = ParseType(f.Value, f.Key);
                string baseType = ConvertPrimitive(info.BaseType, feature);
                return new
                {
                    Name = f.Key,
                    Type = baseType,
                    IsList = info.IsList,
                    IsReference = info.IsReference,
                    RefType = info.RefType,
                    RefClassName = info.RefType != null ? info.RefType + "Data" : null,
                    ReadMethod = GetReadMethod(info.BaseType),
                    LowerName = f.Key.ToLower()
                };
            }).ToList();

            return new
            {
                ProjectName = feature.Name,
                Namespace = nameSpace,
                ClassName = schema.ClassName,
                Key = schema.Key,
                KeyType = GetKeyType(schema, feature),
                Fields = fields
            };
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

        private string ConvertPrimitive(string baseType, FeatureDefinition feature)
        {
            if (feature.TypeMappings != null && feature.TypeMappings.TryGetValue(baseType, out var mapped)) return mapped;
            
            return baseType switch
            {
                "int" => "int",
                "float" => "float",
                "string" => "string",
                "bool" => "bool",
                "long" => "long",
                "double" => "double",
                "uint" => "uint",
                "ulong" => "ulong",
                "short" => "short",
                "byte" => "byte",
                _ => "string"
            };
        }

        private string GetKeyType(SchemaDefinition schema, FeatureDefinition feature)
        {
            if (schema.Fields.TryGetValue(schema.Key, out var type))
            {
                var info = ParseType(type, schema.Key);
                return ConvertPrimitive(info.BaseType, feature);
            }
            return "int";
        }

        private string GetReadMethod(string type)
        {
            return type switch
            {
                "int" => "reader.ReadInt32()",
                "float" => "reader.ReadSingle()",
                "string" => "reader.ReadString()",
                "bool" => "reader.ReadBoolean()",
                "long" => "reader.ReadInt64()",
                "double" => "reader.ReadDouble()",
                "uint" => "reader.ReadUInt32()",
                "ulong" => "reader.ReadUInt64()",
                "short" => "reader.ReadInt16()",
                "byte" => "reader.ReadByte()",
                _ => "reader.ReadString()"
            };
        }
    }
}
