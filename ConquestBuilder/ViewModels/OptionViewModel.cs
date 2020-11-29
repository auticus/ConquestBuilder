using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Automation.Provider;
using ConquestController.Data;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestBuilder.ViewModels
{
    public class OptionViewModel : BaseViewModel
    {
        private IConquestGameElementOption _element;
        private IList<IOption> _magicItems;

        public ObservableCollection<ListViewOption> Options { get; set; } //not worried about notification because the lists are initialized in the constructor
        
        public IConquestGameElementOption Element
        {
            get => _element;
            set
            {
                _element = value;
                NotifyPropertyChanged("Element");
            }
        }

        public OptionViewModel(IConquestGameElementOption element, IList<IOption> magicItems)
        {
            Element = element;
            _magicItems = magicItems;
            InitializeData();
        }

        private void InitializeData()
        {
            Options = new ObservableCollection<ListViewOption>();
            AddOptions();
            AddMagicItems();
            SetCheckedState();
        }

        private void SetCheckedState()
        {
            foreach (var option in Element.ActiveOptions)
            {
                foreach (var lvo in Options.Where(p=>p.Category == OptionCategory.Option))
                {
                    var lvOption = (IOption) lvo.Model;
                    if (option.Name == lvOption.Name)
                    {
                        lvo.IsChecked = true;
                        break;
                    }
                }
            }

            foreach (var item in Element.ActiveItems)
            {
                foreach (var lvo in Options.Where(p => p.Category == OptionCategory.Item))
                {
                    var lvOption = (IOption) lvo.Model;
                    if (item.Name == lvOption.Name)
                    {
                        lvo.IsChecked = true;
                        break;
                    }
                }
            }
        }

        private void AddOptions()
        {
            AddHardCodedOptions();

            //the 0 element or grouping is multi select, the rest of the groupings are single-select
            foreach (var item in Element.Options.Cast<IOption>())
            {
                var option = new ListViewOption()
                {
                    Category = OptionCategory.Option,
                    Model = item,
                    Text = $"{item.Name} - {item.Points} pts",
                    IsChecked = false,
                    CheckChanged = ListViewItem_CheckChanged,
                    OptionGrouping = item.Category == "0" ? "Options" : $"Option Category {item.Category}",
                    GroupCanSelectAll = item.Category == "0",
                    MaxAllowableSelectableForGroup = 1
                };
                Options.Add(option);
            }
        }

        private void AddMagicItems()
        {
            if (Element.MaxAllowableItems == 0) return;

            foreach (var item in _magicItems)
            {
                var option = new ListViewOption()
                {
                    Category = OptionCategory.Item,
                    Model = item,
                    Text = $"{item} - {item.Points} pts",
                    IsChecked = false,
                    CheckChanged = ListViewItem_CheckChanged,
                    OptionGrouping = "Items",
                    GroupCanSelectAll = false,
                    MaxAllowableSelectableForGroup = Element.MaxAllowableItems
                };
                Options.Add(option);
            }
        }

        private void AddHardCodedOptions()
        {
            if (Element.LeaderPoints > 0)
            {
                var optionModel = OptionFactory.CreateAdhocOption(Element.Faction, Element.Unit, "Regiment Leader",
                    "Regiment Leader", Element.LeaderPoints);
                Options.Add(CreateHardCodedOption(optionModel));
            }

            if (Element.StandardPoints > 0)
            {
                var optionModel = OptionFactory.CreateAdhocOption(Element.Faction, Element.Unit, "Regiment Standard",
                    "Regiment Standard", Element.LeaderPoints);
                Options.Add(CreateHardCodedOption(optionModel));
            }
        }

        private ListViewOption CreateHardCodedOption(UnitOptionModel model)
        {
            return new ListViewOption()
            {
                Category = OptionCategory.Option,
                Model = model,
                Text = $"{model.Name} - {model.Points} pts",
                IsChecked = false,
                CheckChanged = ListViewItem_CheckChanged,
                OptionGrouping = "Options",
                GroupCanSelectAll = true
            };
        }

        private void ListViewItem_CheckChanged(object sender, bool newSelectedValue)
        {
            var element = (ListViewOption) sender;
            if (element.GroupCanSelectAll || !newSelectedValue) return;

            //uncheck other options in the same category
            if (element.Category == OptionCategory.Option) SynchronizeOptions(element);

            //magic items 
            if (element.Category == OptionCategory.Item) SynchronizeItems(element);
        }

        private void SynchronizeOptions(ListViewOption element)
        {
            foreach (var option in Options.Where(p => p.OptionGrouping == element.OptionGrouping
                                                      && p.Category == element.Category
                                                      && p.Text != element.Text
                                                      && p.IsChecked))
            {
                option.IsChecked = false;
            }
        }

        private void SynchronizeItems(ListViewOption element)
        {
            //because you can take multiple items but are capped at what is the max available, this will cause a blip
            //if you have 2 or more items you can choose, and you have max selected, then select max + 1, it will clear all of the other items except
            //the latest one selected
            if (MagicItemsAvailableToSelect() == true) return;

            foreach (var option in Options.Where(p => p.OptionGrouping == element.OptionGrouping
                                                      && p.Category == element.Category
                                                      && p.Text != element.Text
                                                      && p.IsChecked))
            {
                option.IsChecked = false;
            }
        }

        private bool MagicItemsAvailableToSelect()
        {
            //count how many selected magic items exist, and compare them to how many are allowed to be selected
            var numberSelectedItems = Options.Count(p => p.Category == OptionCategory.Item && p.IsChecked);
            return Element.MaxAllowableItems > numberSelectedItems;
        }
    }
}
