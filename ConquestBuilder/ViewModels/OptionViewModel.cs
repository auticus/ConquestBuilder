using System.Collections.ObjectModel;
using System.Linq;
using ConquestController.Data;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestBuilder.ViewModels
{
    public class OptionViewModel : BaseViewModel
    {
        private IConquestGameElementOption _element;

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

        public OptionViewModel(IConquestGameElementOption element)
        {
            Element = element;
            InitializeData();
        }

        private void InitializeData()
        {
            Options = new ObservableCollection<ListViewOption>();
            AddOptions();
            SetCheckedState();
        }

        private void SetCheckedState()
        {
            foreach (var option in Element.ActiveOptions)
            {
                foreach (var lvo in Options)
                {
                    var lvOption = (IOption) lvo.Model;
                    if (option.Name == lvOption.Name)
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
                    GroupCanMultiSelect = item.Category == "0"
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
                GroupCanMultiSelect = true
            };
        }

        private void ListViewItem_CheckChanged(object sender, bool e)
        {
            var element = (ListViewOption) sender;
            if (element.GroupCanMultiSelect || !e) return;

            foreach (var option in Options.Where(p => p.OptionGrouping == element.OptionGrouping && p.Text != element.Text && p.IsChecked))
            {
                option.IsChecked = false;
            }
        }
    }
}
