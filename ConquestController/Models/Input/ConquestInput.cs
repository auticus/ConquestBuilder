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

        public int ResistDecay { get; set; }

        public int AlwaysInspire { get; set; }
        public string Image { get; set; }

        /// <summary>
        /// When set to 1 indicates do not show this in the army builder, it is just there to display for Analysis purposes
        /// </summary>
        public int AnalysisOnly { get; set; }

        public List<IConquestInput> Options { get; }

        public abstract bool CanCalculateDefense();
        public abstract bool CanCastSpells();
        public abstract T Copy();
    }
}
