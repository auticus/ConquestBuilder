using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestBuilder.ViewModels
{
    public enum RosterCategory
    {
        Character = 0,
        MainstayRegiment,
        RestrictedRegiment,
        OptionLabel,
        MainstayLabel,
        RestrictedLabel
    }

    public class TreeViewRoster
    {
        public RosterCategory Category { get; set; }
        public object Model { get; set; }
    }
}
