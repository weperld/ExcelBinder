using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ExcelBinder.ViewModels;
using ExcelBinder.Views;

namespace ExcelBinder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
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

    public void NavigateToFeatureBuilder(FeatureBuilderViewModel builderViewModel)
    {
        MainFrame.Navigate(new FeatureBuilderView { DataContext = builderViewModel });
    }

    public void NavigateToSchemaEditor(SchemaEditorViewModel editorViewModel)
    {
        MainFrame.Navigate(new SchemaEditorView { DataContext = editorViewModel });
    }
}