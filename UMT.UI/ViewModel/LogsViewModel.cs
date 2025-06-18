using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using UMT.IServices;
using Unity;

namespace UMT.UI.ViewModel
{
    public class LogsViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        private ObservableCollection<LogEntry> _logs;
        private LogLevel? _selectedLogLevel;
        private string _searchText;
        private ICommand _clearLogsCommand;
        private ICommand _refreshLogsCommand;
        private ICommand _exportLogsCommand;

        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                InvokePropertyChanged(nameof(Logs));
            }
        }

        public LogLevel? SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                _selectedLogLevel = value;
                InvokePropertyChanged(nameof(SelectedLogLevel));
                FilterLogs();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                InvokePropertyChanged(nameof(SearchText));
                FilterLogs();
            }
        }

        public Array LogLevels => Enum.GetValues(typeof(LogLevel));

        public ICommand ClearLogsCommand
        {
            get => _clearLogsCommand;
            set
            {
                _clearLogsCommand = value;
                InvokePropertyChanged(nameof(ClearLogsCommand));
            }
        }

        public ICommand RefreshLogsCommand
        {
            get => _refreshLogsCommand;
            set
            {
                _refreshLogsCommand = value;
                InvokePropertyChanged(nameof(RefreshLogsCommand));
            }
        }

        public ICommand ExportLogsCommand
        {
            get => _exportLogsCommand;
            set
            {
                _exportLogsCommand = value;
                InvokePropertyChanged(nameof(ExportLogsCommand));
            }
        }

        public LogsViewModel()
        {
            _logService = Container.Resolve<ILogService>();
            Logs = new ObservableCollection<LogEntry>();

            // Subscribe to new log entries
            _logService.LogAdded += OnLogAdded;

            // Initialize commands
            ClearLogsCommand = new RelayCommand(ExecuteClearLogs);
            RefreshLogsCommand = new RelayCommand(ExecuteRefreshLogs);
            ExportLogsCommand = new RelayCommand(ExecuteExportLogs);

            // Load initial logs
            LoadLogs();
        }

        private void OnLogAdded(object sender, LogEntry logEntry)
        {
            // Add to UI on main thread
            System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                Logs.Insert(0, logEntry); // Add at top (most recent first)
                
                // Keep only last 500 entries in UI for performance
                while (Logs.Count > 500)
                {
                    Logs.RemoveAt(Logs.Count - 1);
                }
            }));
        }

        private void LoadLogs()
        {
            var logs = _logService.GetLogs().Take(500); // Load last 500 entries
            Logs.Clear();
            
            foreach (var log in logs)
            {
                Logs.Add(log);
            }
        }

        private void FilterLogs()
        {
            var allLogs = _logService.GetLogs().Take(500);

            // Filter by level
            if (SelectedLogLevel.HasValue)
            {
                allLogs = allLogs.Where(l => l.Level == SelectedLogLevel.Value);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                allLogs = allLogs.Where(l => 
                    l.Message.ToLower().Contains(searchLower) ||
                    (l.Details?.ToLower().Contains(searchLower) ?? false) ||
                    (l.Source?.ToLower().Contains(searchLower) ?? false));
            }

            Logs.Clear();
            foreach (var log in allLogs)
            {
                Logs.Add(log);
            }
        }

        private void ExecuteClearLogs(object parameter)
        {
            try
            {
                _logService.ClearLogs();
                Logs.Clear();
                ShowMessageBox(new DataViewModel.MessageBoxDataViewModel
                {
                    Text = "Logs cleared successfully.",
                    Caption = "Success",
                    MessageBoxButton = System.Windows.MessageBoxButton.OK,
                    MessageBoxIcon = System.Windows.MessageBoxImage.Information
                });
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox($"Failed to clear logs: {ex.Message}");
            }
        }

        private void ExecuteRefreshLogs(object parameter)
        {
            LoadLogs();
            FilterLogs();
        }

        private void ExecuteExportLogs(object parameter)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"SharePointBannerManager_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    var logs = _logService.GetLogs();
                    var content = string.Join(Environment.NewLine, logs.Select(l => l.ToString()));
                    System.IO.File.WriteAllText(dialog.FileName, content);

                    ShowMessageBox(new DataViewModel.MessageBoxDataViewModel
                    {
                        Text = $"Logs exported successfully to: {dialog.FileName}",
                        Caption = "Export Complete",
                        MessageBoxButton = System.Windows.MessageBoxButton.OK,
                        MessageBoxIcon = System.Windows.MessageBoxImage.Information
                    });
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox($"Failed to export logs: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            if (_logService != null)
            {
                _logService.LogAdded -= OnLogAdded;
            }
            base.Dispose();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
