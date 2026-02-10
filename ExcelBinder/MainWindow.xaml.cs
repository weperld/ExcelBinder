using System.Windows;

using ExcelBinder.Services;
using ExcelBinder.ViewModels;
using ExcelBinder.Views;

namespace ExcelBinder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INavigationService
{
    private MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        // Register services before creating ViewModel
        AppServices.Navigation = this;
        AppServices.Dialog = new DialogService();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Initial Navigation
        NavigateToDashboard();
    }

    public void NavigateToDashboard()
    {
        MainFrame.Navigate(new DashboardView { DataContext = _viewModel });
    }

    public void NavigateToExecution()
    {
        MainFrame.Navigate(new ExecutionView { DataContext = _viewModel });
    }

    public void NavigateToFeatureBuilder(object viewModel)
    {
        MainFrame.Navigate(new FeatureBuilderView { DataContext = viewModel });
    }

    public void NavigateToSchemaEditor(object viewModel)
    {
        MainFrame.Navigate(new SchemaEditorView { DataContext = viewModel });
    }
}
