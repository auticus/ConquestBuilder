using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestBuilder.ViewModels
{
    public class ArmyBuilderViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly ApplicationData _data;
        private Armies _currentArmy;
        private const string THUMB_PATH = "..\\Images\\Thumbs\\";

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
        #endregion Commands

        #region Public Properties

        public EventHandler OnWindowClosed { get; set; }

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

        private IConquestInput _selectedCharacter;

        public IConquestInput SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                _selectedCharacter = value;
                NotifyPropertyChanged("SelectedCharacter");
            }
        }

        private IConquestInput _selectedUnit;

        public IConquestInput SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                NotifyPropertyChanged("SelectedUnit");
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

        #endregion Public Properties

        #region Constructors
        public ArmyBuilderViewModel(ApplicationData data)
        {
            _data = data;
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
            DataPanelVisible = false;
        }

        #endregion Public Methods

        private void CloseView(object parameter)
        {
            if (!(parameter is IView view)) throw new ArgumentException("Parameter passed to ArmyBuilderViewModel::CloseView was not an IView");
            view.Close();

            OnWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private void InitializeCommands()
        {
            CharacterSelected = new RelayCommand(OnCharacterSelected, param => this.CanExecute);
            MainstaySelected = new RelayCommand(OnMainstaySelected, param => this.CanExecute);
            RestrictedSelected = new RelayCommand(OnRestrictedSelected, param=> this.CanExecute);
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

        private void OnCharacterSelected(object tag)
        {
            if (!(tag is UnitButton unit)) throw new InvalidOperationException("Item passed back was not the expected type");

            var character = unit.Tag as CharacterInputModel;
            SelectedCharacter = character ?? throw new InvalidOperationException("Item tag was not correct type");
            LoadUnits(MainstayButtons, character.MainstayChoices, "MainstayButtons");
            LoadUnits(RestrictedButtons, character.RestrictedChoices, "RestrictedButtons");

            LoadStatGrid(SelectedCharacter);
        }

        private void OnMainstaySelected(object tag)
        {
            SelectUnit(tag);
        }

        private void OnRestrictedSelected(object tag)
        {
            SelectUnit(tag);
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

            SelectedOffense = $"Off: {Math.Round(output.OffenseOutput, 2)}  Avg({Math.Round(averageOutput[0])})";
            SelectedDefense = $"Def: {Math.Round(output.DefenseOutput, 2)}  Avg({Math.Round(averageOutput[1])})"; 
            SelectedOverall = $"Score: {Math.Round(output.TotalOutput, 2)}  Avg({Math.Round(averageOutput[2])})";

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
            SelectedUnit = regiment ?? throw new InvalidOperationException("Item tag was not correct type");

            LoadStatGrid(SelectedUnit);
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
