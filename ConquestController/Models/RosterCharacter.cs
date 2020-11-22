using System.Collections.Generic;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public class RosterCharacter : BaseViewModel, IRosterCharacter
    {
        private IConquestInput _character;
        public IConquestInput Character
        {
            get => _character;
            set
            {
                _character = value;
                NotifyPropertyChanged("Character");
            }
        }
        public List<IConquestInput> MainstayRegiments { get; }
        public List<IConquestInput> RestrictedRegiments { get; }

        public string CharacterHeader
        {
            get => $"{Character.Unit} - {Character.TotalPoints} pts";
        }

        public RosterCharacter(IConquestInput character)
        {
            //we want to copy so that we get new IDs for these guys, we don't want to share IDs with the portrait versions
            Character = character.Copy(); 
            MainstayRegiments = new List<IConquestInput>();
            RestrictedRegiments = new List<IConquestInput>();
        }
    }
}
