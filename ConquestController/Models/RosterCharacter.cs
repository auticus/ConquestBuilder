using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public class RosterCharacter : BaseViewModel, IRosterCharacter
    {
        public EventHandler PointsChanged { get; set; }

        private IConquestCharacter _character;
        public IConquestCharacter Character
        {
            get => _character;
            set
            {
                _character = value;
                NotifyPropertyChanged("Character");
                PointsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public ObservableCollection<IConquestGamePiece> MainstayRegiments { get; }
        public ObservableCollection<IConquestGamePiece> RestrictedRegiments { get; }

        public string CharacterHeader => $"{Character} - {Character.TotalPoints}";

        public RosterCharacter(IConquestBaseGameElement character)
        {
            //we want to copy so that we get new IDs for these guys, we don't want to share IDs with the portrait versions
            Character = character.Clone() as IConquestCharacter;
            MainstayRegiments = new ObservableCollection<IConquestGamePiece>();
            RestrictedRegiments = new ObservableCollection<IConquestGamePiece>();

            Character.PointsChanged += Element_PointsChanged;

            MainstayRegiments.CollectionChanged += SubscribeCollectionEvents;
            RestrictedRegiments.CollectionChanged += SubscribeCollectionEvents;
        }

        private void SubscribeCollectionEvents(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IConquestBaseGameElement element in e.NewItems)
                {
                    element.PointsChanged += Element_PointsChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IConquestBaseGameElement element in e.OldItems)
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
