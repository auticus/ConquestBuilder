﻿using System;
using ConquestController.Models;

namespace ConquestBuilder.ViewModels
{
    public enum RosterCategory
    {
        Character = 0,
        MainstayRegiment,
        RestrictedRegiment,
        OptionLabel,
        MainstayLabel,
        RestrictedLabel,
        Option
    }

    public enum OptionCategory
    {
        OptionLabel = 0,
        Option
    }

    public class TreeViewRoster
    {
        public RosterCategory Category { get; set; }
        public IRosterCharacter RosterCharacter { get; set; }
        public object Model { get; set; }
    }

    public class ListViewOption : BaseViewModel
    {
        public OptionCategory Category { get; set; }
        public object Model { get; set; }
        public EventHandler<bool> CheckChanged { get; set; }

        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyPropertyChanged("IsChecked");
                CheckChanged?.Invoke(this, _isChecked);
            }
        }

        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged("Text");
            }
        }

        private string _optionGrouping;

        public string OptionGrouping
        {
            get => _optionGrouping;
            set
            {
                _optionGrouping = value;
                NotifyPropertyChanged("OptionGrouping");
            }
        }

        private bool _groupCanMultiSelect;

        public bool GroupCanMultiSelect
        {
            get => _groupCanMultiSelect;
            set
            {
                _groupCanMultiSelect = value;
                NotifyPropertyChanged("GroupCanMultiSelect");
            }
        }
    }
}
