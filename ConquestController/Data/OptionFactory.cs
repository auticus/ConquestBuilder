using ConquestController.Models.Input;

namespace ConquestController.Data
{
    public class OptionFactory
    {
        public static UnitOptionModel CreateAdhocOption(string faction, string unit, string optionName, string optionNotes, int points)
        {
            var option = new UnitOptionModel()
            {
                Category="0",
                Faction= faction,
                Name = optionName,
                Notes = optionNotes,
                Points = points,
                Unit = unit
            };

            return option;
        }
    }
}
