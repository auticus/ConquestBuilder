using System;
using System.Collections.Generic;
using ConquestController.Models.Input;
using ConquestController.Models.Output;

namespace ConquestController.Analysis.Components
{
    public class RangedOffense : BaseComponent
    {
        /// <summary>
        /// Calculates the ranged offense of a model against a list of defense values
        /// </summary>
        /// <param name="model"></param>
        /// <param name="defenseValues"></param>
        /// <param name="supportOnly"></param>
        /// <param name="applyFullDeadly">Set to true if you know deadly blades is being applied fully, otherwise it will be halved</param>
        /// <returns>The average ranged output against the range of defense values passed</returns>
        public static double CalculateOutput<T>(T model, List<int> defenseValues, bool supportOnly = false, bool applyFullDeadly = false)
            where T: ConquestInput<T>
        {
            var shotsFired = supportOnly ? 1 : model.Models * model.Barrage;

            var shotsHitProbability = Probabilities[model.Volley];
            var shotsHitProbabilityAim = Probabilities[model.Volley + 1];
            var outputsByDefense = new List<RangedOutput>(); //collection more for debug purposes
            var totalOutput = 0.0;
            var totalScoreCount = 0;

            foreach (var defense in defenseValues)
            {
                var rangedOutput = new RangedOutput() { DefenseValue = defense };

                var finalD = Math.Clamp(defense - model.ArmorPiercing, 0, 6);
                var defenseProbability = Probabilities[finalD];
                var hits = shotsFired * shotsHitProbability;
                var aimedHits = shotsFired * shotsHitProbabilityAim;

                rangedOutput.ObscureHits = CalculateActualRangedHits((hits / 2.0), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly);
                rangedOutput.ObscureAimedHits = CalculateActualRangedHits((aimedHits / 2.0), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly);

                rangedOutput.FullHits = CalculateActualRangedHits(hits, defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly);
                rangedOutput.FullAimedHits = CalculateActualRangedHits(aimedHits, defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly);

                outputsByDefense.Add(rangedOutput);

                totalScoreCount += 4;
                totalOutput += rangedOutput.SumOfOutput();
            }

            var score = totalOutput / totalScoreCount; //give me the average

            score = ApplyRangeModifiersToScore(score, model.Range, model.IsArcOfFire == 1); //adjust it for the range and arc of fire
            return score;
        }

        private static double CalculateActualRangedHits(double hits, double defenseProbability, bool isDeadly, bool applyFullDeadly)
        {
            var deadlyDmg = 1.0d;
            if (isDeadly && applyFullDeadly) deadlyDmg = DeadlyShotBladesDmg;
            if (isDeadly && !applyFullDeadly) deadlyDmg = HalvedDeadlyShotBladesDmg;

            var returnVal = hits - (hits * defenseProbability);
            if (isDeadly) returnVal *= deadlyDmg;

            return returnVal;
        }

        private static double ApplyRangeModifiersToScore(double score, int range, bool isArcOfFire)
        {
            var rangeModifier = (range * RangeModifier) * score;
            var returnScore = score + rangeModifier;

            if (isArcOfFire) returnScore *= ArcOfFireMultiplier;

            return returnScore;
        }
    }
}
