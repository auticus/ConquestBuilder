using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using ConquestController.Analysis;
using ConquestController.Data;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

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

        public IList<IConquestCharacter> Characters { get; private set; }
        public IList<IConquestGamePiece> Units { get; private set; }
        public IList<ISpell> Spells { get; private set; }
        public IList<ITieredBaseOption> Retinues { get; private set; }
        public IList<IMastery> Masteries { get; private set; }
        public IList<IPerkOption> Items { get; private set; }
        public IList<IPerkOption> Perks { get; private set; }
        
        public IList<IConquestAnalysisOutput> UnitOutput { get; private set; }
        public IList<IConquestAnalysisOutput> CharacterOutput { get; private set; }

        public double AverageUnitOffense
        {
            get
            {
                return UnitOutput.Average(p => p.OffenseOutput);
            }
        }

        public double AverageUnitDefense
        {
            get
            {
                return UnitOutput.Average(p => p.DefenseOutput);
            }
        }

        public double AverageUnitOverall
        {
            get
            {
                return UnitOutput.Average(p => p.TotalOutput);
            }
        }

        public double AverageCharacterOffense
        {
            get
            {
                return CharacterOutput.Average(p => p.OffenseOutput);
            }
        }

        public double AverageCharacterDefense
        {
            get
            {
                return CharacterOutput.Average(p => p.DefenseOutput);
            }
        }

        public double AverageCharacterOverall
        {
            get
            {
                return CharacterOutput.Average(p => p.TotalOutput);
            }
        }

        public ApplicationData()
        {
            LoadConfigurations();
            LoadData();
            LoadAnalysis();
            WriteAnalysisDataToFile();
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
                Units = DataRepository.GetInputFromFileToList<UnitGameElementModel>(_appPath + "\\" + _unitInputFile).Cast<IConquestGamePiece>().ToList();
                DataRepository.AssignUnitOptionsToModelsFromFile(Units.ToList(), _appPath + "\\" + _unitOptionsFile);

                var characterInputFilePath = _appPath + "\\" + _characterInputFile;
                var characterOptionFilePath = _appPath + "\\" + _characterOptionFile;

                Spells = DataRepository.GetInputFromFileToList<SpellModel>(_appPath + "\\" + _spellFile).Cast<ISpell>().ToList(); //todo interface this
                Retinues = DataRepository.GetInputFromFileToList<RetinueModel>(_appPath + "\\" + _retinueFile).Cast<ITieredBaseOption>().ToList();
                Masteries = DataRepository.GetInputFromFileToList<MasteryModel>(_appPath + "\\" + _masteriesFile).Cast<IMastery>().ToList();
                Items = DataRepository.GetInputFromFileToList<ItemModel>(_appPath + "\\" + _itemsFile).Cast<IPerkOption>().ToList();

                IList<IPerkOption> perks;
                Characters = Character.GetAllCharacters(characterInputFilePath, characterOptionFilePath, Spells, Masteries, Retinues, Items, out perks).ToList();

                Perks = perks;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred processing the data:  {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAnalysis()
        {
            var analysis = new AnalysisController();
            UnitOutput = analysis.BroadAnalysis(Units, Spells);
            CharacterOutput = analysis.BroadAnalysis(Characters, Spells);
            WriteAnalysisDataToFile();
        }

        private void WriteAnalysisDataToFile()
        {
            AnalysisFile.WriteAnalysis(_appPath + "\\" + _analysisFile, UnitOutput, includeUselessOptions: false);
            AnalysisFile.WriteAnalysis(_appPath + "\\" + _characterAnalysisFile, CharacterOutput, includeUselessOptions: false);
        }
    }
}
