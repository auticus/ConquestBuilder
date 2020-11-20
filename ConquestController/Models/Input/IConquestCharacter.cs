using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public interface IConquestCharacter
    {
        IEnumerable<string> MainstayChoices { get; set; }
        IEnumerable<string> RestrictedChoices { get; set; }
    }
}
