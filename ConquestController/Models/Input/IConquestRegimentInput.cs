using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public interface IConquestRegimentInput: IConquestInput
    {
        public List<IConquestInput> Options { get; }
    }
}
