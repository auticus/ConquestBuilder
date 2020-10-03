using System;
using System.Collections.Generic;
using System.Linq;
using ConquestController.Models.Input;

namespace ConquestController.Analysis.Components
{
    public class Magic : BaseComponent
    {
        //todo: implement magic output

        /// <summary>
        /// Magic can both do damage as well as buff regiments or models, this method has to figure out all of those
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model casting the spell</param>
        /// <param name="spell">The spell being evaluated</param>
        /// <param name="clashValues">For buffs</param>
        /// <param name="defenseValues">For buffs and damage</param>
        /// <param name="resolveValues">For buffs and damage</param>
        /// <returns>An array where the 0 element is raw output and the 1 element is resolve output</returns>
        public static double CalculateOutput<T>(T model, SpellModel spell, List<int> clashValues,
            List<int> defenseValues, List<int> resolveValues) where T : ConquestInput<T>
        {
            var output = 0.0d;
            var tags = GetTags(spell);

            BoostStats(model, spell);

            if (spell.HitsCaused > 0)
                output += HandleOffensiveSpell(model, spell, clashValues, defenseValues, resolveValues, tags);

            return 0.0;
        }

        private static List<string> GetTags(SpellModel spell)
        {
            var tags = spell.Tag.Split('|', StringSplitOptions.RemoveEmptyEntries);

            return tags.Where(tag => tag != "[]").ToList();
        }

        private static void BoostStats<T>(T model, SpellModel spell)
        {

        }

        private static double HandleOffensiveSpell<T>(T model, SpellModel spell, List<int> clashValues,
            List<int> defenseValues, List<int> resolveValues, List<string> tags) where T : ConquestInput<T>
        {
            /*
            int cleave = 0;
            int isDeadlyShot = 0;
            int isDeadlyBlades = 0;

            if (tags.Contains("Cleave1")) cleave = 1;
            if (tags.Contains("Cleave2")) cleave = 2;
            if (tags.Contains("Cleave3")) cleave = 3;
            if (tags.Contains("Cleave4")) cleave = 4;
            isDeadlyShot = tags.Contains("IsDeadlyShot") ? 1 : 0;
            isDeadlyBlades = tags.Contains("IsDeadlyBlades") ? 1 : 0;

            if (model.OneHitPerFile) //add 3 hits to the attack value (as we're going with vs 3 regiment stands)

            //blessed - reroll all failed to hit or all failed defense rolls
            */
            
            return 0.0;
        }


    }
}
