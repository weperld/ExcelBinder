using System;
using System.Threading.Tasks;
using System.Windows;
using ExcelBinder.ViewModels;
using ExcelBinder.Services;
using ExcelBinder.Models;
using System.Linq;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace ExcelBinder;

public partial class App : Application
{
    /// <summary>CLI 모드 여부. CLI에서는 창/메시지박스를 열지 않는다 (헤드리스 실행 보장).</summary>
    public static bool IsCliMode { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            IsCliMode = true;
            try
            {
                RunCli(e.Args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.ExitCode = 1;
            }
            Shutdown(Environment.ExitCode);
            return;
        }

        // GUI 모드 전역 예외 방어선: 처리되지 않은 예외로 앱이 통째로 죽는 대신 알리고 계속한다.
        DispatcherUnhandledException += (_, ev) =>
        {
            LogService.Instance.Error($"Unhandled exception: {ev.Exception}");
            MessageBox.Show(
                $"예기치 않은 오류가 발생했습니다.\n\n{ev.Exception.Message}",
                "ExcelBinder 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            ev.Handled = true;
        };

        base.OnStartup(e);
    }

    private void RunCli(string[] args)
    {
        var featureService = new FeatureService();
        var vm = new MainViewModel();
        
        string featureId = "";
        bool executeExport = false;
        bool executeCodeGen = false;
        bool selectAll = false;
        bool? cliBinary = null;
        bool? cliJson = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case ProjectConstants.CLI.Feature:
                    if (i + 1 >= args.Length) { Console.WriteLine("Error: --feature requires a value."); Environment.ExitCode = 1; return; }
                    featureId = args[++i];
                    break;
                case ProjectConstants.CLI.All:
                    selectAll = true;
                    break;
                case ProjectConstants.CLI.Bind:
                    if (i + 1 >= args.Length) { Console.WriteLine("Error: --bind requires a value."); Environment.ExitCode = 1; return; }
                    vm.Settings.BoundFeatures.Add(args[++i]);
                    featureService.SaveSettings(vm.Settings);
                    break;
                case ProjectConstants.CLI.Export:
                    executeExport = true;
                    break;
                case ProjectConstants.CLI.Codegen:
                    executeCodeGen = true;
                    break;
                case ProjectConstants.CLI.Binary:
                    cliBinary = true;
                    cliJson = false;
                    break;
                case ProjectConstants.CLI.Json:
                    cliJson = true;
                    cliBinary = false;
                    break;
                case ProjectConstants.CLI.Both:
                    cliBinary = true;
                    cliJson = true;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(featureId))
        {
            // Re-refresh to ensure all bound features are loaded
            vm.RefreshFeaturesCommand.Execute(null);
            var feature = vm.Features.FirstOrDefault(f => f.Id == featureId);
            if (feature != null)
            {
                vm.SelectedFeature = feature;

                IExecutionViewModel? execVm = ExecutionViewModelFactory.Create(feature, vm.Settings);

                if (execVm != null)
                {
                    if (cliBinary.HasValue) execVm.IsBinaryChecked = cliBinary.Value;
                    if (cliJson.HasValue) execVm.IsJsonChecked = cliJson.Value;

                    if (selectAll)
                    {
                        foreach (var f in execVm.ExcelFiles)
                        {
                            f.IsSelected = true;
                            foreach (var s in f.Sheets) s.IsSelected = s.CanBeSelected;
                        }
                    }

                    var processor = FeatureProcessorFactory.GetProcessor(feature.Category);
                    Task.Run(async () =>
                    {
                        if (executeExport) await processor.ExecuteExportAsync(execVm);
                        if (executeCodeGen) await processor.ExecuteGenerateAsync(execVm);
                    }).GetAwaiter().GetResult();

                    // Processor는 시트 단위 실패를 Error 로그로 남기고 계속 진행하므로,
                    // CI가 실패를 감지할 수 있게 에러 로그 발생 여부를 exit code에 반영한다.
                    if (LogService.Instance.ErrorCount > 0)
                    {
                        Console.WriteLine($"Finished with {LogService.Instance.ErrorCount} error(s).");
                        Environment.ExitCode = 1;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: Unknown category '{feature.Category}' for feature '{featureId}'.");
                    Environment.ExitCode = 1;
                }
            }
            else
            {
                Console.WriteLine($"Feature '{featureId}' not found.");
                Environment.ExitCode = 1;
            }
        }
        else if (executeExport || executeCodeGen || selectAll)
        {
            Console.WriteLine("Error: --export/--codegen/--all requires --feature <id>.");
            Environment.ExitCode = 1;
        }
    }
}
