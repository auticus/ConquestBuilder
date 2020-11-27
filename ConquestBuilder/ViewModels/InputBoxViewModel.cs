using System;
using System.Collections.Generic;
using System.Text;
using ConquestController.Models;

namespace ConquestBuilder.ViewModels
{
    public class InputBoxViewModel : BaseViewModel
    {

        private string _caption;

        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                NotifyPropertyChanged("Caption");
            }
        }

        private string _message;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyPropertyChanged("Message");
            }
        }

        private string _data;

        public string Data
        {
            get => _data;
            set
            {
                _data = value;
                NotifyPropertyChanged("Data");
            }
        }

        public InputBoxViewModel()
        {

        }
    }
}
