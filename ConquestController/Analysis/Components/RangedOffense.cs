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
        public static double CalculateOutput(IConquestGamePiece model, List<int> defenseValues, bool supportOnly = false, bool applyFullDeadly = false)
        {
            var shotsFired = supportOnly ? 1 : model.Models * model.Barrage;
            var preciseHits = shotsFired * Probabilities[1]; //1 being the 1 on the D6 or 16.7%

            var shotsHitProbability = Probabilities[model.Volley];
            var shotsHitProbabilityAim = Probabilities[model.Volley + 1];
            var outputsByDefense = new List<RangedOutput>(); //collection more for debug purposes
            var totalOutput = 0.0;
            var totalScoreCount = 0;
            var armorPiercing = model.ArmorPiercing;

            //if overcharging every action they can put a token down that gives +2 shots and +1AP.  We will be modeling an overcharge and shot in one turn here.
            if (model.IsOvercharge == 1)
            {
                shotsFired += (2 * model.Models);
                armorPiercing++;
            }

            foreach (var defense in defenseValues)
            {
                var rangedOutput = new RangedOutput() { DefenseValue = defense };

                var finalD = Math.Clamp(defense - armorPiercing, 0, 6);
                var defenseProbability = Probabilities[finalD];
                var hits = shotsFired * shotsHitProbability;
                var aimedHits = shotsFired * shotsHitProbabilityAim;

                if (model.Reroll6_Volley)
                {
                    var rerollableHits = shotsFired * Probabilities[1]; //1 in 6 of the overall shots fired would be a six and can be rerolled
                    preciseHits += rerollableHits * Probabilities[1]; //now add those to the preciseHits since some of those could be 1s as well

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

                    preciseHits += misses * shotsHitProbability * Probabilities[1]; //now add those juicy sweet re rolls to preciseHits because some of those can be 1s
                }

                if (model.IsTorrential == 1)
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
                    preciseHits += hits * Probabilities[1]; //if i hit 3.33 out of 10 times, i'm rolling 3.33 more dice so 16.7% of those are 1s, so add that to my precise hits
                }

                var obscureDivider = model.NoRangeObscure == 1 ? 1.5 : 2.0; //we cut the obscurement penalty down by half in this case
                obscureDivider = model.NoObscure ? 1.0 : obscureDivider; //this model can never be obscured so we eliminate it altogether

                if (model.IsPrecise == 0) preciseHits = 0;

                if (model.IsStrongArm == 1)
                {
                    //strong arm means it ignores obscure for range.  For this we'll just ignore obscure entirely to get a boost to volley score
                    rangedOutput.ObscureHits = CalculateActualRangedHits((hits), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly, preciseHits);
                    rangedOutput.ObscureAimedHits = CalculateActualRangedHits((aimedHits), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly, preciseHits);
                }
                else
                {
                    rangedOutput.ObscureHits = CalculateActualRangedHits((hits / obscureDivider), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly, preciseHits);
                    rangedOutput.ObscureAimedHits = CalculateActualRangedHits((aimedHits / obscureDivider), defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly, preciseHits);
                }

                rangedOutput.FullHits = CalculateActualRangedHits(hits, defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly, preciseHits);
                rangedOutput.FullAimedHits = CalculateActualRangedHits(aimedHits, defenseProbability, model.IsDeadlyShot == 1, applyFullDeadly, preciseHits);

                outputsByDefense.Add(rangedOutput);

                totalScoreCount += 4;
                totalOutput += rangedOutput.SumOfOutput();
            }

            var score = totalOutput / totalScoreCount; //give me the average

            score = ApplyRangeModifiersToScore(score, model.Range, model.IsArcOfFire == 1); //adjust it for the range and arc of fire
            return score;
        }

        private static double CalculateActualRangedHits(double hits, double defenseProbability, bool isDeadly, bool applyFullDeadly, double preciseHits)
        {
            var deadlyDmg = 1.0d;
            if (isDeadly && applyFullDeadly) deadlyDmg = DeadlyShotBladesDmg;
            if (isDeadly && !applyFullDeadly) deadlyDmg = HalvedDeadlyShotBladesDmg;

            double returnVal = 0;
            returnVal = preciseHits > 0 ? CalculatePreciseHits(hits, preciseHits, defenseProbability) 
                                    : CalculateHits(hits, defenseProbability);

            if (isDeadly) returnVal *= deadlyDmg;

            return returnVal;
        }

        private static double CalculateHits(double hits, double defenseProbability)
        {
            return hits - (hits * defenseProbability);
        }

        private static double CalculatePreciseHits(double hits, double preciseHits, double defenseProbability)
        {
            //Precise hits - a roll of a 1 is Defense of 0 or a probability of 0 (so preciseHits are all of the hits that were a "1")
            //so 0.167 of the total attacks came in with a defenseProbability of 0
            
            //scenario - 10 shots volley 2 vs defense 2.  Expect hits = 3.33, preciseHits = 1.67, and defenseProbability = 0.33
            //return value would be 2.7822 (instead of the 2.2311 without precise shot)

            var savableHits = hits - preciseHits;
            return savableHits - (savableHits * defenseProbability) + preciseHits;
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
