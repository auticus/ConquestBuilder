﻿using System;
using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public class CharacterInputModel : ConquestInput<CharacterInputModel>, IConquestSpellcaster
    {
        public int IsQuickSilverStrike { get; set; }
        public int ItemCount { get; set; }

        public string Mainstay { get; set; } //mapped to MainstayChoices
        public string Restricted { get; set; } // mapped to RestrictedChoices

        public int NoWarlord { get; set; }
        public string SupremacyTitle { get; set; }
        public string SupremacyNotes { get; set; }
        public string SpellSchools { get; set; } //mapped to Schools, comes from input
        public int MaxSpells { get; set; } //how many they get to pick from a school, most of the time its unlimited but biomancy restricts to just 1

        public List<string> MainstayChoices { get; set; }
        public List<string> RestrictedChoices { get; set; }
        public List<string> Schools { get; set; }
        public List<SpellModel> Spells { get; set; }

        public CharacterInputModel()
        {
            MainstayChoices = new List<string>();
            RestrictedChoices = new List<string>();
            Schools = new List<string>();
            Spells = new List<SpellModel>();
        }

        public override bool CanCalculateDefense() => false;
        public override bool CanCastSpells() => true;  //characters have the ability to cast spells if they have the rules for it

        public override string ToString()
        {
            return Unit;
        }

        public override CharacterInputModel Copy()
        {
            var model = new CharacterInputModel()
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
                IsTorrential = IsTorrential
            };

            foreach (var option in Options)
            {
                model.Options.Add(option);
            }

            return model;
        }
    }
}
