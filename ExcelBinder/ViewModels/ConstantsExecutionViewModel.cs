using ExcelBinder.Models;

namespace ExcelBinder.ViewModels
{
    public class ConstantsExecutionViewModel : TemplateCodeGenExecutionViewModel
    {
        public ConstantsExecutionViewModel(FeatureDefinition feature)
            : base(feature, "Constants 엑셀 템플릿 저장", "NewConstantsTemplate.xlsx")
        {
        }
    }
}
