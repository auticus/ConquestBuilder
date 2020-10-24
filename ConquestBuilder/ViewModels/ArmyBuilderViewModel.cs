using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;
using ConquestController.Models.Input;

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

        private CharacterInputModel _selectedCharacter;

        public CharacterInputModel SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                _selectedCharacter = value;
                NotifyPropertyChanged("SelectedCharacter");
            }
        }

        private UnitInputModel _selectedUnit;

        public UnitInputModel SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                NotifyPropertyChanged("SelectedUnit");
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
            LoadUnits(MainstayButtons, SelectedCharacter.MainstayChoices, "MainstayButtons");
            LoadUnits(RestrictedButtons, SelectedCharacter.RestrictedChoices, "RestrictedButtons");
        }

        private void OnMainstaySelected(object tag)
        {
            SelectUnit(tag);
        }

        private void OnRestrictedSelected(object tag)
        {
            SelectUnit(tag);
        }

        private void SelectUnit(object tag)
        {
            if (!(tag is UnitButton unit)) throw new InvalidOperationException("Item passed back was not the expected type");

            var regiment = unit.Tag as UnitInputModel;
            SelectedUnit = regiment ?? throw new InvalidOperationException("Item tag was not correct type");
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
