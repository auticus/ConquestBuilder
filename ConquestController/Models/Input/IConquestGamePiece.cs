using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace ConquestController.Models.Input
{
    /// <summary>
    /// Interface that exposes all of the options available to any game piece (be it regiment or character)
    /// </summary>
    public interface IConquestGamePiece : IConquestBaseGameElement
    {
        string Image { get; set; }
        List<IConquestBase> Options { get; }
        ObservableCollection<IBaseOption> ActiveOptions { get; }

        int AdditionalPoints { get; set; }
        int LeaderPoints { get; set; }
        int StandardPoints { get; set; }
        int IsReleased { get; set; }
        int IsFluid { get; set; }
        int IsFly { get; set; }
        int Barrage { get; set; }
        int ArmorPiercing { get; set; }
        int IsDeadlyShot { get; set; }
        int Range { get; set; }
        int BrutalImpact { get; set; }
        int Cleave { get; set; }

        bool Reroll6_Volley { get; set; }
        bool Reroll6_Defense { get; set; }
        
        /// <summary>
        /// From options - means that never take an obscure penalty
        /// </summary>
        bool NoObscure { get; set; } 
        int IsBlessed { get; set; }
        int IsArcOfFire { get; set; }
        int IsSupport { get; set; }
        int IsFury { get; set; }
        int IsImpact { get; set; }
        int IsFlurry { get; set; }
        int IsAuraDeath { get; set; }
        int IsBastion { get; set; }
        int IsShields { get; set; }
        int AlwaysInspire { get; set; }
        int IsTerrifying { get; set; }
        int IsDeadlyBlades { get; set; }
        int Healing { get; set; }
        int Decay { get; set; }
        int IsFearless { get; set; }
        int IsFearsome { get; set; }
        int IsTorrential { get; set; }
        int IsRegen { get; set; }
        int IsPrecise { get; set; }
        bool DoubleAttack { get; set; }
        bool BuffDefenseOrEvasion { get; set; }
        bool D_Volley { get; set; }
        bool MeleeHeal4 { get; set; }
        bool OneHitPerFile { get; set;  }
        bool IsTorrential_Clash { get; set; }
        bool KissFarewell { get; set; }
        bool Reroll_ImpactHits { get; set; }
        int IsSmite { get; set; }
        /// <summary>
        /// Similar to NoObscure - this means that no obscure from RANGE can be applied
        /// </summary>
        int NoRangeObscure { get; set; }
        bool CanCastSpells();
        bool CanCalculateDefense();
    }

}
