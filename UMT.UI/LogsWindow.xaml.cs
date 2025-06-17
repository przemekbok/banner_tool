using System.Windows;
using UMT.UI.ViewModel;

namespace UMT.UI
{
    /// <summary>
    /// Interaction logic for LogsWindow.xaml
    /// </summary>
    public partial class LogsWindow : WindowBase<LogsViewModel>
    {
        public LogsWindow()
        {
            DataContext = new LogsViewModel();
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            ViewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}
