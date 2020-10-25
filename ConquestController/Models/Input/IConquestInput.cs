using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public interface IConquestInput : IConquestBaseInput
    {
        public string Weight { get; set; }
        public string ModelType { get; set; }
        public int Models { get; set; }
        public int Points { get; set; }
        public int AdditionalPoints { get; set; }
        public int LeaderPoints { get; set; }
        public int StandardPoints { get; set; }
        public int Move { get; set; }
        public int Volley { get; set; }
        public int Clash { get; set; }
        public int Attacks { get; set; }
        public int Wounds { get; set; }
        public int Resolve { get; set; }
        public int Defense { get; set; }
        public int Evasion { get; set; }

        public string SpecialRules { get; set; }
    }
}
