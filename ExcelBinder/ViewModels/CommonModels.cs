using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExcelBinder.ViewModels
{
    public class SheetItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _canBeSelected = true;

        public string SheetName { get; set; } = string.Empty;
        public bool IsSchemaFound { get; set; }
        public string SchemaPath { get; set; } = string.Empty;

        public bool CanBeSelected
        {
            get => _canBeSelected;
            set => SetProperty(ref _canBeSelected, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set 
            {
                if (value && !CanBeSelected) return;
                SetProperty(ref _isSelected, value);
            }
        }
    }

    public class FileItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public ObservableCollection<SheetItemViewModel> Sheets { get; } = new();

        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }

        public FileItemViewModel()
        {
            SelectAllCommand = new RelayCommand(() => 
            { 
                foreach (var sheet in Sheets) sheet.IsSelected = true;
                _isSelected = true;
                OnPropertyChanged(nameof(IsSelected));
            });
            DeselectAllCommand = new RelayCommand(() => 
            { 
                foreach (var sheet in Sheets) sheet.IsSelected = false;
                _isSelected = false;
                OnPropertyChanged(nameof(IsSelected));
            });
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    foreach (var sheet in Sheets)
                    {
                        sheet.IsSelected = value;
                    }
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter)
        {
            if (parameter is T typed)
                _execute(typed);
        }
        public event EventHandler? CanExecuteChanged;
    }
}
