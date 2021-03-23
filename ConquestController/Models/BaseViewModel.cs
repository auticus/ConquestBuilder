using System.ComponentModel;
using Newtonsoft.Json;

namespace ConquestController.Models
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        [JsonIgnore]
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
