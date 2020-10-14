using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models.Input
{
    public abstract class ConquestInput<T> : IConquestOptionInput
    {
        protected ConquestInput()
        {
            Options = new List<IConquestInput>();
        }
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Weight { get; set; }
        public string ModelType { get; set; }

        /// <summary>
        /// Total model count of the regiment
        /// </summary>
        public int Models { get; set; }
        public int Points { get; set; }
        public int AdditionalPoints { get; set; }
        public int LeaderPoints { get; set; }
        public int StandardPoints { get; set; }
        public int IsReleased { get; set; }

        public string Intangibles { get; set; }

        //***** STATS **//
        public int Move { get; set; }
        public int Volley { get; set; }
        public int Clash { get; set; }
        public int Attacks { get; set; }
        public int Wounds { get; set; }
        public int Resolve { get; set; }
        public int Defense { get; set; }
        public int Evasion { get; set; }

        //***** RULES **//
        public int Barrage { get; set; }
        public int Range { get; set; }
        public int ArmorPiercing { get; set; }
        public int Cleave { get; set; }
        public int IsImpact { get; set; }
        public int BrutalImpact { get; set; }
        public int IsShields { get; set; }
        public int IsFury { get; set; }
        public int IsFlurry { get; set; }
        public int IsFluid { get; set; }
        public int IsFly { get; set; }
        public int IsDeadlyShot { get; set; }
        public int IsDeadlyBlades { get; set; }
        public int IsAuraDeath { get; set; }
        public int IsSupport { get; set; }
        public int IsBastion { get; set; }
        public int IsTerrifying { get; set; }
        public int IsArcOfFire { get; set; }
        public int IsFearless { get; set; }
        public int IsBlessed { get; set; } //if true, re-roll half offense misses and re-roll half defense fails. If IsFlurry, the offense part is useless
        public int IsFearsome { get; set; } //this is an intangible and currently not on the spreadsheet

        public int ResistDecay { get; set; }

        public int AlwaysInspire { get; set; }
        public string Image { get; set; }
        public string Notes { get; set; }

        //**Rules that get boosted by options but not on input file **//
        public bool BuffDefenseOrEvasion { get; set; } //if true then will boost defense if cleave isn't that big a deal otherwise boosts evasion
        public int Decay { get; set; } //apply decay damage in the defense calculation to lower defense output
        
        /// <summary>
        /// Gain +1D vs volley - situational - take the +1 D score and compare it to the normal D score
        /// </summary>
        public bool D_Volley { get; set; } 
        public bool DoubleAttack { get; set; } //target regiment can make two attacks of the same type (so double volley or double clash)
        public int Healing { get; set; } //if a number exists, can heal fully that number of wounds every turn
        public bool MeleeHeal4 { get; set; } //every impact or clash wound it does it gains a wound back on a 4+
        public bool NoObscure { get; set; } //if true do not calculate obscure penalty in ranged offense calculation
        public bool OneHitPerFile { get; set; } // will do an extra hit per file (so for analysis purposes, +3, for detailed analysis it will need to observe the files)
        public bool Reroll6_Volley { get; set; } //reroll 6s on volley
        public bool Reroll6_Defense { get; set; } //reroll 6s on defense
        public bool Reroll_ImpactHits { get; set; }
        public bool IsTorrential { get; set; } //gain the Torrential special rule
        public bool IsTorrential_Clash { get; set; } //gains torrential only with clash
        public bool KissFarewell { get; set; } //the item. Gives Barrage 3 24" Deadly Shot.  If already have Barrage, adds +3 Barrage value and Deadly Shot

        /// <summary>
        /// When set to 1 indicates do not show this in the army builder, it is just there to display for Analysis purposes
        /// </summary>
        public int AnalysisOnly { get; set; }

        /// <summary>
        /// Character options / upgrade or regiment options / upgrades like Veterans, Armsmaster, etc.
        /// </summary>
        public List<IConquestInput> Options { get; }

        public abstract bool CanCalculateDefense();
        public abstract bool CanCastSpells();
        public abstract T Copy();
    }
}
