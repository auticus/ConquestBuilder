﻿using System;
using System.Windows.Input;
using ConquestBuilder.Models;
using ConquestBuilder.Views;
using ConquestController.Models;
using Microsoft.Win32;

namespace ConquestBuilder.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand NewFaction { get; set; }
        public ICommand LoadFaction { get; set; }
        public ApplicationData Data { get; set; }

        private bool _visible;
        public bool Visible
        {
            get { return _visible;}
            set
            {
                _visible = value;
                NotifyPropertyChanged("Visible");
            }}

        private FactionPickerViewModel _factionPickerVM;
        private ArmyBuilderViewModel _armyBuilderVM;

        public MainViewModel()
        {
            InitializeCommands();
            Data = new ApplicationData();

            InitializeViewModels();
        }

        private void InitializeViewModels()
        {
            _factionPickerVM = new FactionPickerViewModel(Data);
            _factionPickerVM.OnSelectedFaction += OnSelectedFaction;

            _armyBuilderVM = new ArmyBuilderViewModel(Data);
            _armyBuilderVM.OnWindowClosed += ArmyBuilderVM_OnWindowClosed;

            Visible = true;
        }

        private void InitializeCommands()
        {
            NewFaction = new RelayCommand(OnNewFaction, param => this.CanExecute);
            LoadFaction = new RelayCommand(OnLoadFaction, param => this.CanExecute);
        }

        private void OnNewFaction(object parameter)
        {
            var view = new FactionPickerWindow(_factionPickerVM);
            view.ShowDialog();
        }

        private void OnLoadFaction(object parameter)
        {
            //load previously saved army
            var dlg = new OpenFileDialog
            {
                Filter = "Roster Files (*.ros)|*.ros",
                Title = "Open Roster",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dlg.ShowDialog() == true)
            {
                var roster = Roster.Load(dlg.FileName);

                var view = new ArmyBuilderWindow(_armyBuilderVM);

                _armyBuilderVM.SetView(view, roster);
                view.Show();
            }
        }

        /// <summary>
        /// Handles the select item on the faction picker for a new army roster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="card"></param>
        private void OnSelectedFaction(object sender, FactionCarouselCard card)
        {
            Visible = false;
            var view = new ArmyBuilderWindow(_armyBuilderVM);

            _armyBuilderVM.SetView(view, card.Name);
            view.Show();
        }

        private void ArmyBuilderVM_OnWindowClosed(object sender, EventArgs e)
        {
            Visible = true;
        }
    }
}
