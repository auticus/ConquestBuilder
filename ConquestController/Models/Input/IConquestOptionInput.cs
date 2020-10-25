using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models.Input
{
    public interface IConquestOptionInput : IConquestBaseInput
    {
        List<IConquestBaseInput> Options { get; }
        bool CanCalculateDefense();
        bool CanCastSpells();
    }
}
