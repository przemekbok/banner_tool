using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UMT.IoC;
using UMT.UI.DataViewModel;
using Unity;

namespace UMT.UI.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        private int _loadings = 0;
        private List<string> _UIBindedPropNames = new List<string>()
        {
            nameof(IsLoading),
            nameof(ViewEnabled)
        };

        public ViewModelBase()
        {
            Container = MainContainer.CreateChildContainer();
        }

        ~ViewModelBase()
        {
            Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IUnityContainer Container { get; private set; }

        public virtual bool IsLoading => _loadings > 0;

        public virtual bool ViewEnabled => !IsLoading;

        public IUnityContainer MainContainer => UnityConfig.Container;
        public virtual void Dispose()
        {
            Container?.Dispose();
        }

        [Obsolete("Use InvokePropertyChanged and AddUIBindedProperties methods instead")]
        public virtual void OnPropertyChanged(params string[] propertyNames)
        {
            Array.ForEach(propertyNames,
                p => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p)));

            InvokePropertyChanged();
        }

        public void ShowErrorMessageBox(string errorMessage)
        {
            ShowMessageBox(new MessageBoxDataViewModel
            {
                Text = $"Error occured!{Environment.NewLine}{errorMessage}",
                Caption = "ERROR",
                MessageBoxButton = MessageBoxButton.OK,
                MessageBoxIcon = MessageBoxImage.Error
            });
        }

        public virtual void ShowMessageBox(MessageBoxDataViewModel messageBoxData)
        {
            MessageBox.Show(messageBoxData.Text, messageBoxData.Caption, messageBoxData.MessageBoxButton, messageBoxData.MessageBoxIcon);
        }

        protected void AddPropertyChangedHandler(ViewModelBase viewModel, EventHandler<PropertyChangedEventArgs> handler)
        {
            WeakEventManager<ViewModelBase, PropertyChangedEventArgs>.AddHandler(
                viewModel,
                nameof(PropertyChanged),
                handler);
        }

        protected virtual void InvokePropertyChanged()
        {
            _UIBindedPropNames.ForEach(p => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p)));
        }

        protected virtual void InvokePropertyChanged(params string[] propertyNames)
        {
            Array.ForEach(
                propertyNames,
                p => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p)));
        }
        protected void RemovePropertyChangedHandler(ViewModelBase viewModel, EventHandler<PropertyChangedEventArgs> handler)
        {
            WeakEventManager<ViewModelBase, PropertyChangedEventArgs>.RemoveHandler(
                viewModel,
                nameof(PropertyChanged),
                handler);
        }

        protected void TryWarnThrow(Action action, bool withLoader = false)
        {
            try
            {
                if (withLoader)
                {
                    Interlocked.Increment(ref _loadings);
                    OnPropertyChanged();
                }

                action();
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox(ex.GetBaseException().Message);
                //Logger.Error(ex);
                throw;
            }
            finally
            {
                if (withLoader)
                {
                    Interlocked.Decrement(ref _loadings);
                    OnPropertyChanged();
                }
            }
        }

        protected bool ValidateTask(Task task, CancellationToken userCancellationToken, bool skipMessageIfCanceled)
        {
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return true;
            }

            bool canceledByUser = false;

            TaskCanceledException canceledException = task.Exception?.GetBaseException() as TaskCanceledException;

            if (canceledException != null)
            {
                canceledByUser = canceledException.CancellationToken.Equals(userCancellationToken);
            }

            if (!canceledByUser || !skipMessageIfCanceled)
            {
                ShowErrorMessageBox(task.Exception?.GetBaseException().Message);
            }

            return false;
        }

        protected bool ValidateTask(Task task)
        {
            return ValidateTask(task, CancellationToken.None, false);
        }

        protected async Task WithLoaderAsync(Func<Task> asyncAction)
        {
            Interlocked.Increment(ref _loadings);
            OnPropertyChanged();

            try
            {
                await asyncAction();
            }
            finally
            {
                Interlocked.Decrement(ref _loadings);
                OnPropertyChanged();
            }
        }
        private void ExecuteLogic<TTask>(Func<CancellationToken, TTask> runWithToken, Action<TTask> onSuccess, int timeoutSeconds, params string[] updatedPropertyNames) where TTask : Task
        {
            Interlocked.Increment(ref _loadings);

            OnPropertyChanged(updatedPropertyNames);

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            runWithToken(cts.Token).ContinueWith(task =>
            {
                Interlocked.Decrement(ref _loadings);

                using (cts)
                {
                    if (ValidateTask(task) && onSuccess != null)
                    {
                        onSuccess((TTask)task);
                    }
                }

                OnPropertyChanged(updatedPropertyNames);
            });
        }
    }
}
