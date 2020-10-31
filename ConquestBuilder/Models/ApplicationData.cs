﻿using System;
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

        public IList<CharacterInputModel> Characters { get; private set; }
        public IList<UnitInputModel> Units { get; private set; }
        public IList<SpellModel> Spells { get; private set; }
        public IList<RetinueModel> Retinues { get; private set; }
        public IList<MasteryModel> Masteries { get; private set; }
        public IList<ItemModel> Items { get; private set; }
        
        public IList<ConquestUnitOutput> UnitOutput { get; private set; }
        public IList<ConquestUnitOutput> CharacterOutput { get; private set; }

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

                Characters = Character.GetAllCharacters(characterInputFilePath, characterOptionFilePath, Spells).ToList();
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
        }

        private void WriteAnalysisDataToFile()
        {
            AnalysisFile.WriteAnalysis(_appPath + "\\" + _analysisFile, UnitOutput, includeUselessOptions: false);
            AnalysisFile.WriteAnalysis(_appPath + "\\" + _characterAnalysisFile, CharacterOutput, includeUselessOptions: false);
        }
    }
}
