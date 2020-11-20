using System;
using ConquestController.Models;

namespace ConquestBuilder.ViewModels
{
    public class RosterChangedEventArgs : EventArgs
    {
        public RosterCharacter RosterElement { get; set; }
        public Guid SelectedElementID { get; set; }
    }
}
