using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ConquestController.Analysis.Components;
using ConquestController.Data;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestBuilder.ViewModels
{
    public class OptionViewModel : BaseViewModel
    {
        //todo need to create a new Perk data collection of all options that have a perk string, and then those need fed into here and added/removed as retinue is selected
        private IConquestGamePiece _element;
        private readonly IEnumerable<IBaseOption> _magicItems;
        private readonly IEnumerable<ITieredBaseOption> _retinues;
        private readonly IEnumerable<IPerkOption> _perks;
        private readonly Guid _characterID;

        /// <summary>
        /// string is the name of the tag from the mastery model, the value is the retinue its tied to that needs to be set on to choose the mastery
        /// </summary>
        private Dictionary<string, ITieredBaseOption> _retinueRestrictionDictionary;

        /// <summary>
        /// Lets us know all of the masteries that could be filtered out based on a retinue selection
        /// </summary>
        private readonly Dictionary<IMastery, ListViewOption> _retinueRestrictedMasteries;

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
        public OptionViewModel(IConquestGamePiece element, IEnumerable<IBaseOption> magicItems, IEnumerable<ITieredBaseOption> retinues, IEnumerable<IPerkOption> perks, Guid characterID)
        {
            Element = element;
            _magicItems = magicItems;
            _retinues = retinues;
            _characterID = characterID;
            _perks = perks;

            ActiveMasteryState = new List<Tuple<IMastery,bool>>();
            _retinueRestrictionDictionary = new Dictionary<string, ITieredBaseOption>();
            _retinueRestrictedMasteries = new Dictionary<IMastery, ListViewOption>();
            InitializeData();
        }

        private void InitializeData()
        {
            Options = new ObservableCollection<ListViewOption>();
            _retinueRestrictionDictionary = Retinue.GetRetinueRestrictionDictionary(_retinues);
            AddOptions();

            if (Element is IConquestCharacter element)
            {
                AddMagicItems(element);
                AddRetinues(element);
                AddMasteries(element);
                AddPerks(element);
                AddSpells(element);
            }
            
            SetInitialCheckedState();
        }

        private void SetInitialCheckedState()
        {
            foreach (var option in Element.ActiveOptions)
            {
                foreach (var lvo in Options.Where(p=>p.Category == OptionCategory.Option))
                {
                    var lvOption = (IBaseOption) lvo.Model;
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
            var lists = new List<Tuple<IEnumerable<IBaseOption>, OptionCategory>>()
            {
                new Tuple<IEnumerable<IBaseOption>, OptionCategory>(element.ActiveItems, OptionCategory.Item),
                new Tuple<IEnumerable<IBaseOption>, OptionCategory>(element.ActiveMasteries, OptionCategory.Mastery),
                new Tuple<IEnumerable<IBaseOption>, OptionCategory>(element.ActiveRetinues, OptionCategory.Retinue),
                new Tuple<IEnumerable<IBaseOption>, OptionCategory>(element.ActivePerks, OptionCategory.Perk),
                new Tuple<IEnumerable<IBaseOption>, OptionCategory>(element.ActiveSpells, OptionCategory.Spell)
            };

            foreach (var list in lists)
            {
                CheckActiveOptions(list.Item1, list.Item2);
            }
        }

        private void CheckActiveOptions(IEnumerable<IBaseOption> list, OptionCategory category)
        {
            foreach (var component in list)
            {
                foreach (var lvo in Options.Where(p => p.Category == category))
                {
                    var lvOption = (IBaseOption) lvo.Model;
                    if (component.Name == lvOption.Name)
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
            foreach (var item in Element.Options.Cast<IPerkOption>())
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

                AddMastery(mastery, element.MaxAllowableMasteries);
                Console.WriteLine($"Added Mastery {mastery}");
            }

            FilterMasteriesByActiveRetinueChoice(element);
        }

        private void AddMastery(IMastery mastery, int maxAllowable)
        {
            var option = new ListViewOption()
            {
                Category = OptionCategory.Mastery,
                Model = mastery,
                Text = $"{mastery} - {mastery.Points} pts",
                CheckChanged = ListViewItem_CheckChanged,
                OptionGrouping = "Masteries",
                GroupCanSelectAll = false,
                MaxAllowableSelectableForGroup = maxAllowable,
                Tooltip = mastery.Notes
            };
            Options.Add(option);
        }
        
        /// <summary>
        /// Assuming ALL masteries are available, removes the ones that need filtered out by retinue
        /// </summary>
        /// <param name="element"></param>
        private void FilterMasteriesByActiveRetinueChoice(IConquestCharacter element)
        {
            var removeOptions = new List<ListViewOption>();

            foreach (var lvo in Options.Where(p => p.Category == OptionCategory.Mastery))
            {
                var mastery = (IMastery)lvo.Model;
                if (MasteryNeedsFilteredOut(mastery))
                {
                    _retinueRestrictedMasteries.Add(mastery, lvo);
                    removeOptions.Add(lvo);
                }
            }

            foreach (var option in removeOptions)
            {
                Options.Remove(option);
            }
        }

        /// <summary>
        /// Returns TRUE if the mastery should be removed from the Options collection
        /// </summary>
        /// <param name="mastery"></param>
        /// <returns></returns>
        private bool MasteryNeedsFilteredOut(IMastery mastery)
        {
            //default mastery removal to false
            //a mastery is flagged for removal if the following conditions are met
            // 1) it contains a retinue filter tag
            // 2) said retinue that is attached to #1 is not selected
            var character = (IConquestCharacter) Element;
            foreach (var filterTag in _retinueRestrictionDictionary.Keys)
            {
                if (mastery.Restrictions.Split("|").Any(p => p == filterTag) && !RetinueTagIsActive(filterTag))
                {
                    var listViewOption = Options.FirstOrDefault(p => _retinueRestrictionDictionary[filterTag].Equals(p.Model));
                    if (listViewOption == null || !listViewOption.IsChecked) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Given a filterTag, check the ActiveRetinues to see if one is currently selected(active) or not
        /// </summary>
        /// <param name="filterTag"></param>
        /// <returns></returns>
        private bool RetinueTagIsActive(string filterTag)
        {
            var character = (IConquestCharacter) Element;
            var retinue = _retinueRestrictionDictionary[filterTag];
            return character.ActiveRetinues.Any(p => p.Category == retinue.Category && p.Tier == retinue.Tier);
        }

        private void AddRetinues(IConquestCharacter element)
        {

            //RETINUE RULES
            //Restricted - only go to max level 2 , AND they cost double
            //Must choose all tiers below the chosen.  Ex: choose tier 3, you also choose tier 2 and 1

            var availableRetinues =
                element.RetinueMetaData.RetinueAvailabilities.Where(p =>
                    p.Value != RetinueAvailability.Availability.NotAvailable);

            if (!availableRetinues.Any()) return;

            foreach (var retinue in _retinues)
            {
                if (availableRetinues.Any(p => p.Key == retinue.Category) == false) continue;

                //this will either be available or restricted
                var restrictionClass = availableRetinues.First(p => p.Key == retinue.Category);

                var retinueCopy = (ITieredBaseOption)retinue.Clone();

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

        /// <summary>
        /// Checks to see if any active retinues enabled any perks and if so, adds the perks to the option list based on the faction of the Element
        /// </summary>
        /// <param name="element"></param>
        private void AddPerks(IConquestCharacter element)
        {
            //for right now there is only one retinue that can unlock perks and its just a static set of perks
            //it allows for some factions to add a set of options to choose from.  These will be marked in the option data as the name of the retinue in the Perk property (or if none, it stays blank)
            var perkRetinueActive = Options.Where(p => p.Category == OptionCategory.Retinue && p.IsChecked)
                .Select(option => (ITieredBaseOption) option.Model)
                .Any(opt => opt.Tag.Split("|").Contains("Perk"));

            if (!perkRetinueActive) return;

            if (Options.Any(p => p.Category == OptionCategory.Perk)) return;  //we already added perks we aren't doing it again

            foreach (var perk in _perks.Where(p => p.Faction == element.Faction))
            {
                var perkCopy = (IPerkOption)perk.Clone();

                var option = new ListViewOption()
                {
                    Category = OptionCategory.Perk,
                    Model = perkCopy,
                    Text = $"{perk} - {perk.Points} pts",
                    CheckChanged = ListViewItem_CheckChanged,
                    OptionGrouping = $"Perks",
                    GroupCanSelectAll = false,
                    MaxAllowableSelectableForGroup = 0,
                    Tooltip = perkCopy.Notes,
                    TieredSelection = false
                };
                Options.Add(option);
            }
        }

        /// <summary>
        /// Clears all Perks and actively selected perks from the element
        /// </summary>
        /// <param name="element"></param>
        private void RemovePerks(IConquestCharacter element)
        {
            element.ActivePerks.Clear();

            var clearList = new List<ListViewOption>();

            foreach (var option in Options.Where(p => p.Category == OptionCategory.Perk))
            {
                clearList.Add(option);
            }

            foreach(var option in clearList)
                Options.Remove(option);
        }

        private void AddSpells(IConquestCharacter element)
        {
            if (!(element is IConquestSpellcaster caster)) return;
            
            foreach (var spell in caster.Spells)
            {
                var spellCopy = (IBaseOption)spell.Clone();

                var option = new ListViewOption()
                {
                    Category = OptionCategory.Spell,
                    Model = spellCopy,
                    Text = $"[{spellCopy.Category}] {spellCopy.Name} - {spellCopy.Points} pts",
                    CheckChanged = ListViewItem_CheckChanged,
                    OptionGrouping = $"Spells",
                    GroupCanSelectAll = true,
                    MaxAllowableSelectableForGroup = 0,
                    Tooltip = spellCopy.Notes,
                    TieredSelection = false
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

        private ListViewOption CreateHardCodedOption(BaseOption model)
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
            //the first thing - if we are activating a retinue this could add other masteries to the list
            //this comes even if we are in recursion mode because recursion mode means we are adding a series of tiered options together and that could 
            //in fact trigger this to add a mastery or whatever is dependent
            var element = (ListViewOption)sender;
            HandleRetinueChange(element, newSelectedValue);

            //at this point if recursion is active, do not process any further as a chain of items is being selected/deselected and we dont want each one to be processed here
            if (_tierRecursionActivated) return;

            //if mastery, need to adjust the roster's spam collection to keep track of how many have been selected or deselected over all
            //but only if there was something originally selected in the first place
            SynchronizeMasteries(element, newSelectedValue);
            
            //if tiered selection is on and we are turning something on we have to make sure everything under it is turned on as well
            HandleTieredOptionSelection(element, newSelectedValue);

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
        /// Handles when a retinue changes by adding or removing associated mastery selections from the Options list
        /// </summary>
        /// <param name="element"></param>
        /// <param name="selected"></param>
        private void HandleRetinueChange(ListViewOption element, bool selected)
        {
            if (element.Category != OptionCategory.Retinue) return;
            
            var retinue = (ITieredBaseOption) element.Model;
            var tag = _retinueRestrictionDictionary.FirstOrDefault(p => p.Value.Tier == retinue.Tier && p.Value.Category == retinue.Category ).Key;
            if (tag == null) return;

            if (selected)
            {
                var masteries = _retinueRestrictedMasteries.Where(p => p.Key.Restrictions.Split("|").Contains(tag));
                foreach (var mastery in masteries)
                {
                    Options.Add(mastery.Value);
                }

                if (retinue.Tag.Split("|").Contains("Perk"))
                {
                    AddPerks(Element as IConquestCharacter);
                }
            }
            else
            {
                foreach (var lvo in Options.Where(p => p.Category == OptionCategory.Mastery))
                {
                    var mastery = (IMastery) lvo.Model;
                    if (mastery.Restrictions.Split("|").Contains(tag))
                    {
                        Options.Remove(lvo);
                        DeactivateMastery(mastery);
                        break;
                    }
                }

                if (retinue.Tag.Split("|").Contains("Perk"))
                {
                    RemovePerks(Element as IConquestCharacter);
                }
            }
        }

        private void HandleTieredOptionSelection(ListViewOption element, bool selected)
        {
            if (element.TieredSelection == false) return;
            
            _tierRecursionActivated = true;

            if (selected)
                ActivateSubservientTiers(element);
            else
                ValidateDeactivationSubservientTiers(element);

            _tierRecursionActivated = false;
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
            if (element.Category != OptionCategory.Mastery) return;

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

            if (activate)
            {
                ActivateMastery(mastery);
            }
            else
            {
                DeactivateMastery(mastery);
            }
        }

        /// <summary>
        /// Activates a mastery and adds it to the spam list
        /// </summary>
        /// <param name="mastery"></param>
        private void ActivateMastery(IMastery mastery)
        {
            var spamGuidList = Roster.MasterySpam[mastery.Name];

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

        /// <summary>
        /// Deactivates a mastery and removes it from the spam list if needed
        /// </summary>
        /// <param name="mastery"></param>
        private void DeactivateMastery(IMastery mastery)
        {
            if (Roster.MasterySpam.ContainsKey(mastery.Name) == false) return;
            
            var spamGuidList = Roster.MasterySpam[mastery.Name];
            spamGuidList.Remove(_characterID);

            if (ActiveMasteryState.Any(p => p.Item1.Name == mastery.Name)) //was this in there selected initially and they unchecked and rechecked?
            {
                //intent: only run if this item was added in the initial state!!
                var masteryState = ActiveMasteryState.First(p => p.Item1.Name == mastery.Name);
                ActiveMasteryState.Remove(masteryState);

                ActiveMasteryState.Add(new Tuple<IMastery, bool>(mastery, false));
            }
        }

        private void ActivateSubservientTiers(ListViewOption element)
        {
            if (element.IsChecked == false) return;

            //if tier 3 is selected, then make sure tier 1 and 2 are selected - etc
            var item = (ITieredBaseOption)element.Model;

            var tierGroupedItems = Options.Where(p => p.OptionGrouping == element.OptionGrouping);
            foreach (var tgi in tierGroupedItems)
            {
                var tieredItem = (ITieredBaseOption) tgi.Model;
                if (tieredItem.Tier < item.Tier && tgi.IsChecked == false) 
                    tgi.IsChecked = true;
            }
        }

        private void ValidateDeactivationSubservientTiers(ListViewOption element)
        {
            //if tiered items 1 & 2 are selected, and i try to deactivate #1, that should not be allowed because an active checked item of a greater tier is selected
            if (element.IsChecked) return;

            var item = (ITieredBaseOption) element.Model;
            var tierGroupedItems = Options.Where(p => p.OptionGrouping == element.OptionGrouping);
            foreach (var tgi in tierGroupedItems)
            {
                var tieredItem = (ITieredBaseOption)tgi.Model;
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
