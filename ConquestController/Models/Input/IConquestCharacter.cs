using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ConquestController.Models.Input
{
    /// <summary>
    /// interface that houses all of what a Character would have
    /// </summary>
    public interface IConquestCharacter : IConquestGamePiece
    {
        IEnumerable<string> MainstayChoices { get; set; }
        IEnumerable<string> RestrictedChoices { get; set; }

        int MaxAllowableItems { get; set; }
        ObservableCollection<IOption> ActiveItems { get; }

        int MaxAllowableMasteries { get; set; }
        /// <summary>
        /// The masteries available to the character
        /// </summary>
        ObservableCollection<IMastery> MasteryChoices { get; set; }
        ObservableCollection<IMastery> ActiveMasteries { get; }
        RetinueAvailability RetinueChoices { get; set; }
    }
}
