using System;
using System.Linq;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    public struct TypeInfo
    {
        public string BaseType;
        public bool IsList;
        public bool IsReference;
        public string? RefType;
        public string ColumnName;
    }

    public static class TypeParser
    {
        public static TypeInfo ParseType(string schemaType, string fieldName)
        {
            var info = new TypeInfo { ColumnName = fieldName.Trim() };
            string current = schemaType.Trim();

            if (current.StartsWith(ProjectConstants.Excel.ListPrefix) && current.EndsWith(ProjectConstants.Excel.ListSuffix))
            {
                info.IsList = true;
                current = current.Substring(ProjectConstants.Excel.ListPrefix.Length, 
                    current.Length - ProjectConstants.Excel.ListPrefix.Length - ProjectConstants.Excel.ListSuffix.Length).Trim();
            }

            var parts = current.Split(ProjectConstants.Excel.TypeDelimiter[0]).Select(p => p.Trim()).ToArray();
            info.BaseType = parts[0];

            if (parts.Contains(ProjectConstants.Excel.ReferenceMarker))
            {
                info.IsReference = true;
                int refIdx = Array.IndexOf(parts, ProjectConstants.Excel.ReferenceMarker);
                if (refIdx + 1 < parts.Length)
                {
                    info.RefType = parts[refIdx + 1];
                }
            }

            string last = parts.Last();
            if (last != info.BaseType && last != ProjectConstants.Excel.ReferenceMarker && (info.RefType == null || last != info.RefType))
            {
                info.ColumnName = last;
            }

            return info;
        }
    }
}
