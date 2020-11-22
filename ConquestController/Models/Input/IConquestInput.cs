using System;

namespace ConquestController.Models.Input
{
    public interface IConquestInput : IConquestBaseInput
    {
        Guid ID { get; set; }
        string Weight { get; set; }
        string ModelType { get; set; }
        int Models { get; set; }
        int Points { get; set; }
        int AdditionalPoints { get; set; }
        int LeaderPoints { get; set; }
        int StandardPoints { get; set; }
        int Move { get; set; }
        int Volley { get; set; }
        int Clash { get; set; }
        int Attacks { get; set; }
        int Wounds { get; set; }
        int Resolve { get; set; }
        int Defense { get; set; }
        int Evasion { get; set; }

        int TotalPoints { get; }

        string SpecialRules { get; set; }

        IConquestInput Copy();
    }
}
