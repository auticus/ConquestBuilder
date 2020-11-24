using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public interface IRosterCharacter
    {
        IConquestInput Character { get; }
        ObservableCollection<IConquestInput> MainstayRegiments { get; }
        ObservableCollection<IConquestInput> RestrictedRegiments { get; }
    }
}
