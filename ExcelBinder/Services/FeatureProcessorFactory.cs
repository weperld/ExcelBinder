using System.Collections.Generic;
using ExcelBinder.Models;
using ExcelBinder.Services.Processors;

namespace ExcelBinder.Services
{
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
            return _processors[ProjectConstants.Categories.StaticData];
        }
    }
}
