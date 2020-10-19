using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using ConquestController.Analysis;
using ConquestController.Data;
using ConquestController.Models.Input;

namespace ConquestBuilder.Models
{
    public class ApplicationData
    {
        private string _appPath;
        private string _unitInputFile;
        private string _unitOptionsFile;
        private string _characterInputFile;
        private string _characterOptionFile;
        private string _spellFile;
        private string _analysisFile;
        private string _characterAnalysisFile;
        private string _retinueFile;
        private string _masteriesFile;
        private string _itemsFile;

        public IList<CharacterInputModel> Characters { get; private set; }
        public IList<UnitInputModel> Units { get; private set; }
        public IList<SpellModel> Spells { get; private set; }
        public IList<RetinueModel> Retinues { get; private set; }
        public IList<MasteryModel> Masteries { get; private set; }
        public IList<ItemModel> Items { get; private set; }
        

        public ApplicationData()
        {
            LoadConfigurations();
            LoadData();
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
            _retinueFile = ConfigurationManager.AppSettings["RetinueFile"];
            _masteriesFile = ConfigurationManager.AppSettings["MasteriesFile"];
            _itemsFile = ConfigurationManager.AppSettings["ItemsFile"];
        }

        private void LoadData()
        {
            try
            {
                //assign units and their options
                Units = DataRepository.GetInputFromFileToList<UnitInputModel>(_appPath + "\\" + _unitInputFile);
                DataRepository.AssignUnitOptionsToModelsFromFile(Units.Cast<IConquestOptionInput>().ToList(), _appPath + "\\" + _unitOptionsFile);

                var characterInputFilePath = _appPath + "\\" + _characterInputFile;
                var characterOptionFilePath = _appPath + "\\" + _characterOptionFile;
                
                Spells = DataRepository.GetInputFromFileToList<SpellModel>(_appPath + "\\" + _spellFile) as List<SpellModel>;
                Retinues = DataRepository.GetInputFromFileToList<RetinueModel>(_appPath + "\\" + _retinueFile) as List<RetinueModel>;
                Masteries = DataRepository.GetInputFromFileToList<MasteryModel>(_appPath + "\\" + _masteriesFile) as List<MasteryModel>;
                Items = DataRepository.GetInputFromFileToList<ItemModel>(_appPath + "\\" + _itemsFile) as List<ItemModel>;

                Characters = Character.GetAllCharacters(characterInputFilePath,characterOptionFilePath, Spells).ToList();

                /* The below is for processing analysis which this class wont do
                var analysis = new AnalysisController();
                var unitOutput = analysis.BroadAnalysis(units, spells);
                AnalysisFile.WriteAnalysis(_appPath + "\\" + _analysisFile, unitOutput, includeUselessOptions: false);

                var characterOutput = analysis.BroadAnalysis(characters, spells);
                AnalysisFile.WriteAnalysis(_appPath + "\\" + _characterAnalysisFile, characterOutput, includeUselessOptions: false);
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred processing the data:  {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
