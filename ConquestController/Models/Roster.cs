using System;
using System.Collections.Generic;

namespace ConquestController.Models
{
    public class Roster : BaseViewModel
    {
        private Guid _id;
        public Guid ID
        {
            get => _id;
            set
            {
                _id = value;
                NotifyPropertyChanged("ID");
            }
        }

        private string _rosterName;
        public string RosterName
        {
            get => _rosterName;
            set
            {
                _rosterName = value;
                NotifyPropertyChanged("RosterName");
            }
        }

        private int _rosterLimit;

        public int RosterLimit
        {
            get => _rosterLimit;
            set
            {
                _rosterLimit = value;
                NotifyPropertyChanged("RosterLimit");
            }
        }

        public List<RosterCharacter> RosterCharacters { get; }

        public Roster()
        {
            RosterCharacters = new List<RosterCharacter>();
        }
    }
}
