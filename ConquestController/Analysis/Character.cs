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
                        unitOutput.Unit += $" ({spell.Name})";
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
            AssignCharacterExtras(characters, spells, inputOptionsFilePath);

            return characters;
        }
        private static void AssignCharacterExtras(IEnumerable<CharacterInputModel> characters, IEnumerable<SpellModel> spells, string inputOptionsFilePath)
        {
            var spellModels = spells.ToList();

            foreach (var character in characters)
            {
                //assign restricted and mainstay choices
                DataRepository.AssignDelimitedPropertyToList(character.MainstayChoices as IList<string>, character.Mainstay);
                DataRepository.AssignDelimitedPropertyToList(character.RestrictedChoices as IList<string>, character.Restricted);

                //assign character options
                DataRepository.AssignUnitOptionsToModelsFromFile(characters.Cast<IConquestOptionInput>().ToList(), inputOptionsFilePath);
                
                //assign spell schools and individual spells to the character
                DataRepository.AssignDelimitedPropertyToList(character.Schools, character.SpellSchools);
                foreach (var school in character.Schools)
                {
                    character.Spells.AddRange(spellModels.Where(p => p.School == school));
                }
            }
        }
    }
}
