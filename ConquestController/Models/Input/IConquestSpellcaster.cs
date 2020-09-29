using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public interface IConquestSpellcaster
    {
        string SpellSchools { get; set; } //mapped to Schools, comes from input file
        List<string> Schools { get; set; }
        List<SpellModel> Spells { get; set; }
        int MaxSpells { get; set; }
    }
}
