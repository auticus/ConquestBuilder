using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ConquestController;
using ConquestController.Data;
using System.Configuration;
using ConquestController.Models;
using System.Linq;
using ConquestController.Analysis;
using ConquestController.Models.Input;

namespace ConquestBuilder.ViewModels
{
    public class MainViewModel
    {
        public ICommand LoadDataCommand { get; set; }
        private string _appPath;
        private string _unitInputFile;
        private string _unitOptionsFile;
        private string _characterInputFile;
        private string _spellFile;
        private string _analysisFile;

        public MainViewModel()
        {
            InitializeCommands();
            LoadConfigurations();
        }

        public bool CanExecute { get; set; } = true;

        private void InitializeCommands()
        {
            LoadDataCommand = new RelayCommand(LoadData, param => this.CanExecute);
        }

        private void LoadConfigurations()
        {
            _appPath = System.AppDomain.CurrentDomain.BaseDirectory + "Data";
            _unitInputFile = ConfigurationManager.AppSettings["UnitInputFile"];
            _unitOptionsFile = ConfigurationManager.AppSettings["UnitOptionFile"];
            _characterInputFile = ConfigurationManager.AppSettings["CharacterInputFile"];
            _spellFile = ConfigurationManager.AppSettings["SpellFile"];
            _analysisFile = ConfigurationManager.AppSettings["AnalysisFile"];
        }

        private void LoadData(object obj)
        {
            try
            {
                //assign units and their options
                var units = DataRepository.GetInputFromFileToList<UnitInputModel>(_appPath + "\\" + _unitInputFile);
                DataRepository.AssignUnitOptionsToModelsFromFile(units.Cast<IConquestRegimentInput>().ToList(), _appPath + "\\" + _unitOptionsFile);

                //assign characters
                var characters = DataRepository.GetInputFromFileToList<CharacterInputModel>(_appPath + "\\" + _characterInputFile);

                //assign mainstay and restricted choices
                foreach (var character in characters)
                {
                    DataRepository.AssignDelimitedPropertyToList(character.MainstayChoices, character.Mainstay);
                    DataRepository.AssignDelimitedPropertyToList(character.RestrictedChoices, character.Restricted);
                    DataRepository.AssignDelimitedPropertyToList(character.Schools, character.SpellSchools);
                }

                //assign character options
                DataRepository.AssignUnitOptionsToModelsFromFile(characters.Cast<IConquestRegimentInput>().ToList(), _appPath + "\\" + _unitOptionsFile);

                //populate spell schools
                var spells = DataRepository.GetInputFromFileToList<SpellModel>(_appPath + "\\" + _spellFile);

                var analysis = new AnalysisController();
                var output = analysis.AnalyzeTotalGame(units);
                AnalysisFile.WriteAnalysis(_appPath + "\\" + _analysisFile, output);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error has occurred processing the data:  {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
