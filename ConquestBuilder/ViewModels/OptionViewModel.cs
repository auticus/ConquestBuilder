using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xaml;
using ConquestController.Data;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestBuilder.ViewModels
{
    public class OptionViewModel : BaseViewModel
    {
        private IConquestGamePiece _element;
        private readonly IEnumerable<IOption> _magicItems;
        private readonly IEnumerable<ITieredOption> _retinues;
        private readonly Guid _characterID;

        public ObservableCollection<ListViewOption> Options { get; set; } //not worried about notification because the lists are initialized in the constructor

        /// <summary>
        /// Keeps track of masteries that are selected upon entering and if they have been turned off.  A tuple bool of false indicates they were turned off
        /// </summary>
        public List<Tuple<IMastery, bool>> ActiveMasteryState { get; set; }
        
        /// <summary>
        /// The regiment or character that is having options modified on it
        /// </summary>
        public IConquestGamePiece Element
        {
            get => _element;
            set
            {
                _element = value;
                NotifyPropertyChanged("Element");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="element">the element having the options applied to it</param>
        /// <param name="magicItems">Magic Items to display</param>
        /// <param name="retinues">Filtered retinues by the faction</param>
        public OptionViewModel(IConquestGamePiece element, IEnumerable<IOption> magicItems, IEnumerable<ITieredOption> retinues, Guid characterID)
        {
            Element = element;
            _magicItems = magicItems;
            _retinues = retinues;
            _characterID = characterID;

            ActiveMasteryState = new List<Tuple<IMastery,bool>>();
            InitializeData();
        }

        private void InitializeData()
        {
            //todo: good times ahead.  For masteries that depend on retinues, when the retinues are selected that will dynamically generate mastery items here
            //either that or we can disable them and reenable them if restrictions are lifted.  That actually sounds more sane

            Options = new ObservableCollection<ListViewOption>();
            AddOptions();

            if (Element is IConquestCharacter element)
            {
                AddMagicItems(element);
                AddMasteries(element);
                AddRetinues(element);
            }
            
            SetInitialCheckedState();
        }

        private void SetInitialCheckedState()
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

            if (Element is IConquestCharacter)
            {
                SetCharacterInitialCheckedState();
            }
        }

        /// <summary>
        /// sets initial checked state for character
        /// </summary>
        private void SetCharacterInitialCheckedState()
        {
            var element = (IConquestCharacter)Element;
            foreach (var item in element.ActiveItems)
            {
                foreach (var lvo in Options.Where(p => p.Category == OptionCategory.Item))
                {
                    var lvOption = (IOption)lvo.Model;
                    if (item.Name == lvOption.Name)
                    {
                        lvo.IsChecked = true;
                        break;
                    }
                }
            }

            foreach (var mastery in element.ActiveMasteries)
            {
                foreach (var lvo in Options.Where(p => p.Category == OptionCategory.Mastery))
                {
                    var lvOption = (IMastery)lvo.Model;
                    if (mastery.Name == lvOption.Name)
                    {
                        lvo.IsChecked = true;

                        ActiveMasteryState.Add(new Tuple<IMastery, bool>(mastery, true));
                        break;
                    }
                }
            }

            foreach (var retinue in element.ActiveRetinues)
            {
                foreach (var lvo in Options.Where(p => p.Category == OptionCategory.Retinue))
                {
                    var lvOption = (ITieredOption) lvo.Model;
                    if (retinue.Name == lvOption.Name)
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
                    MaxAllowableSelectableForGroup = 1,
                    Tooltip = item.Notes
                };
                Options.Add(option);
            }
        }

        private void AddMagicItems(IConquestCharacter element)
        {
            if (element.MaxAllowableItems == 0) return;

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
                    MaxAllowableSelectableForGroup = element.MaxAllowableItems,
                    Tooltip = item.Notes
                };
                Options.Add(option);
            }
        }

        private void AddMasteries(IConquestCharacter element)
        {
            if (element.MaxAllowableMasteries == 0) return;

            //if the element is an IConquestCharacter then it will have masteries you can choose from
            //so char 1 item 1 25 pts, then char 2 item 1 50 pts but now char 1 item 1 is also 50 pts
            foreach (var mastery in element.MasteryChoices)
            {
                //check the roster to see how many of this already exists
                //rule is for each selected mastery on the roster it doubles in value to spam each time
                
                mastery.Points = element.ActiveMasteries.Any(p => p.Name == mastery.Name) 
                    ? element.ActiveMasteries.First(p => p.Name == mastery.Name).Points 
                    : GetNextSpammedElementPoints(mastery);

                var option = new ListViewOption()
                {
                    Category = OptionCategory.Mastery,
                    Model = mastery,
                    Text =
                        $"{mastery} - {mastery.Points} pts", 
                    CheckChanged = ListViewItem_CheckChanged,
                    OptionGrouping = "Masteries",
                    GroupCanSelectAll = false,
                    MaxAllowableSelectableForGroup = element.MaxAllowableMasteries,
                    Tooltip = mastery.Notes
                };
                Options.Add(option);
            }
        }

        private void AddRetinues(IConquestCharacter element)
        {

            //RETINUE RULES
            //Restricted - only go to max level 2 , AND they cost double
            //Must choose all tiers below the chosen.  Ex: choose tier 3, you also choose tier 2 and 1

            //todo: tac level 2 retinues give bonus items (look for Tag == "Perk"
            //100k - select additional battlefield drill officer p.231 - spires tactic High Clone Executor 252, dweghom additional relic, nords additional aspect 284

            var availableRetinues =
                element.RetinueMetaData.RetinueAvailabilities.Where(p =>
                    p.Value != RetinueAvailability.Availability.NotAvailable);

            if (!availableRetinues.Any()) return;

            foreach (var retinue in _retinues)
            {
                if (availableRetinues.Any(p => p.Key == retinue.Category) == false) continue;

                //this will either be available or restricted
                var restrictionClass = availableRetinues.First(p => p.Key == retinue.Category);

                var retinueCopy = (ITieredOption)retinue.Clone();

                if (restrictionClass.Value == RetinueAvailability.Availability.Restricted &&
                    retinueCopy.Tier > 2) continue; //restricted items cannot exceed 2nd tier

                if (restrictionClass.Value == RetinueAvailability.Availability.Restricted)
                    retinueCopy.Points *= 2; //restricted items cost double to take

                var option = new ListViewOption()
                {
                    Category = OptionCategory.Retinue,
                    Model = retinueCopy,
                    Text = $"[{retinue.Category}] {retinueCopy} - {retinueCopy.Points} pts",
                    CheckChanged = ListViewItem_CheckChanged,
                    OptionGrouping = $"{retinueCopy.Category} Retinue",
                    GroupCanSelectAll = true,
                    MaxAllowableSelectableForGroup = 0,
                    Tooltip = retinueCopy.Notes,
                    TieredSelection = true
                };
                Options.Add(option);
            }
        }

        private int GetNextSpammedElementPoints(IMastery mastery)
        {
            int elements = 0;
            if (Roster.MasterySpam.ContainsKey(mastery.Name))
            {
                elements = Roster.MasterySpam[mastery.Name].Count;
            }

            var points = mastery.BasePoints;
            for (var i = 0; i < elements; i++)
            {
                points *= 2;
            }

            return points;
        }

        private void AddHardCodedOptions()
        {
            if (Element.LeaderPoints > 0)
            {
                var optionModel = OptionFactory.CreateAdhocOption(Element.Faction, Element.Unit, "Regiment Leader",
                    "Regiment Leader", Element.LeaderPoints);
                optionModel.Notes = "Regiment Leader";

                Options.Add(CreateHardCodedOption(optionModel));
            }

            if (Element.StandardPoints > 0)
            {
                var optionModel = OptionFactory.CreateAdhocOption(Element.Faction, Element.Unit, "Regiment Standard",
                    "Regiment Standard", Element.LeaderPoints);
                optionModel.Notes = "Regiment Standard Bearer";
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

        private bool _tierRecursionActivated = false;
        private void ListViewItem_CheckChanged(object sender, bool newSelectedValue)
        {
            if (_tierRecursionActivated) return;

            var element = (ListViewOption) sender;

            //if mastery, need to adjust the roster's spam collection to keep track of how many have been selected or deselected over all
            //but only if there was something originally selected in the first place
            if (element.Category == OptionCategory.Mastery)
            {
                SynchronizeMasteries(element, newSelectedValue);
            }

            //if tiered selection is on and we are turning something on we have to make sure everything under it is turned on as well
            if (element.TieredSelection)
            {
                _tierRecursionActivated = true;

                if (newSelectedValue)
                    ActivateSubservientTiers(element);
                else
                    ValidateDeactivationSubservientTiers(element);

                _tierRecursionActivated = false;
            }

            //now if we can select everything in this group OR the value is false, we are unchecking... so we don't need to synch anything
            if (element.GroupCanSelectAll || !newSelectedValue) return;

            //uncheck other options in the same category
            //but if we are an item then this can have multiple items to a certain point so check if there is room if its an item
            if (element.Category == OptionCategory.Item)
            {
                if (MagicItemsAvailableToSelect(Element as IConquestCharacter)) return;
            }
            
            SynchronizeOptions(element);
        }

        /// <summary>
        /// Unselect everything else in the group if its checked, since only one thing can be checked
        /// </summary>
        /// <param name="element"></param>
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

        private void SynchronizeMasteries(ListViewOption element, bool activate)
        {
            //increment or decrement the spam list
            //fun caveat:  if this was opened and the item was already selected, that means the calling view model needs to adjust any of the other spammed items
            //in its collection
            int elements = 0;
            var mastery = (IMastery) element.Model;

            if (Roster.MasterySpam.ContainsKey(mastery.Name))
            {
                elements = Roster.MasterySpam[mastery.Name].Count;
            }
            else
            {
                Roster.MasterySpam.Add(mastery.Name, new List<Guid>());
            }

            if (elements == 0 && activate == false)
                throw new InvalidOperationException("Masteriers were deactivated, but element was not found in the dictionary which should not be a valid state");

            var spamGuidList = Roster.MasterySpam[mastery.Name];
            if (activate)
            {
                //only add it if it doesn't already exist
                if (spamGuidList.All(p => p != _characterID)) //i hate this way of writing it but resharper wants it, if none of this id exists then add the character
                    Roster.MasterySpam[mastery.Name].Add(_characterID);

                if (ActiveMasteryState.Any(p => p.Item1.Name == mastery.Name)) //was this in there selected initially and they unchecked and rechecked?
                {
                    //intent: only run if this item was added in the initial state!!
                    var masteryState = ActiveMasteryState.First(p => p.Item1.Name == mastery.Name);
                    ActiveMasteryState.Remove(masteryState);

                    ActiveMasteryState.Add(new Tuple<IMastery, bool>(mastery, true));
                }
            }
            else
            {
                spamGuidList.Remove(_characterID);

                if (ActiveMasteryState.Any(p => p.Item1.Name == mastery.Name)) //was this in there selected initially and they unchecked and rechecked?
                {
                    //intent: only run if this item was added in the initial state!!
                    var masteryState = ActiveMasteryState.First(p => p.Item1.Name == mastery.Name);
                    ActiveMasteryState.Remove(masteryState);

                    ActiveMasteryState.Add(new Tuple<IMastery, bool>(mastery, false));
                }
            }
        }

        private void ActivateSubservientTiers(ListViewOption element)
        {
            if (element.IsChecked == false) return;

            //if tier 3 is selected, then make sure tier 1 and 2 are selected - etc
            var item = (ITieredOption)element.Model;

            var tierGroupedItems = Options.Where(p => p.OptionGrouping == element.OptionGrouping);
            foreach (var tgi in tierGroupedItems)
            {
                var tieredItem = (ITieredOption) tgi.Model;
                if (tieredItem.Tier < item.Tier && tgi.IsChecked == false) 
                    tgi.IsChecked = true;
            }
        }

        private void ValidateDeactivationSubservientTiers(ListViewOption element)
        {
            //if tiered items 1 & 2 are selected, and i try to deactivate #1, that should not be allowed because an active checked item of a greater tier is selected
            if (element.IsChecked) return;

            var item = (ITieredOption) element.Model;
            var tierGroupedItems = Options.Where(p => p.OptionGrouping == element.OptionGrouping);
            foreach (var tgi in tierGroupedItems)
            {
                var tieredItem = (ITieredOption)tgi.Model;
                if (tieredItem.Tier <= item.Tier) continue;

                if (tgi.IsChecked)
                {
                    element.IsChecked = true;
                    return;
                }
            }
        }

        private bool MagicItemsAvailableToSelect(IConquestCharacter element)
        {
            //count how many selected magic items exist, and compare them to how many are allowed to be selected
            var numberSelectedItems = Options.Count(p => p.Category == OptionCategory.Item && p.IsChecked);
            return element.MaxAllowableItems > numberSelectedItems;
        }
    }
}
