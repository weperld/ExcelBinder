using System.Collections.Generic;
using ExcelBinder.Models;
using ExcelBinder.Services.Processors;

namespace ExcelBinder.Services
{
    /// <summary>
    /// Processor는 싱글턴으로 관리됩니다. 모든 Processor는 stateless를 유지해야 합니다.
    /// </summary>
    public static class FeatureProcessorFactory
    {
        private static readonly Dictionary<string, IFeatureProcessor> _processors = new()
        {
            { ProjectConstants.Categories.StaticData, new StaticDataProcessor() },
            { ProjectConstants.Categories.Logic, new LogicProcessor() },
            { ProjectConstants.Categories.SchemaGen, new SchemaGenProcessor() },
            { ProjectConstants.Categories.Enum, new EnumProcessor() },
            { ProjectConstants.Categories.Constants, new ConstantsProcessor() }
        };

        public static IFeatureProcessor GetProcessor(string category)
        {
            if (_processors.TryGetValue(category, out var processor))
            {
                return processor;
            }
            throw new System.ArgumentException($"Unknown feature category: '{category}'. Valid categories: {string.Join(", ", _processors.Keys)}", nameof(category));
        }
    }
}
