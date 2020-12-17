using System;
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
        Option,
        Item,
        Mastery,
        Retinue,
        Perk
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
        
        private bool _tieredSelection { get; set; }

        /// <summary>
        /// When TRUE indicates that a tier system is in place and items under this will be selected as well by default when choosing an item
        /// </summary>
        public bool TieredSelection
        {
            get => _tieredSelection;
            set
            {
                _tieredSelection = value;
                NotifyPropertyChanged("TieredSelection");
            }
        }

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

        private int _maxAllowableSelectableForGroup;

        public int MaxAllowableSelectableForGroup
        {
            get => _maxAllowableSelectableForGroup;
            set
            {
                _maxAllowableSelectableForGroup = value;
                NotifyPropertyChanged("MaxAllowableSelectableForGroup");
            }
        }

        private bool _groupCanSelectAll;

        /// <summary>
        /// When TRUE means ALL items of the collection may be selected
        /// </summary>
        public bool GroupCanSelectAll
        {
            get => _groupCanSelectAll;
            set
            {
                _groupCanSelectAll = value;
                NotifyPropertyChanged("GroupCanMultiSelect");
            }
        }

        private string _tooltip;

        public string Tooltip
        {
            get => _tooltip;
            set
            {
                _tooltip = value;
                NotifyPropertyChanged("Tooltip");
            }
        }
    }
}
