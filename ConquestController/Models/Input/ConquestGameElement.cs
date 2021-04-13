using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace ConquestController.Models.Input
{
    public abstract class ConquestGameElement : IConquestGamePiece
    {
        protected ConquestGameElement()
        {
            Options = new List<IConquestBase>();
            ActiveOptions = new ObservableCollection<IBaseOption>();
            ActiveItems = new ObservableCollection<IBaseOption>();
            ActiveMasteries = new ObservableCollection<IMastery>();
            ActiveRetinues = new ObservableCollection<ITieredBaseOption>();
            ActivePerks = new ObservableCollection<IPerkOption>();
            ActiveSpells = new ObservableCollection<IBaseOption>();
            ID = Guid.NewGuid();

            ActiveOptions.CollectionChanged += (sender, args) =>
            {
                PointsChanged?.Invoke(this, EventArgs.Empty);
            };

            ActiveItems.CollectionChanged += (sender, args) =>
            {
                PointsChanged?.Invoke(this, EventArgs.Empty);
            };

            ActiveMasteries.CollectionChanged += (sender, args) =>
            {
                PointsChanged?.Invoke(this, EventArgs.Empty);
            };

            ActiveRetinues.CollectionChanged += (sender, EventArgs) =>
            {
                PointsChanged?.Invoke(this, EventArgs);
            };

            ActivePerks.CollectionChanged += (sender, EventArgs) =>
            {
                PointsChanged?.Invoke(this, EventArgs);
            };

            ActiveSpells.CollectionChanged += (sender, EventArgs) =>
            {
                PointsChanged?.Invoke(this, EventArgs);
            };
        }
        public EventHandler PointsChanged { get; set; }

        public Guid ID { get; set; }
        public string Faction { get; set; }

        /// <summary>
        /// The stock rule name for this element 
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// The user defined name for this element
        /// </summary>
        public string UserName { get; set; }
        public string Weight { get; set; }
        public virtual string ModelType { get; set; }

        /// <summary>
        /// Total model count of the regiment
        /// </summary>
        public int Models { get; set; }
        public int Points { get; set; }
        public int AdditionalPoints { get; set; }
        public int StandCount { get; set; }
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
        public int IsRelentless { get; set; }
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

        public string SpecialRules { get; set; } //used for display special rules in the builder

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
        public int IsTorrential { get; set; } //gain the Torrential special rule
        public int IsRegen { get; set; } //gain regeneration
        public int IsPrecise { get; set; } //gain Precise Shot special rule
        public bool IsTorrential_Clash { get; set; } //gains torrential only with clash
        public bool KissFarewell { get; set; } //the item. Gives Barrage 3 24" Deadly Shot.  If already have Barrage, adds +3 Barrage value and Deadly Shot
        public int NoRangeObscure { get; set; } //do not take the range obscure penalty
        public int IsSmite { get; set; } //melee gets smite attacks
        /// <summary>
        /// When set to 1 indicates do not show this in the army builder, it is just there to display for Analysis purposes
        /// </summary>
        public int AnalysisOnly { get; set; }

        [JsonIgnore]
        public int TotalPoints =>
            //take all of the total
            Points + ActiveOptions.Sum(p => p.Points)
                   + ActiveItems.Sum(p => p.Points)
                   + ActiveMasteries.Sum(p => p.Points)
                   + ActiveRetinues.Sum(p => p.Points)
                   + ActivePerks.Sum(p => p.Points)
                   + ActiveSpells.Sum(p => p.Points);

        /// <summary>
        /// Character options / upgrade or regiment options / upgrades like Veterans, Armsmaster, etc.
        /// </summary>
        public List<IConquestBase> Options { get; }

        /// <summary>
        /// For a roster element, the actively selected options
        /// </summary>
        public ObservableCollection<IBaseOption> ActiveOptions { get; set; }

        public ObservableCollection<IBaseOption> ActiveItems { get; set; }
        public ObservableCollection<IMastery> ActiveMasteries { get; set; }

        public ObservableCollection<ITieredBaseOption> ActiveRetinues { get; set; }
        public ObservableCollection<IPerkOption> ActivePerks { get; set; }
        public ObservableCollection<IBaseOption> ActiveSpells { get; set; }

        public int MaxAllowableItems { get; set; }
        public int MaxAllowableMasteries { get; set; }

        public abstract bool CanCalculateDefense();
        public abstract bool CanCastSpells();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>An IConquestGameElement</returns>
        public abstract object Clone();

        public override string ToString()
        {
            if (UserName.Trim() != Unit)
            {
                return $"{UserName} - [{Unit}]";
            }

            return Unit;
        }
    }
}
