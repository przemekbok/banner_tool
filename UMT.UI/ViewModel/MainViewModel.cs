using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UMT.IServices.Banner;
using UMT.UI.Constants;
using Unity;

namespace UMT.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _siteUrl;
        private string _redirectionUrl;
        private int _countdownSeconds;
        private string _bannerMessage;
        private string _jsCode;
        private bool _isProcessing;
        private ICommand _applyActionCommand;
        private ICommand _removeActionCommand;
        public IBannerService _bannerService { get; set; }

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

        private bool CanExecuteApplyAction(object parameter)
        {
            // Can execute if not currently processing
            return !IsProcessing;
        }

        private async Task ExecuteApplyActionAsync(object parameter)
        {
            IsProcessing = true;

            try
            {
                switch (SelectedOption) 
                {
                    case AppMode.DefaultBanner:
                        _bannerService.CreateAutoRedirectNotification(_siteUrl, null, null, _bannerMessage);
                        break;
                    case AppMode.DefaultBannerRedirect:
                        _bannerService.CreateAutoRedirectNotification(_siteUrl, _redirectionUrl, _countdownSeconds, _bannerMessage);
                        break;
                    case AppMode.CustomBanner:
                        _bannerService.CreateCustomBanner(_siteUrl, JsCode);
                        break;
                    default:
                        throw new Exception();
                }

                //var result = await _bannerManager.ApplyBannerAsync(SiteUrl, BannerMessage, JsCode);

                //if (result.IsSuccess)
                //{
                //    MessageBox.Show(
                //        result.Message,
                //        "Success",
                //        MessageBoxButton.OK,
                //        MessageBoxImage.Information);
                //}
                //else
                //{
                //    MessageBox.Show(
                //        result.ErrorMessage,
                //        "Error",
                //        MessageBoxButton.OK,
                //        MessageBoxImage.Error);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ExecuteRemoveActionAsync(object parameter)
        {
            IsProcessing = true;

            try
            {
                _bannerService.RemoveAllOptions(_siteUrl);

                //var result = new { Message = "xD", IsSuccess = true, ErrorMessage = "xD" };//await _bannerManager.ApplyBannerAsync(SiteUrl, BannerMessage, JsCode);

                //if (result.IsSuccess)
                //{
                //    MessageBox.Show(
                //        result.Message,
                //        "Success",
                //        MessageBoxButton.OK,
                //        MessageBoxImage.Information);
                //}
                //else
                //{
                //    MessageBox.Show(
                //        result.ErrorMessage,
                //        "Error",
                //        MessageBoxButton.OK,
                //        MessageBoxImage.Error);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public MainViewModel()
        {
            SiteUrl = "https://glob.1sharepoint.roche.com/team/xyz";
            CountdownSeconds = 5;
            BannerMessage = "Important Notice: Scheduled maintenance will occur on [Date]. Please check the status page for updates.";
            JsCode = $@"
                    (function() {{
                        // Check if user has opted out of auto-redirect
                        if (localStorage.getItem('disableAutoRedirect') === 'true') return;
                        
                        var countdown = {_countdownSeconds};
                        var redirectUrl = '{_redirectionUrl}';
                        
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
                    }})();"; ;

            AvailableOptions = Enum.GetValues(typeof(AppMode)).Cast<AppMode>().ToList();
            SelectedOption = AppMode.DefaultBanner;

            _bannerService = Container.Resolve<IBannerService>();

            ApplyActionCommand = new AsyncRelayCommand(ExecuteApplyActionAsync, CanExecuteApplyAction);
            RemoveActionCommand = new AsyncRelayCommand(ExecuteRemoveActionAsync, CanExecuteApplyAction);
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