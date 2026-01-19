using System;
using System.Linq;

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
            var info = new TypeInfo { ColumnName = fieldName };
            string current = schemaType;

            if (current.StartsWith("List<") && current.EndsWith(">"))
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
            if (last != info.BaseType && last != "ref" && (info.RefType == null || last != info.RefType))
            {
                info.ColumnName = last;
            }

            return info;
        }
    }
}
