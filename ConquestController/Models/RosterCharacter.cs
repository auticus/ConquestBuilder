using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public class RosterCharacter : BaseViewModel, IRosterCharacter
    {
        public EventHandler PointsChanged { get; set; }

        private IConquestInput _character;
        public IConquestInput Character
        {
            get => _character;
            set
            {
                _character = value;
                NotifyPropertyChanged("Character");
                PointsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public ObservableCollection<IConquestInput> MainstayRegiments { get; }
        public ObservableCollection<IConquestInput> RestrictedRegiments { get; }

        public string CharacterHeader => $"{Character.Unit} - {Character.TotalPoints} pts";

        public RosterCharacter(IConquestInput character)
        {
            //we want to copy so that we get new IDs for these guys, we don't want to share IDs with the portrait versions
            Character = character.Copy(); 
            MainstayRegiments = new ObservableCollection<IConquestInput>();
            RestrictedRegiments = new ObservableCollection<IConquestInput>();

            Character.PointsChanged += Element_PointsChanged;
            MainstayRegiments.CollectionChanged += SubscribeCollectionEvents;
            RestrictedRegiments.CollectionChanged += SubscribeCollectionEvents;
        }

        private void SubscribeCollectionEvents(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IConquestInput element in e.NewItems)
                {
                    element.PointsChanged += Element_PointsChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IConquestInput element in e.OldItems)
                {
                    element.PointsChanged -= Element_PointsChanged;
                }
            }

            PointsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Element_PointsChanged(object sender, EventArgs e)
        {
            //todo: the various input objects need to fire this event off whenever their points change
            PointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
