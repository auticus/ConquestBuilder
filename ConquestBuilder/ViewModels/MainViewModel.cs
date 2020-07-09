using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ConquestController;
using ConquestController.Data;
using System.Configuration;

namespace ConquestBuilder.ViewModels
{
    public class MainViewModel
    {
        public ICommand LoadDataCommand { get; set; }
        private string _appPath;
        private string _unitInputFile;
        private string _unitOptionsFile;

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
        }

        private void LoadData(object obj)
        {
            try
            {
                var units = DataRepository.GetUnitInputFromFile(_appPath + "\\" + _unitInputFile);
                DataRepository.AssignUnitOptionsToModelsFromFile(units, _appPath + "\\" + _unitOptionsFile);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"An error has occurred processing the data:  {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
