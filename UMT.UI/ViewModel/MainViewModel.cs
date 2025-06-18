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

                    CommandManager.InvalidateRequerySu