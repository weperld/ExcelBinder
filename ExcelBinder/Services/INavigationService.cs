namespace ExcelBinder.Services
{
    public interface INavigationService
    {
        void NavigateToDashboard();
        void NavigateToExecution();
        void NavigateToFeatureBuilder(object viewModel);
        void NavigateToSchemaEditor(object viewModel);
    }
}
