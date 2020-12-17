using ConquestController.Models.Input;

namespace ConquestController.Data
{
    public class OptionFactory
    {
        public static BaseOption CreateAdhocOption(string faction, string unit, string optionName, string optionNotes, int points)
        {
            var option = new BaseOption()
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
