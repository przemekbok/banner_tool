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
        private string _popupMessage;
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

        public string PopupMessage
        {
            get => _popupMessage;
            set
            {
                if (_popupMessage != value)
                {
                    _popupMessage = value;
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
                        // Create ONLY ONE custom action that combines both banner and redirect functionality
                        string combinedJs = GenerateCombinedBannerAndRedirectJs(BannerMessage, RedirectionUrl, CountdownSeconds.ToString(), PopupMessage);
                        _bannerService.CreateCustomBanner(SiteUrl, combinedJs);
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
                    return "Banner with redirect";
                case AppMode.CustomBanner:
                    return "Custom banner";
                default:
                    return "Banner action";
            }
        }

        private string GenerateCombinedBannerAndRedirectJs(string bannerMessage, string redirectUrl, string countdownSeconds, string popupMessage)
        {
            return $@"
                (function() {{
                    // === DUPLICATE PREVENTION SYSTEM ===
                    var globalKey = 'migrationBannerExecuted_' + window.location.href;
                    var executionKey = 'bannerExecution_' + Date.now() + '_' + Math.random();
                    
                    // Check if banner already executed for this exact page
                    if (window[globalKey]) {{
                        console.log('Migration banner already executed for this page, skipping');
                        return;
                    }}
                    
                    // Mark immediately to prevent any other executions
                    window[globalKey] = true;
                    
                    // Double-check no banner elements exist
                    var existingBanner = document.querySelector('[data-migration-banner=""true""]');
                    var existingModal = document.querySelector('[data-migration-modal=""true""]');
                    if (existingBanner || existingModal) {{
                        console.log('Migration elements already present, skipping');
                        return;
                    }}
                    
                    console.log('Executing migration banner script with execution key:', executionKey);
                    
                    // === USER PREFERENCES ===
                    var siteKey = 'migrationRedirect_' + window.location.hostname + window.location.pathname.replace(/\//g, '_');
                    var disableKey = siteKey + '_disabled';
                    var lastDeclinedKey = siteKey + '_lastDeclined';
                    
                    var isPermanentlyDisabled = localStorage.getItem(disableKey) === 'true';
                    var hasRecentlyDeclined = false;
                    
                    // Check if user declined recently (within 24 hours)
                    var lastDeclined = localStorage.getItem(lastDeclinedKey);
                    if (lastDeclined) {{
                        var declinedTime = parseInt(lastDeclined);
                        var hoursSinceDeclined = (Date.now() - declinedTime) / (1000 * 60 * 60);
                        hasRecentlyDeclined = hoursSinceDeclined < 24;
                    }}
                    
                    if (isPermanentlyDisabled || hasRecentlyDeclined) {{
                        console.log('Migration banner suppressed due to user preferences');
                        return;
                    }}
                    
                    // === BANNER CREATION ===
                    var banner = document.createElement('div');
                    banner.setAttribute('data-migration-banner', 'true');
                    banner.setAttribute('data-execution-key', executionKey);
                    banner.style.cssText = 'width:100%;background:#ff6b35;color:white;padding:10px;text-align:center;position:relative;z-index:1000;';
                    banner.innerHTML = '{bannerMessage.Replace("'", "\\'").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>")}';
                    
                    // Insert banner at the top of the page
                    if (document.body && document.body.firstChild) {{
                        document.body.insertBefore(banner, document.body.firstChild);
                    }} else if (document.body) {{
                        document.body.appendChild(banner);
                    }}
                    
                    // === REDIRECT MODAL ===
                    setTimeout(function() {{
                        // Check again if modal should be shown (in case page changed)
                        if (!window[globalKey] || document.querySelector('[data-migration-modal=""true""]')) {{
                            return;
                        }}
                        
                        var countdown = {countdownSeconds};
                        var redirectUrlFinal = '{redirectUrl}';
                        
                        var modal = document.createElement('div');
                        modal.setAttribute('data-migration-modal', 'true');
                        modal.setAttribute('data-execution-key', executionKey);
                        modal.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.8);z-index:20000;display:flex;align-items:center;justify-content:center;';
                        
                        var content = document.createElement('div');
                        content.style.cssText = 'background:white;padding:40px;border-radius:10px;text-align:center;max-width:500px;box-shadow:0 4px 6px rgba(0,0,0,0.1);';
                        
                        var icon = document.createElement('div');
                        icon.innerHTML = '⚠️';
                        icon.style.cssText = 'font-size:48px;margin-bottom:20px;';
                        
                        var messageEl = document.createElement('h2');
                        messageEl.innerHTML = '{popupMessage.Replace("'", "\\'").Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>")}';
                        messageEl.style.cssText = 'color:#333;margin-bottom:10px;';
                        
                        var subMessageEl = document.createElement('p');
                        subMessageEl.innerHTML = 'You will be redirected to the new location in';
                        subMessageEl.style.cssText = 'color:#666;margin-bottom:10px;font-size:16px;';
                        
                        var countdownEl = document.createElement('div');
                        countdownEl.style.cssText = 'font-size:36px;color:#ff6347;margin:20px 0;font-weight:bold;';
                        countdownEl.innerHTML = countdown + ' seconds';
                        
                        var buttonContainer = document.createElement('div');
                        buttonContainer.style.cssText = 'margin-top:30px;';
                        
                        var timer;
                        
                        // === SEPARATE CLEANUP FUNCTIONS ===
                        
                        // Only removes the modal (used when user declines redirect)
                        function cleanupModal() {{
                            console.log('Cleaning up modal only, keeping banner visible');
                            
                            // Clear timer
                            if (timer) {{
                                clearInterval(timer);
                                timer = null;
                            }}
                            
                            // Remove modal only
                            if (modal && modal.parentNode) {{
                                modal.parentNode.removeChild(modal);
                            }}
                            
                            // Banner remains visible - do NOT remove it
                            console.log('Modal removed, banner remains displayed');
                        }}
                        
                        // Removes everything (used for redirect or navigation)
                        function cleanupAll() {{
                            console.log('Cleaning up all migration elements');
                            
                            // Clear timer
                            if (timer) {{
                                clearInterval(timer);
                                timer = null;
                            }}
                            
                            // Remove modal
                            if (modal && modal.parentNode) {{
                                modal.parentNode.removeChild(modal);
                            }}
                            
                            // Remove banner
                            var bannerToRemove = document.querySelector('[data-migration-banner=""true""]');
                            if (bannerToRemove && bannerToRemove.parentNode) {{
                                bannerToRemove.parentNode.removeChild(bannerToRemove);
                            }}
                            
                            // Reset global flag
                            window[globalKey] = false;
                        }}
                        
                        var goNowBtn = document.createElement('button');
                        goNowBtn.innerHTML = '→ Go to New Site Now';
                        goNowBtn.style.cssText = 'background:#4CAF50;color:white;border:none;padding:12px 30px;margin:0 10px;border-radius:5px;cursor:pointer;font-size:16px;transition:background 0.3s;';
                        goNowBtn.onmouseover = function() {{ this.style.background = '#45a049'; }};
                        goNowBtn.onmouseout = function() {{ this.style.background = '#4CAF50'; }};
                        goNowBtn.onclick = function() {{
                            cleanupAll(); // Full cleanup for redirect
                            window.location.href = redirectUrlFinal;
                        }};
                        
                        var cancelBtn = document.createElement('button');
                        cancelBtn.innerHTML = 'Stay on Current Site';
                        cancelBtn.style.cssText = 'background:#f44336;color:white;border:none;padding:12px 30px;margin:0 10px;border-radius:5px;cursor:pointer;font-size:16px;transition:background 0.3s;';
                        cancelBtn.onmouseover = function() {{ this.style.background = '#da190b'; }};
                        cancelBtn.onmouseout = function() {{ this.style.background = '#f44336'; }};
                        cancelBtn.onclick = function() {{
                            cleanupModal(); // Only cleanup modal, keep banner visible
                            
                            // Record user choice
                            localStorage.setItem(lastDeclinedKey, Date.now().toString());
                            
                            var disableCheckbox = document.getElementById('disableRedirect');
                            if (disableCheckbox && disableCheckbox.checked) {{
                                localStorage.setItem(disableKey, 'true');
                                console.log('User opted to permanently disable migration redirects');
                            }}
                        }};
                        
                        var optionsContainer = document.createElement('div');
                        optionsContainer.style.cssText = 'margin-top:30px;padding-top:20px;border-top:1px solid #eee;';
                        
                        var disableCheckbox = document.createElement('div');
                        disableCheckbox.style.cssText = 'font-size:14px;color:#666;';
                        disableCheckbox.innerHTML = '<label style=""cursor:pointer;""><input type=""checkbox"" id=""disableRedirect"" style=""margin-right:8px;cursor:pointer;""> Don\'t show this message again for this site</label>';
                        
                        var noteEl = document.createElement('p');
                        noteEl.style.cssText = 'font-size:12px;color:#999;margin-top:10px;font-style:italic;';
                        noteEl.innerHTML = 'This preference is saved for this specific site only. The banner will remain visible.';
                        
                        content.appendChild(icon);
                        content.appendChild(messageEl);
                        content.appendChild(subMessageEl);
                        content.appendChild(countdownEl);
                        buttonContainer.appendChild(goNowBtn);
                        buttonContainer.appendChild(cancelBtn);
                        content.appendChild(buttonContainer);
                        optionsContainer.appendChild(disableCheckbox);
                        optionsContainer.appendChild(noteEl);
                        content.appendChild(optionsContainer);
                        modal.appendChild(content);
                        
                        document.body.appendChild(modal);
                        
                        timer = setInterval(function() {{
                            countdown--;
                            countdownEl.innerHTML = countdown + ' second' + (countdown !== 1 ? 's' : '');
                            
                            if (countdown <= 0) {{
                                cleanupAll(); // Full cleanup for automatic redirect
                                window.location.href = redirectUrlFinal;
                            }}
                        }}, 1000);
                        
                        // Handle escape key - only close modal, keep banner
                        var escapeHandler = function(e) {{
                            if (e.key === 'Escape') {{
                                cleanupModal(); // Only cleanup modal on escape
                                document.removeEventListener('keydown', escapeHandler);
                            }}
                        }};
                        document.addEventListener('keydown', escapeHandler);
                        
                        // Handle navigation - full cleanup only
                        var handleNavigation = function() {{
                            console.log('Navigation detected, cleaning up all elements');
                            cleanupAll(); // Full cleanup on navigation
                            document.removeEventListener('keydown', escapeHandler);
                        }};
                        
                        window.addEventListener('beforeunload', handleNavigation);
                        window.addEventListener('hashchange', handleNavigation);
                        
                        console.log('Migration modal displayed');
                    }}, 1000); // 1 second delay before showing modal
                    
                    console.log('Migration banner setup complete');
                }})();";
        }

        // Keep the old method for backward compatibility with custom JS
        private string GenerateCustomJsBanner(string countdownSeconds, string redirectionUrl, string message)
        {
            return GenerateCombinedBannerAndRedirectJs("Migration Notice", redirectionUrl, countdownSeconds, message);
        }

        public MainViewModel()
        {
            // Resolve services
            _logService = Container.Resolve<ILogService>();
            _bannerService = Container.Resolve<IBannerService>();

            // Initialize properties with default values
            SiteUrl = "https://glob.1sharepoint.roche.com/team/xyz";
            CountdownSeconds = 15;
            BannerMessage = "Important Notice: Scheduled maintenance will occur on [Date]. Please check the status page for updates.";
            RedirectionUrl = "https://google.com";
            PopupMessage = "This site has been migrated. Please update the bookmarks witha a new link.";
            JsCode = GenerateCombinedBannerAndRedirectJs("Site Migration Notice", "https://google.com", "5", "This site is being migrated");

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