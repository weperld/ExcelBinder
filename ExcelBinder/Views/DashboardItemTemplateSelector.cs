using System.Windows;
using System.Windows.Controls;
using ExcelBinder.Models;

namespace ExcelBinder.Views
{
    public class DashboardItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StaticDataTemplate { get; set; }
        public DataTemplate? LogicTemplate { get; set; }
        public DataTemplate? SchemaGenTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is FeatureDefinition feature)
            {
                return feature.Category switch
                {
                    ProjectConstants.Categories.StaticData => StaticDataTemplate,
                    ProjectConstants.Categories.Logic => LogicTemplate,
                    ProjectConstants.Categories.SchemaGen => SchemaGenTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }
            return base.SelectTemplate(item, container);
        }
    }
}
