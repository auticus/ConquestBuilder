using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConquestController.Models.Input
{
    /// <summary>
    /// interface that houses all of what a Character would have
    /// </summary>
    public interface IConquestCharacter : IConquestGamePiece
    {
        string Mainstay { get; set; }
        string Restricted { get; set; }
        IEnumerable<string> MainstayChoices { get; set; }
        IEnumerable<string> RestrictedChoices { get; set; }

        int MaxAllowableItems { get; set; }
        ObservableCollection<IBaseOption> ActiveItems { get; }

        int MaxAllowableMasteries { get; set; }
        
        /// <summary>
        /// Data Input Field
        /// </summary>
        string Masteries { get; set; }

        /// <summary>
        /// The masteries available to the character
        /// </summary>
        ObservableCollection<IMastery> MasteryChoices { get; set; }
        ObservableCollection<IMastery> ActiveMasteries { get; }
        ObservableCollection<ITieredBaseOption> ActiveRetinues { get; }
        ObservableCollection<IPerkOption> ActivePerks { get; }
        ObservableCollection<IBaseOption> ActiveSpells { get; }

        /// <summary>
        /// Data input field
        /// </summary>
        string Retinue { get; set; }
        RetinueAvailability RetinueMetaData { get; set; }
    }
}
