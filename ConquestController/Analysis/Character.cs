using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquestController.Analysis.Components;
using ConquestController.Data;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis
{
    /// <summary>
    /// Utility class that contains the methods and data to calculate character-specific logic
    /// </summary>
    public class Character
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseOutput"></param>
        /// <param name="model">This method currently only works with CharacterInputModel as that is all that has spells</param>
        /// <param name="allClash"></param>
        /// <param name="allDefenses"></param>
        /// <param name="allResolve"></param>
        /// <param name="spells"></param>
        /// <returns></returns>
        public static IList<ConquestUnitOutput> CalculateSpellOutput(ConquestUnitOutput baseOutput, CharacterInputModel model, List<int> allClash,
            List<int> allDefenses, List<int> allResolve, IEnumerable<SpellModel> spells)
        {
            //loop through all spells that this model can have applied to it
            var output = new List<ConquestUnitOutput>();

            if (!model.Schools.Any()) return output;

            foreach (var school in model.Schools)
            {
                foreach (var spell in spells.Where(p => p.School == school))
                {
                    var spellOutput = Magic.CalculateOutput(model, spell, allClash, allDefenses, allResolve);

                    if (spellOutput > 0)
                    {
                        var unitOutput = baseOutput.Copy();
                        unitOutput.Unit += $" ({spell.Spell})";
                        unitOutput.Stands[ConquestUnitOutput.FULL_OUTPUT].Magic.Output = spellOutput;

                        output.Add(unitOutput);
                    }
                }
            }

            return output;
        }

        public static IEnumerable<CharacterInputModel> GetAllCharacters(string inputFilePath, string inputOptionsFilePath, IEnumerable<SpellModel> spells)
        {
            //assign characters
            var characters = DataRepository.GetInputFromFileToList<CharacterInputModel>(inputFilePath);

            //assign mainstay and restricted choices
            AssignMainstayRestrictedUnits(characters);

            //assign character options
            DataRepository.AssignUnitOptionsToModelsFromFile(characters.Cast<IConquestOptionInput>().ToList(), inputOptionsFilePath);

            //todo: assign spells now to the options as well
            //spell school was passed, need to pass those spells in and add it to a new collection in the model that holds all of the available spells
            //spells have the point cost so just add them to the character where the school matches up

            //todo: will need to figure out how to deal with masteries that let you take more than one spell school
            return characters;
        }
        private static void AssignMainstayRestrictedUnits(IEnumerable<CharacterInputModel> characters)
        {
            foreach (var character in characters)
            {
                DataRepository.AssignDelimitedPropertyToList(character.MainstayChoices, character.Mainstay);
                DataRepository.AssignDelimitedPropertyToList(character.RestrictedChoices, character.Restricted);
                DataRepository.AssignDelimitedPropertyToList(character.Schools, character.SpellSchools);
            }
        }
    }
}
