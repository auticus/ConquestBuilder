using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;

namespace ConquestBuilder.ViewModels
{
    public class FactionPickerViewModel : BaseViewModel
    {
        private readonly ApplicationData _data;

        private ObservableCollection<FactionCarouselCard> _factionsCollection;
        public ObservableCollection<FactionCarouselCard> FactionsCollection
        {
            get => _factionsCollection;
            set
            {
                _factionsCollection = value;
                NotifyPropertyChanged("FactionsCollection");
            }
        }

        private FactionCarouselCard _selectedFaction;

        public FactionCarouselCard SelectedFaction
        {
            get => _selectedFaction;
            set
            {
                _selectedFaction = value;
                NotifyPropertyChanged("SelectedFaction");
            }
        }

        public ICommand CancelFaction { get; set; }

        public EventHandler<FactionCarouselCard> OnSelectedFaction { get; set; }

        public FactionPickerViewModel(ApplicationData data)
        {
            _data = data;
            InitializeFactionLoadout();
            InitializeCommands();
        }

        public void FinalizeSelection(object parameter)
        {
            CloseView(parameter);
            OnSelectedFaction?.Invoke(this, SelectedFaction);
        }

        private void InitializeFactionLoadout()
        {
            FactionsCollection = new ObservableCollection<FactionCarouselCard>
            {
                new FactionCarouselCard()
                {
                    Name = "100 Kingdoms",
                    ImageSource = "../Images/iconic_100k.jpg",
                    ShortName = "100k",
                    Text = "100 Kingdoms"
                },
                new FactionCarouselCard()
                {
                    Name = "Spires",
                    ImageSource = "../Images/iconic-spire.jpg",
                    ShortName = "Spires",
                    Text = "The Exiles known as the Spire Faction"
                },
                new FactionCarouselCard()
                {
                    Name = "Dweghom",
                    ImageSource = "../Images/iconic_dweghom.jpg",
                    ShortName = "Dweghom",
                    Text = "Dweghom"
                },
                new FactionCarouselCard()
                {
                    Name = "Nords",
                    ImageSource = "../Images/iconic-nord.jpg",
                    ShortName = "Nords",
                    Text = "Nords"
                }
            };

            SelectedFaction = FactionsCollection[0];
        }

        private void InitializeCommands()
        {
            CancelFaction = new RelayCommand(OnCancelFaction, param => this.CanExecute);
        }

        private static void OnCancelFaction(object parameter)
        {
            CloseView(parameter);
        }

        private static void CloseView(object parameter)
        {
            if (!(parameter is IView view)) throw new ArgumentException("Parameter passed to FactionPickerViewModel::CloseView was not an IView");
            view.Close();
        }
    }
}
