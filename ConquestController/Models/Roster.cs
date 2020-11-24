using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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

        private string _totalPoints;
        public string TotalPoints
        {
            get => _totalPoints;
            set
            {
                _totalPoints = value;
                NotifyPropertyChanged("TotalPoints");
            }
        }

        public ObservableCollection<RosterCharacter> RosterCharacters { get; }

        public Roster()
        {
            RosterCharacters = new ObservableCollection<RosterCharacter>();
            RosterName = "New Roster";
            RosterCharacters.CollectionChanged += RosterCharacters_OnCollectionChanged;

            TotalPoints = FormatPoints(0);
        }

        private void RosterCharacters_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (RosterCharacter rosterCharacter in e.NewItems)
                {
                    rosterCharacter.PointsChanged += PointsChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (RosterCharacter rosterCharacter in e.OldItems)
                {
                    rosterCharacter.PointsChanged -= PointsChanged;
                }
            }

            PointsChanged(this, EventArgs.Empty);
        }

        private void PointsChanged(object sender, EventArgs e)
        {
            var sumPoints = 0;
            foreach (var element in RosterCharacters)
            {
                sumPoints += element.Character.TotalPoints;
                sumPoints += element.MainstayRegiments.Sum(regiment => regiment.TotalPoints);
                sumPoints += element.RestrictedRegiments.Sum(regiment => regiment.TotalPoints);
            }

            TotalPoints = FormatPoints(sumPoints);
        }

        private static string FormatPoints(int points)
        {
            return $"Points:  {points}";
        }
    }
}
