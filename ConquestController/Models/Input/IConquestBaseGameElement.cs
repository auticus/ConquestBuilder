using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConquestController.Models.Input
{
    public interface IConquestBaseGameElement : IConquestBase, ICloneable
    {
        EventHandler PointsChanged { get; set; }
        Guid ID { get; set; }
        string UserName { get; set; } //user name of the regiment
        string Weight { get; set; }
        string ModelType { get; set; }
        int Models { get; set; }
        int Points { get; set; }
        int Move { get; set; }
        int Volley { get; set; }
        int Clash { get; set; }
        int Attacks { get; set; }
        int Wounds { get; set; }
        int Resolve { get; set; }
        int Defense { get; set; }
        int Evasion { get; set; }

        int TotalPoints { get; }
        int StandCount { get; set; }

        string SpecialRules { get; set; }
    }
}
