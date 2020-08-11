using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models.Input
{
    public interface IConquestOptionInput : IConquestInput
    {
        public List<IConquestInput> Options { get; }
    }
}
