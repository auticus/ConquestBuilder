using System;
using System.Collections.Generic;
using System.Linq;
using ConquestController.Models.Input;

namespace ConquestController.Analysis.Components
{
    public class Mastery
    {
        public static IList<IOption> GetFilteredMasteries(IConquestBaseGameElement character,
            IList<IMastery> masteries)
        {
            var returnList = new List<IMastery>();
            returnList.AddRange(masteries.Where(p => p.Faction == "ALL" || p.Faction == character.Faction));

            FilterByCharacterType(character, returnList);
            FilterByWeightClass(character, returnList);
            FilterByRetinueRestrictions(character, returnList);
            AssignBasePoints(returnList);

            var castedList = returnList.Cast<IOption>().ToList();
            return castedList;
        }

        private static void AssignBasePoints(List<IMastery> masteries)
        {
            //the basepoints is what it costs before taking the item.  Points can get higher depending on how much you spam.  
            //each time a mastery is taken, taking it again costs double
            foreach (var mastery in masteries)
            {
                mastery.BasePoints = mastery.Points;
            }
        }

        private static void FilterByCharacterType(IConquestBaseGameElement character, List<IMastery> masteries)
        {
            for (var i = 0; i < masteries.Count; i++)
            {
                var mastery = masteries[i];
                var restrictions = mastery.Restrictions.Split("|");
                bool? classRestrictionSuccess = null;

                foreach (var restriction in restrictions)
                {
                    switch (restriction)
                    {
                        case "Imperial Officer":
                        case "Priory Commander":
                        case "Noble Lord - Infantry":
                        case "Theist Priest":
                        case "High Clone Executor":
                        case "Lineage Highborn":
                        case "Biomancer":
                        case "Hold Raegh":
                        case "Tempered Sorcerer":
                        case "Tempered Steelshaper":
                        case "Ardent Keraweg":
                        case "Blooded":
                        case "Volva":
                            classRestrictionSuccess = character.Unit == restriction;
                            break;
                    }

                    if (classRestrictionSuccess == true) break;
                }

                //remove if necessary
                if (classRestrictionSuccess == false)
                {
                    masteries.Remove(masteries[i]);
                }
            }
        }

        private static void FilterByWeightClass(IConquestBaseGameElement character, List<IMastery> masteries)
        {
            for (var i = 0; i < masteries.Count; i++)
            {
                var restrictions = masteries[i].Restrictions.Split("|");
                bool? classRestrictionSuccess = null;
                foreach (var restriction in restrictions)
                {
                    switch (restriction.ToLower())
                    {
                        case "light":
                        case "medium":
                        case "heavy":
                            classRestrictionSuccess = String.Equals(character.Weight, restriction, StringComparison.CurrentCultureIgnoreCase);
                            break;
                    }

                    if (classRestrictionSuccess == true) break;
                }

                if (classRestrictionSuccess == false)
                {
                    masteries.Remove(masteries[i]);
                }

            }
        }

        private static void FilterByRetinueRestrictions(IConquestBaseGameElement character, List<IMastery> masteries)
        {
            //todo: implement in issue 18
        }
    }
}
