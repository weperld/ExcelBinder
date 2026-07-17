using ExcelBinder.Models;

namespace ExcelBinder.ViewModels
{
    /// <summary>
    /// Feature의 Category에 따라 알맞은 ExecutionViewModel을 생성합니다.
    /// App.xaml.cs(CLI)와 MainViewModel(GUI) 양쪽에서 공유합니다.
    /// </summary>
    public static class ExecutionViewModelFactory
    {
        public static ExecutionViewModelBase? Create(FeatureDefinition feature, AppSettings settings)
        {
            return feature.Category switch
            {
                ProjectConstants.Categories.StaticData => new StaticDataExecutionViewModel(feature, settings),
                ProjectConstants.Categories.Logic => new LogicExecutionViewModel(feature),
                ProjectConstants.Categories.SchemaGen => new SchemaGenExecutionViewModel(feature),
                ProjectConstants.Categories.Enum => new EnumExecutionViewModel(feature),
                ProjectConstants.Categories.Constants => new ConstantsExecutionViewModel(feature),
                _ => null
            };
        }
    }
}
