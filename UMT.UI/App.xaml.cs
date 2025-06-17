using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using UMT.IoC;
using Unity;

namespace UMT.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            // Process command line args

            MainWindow mainWindow = new MainWindow();

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Create main application window, starting minimized if specified
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //var logger = UnityConfig.Container.Resolve<ILogger.ILogger>();
            //logger.Error(e.Exception);

            MessageBox.Show(
                $"Unhandled error occured!{Environment.NewLine}{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}Please send the newest log file to development team.",
                "UNHANDLED ERROR!",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }
    }
}
