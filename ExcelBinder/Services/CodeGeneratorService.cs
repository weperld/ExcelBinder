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
                var info = TypeParser.ParseType(f.Value, f.Key);
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

        private string ConvertPrimitive(string baseType, FeatureDefinition feature)
        {
            if (feature.TypeMappings != null && feature.TypeMappings.TryGetValue(baseType, out var mapped)) return mapped;
            
            return baseType switch
            {
                ProjectConstants.Types.Int => ProjectConstants.Types.Int,
                ProjectConstants.Types.Float => ProjectConstants.Types.Float,
                ProjectConstants.Types.String => ProjectConstants.Types.String,
                ProjectConstants.Types.Bool => ProjectConstants.Types.Bool,
                ProjectConstants.Types.Long => ProjectConstants.Types.Long,
                ProjectConstants.Types.Double => ProjectConstants.Types.Double,
                ProjectConstants.Types.UInt => ProjectConstants.Types.UInt,
                ProjectConstants.Types.ULong => ProjectConstants.Types.ULong,
                ProjectConstants.Types.Short => ProjectConstants.Types.Short,
                ProjectConstants.Types.Byte => ProjectConstants.Types.Byte,
                _ => ProjectConstants.Types.String
            };
        }

        private string GetKeyType(SchemaDefinition schema, FeatureDefinition feature)
        {
            if (schema.Fields.TryGetValue(schema.Key, out var type))
            {
                var info = TypeParser.ParseType(type, schema.Key);
                return ConvertPrimitive(info.BaseType, feature);
            }
            return ProjectConstants.Types.Int;
        }

        private string GetReadMethod(string type)
        {
            return type switch
            {
                ProjectConstants.Types.Int => "reader.ReadInt32()",
                ProjectConstants.Types.Float => "reader.ReadSingle()",
                ProjectConstants.Types.String => "reader.ReadString()",
                ProjectConstants.Types.Bool => "reader.ReadBoolean()",
                ProjectConstants.Types.Long => "reader.ReadInt64()",
                ProjectConstants.Types.Double => "reader.ReadDouble()",
                ProjectConstants.Types.UInt => "reader.ReadUInt32()",
                ProjectConstants.Types.ULong => "reader.ReadUInt64()",
                ProjectConstants.Types.Short => "reader.ReadInt16()",
                ProjectConstants.Types.Byte => "reader.ReadByte()",
                _ => "reader.ReadString()"
            };
        }
    }
}
