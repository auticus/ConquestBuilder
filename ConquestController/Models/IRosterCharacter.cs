using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public interface IRosterCharacter
    {
        IConquestCharacter Character { get; }
        ObservableCollection<IConquestGamePiece> MainstayRegiments { get; }
        ObservableCollection<IConquestGamePiece> RestrictedRegiments { get; }
    }
}
