using System.Windows;

namespace UMT.UI.DataViewModel
{
    public class MessageBoxDataViewModel
    {
        public string Text { get; set; }
        public string Caption { get; set; }
        public MessageBoxButton MessageBoxButton { get; set; }
        public MessageBoxImage MessageBoxIcon { get; set; }
    }
}
