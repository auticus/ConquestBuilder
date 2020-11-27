using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis.Components
{
    public class ClashOffense : BaseComponent
    {
        /// <summary>
        /// returns an array in the format of 0 = normal attacks, and 1 = the impact hits if applicable (or 0 otherwise)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="defenseValues"></param>
        /// <param name="resolveValues"></param>
        /// <param name="supportOnly"></param>
        /// <param name="applyFullyDeadly">Set to true if you know deadly blades is being applied fully, otherwise it will be halved</param>
        /// <returns></returns>
        public static double[] CalculateOutput<T>(ConquestGameElement<T> model, List<int> defenseValues, List<int> resolveValues, bool supportOnly = false,
                bool applyFullyDeadly = false) 
        {
            return CalculateFullAttacksOutput(model, defenseValues, resolveValues, supportOnly, applyFullyDeadly);
        }

        private static double CalculateActualClashHits(double hits, double defenseProbability, bool isAuraOfDeathApplied,
            bool isDeadly, bool applyFullDeadly)
        {
            //if aura of death was applied then the hits had the AURA_DEATH_STANDS const added to it so we need to remove them here and not include that in the calculation
            //of deadly blades
            var deadlyDmg = 1.0d;
            if (isDeadly && applyFullDeadly) deadlyDmg = DeadlyShotBladesDmg;
            if (isDeadly && !applyFullDeadly) deadlyDmg = HalvedDeadlyShotBladesDmg;

            if (isDeadly)
            {
                if (isAuraOfDeathApplied)
                {
                    hits -= AuraDeathStands;
                    return ((hits - (hits * defenseProbability)) * deadlyDmg) + AuraDeathStands;
                }
                return (hits - (hits * defenseProbability)) * deadlyDmg;
            }

            return hits - (hits * defenseProbability);
        }

        /// <summary>
        /// Returns a value in an array where 0 = normal attacks, and 1 = impact hits
        /// </summary>
        /// <param name="model"></param>
        /// <param name="defenseValues"></param>
        /// <param name="resolveValues"></param>
        /// <param name="supportOnly"></param>
        /// <returns></returns>
        private static double[] CalculateFullAttacksOutput<T>(ConquestGameElement<T> model, IEnumerable<int> defenseValues, List<int> resolveValues, 
            bool supportOnly = false, bool applyFullyDeadly = false) 
        {
            if (!defenseValues.Any()) return new[] {0.0d, 0.0d};

            int attacks;

            if (supportOnly)
            {
                attacks = model.IsSupport == 1 ? 2 : 1;
            }
            else
            {
                attacks = model.Attacks * model.Models;
                if (model.IsFury == 1) attacks += ConquestUnitOutput.BASE_STAND_COUNT;
            }

            var hitProbability = Probabilities[model.Clash];
            var inspiredHitProbability = Probabilities[model.Clash + 1];

            var totalOutput = 0.0d;
            var totalScores = 0;

            var totalImpact = 0.0d;
            var totalImpacts = 0;

            foreach (var defense in defenseValues)
            {
                var finalOutput = 0.0d;

                //calculate standard hits
                var normalHitProbability = model.AlwaysInspire == 1 ? inspiredHitProbability : hitProbability;
                var output = CalculateClashFragment(defense, model, attacks, normalHitProbability, 
                    resolveValues, applyFullyDeadly, thisIsImpactHits: false);

                finalOutput += output;

                //calculate inspired hits
                output = CalculateClashFragment(defense, model, attacks, inspiredHitProbability, 
                    resolveValues, applyFullyDeadly, thisIsImpactHits: false);

                finalOutput += output;

                totalScores += 2;

                //calculate impact hits
                if (model.IsImpact == 1)
                {
                    //impact hits are half of the attack value of the stands in contact (rounded down).  support stands grant +1 impact hit each;
                    var impactHits = attacks / 2.0d;
                    totalImpact += CalculateClashFragment(defense, model, Math.Floor(impactHits), hitProbability, 
                        resolveValues, applyFullyDeadly, thisIsImpactHits: true);
                    totalImpacts++;
                }

                totalOutput += finalOutput;
            }

            return model.IsImpact == 1 ? new[] {totalOutput / totalScores, totalImpact / totalImpacts} 
                                        : new[] {totalOutput / totalScores, 0};
        }

        private static double CalculateClashFragment<T>(int defense, ConquestGameElement<T> model, double attacks, double hitProbability, 
            List<int> resolveValues, bool applyFullyDeadly, bool thisIsImpactHits) 
        {
            var finalD = thisIsImpactHits ? Math.Clamp(defense - model.BrutalImpact, 0, 6) : Math.Clamp(defense - model.Cleave, 0, 6);
            var defenseProbability = Probabilities[finalD];

            var totalHits = attacks * hitProbability;
            if (!thisIsImpactHits && (model.IsFlurry == 1 || model.IsBlessed == 1))
            {
                var misses = attacks - totalHits;

                if (model.IsFlurry == 1)
                    totalHits += (misses * hitProbability);
                else if (model.IsBlessed == 1)
                {
                    //blessed lets them also reroll hits like flurry, but also defense, so we halve the output from offense and defense gains to get a mean score overall
                    misses /= 2;
                    totalHits += (misses * hitProbability);
                }
            }

            if (model.IsAuraDeath == 1)
            {
                //aura of death hits add to the total hits
                totalHits += AuraDeathStands;
            }

            var avgResolveFailures = CalculateMeanResolveFailures(totalHits, resolveValues, model.IsTerrifying == 1);
            var finalOutput = CalculateActualClashHits(totalHits, defenseProbability, model.IsAuraDeath == 1,
                model.IsDeadlyBlades == 1, applyFullyDeadly) + avgResolveFailures;

            return finalOutput;
        }
    }
}
