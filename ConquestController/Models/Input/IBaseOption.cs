using System;

namespace ConquestController.Models.Input
{
    /// <summary>
    /// Base interface for all options in the game, from Option to Mastery to Retinue etc...
    /// </summary>
    public interface IBaseOption : ICloneable
    {
        string Faction { get; set; }
        string Name { get; set; }
        string Tag { get; set; }
        int Points { get; set; }
        int SelfOnly { get; set; }
        int WarlordOnly { get; set; }
        string Notes { get; set; }
        string Category { get; set; }
    }
}
