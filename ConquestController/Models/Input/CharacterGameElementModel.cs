using System.Collections.Generic;
using System.Collections.ObjectModel;
using ConquestController.Extensions;
using Newtonsoft.Json;

namespace ConquestController.Models.Input
{
    public class CharacterGameElementModel : ConquestGameElement, IConquestSpellcaster, IConquestCharacter
    {
        public int IsQuickSilverStrike { get; set; }
        public int ItemCount { get; set; }

        public string Mainstay { get; set; } //mapped to MainstayChoices
        public string Restricted { get; set; } // mapped to RestrictedChoices
        public string Retinue { get; set; } //mapped to RetinueChoices
        public string Masteries { get; set; } //mapped to MasteryChoices

        public int NoWarlord { get; set; }
        public string SupremacyTitle { get; set; }
        public string SupremacyNotes { get; set; }
        public string SpellSchools { get; set; } //mapped to Schools, comes from input
        public int MaxSpells { get; set; } //how many they get to pick from a school, most of the time its unlimited but biomancy restricts to just 1 (currently just not going to care)
        
        /// <summary>
        /// The list of units that can be chosen by this character as a mainstay
        /// </summary>
        public IEnumerable<string> MainstayChoices { get; set; }

        /// <summary>
        /// The list of units that can be chosen by this character as restricted
        /// </summary>
        public IEnumerable<string> RestrictedChoices { get; set; }

        /// <summary>
        /// Contains a string list of Retinue categories and if their level of access
        /// </summary>
        public RetinueAvailability RetinueMetaData { get; set; }

        public ObservableCollection<IMastery> MasteryChoices { get; set; }
        public List<string> Schools { get; set; }
        public List<ISpell> Spells { get; set; }
        public int WizardLevel { get; set; }

        public CharacterGameElementModel()
        {
            MainstayChoices = new List<string>();
            RestrictedChoices = new List<string>();
            Schools = new List<string>();
            Spells = new List<ISpell>();
            MasteryChoices = new ObservableCollection<IMastery>();
            ActiveRetinues = new ObservableCollection<ITieredBaseOption>();
            ActivePerks = new ObservableCollection<IPerkOption>();
            ActiveSpells = new ObservableCollection<IBaseOption>();
            RetinueMetaData = new RetinueAvailability();

            MaxAllowableItems = 1;
            MaxAllowableMasteries = 1;
            StandCount = 1;
        }

        public override bool CanCalculateDefense() => false;
        public override bool CanCastSpells() => WizardLevel > 0;  

        public override string ToString()
        {
            return Unit;
        }

        /// <summary>
        /// returns an IConquestGameElement
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var model = new CharacterGameElementModel()
            {
                Faction = Faction,
                Unit = Unit,
                Weight = Weight,
                Models = Models,
                Points = Points,
                AdditionalPoints = AdditionalPoints,
                LeaderPoints = LeaderPoints,
                StandardPoints = StandardPoints,
                IsReleased = IsReleased,
                Move = Move,
                Volley = Volley,
                Clash = Clash,
                Attacks = Attacks,
                Wounds = Wounds,
                Resolve = Resolve,
                Defense = Defense,
                Evasion = Evasion,
                Barrage = Barrage,
                Range = Range,
                ArmorPiercing = ArmorPiercing,
                Cleave = Cleave,
                IsImpact = IsImpact,
                BrutalImpact = BrutalImpact,
                IsShields = IsShields,
                IsFury = IsFury,
                IsFlurry = IsFlurry,
                IsRelentless = IsRelentless,
                IsFluid = IsFluid,
                IsFly = IsFly,
                IsDeadlyBlades = IsDeadlyBlades,
                IsDeadlyShot = IsDeadlyShot,
                IsAuraDeath = IsAuraDeath,
                IsSupport = IsSupport,
                IsBastion = IsBastion,
                IsBlessed = IsBlessed,
                IsTerrifying = IsTerrifying,
                IsArcOfFire = IsArcOfFire,
                IsFearless = IsFearless,
                ResistDecay = ResistDecay,
                AlwaysInspire = AlwaysInspire,
                Image = Image,
                SpellSchools = SpellSchools,
                Schools = Schools,
                BuffDefenseOrEvasion = BuffDefenseOrEvasion,
                Healing = Healing,
                NoObscure = NoObscure,
                Reroll6_Defense = Reroll6_Defense,
                Reroll6_Volley = Reroll6_Volley,
                MeleeHeal4 = MeleeHeal4,
                DoubleAttack = DoubleAttack,
                OneHitPerFile = OneHitPerFile,
                D_Volley = D_Volley,
                Decay = Decay,
                Notes = Notes,
                IsTorrential = IsTorrential,
                IsRegen = IsRegen,
                IsPrecise = IsPrecise,
                ModelType = ModelType,
                UserName = this.Unit,
                MasteryChoices = MasteryChoices.CopyCollection(),
                ActiveRetinues = ActiveRetinues.CopyCollection(),
                Spells = Spells.CopyList(),
                RetinueMetaData = RetinueMetaData,
                StandCount = StandCount,
                WizardLevel = WizardLevel
            };

            foreach (var option in Options)
            {
                model.Options.Add(option);
            }

            model.MainstayChoices = new List<string>(this.MainstayChoices);
            model.RestrictedChoices = new List<string>(this.RestrictedChoices);

            return model;
        }
    }
}
