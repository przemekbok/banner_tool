using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UMT.UI.ViewModel;

namespace UMT.UI.View
{
    public abstract class UserControlBase<Tvm> : UserControl where Tvm : ViewModelBase
    {
        public Tvm ViewModel => DataContext as Tvm;

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            if (VisualParent == null)
            {
                ViewModel?.Dispose();
            }

            base.OnVisualParentChanged(oldParent);
        }

        public void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
