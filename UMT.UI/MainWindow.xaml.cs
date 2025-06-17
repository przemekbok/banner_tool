using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using UMT.IoC;
using UMT.UI.Constants;
using UMT.UI.DataViewModel;
using UMT.UI.ViewModel;
using Unity;

namespace UMT.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase<MainViewModel>
    {
        private SynchronizationContext _mainWindowSynchronizationContext;

        public MainWindow()
        {
            _mainWindowSynchronizationContext = SynchronizationContext.Current;
            ConfigureUnityAndMapper();

            DataContext = new MainViewModel();
            Title = System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Remove(0, 8);

            InitializeComponent();
        }

        private void ConfigureUnityAndMapper()
        {
            UnityConfig.SetMainContainer(new UnityContainer());
            UnityConfig.ConfigureCommon(message =>
            {
                _mainWindowSynchronizationContext.Send(state =>
                 {
                     ViewModel.ShowMessageBox(new MessageBoxDataViewModel
                     {
                         Caption = "WARNING",
                         MessageBoxButton = MessageBoxButton.OK,
                         MessageBoxIcon = MessageBoxImage.Warning,
                         Text = message
                     });
                 }, null);
            });
        }
    }
}
