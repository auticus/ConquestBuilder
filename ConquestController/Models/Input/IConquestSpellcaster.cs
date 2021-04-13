using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public interface IConquestSpellcaster : IConquestGamePiece
    {
        //spellcasters aren't always casters - which is why it implements the game element interface and not the character interface

        string SpellSchools { get; set; } //mapped to Schools, comes from input file
        List<string> Schools { get; set; }
        List<ISpell> Spells { get; set; }
        int MaxSpells { get; set; }
        int WizardLevel { get; set; }
    }
}
