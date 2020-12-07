using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConquestController.Models.Input
{
    /// <summary>
    /// Interface that exposes all of the options available to any game piece (be it regiment or character)
    /// </summary>
    public interface IConquestGamePiece : IConquestBaseGameElement
    {
        List<IConquestBase> Options { get; }
        ObservableCollection<IOption> ActiveOptions { get; }

        int AdditionalPoints { get; set; }
        int LeaderPoints { get; set; }
        int StandardPoints { get; set; }
    }

}
