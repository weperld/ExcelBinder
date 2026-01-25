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
                    vm.IsBinaryChecked = true;
                    vm.IsJsonChecked = false;
                    break;
                case ProjectConstants.CLI.Json:
                    vm.IsJsonChecked = true;
                    vm.IsBinaryChecked = false;
                    break;
                case ProjectConstants.CLI.Both:
                    vm.IsBinaryChecked = true;
                    vm.IsJsonChecked = true;
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
                if (selectAll)
                {
                    foreach (var f in vm.ExcelFiles) f.IsSelected = true;
                }
                if (executeExport) vm.ExecuteExport();
                if (executeCodeGen) vm.ExecuteGenerateCode();
            }
            else
            {
                Console.WriteLine($"Feature '{featureId}' not found.");
            }
        }
    }
}
