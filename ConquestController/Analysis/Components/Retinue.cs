using System.Collections.Generic;
using System.Linq;
using ConquestController.Models.Input;

namespace ConquestController.Analysis.Components
{
    public static class Retinue
    {
        /// <summary>
        /// string is the name of the tag from the mastery model, the value is the retinue its tied to that needs to be set on to choose the mastery
        /// </summary>
        public static Dictionary<string, ITieredBaseOption> GetRetinueRestrictionDictionary(IEnumerable<ITieredBaseOption> retinues)
        {
            var dictionary = new Dictionary<string, ITieredBaseOption>
            {
                {"Tier1TacticalRetinue", retinues.First(p => p.Category == "Tactical" && p.Tier == 1)},
                {"Tier2TacticalRetinue", retinues.First(p => p.Category == "Tactical" && p.Tier == 2)},
                {"Tier3TacticalRetinue", retinues.First(p => p.Category == "Tactical" && p.Tier == 3)},
                {"Tier1CombatRetinue", retinues.First(p => p.Category == "Combat" && p.Tier == 1)},
                {"Tier2CombatRetinue", retinues.First(p => p.Category == "Combat" && p.Tier == 2)},
                {"Tier3CombatRetinue", retinues.First(p => p.Category == "Combat" && p.Tier == 3)},
                {"Tier1MagicRetinue", retinues.First(p => p.Category == "Magic" && p.Tier == 1)},
                {"Tier2MagicRetinue", retinues.First(p => p.Category == "Magic" && p.Tier == 2)},
                {"Tier3MagicRetinue", retinues.First(p => p.Category == "Magic" && p.Tier == 3)}
            };

            return dictionary;
        }
    }
}
