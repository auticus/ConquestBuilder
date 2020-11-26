using System.Collections.ObjectModel;
using System.Linq;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestBuilder.ViewModels
{
    public class OptionViewModel : BaseViewModel
    {
        private IConquestOptionInput _element;

        public ObservableCollection<ListViewOption> Options { get; set; } //not worried about notification because the lists are initialized in the constructor
        
        public IConquestOptionInput Element
        {
            get => _element;
            set
            {
                _element = value;
                NotifyPropertyChanged("Element");
            }
        }

        public OptionViewModel(IConquestOptionInput element)
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
            //todo: set the checked state of the options based on the model passed in
        }

        private void AddOptions()
        {
            foreach (var item in Element.Options.Cast<IOption>())
            {
                var option = new ListViewOption()
                {
                    Category = OptionCategory.Option,
                    Model = item,
                    Text = $"{item.Name} - {item.Points} pts",
                    IsChecked = false,
                    CheckChanged = listViewItem_CheckChanged,
                    OptionGrouping = $"Option Category {item.Category}"
                };
                Options.Add(option);
            }
        }
        private void listViewItem_CheckChanged(object sender, bool e)
        {
            //todo: make sure only one item in the group is selected at a time
        }
    }
}
