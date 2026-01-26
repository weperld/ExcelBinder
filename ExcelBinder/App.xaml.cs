using System;
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
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            RunCli(e.Args);
            Shutdown();
            return;
        }

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
                    featureId = args[++i];
                    break;
                case ProjectConstants.CLI.All:
                    selectAll = true;
                    break;
                case ProjectConstants.CLI.Bind:
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
                
                // Create appropriate Execution ViewModel
                IExecutionViewModel? execVm = feature.Category switch
                {
                    ProjectConstants.Categories.StaticData => new StaticDataExecutionViewModel(feature, vm.Settings),
                    ProjectConstants.Categories.Logic => new LogicExecutionViewModel(feature),
                    ProjectConstants.Categories.SchemaGen => new SchemaGenExecutionViewModel(feature),
                    ProjectConstants.Categories.Enum => new EnumExecutionViewModel(feature),
                    ProjectConstants.Categories.Constants => new ConstantsExecutionViewModel(feature),
                    _ => null
                };

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
                    if (executeExport) processor.ExecuteExportAsync(execVm).GetAwaiter().GetResult();
                    if (executeCodeGen) processor.ExecuteGenerateAsync(execVm).GetAwaiter().GetResult();
                }
            }
            else
            {
                Console.WriteLine($"Feature '{featureId}' not found.");
            }
        }
    }
}
