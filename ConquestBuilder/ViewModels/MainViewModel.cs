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
        private string _characterOptionFile;
        private string _spellFile;
        private string _analysisFile;
        private string _characterAnalysisFile;

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
            _characterOptionFile = ConfigurationManager.AppSettings["CharacterOptionFile"];
            _spellFile = ConfigurationManager.AppSettings["SpellFile"];
            _analysisFile = ConfigurationManager.AppSettings["AnalysisFile"];
            _characterAnalysisFile = ConfigurationManager.AppSettings["CharacterAnalysisFile"];
        }

        private void LoadData(object obj)
        {
            try
            {
                //assign units and their options
                var units = DataRepository.GetInputFromFileToList<UnitInputModel>(_appPath + "\\" + _unitInputFile);
                DataRepository.AssignUnitOptionsToModelsFromFile(units.Cast<IConquestOptionInput>().ToList(), _appPath + "\\" + _unitOptionsFile);

                var characterInputFilePath = _appPath + "\\" + _characterInputFile;
                var characterOptionFilePath = _appPath + "\\" + _characterOptionFile;
                var spells = DataRepository.GetInputFromFileToList<SpellModel>(_appPath + "\\" + _spellFile) as List<SpellModel>;
                var characters = Character.GetAllCharacters(characterInputFilePath,
                                                                              characterOptionFilePath,
                                                                                            spells);

                var analysis = new AnalysisController();
                var unitOutput = analysis.BroadAnalysis(units, spells);
                AnalysisFile.WriteAnalysis(_appPath + "\\" + _analysisFile, unitOutput);

                var characterOutput = analysis.BroadAnalysis(characters, spells);
                AnalysisFile.WriteAnalysis(_appPath + "\\" + _characterAnalysisFile, characterOutput);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error has occurred processing the data:  {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
