using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models.Input
{
    public interface IConquestOptionInput : IConquestInput
    {
        List<IConquestInput> Options { get; }
        bool CanCalculateDefense();
        bool CanCastSpells();
    }
}
