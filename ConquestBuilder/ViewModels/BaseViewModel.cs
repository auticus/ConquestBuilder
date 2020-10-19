using System.ComponentModel;

namespace ConquestBuilder.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public bool CanExecute { get; set; } = true;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        #endregion
    }
}
