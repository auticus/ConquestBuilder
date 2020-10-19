using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;

namespace ConquestBuilder.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand NewFaction { get; set; }
        public ICommand LoadFaction { get; set; }
        public ApplicationData Data { get; set; }

        private FactionPickerViewModel _factionPickerVM;

        public MainViewModel()
        {
            InitializeCommands();
            Data = new ApplicationData();

            InitializeViewModels();
        }

        private void InitializeViewModels()
        {
            _factionPickerVM = new FactionPickerViewModel(Data);
            _factionPickerVM.OnSelectedFaction += OnSelectedFaction;
        }

        private void InitializeCommands()
        {
            NewFaction = new RelayCommand(OnNewFaction, param => this.CanExecute);
            LoadFaction = new RelayCommand(OnLoadFaction, param => this.CanExecute);
        }

        private void OnNewFaction(object parameter)
        {
            var view = new FactionPickerWindow(_factionPickerVM);
            view.ShowDialog();
        }

        private void OnLoadFaction(object parameter)
        {
            //load previously saved army
            //todo: implement a previously loaded roster
        }

        /// <summary>
        /// Handles the select item on the faction picker for a new army roster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="card"></param>
        private void OnSelectedFaction(object? sender, FactionCarouselCard card)
        {

        }
    }
}
