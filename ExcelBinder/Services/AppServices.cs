using System;

namespace ExcelBinder.Services
{
    public static class AppServices
    {
        private static INavigationService? _navigation;
        private static IDialogService? _dialog;

        public static INavigationService Navigation
        {
            get => _navigation ?? throw new InvalidOperationException("AppServices.Navigation has not been initialized.");
            set => _navigation = value;
        }

        public static IDialogService Dialog
        {
            get => _dialog ?? throw new InvalidOperationException("AppServices.Dialog has not been initialized.");
            set => _dialog = value;
        }
    }
}
