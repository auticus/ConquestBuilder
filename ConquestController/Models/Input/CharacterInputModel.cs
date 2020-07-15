using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public class CharacterInputModel : IConquestRegimentInput
    {
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Weight { get; set; }
        public int Points { get; set; }
        public int IsReleased { get; set; }

        //****STATS AND RULES **//
        public int Volley { get; set; }
        public int Clash { get; set; }
        public int Attacks { get; set; }
        public int Wounds { get; set; }
        public int Resolve { get; set; }
        public int Defense { get; set; }
        public int Evasion { get; set; }
        public int Barrage { get; set; }
        public int Range { get; set; }
        public int ArmorPiercing { get; set; }
        public int Cleave { get; set; }
        public int IsImpact { get; set; }
        public int BrutalImpact { get; set; }
        public int IsShields { get; set; }
        public int WizardLevel { get; set; }
        public int IsFearless { get; set; }
        public int ResistDecay { get; set; }
        public int IsQuickSilverStrike { get; set; }
        public int IsFlurry { get; set; }
        public string Intangibles { get; set; }
        public int ItemCount { get; set; }

        public string Mainstay { get; set; } //mapped to MainstayChoices
        public string Restricted { get; set; } // mapped to RestrictedChoices

        public int NoWarlord { get; set; }
        public string Supremacy { get; set; }
        public string SupremacyNotes { get; set; }
        public string SpellSchools { get; set; } //mapped to Schools
        public int MaxSpells { get; set; } //how many they get to pick from a school, most of the time its unlimited but biomancy restricts to just 1

        public List<IConquestInput> Options { get; set; }
        public List<string> MainstayChoices { get; set; }
        public List<string> RestrictedChoices { get; set; }
        public List<string> Schools { get; set; }

        public CharacterInputModel()
        {
            Options = new List<IConquestInput>();
            MainstayChoices = new List<string>();
            RestrictedChoices = new List<string>();
            Schools = new List<string>();
        }

        public override string ToString()
        {
            return Unit;
        }
    }
}
