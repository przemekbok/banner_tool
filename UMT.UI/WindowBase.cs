using System.Windows;

namespace UMT.UI
{
    public abstract class WindowBase<Tvm> : Window where Tvm : class
    {
        public Tvm ViewModel => DataContext as Tvm;
    }
}