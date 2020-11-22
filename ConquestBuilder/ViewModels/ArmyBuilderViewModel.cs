using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;
using ConquestController.Models;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestBuilder.ViewModels
{
    public class ArmyBuilderViewModel : BaseViewModel
    {
        private enum LastItemSelected
        {
            Character,
            Regiment
        }

        #region Private Fields
        private readonly ApplicationData _data;
        private Armies _currentArmy;
        private const string THUMB_PATH = "..\\Images\\Thumbs\\";
        private LastItemSelected _lastItemSelected = LastItemSelected.Character; //for logic where we need to act on the last selected item 
        private const int MAX_NUMBER_REGIMENTS_ALLOWED = 4;
        
        private string Filter
        {
            get
            {
                return _currentArmy switch
                {
                    Armies.HundredKingdoms => "100k",
                    Armies.Spires => "Spires",
                    Armies.Dweghom => "Dweghom",
                    Armies.Nords => "Nords",
                    _ => throw new InvalidOperationException("Current Army not set up properly to filter")
                };
            }
        }
        #endregion Private Fields

        #region Commands
        public ICommand CharacterSelected { get; set; }
        public ICommand MainstaySelected { get; set; }
        public ICommand RestrictedSelected { get; set; }
        public ICommand AddSelectedToRoster { get; set; }
        public ICommand DeleteRosterElement { get; set; }
        #endregion Commands

        #region Public Properties

        public EventHandler OnWindowClosed { get; set; }
        public EventHandler<string> SendMessageToView { get; set; }

        private ObservableCollection<UnitButton> _characterButtons;

        public ObservableCollection<UnitButton> CharacterButtons
        {
            get => _characterButtons;
            set
            {
                _characterButtons = value;
                NotifyPropertyChanged("CharacterButtons");
            }
        }

        private ObservableCollection<UnitButton> _mainstayButtons;
        public ObservableCollection<UnitButton> MainstayButtons
        {
            get => _mainstayButtons;
            set
            {
                _mainstayButtons = value;
                NotifyPropertyChanged("MainstayButtons");
            }
        }

        private ObservableCollection<UnitButton> _restrictedButtons;

        public ObservableCollection<UnitButton> RestrictedButtons
        {
            get => _restrictedButtons;
            set
            {
                _restrictedButtons = value;
                NotifyPropertyChanged("RestrictedButtons");
            }
        }

        private IConquestInput _selectedPortraitCharacter;

        /// <summary>
        /// Represents the character portrait that is selected in the UI (not the selected character on the roster)
        /// </summary>
        public IConquestInput SelectedPortraitCharacter
        {
            get => _selectedPortraitCharacter;
            set
            {
                _selectedPortraitCharacter = value;
                NotifyPropertyChanged("SelectedPortraitCharacter");
            }
        }

        private IConquestInput _selectedPortraitUnit;

        public IConquestInput SelectedPortraitUnit
        {
            get => _selectedPortraitUnit;
            set
            {
                _selectedPortraitUnit = value;
                NotifyPropertyChanged("SelectedPortraitUnit");
            }
        }

        private IRosterCharacter _selectedRosterCharacter;

        /// <summary>
        /// The selected character from the roster (not the portrait)
        /// </summary>
        public IRosterCharacter SelectedRosterCharacter
        {
            get => _selectedRosterCharacter;
            set
            {
                _selectedRosterCharacter = value;
                NotifyPropertyChanged("SelectedRosterCharacter");

                DeleteElementEnabled = SelectedRosterCharacter != null || SelectedRosterUnit != null;
            }
        }

        private IConquestInput _selectedRosterUnit;

        /// <summary>
        /// The selected unit from the roster
        /// </summary>
        public IConquestInput SelectedRosterUnit
        {
            get => _selectedRosterUnit;
            set
            {
                _selectedRosterUnit = value;
                NotifyPropertyChanged("SelectedRosterUnit");

                DeleteElementEnabled = SelectedRosterCharacter != null || SelectedRosterUnit != null;
            }
        }


        private bool _dataPanelVisible;

        /// <summary>
        /// Dictates whether the data panel with the unit information is displayed or not
        /// </summary>
        public bool DataPanelVisible
        {
            get => _dataPanelVisible;
            set
            {
                _dataPanelVisible = value;
                NotifyPropertyChanged("DataPanelVisible");
            }
        }

        private string _selectedUnitName;

        public string SelectedUnitName
        {
            get => _selectedUnitName;
            set
            {
                _selectedUnitName = value;
                NotifyPropertyChanged("SelectedUnitName");
            }
        }

        private string _selectedUnitPoints;

        public string SelectedUnitPoints
        {
            get => _selectedUnitPoints;
            set
            {
                _selectedUnitPoints = value;
                NotifyPropertyChanged("SelectedUnitPoints");
            }
        }

        private string _selectedUnitType;
        public string SelectedUnitType
        {
            get => _selectedUnitType;
            set
            {
                _selectedUnitType = value;
                NotifyPropertyChanged("SelectedUnitType");
            }
        }

        private string _selectedUnitClass;
        public string SelectedUnitClass
        {
            get => _selectedUnitClass;
            set
            {
                _selectedUnitClass = value;
                NotifyPropertyChanged("SelectedUnitClass");
            }
        }

        private string _selectedUnitM;
        public string SelectedUnitM
        {
            get => _selectedUnitM;
            set
            {
                _selectedUnitM = value;
                NotifyPropertyChanged("SelectedUnitM");
            }
        }

        private string _selectedUnitV;
        public string SelectedUnitV
        {
            get => _selectedUnitV;
            set
            {
                _selectedUnitV = value;
                NotifyPropertyChanged("SelectedUnitV");
            }
        }

        private string _selectedUnitC;
        public string SelectedUnitC
        {
            get => _selectedUnitC;
            set
            {
                _selectedUnitC = value;
                NotifyPropertyChanged("SelectedUnitC");
            }
        }

        private string _selectedUnitW;
        public string SelectedUnitW
        {
            get => _selectedUnitW;
            set
            {
                _selectedUnitW = value;
                NotifyPropertyChanged("SelectedUnitW");
            }
        }

        private string _selectedUnitA;
        public string SelectedUnitA
        {
            get => _selectedUnitA;
            set
            {
                _selectedUnitA = value;
                NotifyPropertyChanged("SelectedUnitA");
            }
        }

        private string _selectedUnitR;
        public string SelectedUnitR
        {
            get => _selectedUnitR;
            set
            {
                _selectedUnitR = value;
                NotifyPropertyChanged("SelectedUnitR");
            }
        }

        private string _selectedUnitD;
        public string SelectedUnitD
        {
            get => _selectedUnitD;
            set
            {
                _selectedUnitD = value;
                NotifyPropertyChanged("SelectedUnitD");
            }
        }

        private string _selectedUnitE;
        public string SelectedUnitE
        {
            get => _selectedUnitE;
            set
            {
                _selectedUnitE = value;
                NotifyPropertyChanged("SelectedUnitE");
            }
        }

        private string _selectedUnitSpecialRules;

        public string SelectedUnitSpecialRules
        {
            get
            {
                if (_selectedUnitSpecialRules == null) return String.Empty;
                return "Special Rules - " + _selectedUnitSpecialRules.Replace("|", ", ");
            }
            set
            {
                _selectedUnitSpecialRules = value;
                NotifyPropertyChanged("SelectedUnitSpecialRules");
            }
        }

        private string _selectedOffense;

        public string SelectedOffense
        {
            get => _selectedOffense;
            set
            {
                _selectedOffense = value;
                NotifyPropertyChanged("SelectedOffense");
            }
        }

        private string _selectedDefense;

        public string SelectedDefense
        {
            get => _selectedDefense;
            set
            {
                _selectedDefense = value;
                NotifyPropertyChanged("SelectedDefense");
            }
        }

        private string _selectedOverall;

        public string SelectedOverall
        {
            get => _selectedOverall;
            set
            {
                _selectedOverall = value;
                NotifyPropertyChanged("SelectedOverall");
            }
        }

        private Roster _roster;

        public Roster Roster
        {
            get => _roster;
            set
            {
                _roster = value;
                NotifyPropertyChanged("Roster");
            }
        }

        private bool _deleteElementEnabled;

        public bool DeleteElementEnabled
        {
            get => _deleteElementEnabled;
            set
            {
                _deleteElementEnabled = value;
                NotifyPropertyChanged("DeleteElementEnabled");
            }
        }

        //The following area deals with the treeview, which I admit my WPF-fu is not powerful enough to hook up via xaml properly so am using code behind to make things work
        public EventHandler<RosterChangedEventArgs> RefreshRosterTreeView { get; set; }
        #endregion Public Properties

        #region Constructors
        public ArmyBuilderViewModel(ApplicationData data)
        {
            _data = data;
            _roster = new Roster();
            InitializeCommands();
        }

        #endregion Constructors

        #region Public Methods
        public void SetArmy(string army)
        {
            _currentArmy = army.ToLower() switch
            {
                "100 kingdoms" => Armies.HundredKingdoms,
                "spires" => Armies.Spires,
                "dweghom" => Armies.Dweghom,
                "nords" => Armies.Nords,
                _ => throw new InvalidOperationException($"{army} was passed but is not supported")
            };

            InitializeControls();
            InitializeRoster();
            DataPanelVisible = false;
        }

        #endregion Public Methods

        private void CloseView(object parameter)
        {
            //todo: implement
            if (!(parameter is IView view)) throw new ArgumentException("Parameter passed to ArmyBuilderViewModel::CloseView was not an IView");
            view.Close();

            OnWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeCommands()
        {
            CharacterSelected = new RelayCommand(OnCharacterSelected, param => this.CanExecute);
            MainstaySelected = new RelayCommand(OnMainstaySelected, param => this.CanExecute);
            RestrictedSelected = new RelayCommand(OnRestrictedSelected, param=> this.CanExecute);
            AddSelectedToRoster = new RelayCommand(OnSelectionAdded, param=>CanExecute);
            DeleteRosterElement = new RelayCommand(OnRosterElementDeleted, param => this.CanExecute);
        }

        private void InitializeControls()
        {
            CharacterButtons = new ObservableCollection<UnitButton>();
            MainstayButtons = new ObservableCollection<UnitButton>();
            RestrictedButtons = new ObservableCollection<UnitButton>();

            foreach (var character in _data.Characters.Where(character=>character.Faction == Filter))
            {
                var btn = new UnitButton(){UnitName = character.Unit, UnitThumb= $"{THUMB_PATH}{character.Image}", Tag=character};
                CharacterButtons.Add(btn);
                NotifyPropertyChanged("CharacterButtons");
            }
        }

        private void InitializeRoster()
        {
            //todo: need to have a way to hook in the roster limit here
            Roster = new Roster
            {
                RosterName = "New Roster", 
                RosterLimit = 0, 
                ID = Guid.NewGuid()
            };

            RefreshRosterTreeView(this, new RosterChangedEventArgs());
        }

        private void OnCharacterSelected(object tag)
        {
            if (!(tag is UnitButton unit)) throw new InvalidOperationException("Item passed back was not the expected type");

            var character = unit.Tag as CharacterInputModel;
            SelectedPortraitCharacter = character ?? throw new InvalidOperationException("Item tag was not correct type");
            LoadUnits(MainstayButtons, character.MainstayChoices, "MainstayButtons");
            LoadUnits(RestrictedButtons, character.RestrictedChoices, "RestrictedButtons");

            LoadStatGrid(SelectedPortraitCharacter);
            _lastItemSelected = LastItemSelected.Character;
        }

        private void OnMainstaySelected(object tag)
        {
            SelectUnit(tag);
        }

        private void OnRestrictedSelected(object tag)
        {
            SelectUnit(tag);
        }

        private void OnSelectionAdded(object tag)
        {
            if (_lastItemSelected == LastItemSelected.Character)
            {
                AddSelectedCharacterToRoster();
            }
            else AddSelectedRegimentToSelectedCharacter();
        }

        private void OnRosterElementDeleted(object tag)
        {
            //the selected roster unit or character should have a value.  delete the id of that item in the Roster element
            if (SelectedRosterCharacter != null && SelectedRosterUnit == null)
            {
                DeleteSelectedRosterCharacter();
            }
            else DeleteSelectedRegimentFromRoster();

            RefreshRosterTreeView?.Invoke(this, new RosterChangedEventArgs());
        }

        private void DeleteSelectedRosterCharacter()
        {
            //this assumes SelectedRosterCharacter has a value.  if it doesn't, it will break, and that is good we want it to break because that should not happen
            for (var i = 0; i < Roster.RosterCharacters.Count; i++)
            {
                var element = Roster.RosterCharacters[i];
                if (element.Character.ID == SelectedRosterCharacter.Character.ID)
                {
                    Roster.RosterCharacters.Remove(element);
                    return;
                }
            }

            throw new InvalidOperationException("The selected character was not found to be deleted!");
        }

        private void DeleteSelectedRegimentFromRoster()
        {
            //ah yes but where do they reside?  we must explore the depths of the roster to find the selected regiment
            foreach (var element in Roster.RosterCharacters)
            {
                var dataStructures = new []{element.MainstayRegiments, element.RestrictedRegiments};

                var checkingMainstay = true; //first element is the mainstay, afterward set to false
                foreach (var list in dataStructures)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var regiment = list[i];
                        if (regiment.ID == SelectedRosterUnit.ID)
                        {
                            //we have a valid item to remove but doing so may break the roster if there are too many restricted regiments after!
                            if (checkingMainstay && !CanRemoveMainstay(element))
                            {
                                SendMessageToView?.Invoke(this, "Removing this regiment is not allowed as this would create an invalid roster.");
                                return;
                            }
                            list.Remove(regiment);
                            return;
                        }
                    }

                    checkingMainstay = false;
                }

                throw new InvalidOperationException("The selected regiment was not found to be deleted!");
            }
        }

        private bool CanRemoveMainstay(IRosterCharacter element)
        {
            //can never have more restricted regiments than you do mainstay so you cannot delete a mainstay if that would create an illegal roster
            return element.MainstayRegiments.Count > element.RestrictedRegiments.Count;
        }

        private void AddSelectedCharacterToRoster()
        {
            if (SelectedPortraitCharacter == null) throw new InvalidOperationException("AddSelectedCharacterToRoster called but SelectedPortraitCharacter is null");
            var newCharacter = new RosterCharacter(SelectedPortraitCharacter);
            Roster.RosterCharacters.Add(newCharacter);
            SelectedRosterCharacter = newCharacter;

            var e = new RosterChangedEventArgs
            {
                RosterElement = newCharacter, SelectedElementID = newCharacter.Character.ID
            };

            RefreshRosterTreeView?.Invoke(this, e);
        }

        

        private void AddSelectedRegimentToSelectedCharacter()
        {
            if (SelectedPortraitUnit == null)
                throw new InvalidOperationException("AddSelectedRegimentToSelectedCharacter called but SelectedPortraitUnit is null");

            //note this is the SELECTED ROSTER (Not the Portrait!!)
            if (SelectedRosterCharacter == null)
            {
                SendMessageToView?.Invoke(this, "Selected regiment cannot be added as no character is selected to add the regiment to");
                return;
            }

            //it is important that when you add new units that you COPY them to ensure that a specific ID for them is brought in
            var character = Roster.RosterCharacters.First(p => p.Character.ID == SelectedRosterCharacter.Character.ID);

            //determine if the selected regiment is mainstay or restricted.  If its not in either, you will get an error when trying to pull from restricted
            //as we are intentionally using the First directive assuming there is one there (as this should never happen that you find nothing in either)
            if (!(character.Character is IConquestCharacter selectedCharacterWarbands)) 
                throw new InvalidOperationException("The character passed in the roster was not of type IConquestCharacter");

            var unit = selectedCharacterWarbands.MainstayChoices.FirstOrDefault(p => p == SelectedPortraitUnit.Unit);
            if (unit != null)
            {
                if (CanMainstayBeAdded(character))
                {
                    var regiment = SelectedPortraitUnit.Copy();
                    character.MainstayRegiments.Add(regiment);

                    var e = new RosterChangedEventArgs
                    {
                        RosterElement = character,
                        SelectedElementID = regiment.ID
                    };
                    RefreshRosterTreeView?.Invoke(this, e);
                }
                else
                {
                    SendMessageToView?.Invoke(this, "Character has maximum allowed regiments");
                }

                return;
            }

            unit = selectedCharacterWarbands.RestrictedChoices.First(p => p == SelectedPortraitUnit.Unit); //called to trap for another value not existing, we want that to fail
            if (CanRestrictedBeAdded(character))
            {
                var regiment = SelectedPortraitUnit.Copy();
                character.RestrictedRegiments.Add(regiment);

                var e = new RosterChangedEventArgs
                {
                    RosterElement = character,
                    SelectedElementID = regiment.ID
                };
                RefreshRosterTreeView?.Invoke(this, e);
            }
            else
            {
                SendMessageToView?.Invoke(this, "Restricted regiment cannot be added to this character currently");
            }
        }

        private bool CanMainstayBeAdded(IRosterCharacter character)
        {
            var score = character.MainstayRegiments.Count;
            score += (character.RestrictedRegiments.Count * 2);

            return score < MAX_NUMBER_REGIMENTS_ALLOWED;
        }

        private bool CanRestrictedBeAdded(IRosterCharacter character)
        {
            return ((character.MainstayRegiments.Count + character.RestrictedRegiments.Count < MAX_NUMBER_REGIMENTS_ALLOWED) &&
                    character.RestrictedRegiments.Count < character.MainstayRegiments.Count);
        }

        private void LoadStatGrid(IConquestInput data)
        {
            SelectedUnitName = data.Unit;
            SelectedUnitPoints = data.Points + " pts";
            SelectedUnitType = data.ModelType;
            SelectedUnitClass = data.Weight;

            SelectedUnitM = data.Move.ToString();
            SelectedUnitV = data.Volley.ToString();
            SelectedUnitC = data.Clash.ToString();
            SelectedUnitW = data.Wounds.ToString();
            SelectedUnitA = data.Attacks.ToString();
            SelectedUnitR = data.Resolve.ToString();
            SelectedUnitD = data.Defense.ToString();
            SelectedUnitE = data.Evasion.ToString();
            SelectedUnitSpecialRules = data.SpecialRules;

            var dataOutput = GetSelectedOutput(data);
            var output = dataOutput.Item1;
            var averageOutput = dataOutput.Item2;

            SelectedOffense = $"Off: {Math.Round(output.OffenseOutput, 2)}  Avg({Math.Round(averageOutput[0], 2)})";
            SelectedDefense = $"Def: {Math.Round(output.DefenseOutput, 2)}  Avg({Math.Round(averageOutput[1], 2)})"; 
            SelectedOverall = $"Score: {Math.Round(output.TotalOutput, 2)}  Avg({Math.Round(averageOutput[2], 2)})";

            DataPanelVisible = true;
        }

        private Tuple<IConquestAnalysisOutput, double[]> GetSelectedOutput(IConquestInput data)
        {
            //try to find it in the unit collection first
            IConquestAnalysisOutput output = null;
            var averages = new double[3];

            output = _data.UnitOutput.FirstOrDefault(output => output.Unit == data.Unit);
            if (output != null)
            {
                averages[0] = _data.AverageUnitOffense;
                averages[1] = _data.AverageUnitDefense;
                averages[2] = _data.AverageUnitOverall;
                return new Tuple<IConquestAnalysisOutput, double[]>(output, averages);
            }

            output = _data.CharacterOutput.FirstOrDefault(output => output.Unit == data.Unit);
            if (output != null)
            {
                averages[0] = _data.AverageCharacterOffense;
                averages[1] = _data.AverageCharacterDefense;
                averages[2] = _data.AverageCharacterOverall;
                return new Tuple<IConquestAnalysisOutput, double[]>(output, averages);
            }

            throw new InvalidOperationException($"Output data could not be found for input {data.Unit}");
        }
        
        private void SelectUnit(object tag)
        {
            if (!(tag is UnitButton unit)) throw new InvalidOperationException("Item passed back was not the expected type");

            var regiment = unit.Tag as UnitInputModel;
            SelectedPortraitUnit = regiment ?? throw new InvalidOperationException("Item tag was not correct type");

            LoadStatGrid(SelectedPortraitUnit);
            _lastItemSelected = LastItemSelected.Regiment;
        }

        private void LoadUnits(ObservableCollection<UnitButton> collection, IEnumerable<string> units, string collectionName)
        {
            collection.Clear();

            foreach (var unit in units)
            {
                var data = _data.Units.FirstOrDefault((p => p.Unit == unit));

                if (data != null)
                {
                    var btn = new UnitButton() {UnitName = unit, UnitThumb = THUMB_PATH + data.Image, Tag = data};
                    collection.Add(btn);

                    NotifyPropertyChanged(collectionName);
                }
                else
                {
                    Console.WriteLine($"{unit} is not found in the unit dictionary");
                }
            }
        }
    }
}
