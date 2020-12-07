using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ConquestBuilder.UserInterfaceElements;
using ConquestBuilder.ViewModels;
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
            //clearing the roster cleared the selected character, if that has a value we need to put that back
            if (e.RosterElement != null)
            {
                _vm.SelectedRosterCharacter = e.RosterElement;
            }

            ArmyRosterTreeView.RefreshRosterTreeView(tvRoster, e.SelectedElementID, _vm.Roster.RosterCharacters);

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
                case RosterCategory.OptionLabel:
                case RosterCategory.MainstayLabel:
                case RosterCategory.RestrictedLabel:
                case RosterCategory.Option:
                    _vm.SelectedRosterUnit = null;
                    break;
                case RosterCategory.MainstayRegiment:
                case RosterCategory.RestrictedRegiment:
                    _vm.SelectedRosterUnit = (IConquestGamePiece)rosterElement.Model;
                    break;
                default:
                    throw new InvalidOperationException($"Roster Element category {rosterElement.Category} is not recognized");
            }
        }
    }
}
