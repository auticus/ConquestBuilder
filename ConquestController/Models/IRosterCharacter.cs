using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public interface IRosterCharacter
    {
        IConquestGameElement Character { get; }
        ObservableCollection<IConquestGameElement> MainstayRegiments { get; }
        ObservableCollection<IConquestGameElement> RestrictedRegiments { get; }
    }
}
