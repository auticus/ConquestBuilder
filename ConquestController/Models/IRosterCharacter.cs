using System.Collections.Generic;
using ConquestController.Models.Input;

namespace ConquestController.Models
{
    public interface IRosterCharacter
    {
        IConquestInput Character { get; }
        List<IConquestInput> MainstayRegiments { get; }
        List<IConquestInput> RestrictedRegiments { get; }
    }
}
