namespace ConquestController.Models.Input
{
    public class UnitInputModel : ConquestInput<UnitInputModel>
    {
        public UnitInputModel()
        {
            
        }

        public override bool CanCalculateDefense() => true;
        public override bool CanCastSpells() => false;

        public override string ToString()
        {
            return Unit;
        }

        public override UnitInputModel Copy()
        {
            var model = new UnitInputModel
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
                AnalysisOnly = AnalysisOnly,
                Image = Image,
                BuffDefenseOrEvasion = BuffDefenseOrEvasion,
                Healing = Healing,
                NoObscure = NoObscure,
                Reroll6_Defense = Reroll6_Defense,
                Reroll6_Volley = Reroll6_Volley,
                MeleeHeal4 = MeleeHeal4,
                DoubleAttack = DoubleAttack,
                OneHitPerFile = OneHitPerFile,
                D_Volley = D_Volley,
                Decay = Decay
            };

            foreach (var option in Options)
            {
                model.Options.Add(option);
            }

            return model;
        }
    }
}
