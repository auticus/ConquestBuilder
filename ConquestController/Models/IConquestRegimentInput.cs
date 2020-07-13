using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models
{
    public interface IConquestRegimentInput: IConquestInput
    {
        public List<IConquestInput> Options { get; }
    }
}
