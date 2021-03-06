﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;
using ConquestController.Models;
using ConquestController.Models.Input;
using ConquestController.Models.Output;
using Microsoft.Win32;

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
        private readonly Dictionary<IConquestCharacter, IEnumerable<string>> _originalCharacterRestrictedTemplate; //for loading the original restricted templates if they change

        private Window _view; //the window
        
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

        private List<IBaseOption> FilteredMagicItems
        {
            get
            {
                return _data.Items.Where(p => p.Faction == Filter).OrderBy(p => p.Category).ThenBy(p => p.Name)
                    .Cast<IBaseOption>().ToList();
            }
        }
        #endregion Private Fields

        #region Commands
        public ICommand CharacterSelected { get; set; }
        public ICommand MainstaySelected { get; set; }
        public ICommand RestrictedSelected { get; set; }
        public ICommand AddSelectedToRoster { get; set; }
        public ICommand DeleteRosterElement { get; set; }
        public ICommand OptionElement { get; set; }
        public ICommand RenameElement { get; set; }
        public ICommand DecrementElement { get; set; }
        public ICommand IncrementElement { get; set; }
        public ICommand SaveRoster { get; set; }
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

        private IConquestGamePiece _selectedPortraitCharacter;

        /// <summary>
        /// Represents the character portrait that is selected in the UI (not the selected character on the roster)
        /// </summary>
        public IConquestGamePiece SelectedPortraitCharacter
        {
            get => _selectedPortraitCharacter;
            set
            {
                _selectedPortraitCharacter = value;
                NotifyPropertyChanged("SelectedPortraitCharacter");
            }
        }

        private IConquestGamePiece _selectedPortraitUnit;

        public IConquestGamePiece SelectedPortraitUnit
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

                SelectedRosterElementEnabled = SelectedRosterCharacter != null || SelectedRosterUnit != null;
                CanSelectedRosterElementIncrement();
            }
        }

        private IConquestGamePiece _selectedRosterUnit;

        /// <summary>
        /// The selected unit from the roster
        /// </summary>
        public IConquestGamePiece SelectedRosterUnit
        {
            get => _selectedRosterUnit;
            set
            {
                _selectedRosterUnit = value;
                NotifyPropertyChanged("SelectedRosterUnit");

                SelectedRosterElementEnabled = SelectedRosterCharacter != null || SelectedRosterUnit != null;
                CanSelectedRosterElementIncrement();
            }
        }

        public IConquestGamePiece SelectedElement => SelectedRosterUnit ?? SelectedRosterCharacter?.Character; //three chances for that sweet sweet null


        private bool _dataPanelEnabled;

        /// <summary>
        /// Dictates whether the data panel with the unit information is displayed or not
        /// </summary>
        public bool DataPanelEnabled
        {
            get => _dataPanelEnabled;
            set
            {
                _dataPanelEnabled = value;
                NotifyPropertyChanged("DataPanelEnabled");
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

        private bool _selectedRosterElementEnabled;

        public bool SelectedRosterElementEnabled
        {
            get => _selectedRosterElementEnabled;
            set
            {
                _selectedRosterElementEnabled = value;
                NotifyPropertyChanged("SelectedRosterElementEnabled");
            }
        }

        private bool _selectedRosterElementCanGrow;

        public bool SelectedRosterElementCanGrow
        {
            get => _selectedRosterElementCanGrow;
            set
            {
                _selectedRosterElementCanGrow = value;
                NotifyPropertyChanged("SelectedRosterElementCanGrow");
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
            _originalCharacterRestrictedTemplate = new Dictionary<IConquestCharacter, IEnumerable<string>>();
            InitializeCommands();
        }

        #endregion Constructors

        #region Public Methods
        public void SetView(Window view, string army)
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
            InitializeRoster(army);
            DataPanelEnabled = false;
            _view = view;
        }

        public void SetView(Window view, Roster roster)
        {
            _currentArmy = roster.RosterFaction.ToLower() switch
            {
                "100 kingdoms" => Armies.HundredKingdoms,
                "spires" => Armies.Spires,
                "dweghom" => Armies.Dweghom,
                "nords" => Armies.Nords,
                _ => throw new InvalidOperationException($"{roster.RosterFaction} was passed but is not supported")
            };

            InitializeControls();
            InitializeRoster(roster);
            DataPanelEnabled = false;
            _view = view;
        }

        #endregion Public Methods

        private void InitializeCommands()
        {
            CharacterSelected = new RelayCommand(OnCharacterSelected, param => this.CanExecute);
            MainstaySelected = new RelayCommand(OnMainstaySelected, param => this.CanExecute);
            RestrictedSelected = new RelayCommand(OnRestrictedSelected, param=> this.CanExecute);
            AddSelectedToRoster = new RelayCommand(OnSelectionAdded, param=>CanExecute);
            DeleteRosterElement = new RelayCommand(OnRosterElementDeleted, param => this.CanExecute);
            OptionElement = new RelayCommand(OnRosterElementOption, param => this.CanExecute);
            RenameElement = new RelayCommand(OnRenameElement, param => this.CanExecute);
            DecrementElement = new RelayCommand(OnDecrementElement, param => this.SelectedRosterElementCanGrow);
            IncrementElement = new RelayCommand(OnIncrementElement, param => this.SelectedRosterElementCanGrow);
            SaveRoster = new RelayCommand(OnSaveRoster, param=>this.CanExecute);
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

        private void InitializeRoster(string army)
        {
            Roster = new Roster
            {
                RosterName = "New Roster",
                RosterFaction = army,
                RosterLimit = 0, 
                ID = Guid.NewGuid()
            };

            RefreshRosterTreeView(this, new RosterChangedEventArgs());
        }

        private void InitializeRoster(Roster roster)
        {
            Roster = roster;
            RefreshRosterTreeView(this, new RosterChangedEventArgs());
        }

        private void CanSelectedRosterElementIncrement()
        {
            //but can we now increment or decrement it?
            SelectedRosterElementCanGrow = (
                (SelectedElement != null) &&
                (SelectedElement is IConquestCharacter) == false &&
                (
                    SelectedElement.ModelType.ToUpper() == "INFANTRY" ||
                    SelectedElement.ModelType.ToUpper() == "CAVALRY" ||
                    SelectedElement.ModelType.ToUpper() == "BRUTE"
                )
            );
        }

        private void OnCharacterSelected(object tag)
        {
            if (!(tag is UnitButton unit)) throw new InvalidOperationException("Item passed back was not the expected type");

            var character = unit.Tag as IConquestCharacter;
            SelectedPortraitCharacter = character ?? throw new InvalidOperationException("Item tag was not correct type");
            LoadUnits(MainstayButtons, character.MainstayChoices, "MainstayButtons");
            LoadRestrictedUnits(character);

            LoadStatGrid(SelectedPortraitCharacter);
            _lastItemSelected = LastItemSelected.Character;
        }

        /// <summary>
        /// Loads the characters restrictions or all restrictions if the character contains a mastery that unlocks all restrictions across the faction
        /// </summary>
        /// <param name="character"></param>
        private void LoadRestrictedUnits(IConquestCharacter character)
        {
            var restrictedChoices = GetAvailableRestrictedRegiments(character);

            //reconcile the character's choices
            character.RestrictedChoices = restrictedChoices;
            LoadUnits(RestrictedButtons, restrictedChoices, "RestrictedButtons");
        }

        private IEnumerable<string> GetAvailableRestrictedRegiments(IConquestCharacter character)
        {
            //Eccentric is the tag that means they get access to all restricted regiments
            if (character.ActiveMasteries.Any(mastery => mastery.Tag.Split("|").Any(p => p == "Eccentric")))
            {
                var restrictedChoices = new List<string>();
                restrictedChoices.AddRange(character.RestrictedChoices);

                //if this has not been templated yet, template it so it can be restored later
                if (_originalCharacterRestrictedTemplate.ContainsKey(character) == false)
                {
                    var copyList = new List<string>();
                    copyList.AddRange(character.RestrictedChoices);
                    _originalCharacterRestrictedTemplate.Add(character, copyList);
                }

                foreach (var dude in _data.Characters.Where(ch => ch.Faction == Filter))
                {
                    foreach (var choice in dude.RestrictedChoices)
                    {
                        if (restrictedChoices.Any(p=>p == choice) == false)
                            restrictedChoices.Add(choice);
                    }
                }

                ReconcileRestrictedChoicesForCharacter(character, restrictedChoices);
                return restrictedChoices;
            }
            else
            {
                //reset the character's template to the default if necessary
                if (_originalCharacterRestrictedTemplate.ContainsKey(character)) //if this is there that means we modified it, otherwise it was never modified
                {
                    var template = _originalCharacterRestrictedTemplate[character];
                    var restrictedChoices = character.RestrictedChoices as List<string>;
                    restrictedChoices?.Clear();
                    restrictedChoices?.AddRange(template);
                }
            }

            ReconcileRestrictedChoicesForCharacter(character, character.RestrictedChoices);
            return character.RestrictedChoices;
        }

        /// <summary>
        /// Remove any restricted choices from the character that are no longer valid
        /// </summary>
        /// <param name="character"></param>
        /// <param name="availableRestrictedChoices"></param>
        private void ReconcileRestrictedChoicesForCharacter(IConquestCharacter character, IEnumerable<string> availableRestrictedChoices)
        {
            var rosterCharacter = Roster.RosterCharacters.FirstOrDefault(p => p.Character.ID == character.ID);
            if (rosterCharacter == null) return;

            var removeList = new List<IConquestGamePiece>();

            foreach (var unit in rosterCharacter.RestrictedRegiments)
            {
                if (availableRestrictedChoices.Any(p => p == unit.Unit) == false)
                {
                    removeList.Add(unit);
                }
            }

            //if the character has any actual regiments selected, remove them if applicable
            foreach (var unit in removeList)
            {
                rosterCharacter.RestrictedRegiments.Remove(unit);
            }
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

        private void OnIncrementElement(object canExecute)
        {
            if (!IsEligibleForStandCountChange()) return;

            SelectedElement.StandCount++;
            SelectedElement.Points += SelectedElement.AdditionalPoints;
            SelectedElement.PointsChanged?.Invoke(this, EventArgs.Empty);
            RefreshRosterTreeView?.Invoke(this, new RosterChangedEventArgs() { RosterElement = SelectedRosterCharacter, SelectedElementID = SelectedElement.ID });
        }

        private void OnDecrementElement(object canExecute)
        {
            if (!IsEligibleForStandCountChange()) return;
            if (SelectedElement.StandCount == 3) return;  //3 is the min stand count for a regiment

            SelectedElement.StandCount--;
            SelectedElement.Points -= SelectedElement.AdditionalPoints;
            SelectedElement.PointsChanged?.Invoke(this, EventArgs.Empty);
            RefreshRosterTreeView?.Invoke(this, new RosterChangedEventArgs() { RosterElement = SelectedRosterCharacter, SelectedElementID = SelectedElement.ID });
        }

        private void OnSaveRoster(object canExecute)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Roster File (*.ros)|*.ros",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Save Roster"
            };

            try
            {
                if (dlg.ShowDialog() == true)
                {
                    _roster.Save(dlg.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Shit blew up saving! {e}");
                SendMessageToView?.Invoke(this, $"An error occurred saving the roster:  {e.Message}");
            }
        }

        private bool IsEligibleForStandCountChange()
        {
            if (SelectedElement == null) return false; //this should not be happening
            if (SelectedElement is IConquestCharacter) return false;
            if (SelectedElement.ModelType.ToUpper() == "MONSTER") return false;

            return true;
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
                    var regiment = SelectedPortraitUnit.Clone() as IConquestGamePiece;
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
                var regiment = SelectedPortraitUnit.Clone() as IConquestGamePiece;
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

        private void LoadStatGrid(IConquestBaseGameElement data)
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

            DataPanelEnabled = true;
        }

        private Tuple<IConquestAnalysisOutput, double[]> GetSelectedOutput(IConquestBaseGameElement data)
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

            var regiment = unit.Tag as UnitGameElementModel;
            SelectedPortraitUnit = regiment ?? throw new InvalidOperationException("Item tag was not correct type");

            LoadStatGrid(SelectedPortraitUnit);
            _lastItemSelected = LastItemSelected.Regiment;
        }

        /// <summary>
        /// Loads the respective units that are available to select into their button columns
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="units"></param>
        /// <param name="collectionName"></param>
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

        /// <summary>
        /// Handles when a mastery is deleted from the Option Window
        /// </summary>
        /// <param name="mastery"></param>
        private void HandleMasteryDeletion(IMastery mastery)
        {
            if (!Roster.MasterySpam.ContainsKey(mastery.Name)) return;
            if (Roster.MasterySpam[mastery.Name].Count == 0) return;

            //at this point we know there are active spammed items (or they were spammed when this was called, which is invoked in the option window) that exist
            //that need to have their points adjusted down because one of the spammed items got deleted

            //***the trick here is that the mastery will still be in the active mastery list of the character so we have to ignore the character because this method gets
            //fired from the option window as soon as the check mark is unchecked***
            var affectedElements = new List<Tuple<RosterCharacter, IMastery>>();

            foreach (var rosterElement in this.Roster.RosterCharacters)
            {
                var target = rosterElement.Character.ActiveMasteries.FirstOrDefault(p => p.Name == mastery.Name);
                if (target == null) continue;

                affectedElements.Add(new Tuple<RosterCharacter, IMastery>(rosterElement, target));
            }

            affectedElements = affectedElements.OrderBy(p => p.Item2.Points).ToList();
            var iteration = 0;

            //the sort order is very important for this loop, we must start with the lowest and work our way up
            foreach (var (character, curMastery) in affectedElements)
            {
                curMastery.Points = curMastery.BasePoints;
                for (var i = 0; i < iteration; i++)
                {
                    curMastery.Points *= 2;
                }

                //every time we do this, pump up the iteration by one because we need to modify the next one in the chain to be an additional doubling
                iteration++;
            }

            this.Roster.RefreshPoints();
        }

        /// <summary>
        /// Opens the option dialog modal
        /// </summary>
        /// <param name="element"></param>
        private void OnRosterElementOption(object element)
        {
            var selectedGuid = SelectedElement.ID;

            var optionVM = new OptionViewModel(SelectedElement, 
                FilteredMagicItems, 
                _data.Retinues.Where(p=>p.Faction == "ALL" || p.Faction == SelectedElement.Faction), 
                _data.Perks,
                _data.Spells,
                selectedGuid);
            var window = new OptionsWindow(_view, optionVM);

            if (window.ShowDialog() == true)
            {
                //add and remove the elements from the collections
                SynchronizeGameElement(optionVM, SelectedElement);

                //adjust the point values for any masteries that were deleted which can cause a chain reaction of spammed items needing readjusted
                foreach (var mastery in optionVM.ActiveMasteryState.Where(p => p.Item2 == false))
                    HandleMasteryDeletion(mastery.Item1);

                //now readjust the restricted choices because some options may have opened them up more (or closed them)
                if (SelectedElement is IConquestCharacter ele)
                    LoadRestrictedUnits(ele);

                //redraw the tree
                RefreshRosterTreeView?.Invoke(this, new RosterChangedEventArgs(){RosterElement = SelectedRosterCharacter, SelectedElementID = selectedGuid});

                //the very last thing should be refreshing points
                Roster.RefreshPoints();
            }
        }

        /// <summary>
        /// Add and remove the new options and masteries that were selected 
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="element"></param>
        private void SynchronizeGameElement(OptionViewModel vm, IConquestGamePiece element)
        {
            element.ActiveOptions.Clear();

            if (element is IConquestCharacter character)
            {
                SynchronizeCharacterElement(vm, character);
                return;
            }
            
            foreach (var option in vm.Options.Where(p => p.IsChecked))
            {
                switch (option.Category)
                {
                    case OptionCategory.Option:
                        element.ActiveOptions.Add((IBaseOption)option.Model);
                        break;
                    case OptionCategory.Item:
                    case OptionCategory.Mastery:
                        throw new InvalidOperationException($"Category '{option.Category}' was passed for a base game piece which is not supported");
                    default:
                        throw new InvalidOperationException($"The category '{option.Category}' was not accounted for in ArmyBuilderViewModel::SynchronizeElement");
                }
            }
        }

        private void SynchronizeCharacterElement(OptionViewModel vm, IConquestCharacter element)
        {
            element.ActiveItems.Clear();
            element.ActiveMasteries.Clear();
            element.ActiveRetinues.Clear();
            element.ActivePerks.Clear();
            element.ActiveSpells.Clear();

            foreach (var option in vm.Options.Where(p => p.IsChecked))
            {
                switch (option.Category)
                {
                    case OptionCategory.Option:
                        element.ActiveOptions.Add((IBaseOption)option.Model);
                        break;
                    case OptionCategory.Item:
                        element.ActiveItems.Add((IBaseOption)option.Model);
                        break;
                    case OptionCategory.Mastery:
                        element.ActiveMasteries.Add((IMastery)option.Model);
                        break;
                    case OptionCategory.Retinue:
                        element.ActiveRetinues.Add((ITieredBaseOption)option.Model);
                        break;
                    case OptionCategory.Perk:
                        element.ActivePerks.Add((IPerkOption)option.Model);
                        break;
                    case OptionCategory.Spell:
                    case OptionCategory.LearnedInOccultSpell:
                        element.ActiveSpells.Add((IBaseOption)option.Model);
                        break;
                    default:
                        throw new InvalidOperationException($"The category '{option.Category}' was not accounted for in ArmyBuilderViewModel::SynchronizeElement");
                }
            }
        }

        private void OnRenameElement(object element)
        {
            var vm = new InputBoxViewModel(){Caption = "Rename Regiment", Data = SelectedElement.UserName, Message = "Enter the new name of the Selected Regiment or Character"};
            var window = new InputBox(_view, vm);
            Guid selectedGuid = SelectedElement.ID;

            if (window.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(vm.Data))
                {
                    SelectedElement.UserName = vm.Data;
                    RefreshRosterTreeView?.Invoke(this, new RosterChangedEventArgs() { RosterElement = SelectedRosterCharacter, SelectedElementID = selectedGuid });
                }
            }
        }
    }
}
