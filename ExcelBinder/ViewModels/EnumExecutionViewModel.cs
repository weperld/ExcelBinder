using ExcelBinder.Models;

namespace ExcelBinder.ViewModels
{
    public class EnumExecutionViewModel : TemplateCodeGenExecutionViewModel
    {
        public EnumExecutionViewModel(FeatureDefinition feature)
            : base(feature, "Enum 엑셀 템플릿 저장", "NewEnumTemplate.xlsx")
        {
        }
    }
}
