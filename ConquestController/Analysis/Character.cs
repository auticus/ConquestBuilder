using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
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
        /// <param name="gameElementModel">This method currently only works with CharacterGameElementGameElementModel as that is all that has spells</param>
        /// <param name="allClash"></param>
        /// <param name="allDefenses"></param>
        /// <param name="allResolve"></param>
        /// <param name="spells"></param>
        /// <returns></returns>
        public static IList<IConquestAnalysisOutput> CalculateSpellOutput(ConquestUnitOutput baseOutput, CharacterGameElementModel gameElementModel, List<int> allClash,
            List<int> allDefenses, List<int> allResolve, IEnumerable<SpellModel> spells)
        {
            //loop through all spells that this gameElementModel can have applied to it
            var output = new List<IConquestAnalysisOutput>();

            if (!gameElementModel.Schools.Any()) return output;

            foreach (var school in gameElementModel.Schools)
            {
                foreach (var spell in spells.Where(p => p.Category == school))
                {
                    var spellOutput = Magic.CalculateOutput(gameElementModel, spell, allClash, allDefenses, allResolve);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="inputOptionsFilePath"></param>
        /// <param name="spells"></param>
        /// <param name="masteries">All masteries in the game not limited by faction</param>
        /// <returns></returns>
        public static IEnumerable<IConquestCharacter> GetAllCharacters(string inputFilePath, 
            string inputOptionsFilePath, 
            IEnumerable<ISpell> spells, 
            IEnumerable<IBaseOption> masteries,
            IEnumerable<ITieredBaseOption> retinues,
            IEnumerable<IPerkOption> items,
            out IList<IPerkOption> perks)
        {
            //assign characters
            var characters = DataRepository.GetInputFromFileToList<CharacterGameElementModel>(inputFilePath);

            //assign mainstay and restricted choices
            AssignCharacterExtras(characters, spells, masteries, retinues, items, inputOptionsFilePath, out perks);

            return characters;
        }
        private static void AssignCharacterExtras(IEnumerable<IConquestCharacter> characters, 
            IEnumerable<ISpell> spells, 
            IEnumerable<IBaseOption> masteries,
            IEnumerable<ITieredBaseOption> retinues,
            IEnumerable<IPerkOption> items,
            string inputOptionsFilePath,
            out IList<IPerkOption> perks)
        {
            var spellModels = spells.ToList();
            var factionRetinues = new Dictionary<string, IEnumerable<string>>();
            perks = new List<IPerkOption>();

            var factionsProcessed = new List<string>();

            foreach (var character in characters)
            {
                //assign restricted and mainstay choices
                DataRepository.AssignDelimitedPropertyToList(character.MainstayChoices as IList<string>, character.Mainstay);
                DataRepository.AssignDelimitedPropertyToList(character.RestrictedChoices as IList<string>, character.Restricted);
                DataRepository.AssignUnitOptionsToModelsFromFile(new List<IConquestGamePiece>(){character}, inputOptionsFilePath);
                
                //assign spell schools and individual spells to the character
                if (character is IConquestSpellcaster spellcaster)
                {
                    AssignSpells(spellcaster, spellModels);
                }

                //harvest out the perks from the options to put into the perks collection
                foreach (var option in character.Options)
                {
                    if (option is IPerkOption opt && !string.IsNullOrEmpty(opt.Perk))
                    {
                        perks.Add(opt);
                    }
                }

                //masteries chosen by retinues cannot be set here as there is no way of knowing what retinue the character has at this point
                var filteredMasteries = Mastery.GetFilteredMasteries(character, masteries.Cast<IMastery>().ToList());
                var masteryList = character.MasteryChoices.Cast<IBaseOption>().ToList();
                DataRepository.AssignDelimitedPropertyToOptionList(masteryList,
                        filteredMasteries, 
                            character.Masteries);
                foreach (var mastery in masteryList)
                {
                    character.MasteryChoices.Add((IMastery)mastery);
                }

                //retinues
                if (!factionRetinues.ContainsKey(character.Faction))
                {
                    var retinueList = BuildFactionRetinueList(character.Faction, retinues);
                    factionRetinues.Add(character.Faction, retinueList);
                }

                //need array of categories and the array of data (Tactical|Combat|Magic)
                //IMPORTANT if that order gets changed, your data will be messed up... the categories should always be Tactical|Combat|Magic|Misc faction specific!!!
                DataRepository.AssignRetinueAvailabilities(character.RetinueMetaData, factionRetinues[character.Faction].ToArray(),character.Retinue.Split("|"));

                //one time faction processing here
                if (!factionsProcessed.Any(p => p == character.Faction)) //no resharper I'm not going to refactor this to say All != 
                {
                    foreach (var item in items.Where(p => p.Faction == character.Faction && !string.IsNullOrEmpty(p.Perk)))
                        perks.Add(item);

                    factionsProcessed.Add(character.Faction);
                }
            }
        }

        private static IEnumerable<string> BuildFactionRetinueList(string faction, IEnumerable<ITieredBaseOption> retinues)
        {
            return retinues.Where(p => p.Faction == "ALL" || p.Faction == faction).Select(p => p.Category).Distinct().ToList();
        }

        private static void AssignSpells(IConquestSpellcaster caster, List<ISpell> spellModels)
        {
            //assign spell schools and individual spells to the character
            DataRepository.AssignDelimitedPropertyToList(caster.Schools, caster.SpellSchools);
            foreach (var school in caster.Schools)
            {
                caster.Spells.AddRange(spellModels.Where(p => p.Category == school && p.Faction == caster.Faction));
            }
        }
        
    }
}
