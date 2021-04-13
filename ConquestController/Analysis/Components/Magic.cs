using System;
using System.Collections.Generic;
using System.Linq;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis.Components
{
    public class Magic : BaseComponent
    {
        /// <summary>
        /// Magic can both do damage as well as buff regiments or models, this method has to figure out all of those
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">The model casting the spell</param>
        /// <param name="spell">The spell being evaluated</param>
        /// <param name="clashValues">For buffs</param>
        /// <param name="defenseValues">For buffs and damage</param>
        /// <param name="resolveValues">For buffs and damage</param>
        /// <param name="useSmartCasting">When TRUE will adjust defense values sent in to not target high armor with little/no cleave</param>
        public static double CalculateOutput(IConquestGamePiece model, ISpell spell, List<int> clashValues,
            List<int> defenseValues, List<int> resolveValues, bool useSmartCasting)
        {
            if (!(model is IConquestSpellcaster) || spell == null)
            {
                return 0.0d;
            }

            var outputScore = 0.0d;
            var tags = GetTags(spell);

            BoostStats(model, spell);

            if (spell.HitsCaused > 0)
                outputScore += HandleOffensiveSpell((IConquestSpellcaster)model, spell, clashValues, defenseValues, resolveValues, tags, useSmartCasting);

            
            return outputScore;
        }

        private static List<string> GetTags(ISpell spell)
        {
            var tags = spell.Tag.Split('|', StringSplitOptions.RemoveEmptyEntries);

            return tags.Where(tag => tag != "[]").ToList();
        }

        private static void BoostStats<T>(T model, ISpell spell)
        {
            //todo
        }

        private static double HandleOffensiveSpell(IConquestSpellcaster model, ISpell spell, List<int> clashValues,
            List<int> defenseValues, List<int> resolveValues, List<string> tags, bool useSmartCasting)
        {           
            int cleave = 0;
            int isDeadlyShot = 0;
            int isDeadlyBlades = 0;
            int isDoubleAttack = 0;
            int isEruption = 0; //2 hits every stand within 6 inches - just do 8 hits
            int isBlessed = 0; //reroll all failed to hit or all failed defense rolls
            int oneHitPerFile = 0; //ala flame wall

            if (tags.Contains("Cleave1")) cleave = 1;
            if (tags.Contains("Cleave2")) cleave = 2;
            if (tags.Contains("Cleave3")) cleave = 3;
            if (tags.Contains("Cleave4")) cleave = 4;
            isDeadlyShot = CheckToggle(tags, "IsDeadlyShot");
            isDeadlyBlades = CheckToggle(tags, "IsDeadlyBlades");
            isDoubleAttack = CheckToggle(tags, "IsDoubleAttack");
            isEruption = CheckToggle(tags, "Eruption");
            isBlessed = CheckToggle(tags, "IsBlessed");
            oneHitPerFile = CheckToggle(tags, "OneHitPerFile");

            if (useSmartCasting)
            {
                //Cleave 0 - only target def 1-3, 1 = 1-4, 2 = 1-5, and 3 = 1-6
                if (cleave == 0 || cleave == 1 || cleave == 2)
                    defenseValues.Remove(6);
                
                if (cleave == 0 || cleave == 1)
                    defenseValues.Remove(5);
                
                if (cleave == 0)
                    defenseValues.Remove(4);
            }

            var hits = CalculateHits(spell.Difficulty, model.WizardLevel);
            if (model.OneHitPerFile || oneHitPerFile == 1) //add 3 hits to the attack value (as we're going with vs 3 regiment stands)
            {
                hits += 3;
            }
            
            if (isEruption == 1) hits = 8;

            var output = 0.0d;
            foreach (var defense in defenseValues)
            {
                var actualHits = ClashOffense.CalculateActualHits(hits, Probabilities[defense], isAuraOfDeathApplied: false, isDeadly: isDeadlyShot == 1, 
                    applyFullDeadly: false, smiteHits: 0);

                //now rip through the resolve values
                var resolveFails = CalculateMeanResolveFailures(actualHits, resolveValues, isTerrifying: false);

                output += actualHits + resolveFails;
            }
                                
            return output / defenseValues.Count;
        }

        private static int CheckToggle(List<string> tags, string key)
        {
            return tags.Contains(key) ? 1 : 0;
        }

        private static double CalculateHits(int difficulty, int dice)
        {
            return dice * Probabilities[difficulty];
        }
    }
}
