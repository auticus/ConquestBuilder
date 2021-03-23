using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ConquestController.Json;
using Newtonsoft.Json;

namespace ConquestController.Models
{
    
    public class Roster : BaseViewModel
    {
        /// <summary>
        /// List of mastery strings (the name) and the character guid they are attached to so that they can be counted up
        /// </summary>
        public static Dictionary<string, List<Guid>> MasterySpam { get; private set; }

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

        private string _faction;
    
        public string RosterFaction
        {
            get => _faction;
            set
            {
                _faction = value;
                NotifyPropertyChanged("RosterFaction");
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
            MasterySpam = new Dictionary<string, List<Guid>>();
        }

        public void RefreshPoints()
        {
            PointsChanged(this, EventArgs.Empty);
        }

        public void Save(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, 
                ContractResolver = new IgnoreEventHandlerContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto //will save the interface/abstract type so that it can deserialize properly
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            Debug.WriteLine(json);
            File.WriteAllText(filePath, json);
        }

        public static Roster Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            Debug.WriteLine(json);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            };

            var roster = JsonConvert.DeserializeObject<Roster>(json, settings);
            return roster;
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
