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
        public static double CalculateOutput<T>(ConquestGameElement<T> model, List<int> defenseValues, bool supportOnly = false, bool applyFullDeadly = false)
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

                if (model.Reroll6_Volley)
                {
                    var rerollableHits = shotsFired * Probabilities[1]; //1 in 6 of the overall shots fired would be a six and can be rerolled

                    var rerolledHits = rerollableHits * shotsHitProbability;
                    var rerolledAimedHits = rerollableHits * shotsHitProbabilityAim;

                    hits += rerolledHits;
                    aimedHits += rerolledAimedHits;
                }

                if (model.IsBlessed == 1)
                {
                    //blessed lets them reroll hits, but also defense, so we halve the output from offense and defense gains to get a mean score overall
                    var misses = shotsFired - hits;
                    var aimedMisses = shotsFired - aimedHits;

                    misses /= 2;
                    aimedMisses /= 2;

                    hits += (misses * shotsHitProbability);
                    aimedHits += (aimedMisses * shotsHitProbabilityAim);
                }

                if (model.IsTorrential)
                {
                    //Torrential means for every hit, the unit gets an additional dice to try and hit (which does not generate another hit regardless)
                    //it stands to reason that if you have 50% hits that 50% of those would be hits with torrential
                    //ie i have 10 shots, 5 hit.  Then 5 dice roll again and 2.5 of those would hit for a total of 7.5 hits
                    var hitPercent = hits / shotsFired;
                    var aimedHitPercent = aimedHits / shotsFired;

                    var torrentialHits = hits * hitPercent;
                    var aimedTorrentialHits = aimedHits * aimedHitPercent;

                    hits += torrentialHits;
                    aimedHits += aimedTorrentialHits;
                }

                var obscureDivider = model.NoObscure ? 1.0 : 2.0;

                rangedOutput.ObscureHits = CalculateActualRangedHits((hits / obscureDivider), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly);
                rangedOutput.ObscureAimedHits = CalculateActualRangedHits((aimedHits / obscureDivider), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly);

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
