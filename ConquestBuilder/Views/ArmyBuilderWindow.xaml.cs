using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ConquestBuilder.ViewModels;
using ConquestController.Models;
using ConquestController.Models.Input;

namespace ConquestBuilder.Views
{
    /// <summary>
    /// Interaction logic for ArmyBuilderWindow.xaml
    /// </summary>
    public partial class ArmyBuilderWindow : Window, IView
    {
        private readonly ArmyBuilderViewModel _vm;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXBUTTON = 0x10000;

        public ArmyBuilderWindow(ArmyBuilderViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
            _vm.RefreshRosterTreeView += RefreshRosterTreeView;
            _vm.SendMessageToView += PostMessageToUser;
        }

        private void PostMessageToUser(object sender, string e)
        {
            MessageBox.Show(e, "Conquest Builder", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ArmyBuilderWindow_OnClosed(object sender, EventArgs e)
        {
            _vm.OnWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLongPtr(hwnd, GWL_STYLE, (int) (value & ~WS_MAXBUTTON));
        }

        private void RefreshRosterTreeView(object sender, RosterChangedEventArgs e)
        {
            //for right now we will go ahead and clear the tree out, repopulate it, and then select the item in the RosterChangedEventArgs
            //additionally make sure that they are always expanded
            tvRoster.Items.Clear();

            //clearing the roster cleared the selected character, if that has a value we need to put that back
            if (e.RosterElement != null)
            {
                _vm.SelectedRosterCharacter = e.RosterElement;
            }

            foreach (var character in _vm.Roster.RosterCharacters)
            {
                var tvItem = new TreeViewItem()
                {
                    Header = character.CharacterHeader,
                    IsSelected = (character.Character.ID == e.SelectedElementID),
                    IsExpanded = true,
                    Tag = new TreeViewRoster(){Category = RosterCategory.Character, Model = character, RosterCharacter = character}
                };
                
                var optionsNode = new TreeViewItem()
                {
                    Header = "Options",
                    IsSelected = false,
                    IsExpanded = true,
                    Tag = new TreeViewRoster() {Category = RosterCategory.OptionLabel, RosterCharacter = character}
                };

                var mainstayNode = new TreeViewItem()
                {
                    Header = "Mainstay Regiments",
                    IsSelected = false,
                    IsExpanded = true,
                    Tag = new TreeViewRoster() { Category = RosterCategory.MainstayLabel, RosterCharacter = character}
                };

                var restrictedNode = new TreeViewItem()
                {
                    Header = "Restricted Regiments",
                    IsSelected = false,
                    IsExpanded = true,
                    Tag = new TreeViewRoster() { Category = RosterCategory.RestrictedLabel, RosterCharacter = character}
                };

                foreach (var regiment in character.MainstayRegiments)
                {
                    var mainstayRegiment = new TreeViewItem()
                    {
                        Header = $"{regiment} - {regiment.TotalPoints} pts",
                        IsSelected = (regiment.ID == e.SelectedElementID),
                        IsExpanded = true,
                        Tag = new TreeViewRoster() { Category = RosterCategory.MainstayRegiment, Model = regiment, RosterCharacter = character}
                    };

                    foreach (var option in regiment.ActiveOptions)
                    {
                        var regimentOptions = new TreeViewItem()
                        {
                            Header = $"{option.Name}",
                            IsSelected = false,
                            IsExpanded = true,
                            Tag = new TreeViewRoster() {Category = RosterCategory.Option, Model = option, RosterCharacter = character}
                        };

                        mainstayRegiment.Items.Add(regimentOptions);
                    }

                    mainstayNode.Items.Add(mainstayRegiment);
                }

                foreach (var regiment in character.RestrictedRegiments)
                {
                    var restrictedRegiment = new TreeViewItem()
                    {
                        Header = $"{regiment} - {regiment.TotalPoints} pts",
                        IsSelected = (regiment.ID == e.SelectedElementID),
                        IsExpanded = true,
                        Tag = new TreeViewRoster() { Category = RosterCategory.RestrictedRegiment, Model = regiment, RosterCharacter = character}
                    };

                    foreach (var option in regiment.ActiveOptions)
                    {
                        var regimentOptions = new TreeViewItem()
                        {
                            Header = $"{option.Name}",
                            IsSelected = false,
                            IsExpanded = true,
                            Tag = new TreeViewRoster() {Category = RosterCategory.Option, Model = option, RosterCharacter = character}
                        };

                        restrictedRegiment.Items.Add(regimentOptions);
                    }

                    restrictedNode.Items.Add(restrictedRegiment);
                }

                foreach (var option in character.Character.ActiveOptions)
                {
                    var characterOption = new TreeViewItem()
                    {
                        Header = $"{option.Name}",
                        IsSelected = false,
                        IsExpanded = true,
                        Tag = new TreeViewRoster() {Category = RosterCategory.Option, Model = option, RosterCharacter = character }
                    };

                    optionsNode.Items.Add(characterOption);
                }

                tvItem.Items.Add(optionsNode);
                tvItem.Items.Add(mainstayNode);
                tvItem.Items.Add(restrictedNode);
                tvRoster.Items.Add(tvItem);
            }

            //todo: make sure the selected item is set to keep the mainstay and restricted portraits loaded when you select from the treeview
        }

        private void TreeView_RosterSelectedChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //case: treeview is cleared which will fire this event and will be null coming in
            if (!(tvRoster.SelectedItem is TreeViewItem selectedItem))
            {
                _vm.SelectedRosterCharacter = null;
                _vm.SelectedRosterUnit = null;
                return;
            }

            var rosterElement = selectedItem.Tag as TreeViewRoster;
            _vm.SelectedRosterCharacter = rosterElement.RosterCharacter;

            switch (rosterElement.Category) //potential null warning but yes if its null i want this to throw up because thats bad
            {
                case RosterCategory.Character:
                    _vm.SelectedRosterUnit = null;
                    break;
                case RosterCategory.MainstayRegiment:
                case RosterCategory.RestrictedRegiment:
                    _vm.SelectedRosterUnit = (IConquestGameElement)rosterElement.Model;
                    break;
                case RosterCategory.MainstayLabel:
                case RosterCategory.OptionLabel:
                case RosterCategory.RestrictedLabel:
                    //currently do nothing when these tree view branches are selected
                    break;
                default:
                    throw new InvalidOperationException($"Roster Element category {rosterElement.Category} is not recognized");
            }
        }
    }
}
