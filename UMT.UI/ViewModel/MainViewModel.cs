using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UMT.IServices;
using UMT.IServices.Banner;
using UMT.UI.Constants;
using UMT.UI.DataViewModel;
using Unity;

namespace UMT.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogService _logService;
        private readonly IBannerService _bannerService;
        
        private string _siteUrl;
        private string _redirectionUrl;
        private int _countdownSeconds;
        private string _bannerMessage;
        private string _jsCode;
        private bool _isProcessing;
        private ICommand _applyActionCommand;
        private ICommand _removeActionCommand;
        private ICommand _showLogsCommand;

        private AppMode _selectedOption;

        public List<AppMode> AvailableOptions { get; set; }

        public AppMode SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;

                    CommandManager.InvalidateRequerySuggested();

                    OnPropertyChanged(nameof(SelectedOption));
                }
            }
        }

        public string SiteUrl
        {
            get => _siteUrl;
            set
            {
                if (_siteUrl != value)
                {
                    _siteUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RedirectionUrl
        {
            get => _redirectionUrl;
            set
            {
                if (_redirectionUrl != value)
                {
                    _redirectionUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CountdownSeconds
        {
            get => _countdownSeconds;
            set
            {
                if (_countdownSeconds != value)
                {
                    _countdownSeconds = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BannerMessage
        {
            get => _bannerMessage;
            set
            {
                if (_bannerMessage != value)
                {
                    _bannerMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string JsCode
        {
            get => _jsCode;
            set
            {
                if (_jsCode != value)
                {
                    _jsCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ApplyActionCommand
        {
            get => _applyActionCommand;
            set
            {
                _applyActionCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand RemoveActionCommand
        {
            get => _removeActionCommand;
            set
            {
                _removeActionCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowLogsCommand
        {
            get => _showLogsCommand;
            set
            {
                _showLogsCommand = value;
                OnPropertyChanged();
            }
        }

        private bool CanExecuteApplyAction(object parameter)
        {
            // Can execute if not currently processing and required fields are filled
            return !IsProcessing && !string.IsNullOrWhiteSpace(SiteUrl);
        }

        private async Task ExecuteApplyActionAsync(object parameter)
        {
            IsProcessing = true;
            const string source = "MainViewModel";

            try
            {
                _logService.LogInfo($"User initiated banner action", $"Mode: {SelectedOption}, Site: {SiteUrl}", source);

                var actionDescription = GetActionDescription();
                
                switch (SelectedOption) 
                {
                    case AppMode.DefaultBanner:
                        if (string.IsNullOrWhiteSpace(BannerMessage))
                        {
                            throw new ArgumentException("Banner message is required for default banner.");
                        }
                        _bannerService.CreateAutoRedirectNotification(SiteUrl, null, null, BannerMessage);
                        break;
                        
                    case AppMode.DefaultBannerRedirect:
                        if (string.IsNullOrWhiteSpace(BannerMessage))
                        {
                            throw new ArgumentException("Banner message is required.");
                        }
                        if (string.IsNullOrWhiteSpace(RedirectionUrl))
                        {
                            throw new ArgumentException("Redirection URL is required for redirect banner.");
                        }
                        if (CountdownSeconds <= 0)
                        {
                            throw new ArgumentException("Countdown seconds must be greater than 0.");
                        }
                        _bannerService.CreateAutoRedirectNotification(SiteUrl, RedirectionUrl, CountdownSeconds, BannerMessage);
                        break;
                        
                    case AppMode.CustomBanner:
                        if (string.IsNullOrWhiteSpace(JsCode))
                        {
                            throw new ArgumentException("JavaScript code is required for custom banner.");
                        }
                        _bannerService.CreateCustomBanner(SiteUrl, JsCode);
                        break;
                        
                    default:
                        throw new InvalidOperationException($"Unsupported banner mode: {SelectedOption}");
                }

                _logService.LogSuccess($"Banner action completed successfully", $"Mode: {SelectedOption}, Site: {SiteUrl}", source);

                // Show success message
                ShowMessageBox(new MessageBoxDataViewModel
                {
                    Text = $"{actionDescription} applied successfully!\n\nSite: {SiteUrl}",
                    Caption = "Success",
                    MessageBoxButton = MessageBoxButton.OK,
                    MessageBoxIcon = MessageBoxImage.Information
                });
            }
            catch (ArgumentException argEx)
            {
                _logService.LogError($"Invalid input for banner action", $"Error: {argEx.Message}, Mode: {SelectedOption}", source);
                
                ShowMessageBox(new MessageBoxDataViewModel
                {
                    Text = $"Invalid input: {argEx.Message}",
                    Caption = "Validation Error",
                    MessageBoxButton = MessageBoxButton.OK,
                    MessageBoxIcon = MessageBoxImage.Warning
                });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to apply banner action", $"Error: {ex.Message}, Mode: {SelectedOption}, Site: {SiteUrl}", source);
                
                ShowMessageBox(new MessageBoxDataViewModel
                {
                    Text = $"An error occurred while applying the banner:\n\n{ex.Message}\n\nPlease check the logs for more details.",
                    Caption = "Error",
                    MessageBoxButton = MessageBoxButton.OK,
                    MessageBoxIcon = MessageBoxImage.Error
                });
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ExecuteRemoveActionAsync(object parameter)
        {
            IsProcessing = true;
            const string source = "MainViewModel";

            try
            {
                if (string.IsNullOrWhiteSpace(SiteUrl))
                {
                    throw new ArgumentException("Site URL is required.");
                }

                _logService.LogInfo($"User initiated remove all banners action", $"Site: {SiteUrl}", source);

                _bannerService.RemoveAllOptions(SiteUrl);

                _logService.LogSuccess($"Remove all banners action completed successfully", $"Site: {SiteUrl}", source);

                // Show success message
                ShowMessageBox(new MessageBoxDataViewModel
                {
                    Text = $"All banners removed successfully!\n\nSite: {SiteUrl}",
                    Caption = "Success",
                    MessageBoxButton = MessageBoxButton.OK,
                    MessageBoxIcon = MessageBoxImage.Information
                });
            }
            catch (ArgumentException argEx)
            {
                _logService.LogError($"Invalid input for remove action", $"Error: {argEx.Message}", source);
                
                ShowMessageBox(new MessageBoxDataViewModel
                {
                    Text = $"Invalid input: {argEx.Message}",
                    Caption = "Validation Error",
                    MessageBoxButton = MessageBoxButton.OK,
                    MessageBoxIcon = MessageBoxImage.Warning
                });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Failed to remove banners", $"Error: {ex.Message}, Site: {SiteUrl}", source);
                
                ShowMessageBox(new MessageBoxDataViewModel
                {
                    Text = $"An error occurred while removing banners:\n\n{ex.Message}\n\nPlease check the logs for more details.",
                    Caption = "Error",
                    MessageBoxButton = MessageBoxButton.OK,
                    MessageBoxIcon = MessageBoxImage.Error
                });
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ExecuteShowLogs(object parameter)
        {
            try
            {
                var logsWindow = new LogsWindow();
                logsWindow.Owner = Application.Current.MainWindow;
                logsWindow.Show();
                
                _logService.LogInfo("Logs window opened", null, "MainViewModel");
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to open logs window", ex.Message, "MainViewModel");
                ShowErrorMessageBox($"Failed to open logs window: {ex.Message}");
            }
        }

        private string GetActionDescription()
        {
            switch(SelectedOption)
            {
                case AppMode.DefaultBanner:
                    return "Default banner";
                case AppMode.DefaultBannerRedirect:
                    return "Default banner";
                case AppMode.CustomBanner:
                    return "Default banner";
                default:
                    return "Banner action";
            }
        }

        public MainViewModel()
        {
            // Resolve services
            _logService = Container.Resolve<ILogService>();
            _bannerService = Container.Resolve<IBannerService>();

            // Initialize properties with default values
            SiteUrl = "https://glob.1sharepoint.roche.com/team/xyz";
            CountdownSeconds = 5;
            BannerMessage = "Important Notice: Scheduled maintenance will occur on [Date]. Please check the status page for updates.";
            RedirectionUrl = "google.com";
            JsCode = $@"
                    (function() {{
                        // Check if user has opted out of auto-redirect
                        if (localStorage.getItem('disableAutoRedirect') === 'true') return;
                        
                        var countdown = {CountdownSeconds};
                        var redirectUrl = '{RedirectionUrl}';
                        
                        var modal = document.createElement('div');
                        modal.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.8);z-index:20000;display:flex;align-items:center;justify-content:center;';
                        
                        var content = document.createElement('div');
                        content.style.cssText = 'background:white;padding:40px;border-radius:10px;text-align:center;max-width:500px;';
                        
                        var icon = document.createElement('div');
                        icon.innerHTML = '⚠️';
                        icon.style.cssText = 'font-size:48px;margin-bottom:20px;';
                        
                        var messageEl = document.createElement('h2');
                        messageEl.innerHTML = 'This site is being migrated. You will be redirected to the new location in';
                        messageEl.style.cssText = 'color:#333;margin-bottom:20px;';
                        
                        var countdownEl = document.createElement('div');
                        countdownEl.style.cssText = 'font-size:36px;color:#ff6347;margin:20px 0;font-weight:bold;';
                        countdownEl.innerHTML = countdown + ' seconds';
                        
                        var buttonContainer = document.createElement('div');
                        buttonContainer.style.cssText = 'margin-top:30px;';
                        
                        var goNowBtn = document.createElement('button');
                        goNowBtn.innerHTML = 'Go Now';
                        goNowBtn.style.cssText = 'background:#4CAF50;color:white;border:none;padding:12px 30px;margin:0 10px;border-radius:5px;cursor:pointer;font-size:16px;';
                        goNowBtn.onclick = function() {{
                            window.location.href = redirectUrl;
                        }};
                        
                        var cancelBtn = document.createElement('button');
                        cancelBtn.innerHTML = 'Stay Here';
                        cancelBtn.style.cssText = 'background:#f44336;color:white;border:none;padding:12px 30px;margin:0 10px;border-radius:5px;cursor:pointer;font-size:16px;';
                        cancelBtn.onclick = function() {{
                            clearInterval(timer);
                            modal.style.display = 'none';
                        }};
                        
                        var disableCheckbox = document.createElement('div');
                        disableCheckbox.style.cssText = 'margin-top:20px;font-size:14px;color:#666;';
                        disableCheckbox.innerHTML = '<label><input type=""checkbox"" id=""disableRedirect""> Don\'t show this again</label>';
                        
                        content.appendChild(icon);
                        content.appendChild(messageEl);
                        content.appendChild(countdownEl);
                        buttonContainer.appendChild(goNowBtn);
                        buttonContainer.appendChild(cancelBtn);
                        content.appendChild(buttonContainer);
                        content.appendChild(disableCheckbox);
                        modal.appendChild(content);
                        
                        document.body.appendChild(modal);
                        
                        var timer = setInterval(function() {{
                            countdown--;
                            countdownEl.innerHTML = countdown + ' seconds';
                            
                            if (countdown <= 0) {{
                                clearInterval(timer);
                                window.location.href = redirectUrl;
                            }}
                        }}, 1000);
                        
                        document.getElementById('disableRedirect').onchange = function() {{
                            localStorage.setItem('disableAutoRedirect', this.checked.toString());
                        }};
                    }})();";

            AvailableOptions = Enum.GetValues(typeof(AppMode)).Cast<AppMode>().ToList();
            SelectedOption = AppMode.DefaultBanner;

            // Initialize commands
            ApplyActionCommand = new AsyncRelayCommand(ExecuteApplyActionAsync, CanExecuteApplyAction);
            RemoveActionCommand = new AsyncRelayCommand(ExecuteRemoveActionAsync, CanExecuteApplyAction);
            ShowLogsCommand = new RelayCommand(ExecuteShowLogs);

            // Log application startup
            _logService.LogInfo("SharePoint Banner Manager started", null, "MainViewModel");
        }

        public override void Dispose()
        {
            _logService?.LogInfo("SharePoint Banner Manager closed", null, "MainViewModel");
            base.Dispose();
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Predicate<object> _canExecute;

        public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public async void Execute(object parameter)
        {
            await _executeAsync(parameter);
        }
    }

    // CommandManager helper for WPF
    public static class CommandManager
    {
        public static event EventHandler RequerySuggested
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public static void InvalidateRequerySuggested()
        {
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}
