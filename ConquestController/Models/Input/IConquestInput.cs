using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public interface IConquestInput
    {
        public string Faction { get; set; }
        public string Unit { get; set; }
    }
}
