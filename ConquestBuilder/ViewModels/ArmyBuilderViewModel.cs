using System;
using System.Collections.Generic;
using System.Text;
using ConquestBuilder.Models;
using ConquestBuilder.Views;

namespace ConquestBuilder.ViewModels
{
    public class ArmyBuilderViewModel : BaseViewModel
    {
        private readonly ApplicationData _data;

        public EventHandler OnWindowClosed { get; set; }
        
        public ArmyBuilderViewModel(ApplicationData data)
        {
            _data = data;
        }

        private void CloseView(object parameter)
        {
            if (!(parameter is IView view)) throw new ArgumentException("Parameter passed to ArmyBuilderViewModel::CloseView was not an IView");
            view.Close();

            OnWindowClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
