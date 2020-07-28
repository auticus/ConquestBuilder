using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace ConquestController.Models.Input
{
    public class UnitInputModel : IConquestRegimentInput
    {
        public UnitInputModel()
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

        public List<IConquestInput> Options { get; }

        public override string ToString()
        {
            return Unit;
        }

        public UnitInputModel Copy()
        {
            var model = new UnitInputModel();
            model.Faction = Faction;
            model.Unit = Unit;
            model.Weight = Weight;
            model.Models = Models;
            model.Points = Points;
            model.AdditionalPoints = AdditionalPoints;
            model.LeaderPoints = LeaderPoints;
            model.StandardPoints = StandardPoints;
            model.IsReleased = IsReleased;
            model.Move = Move;
            model.Volley = Volley;
            model.Clash = Clash;
            model.Attacks = Attacks;
            model.Wounds = Wounds;
            model.Resolve = Resolve;
            model.Defense = Defense;
            model.Evasion = Evasion;
            model.Barrage = Barrage;
            model.Range = Range;
            model.ArmorPiercing = ArmorPiercing;
            model.Cleave = Cleave;
            model.IsImpact = IsImpact;
            model.BrutalImpact = BrutalImpact;
            model.IsShields = IsShields;
            model.IsFury = IsFury;
            model.IsFlurry = IsFlurry;
            model.IsFluid = IsFluid;
            model.IsFly = IsFly;
            model.IsDeadlyBlades = IsDeadlyBlades;
            model.IsDeadlyShot = IsDeadlyShot;
            model.IsAuraDeath = IsAuraDeath;
            model.IsSupport = IsSupport;
            model.IsBastion = IsBastion;
            model.IsTerrifying = IsTerrifying;
            model.IsArcOfFire = IsArcOfFire;
            model.IsFearless = IsFearless;
            model.ResistDecay = ResistDecay;
            model.AlwaysInspire = AlwaysInspire;

            foreach (var option in Options)
            {
                model.Options.Add(option);
            }

            return model;
        }
    }
}
