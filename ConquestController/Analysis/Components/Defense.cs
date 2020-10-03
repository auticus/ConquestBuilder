using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ConquestController.Models.Input;

namespace ConquestController.Analysis.Components
{
    public class Defense : BaseComponent
    {
        //ultimately pull back your raw and your resolve defense scores

        /// <summary>
        /// Calculates Defensive output and returns an array of raw output and resolve output (which given the mean of the two gives total output)
        /// </summary>
        /// <param name="model">The model whose defense score you are looking for</param>
        /// <param name="defenseModificationValues">The cleave or AP modifiers you are calculating against</param>
        /// <param name="standCount">How many stands are we calculating defense for here</param>
        /// <param name="noShields">If shields are being bypassed in this analysis</param>
        /// <returns>an array of doubles in the format of [raw, resolve] where the mean of the two is the total</returns>
        public static double[] CalculateOutput<T>(T model, List<int> defenseModificationValues, 
            int standCount, bool noShields = false) where T: ConquestInput<T>
        {
            if (model.BuffDefenseOrEvasion)
                return CalculateDefenseBuffed(model, defenseModificationValues, standCount, noShields);

            if (model.D_Volley) //+1 defense vs volleys, so break the defense buffed in half and add that to the base defense
                return CalculateDefenseBuffedForHalfAttacks(model, defenseModificationValues, standCount, noShields);

            return CalculateDefense(model, defenseModificationValues, standCount, noShields);
        }

        /// <summary>
        /// Calculate defense and add half of the defense output if it was buffed by +1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="defenseModificationValues"></param>
        /// <param name="standCount"></param>
        /// <param name="noShields"></param>
        /// <returns></returns>
        private static double[] CalculateDefenseBuffedForHalfAttacks<T>(T model, List<int> defenseModificationValues, int standCount,
            bool noShields = false) where T : ConquestInput<T>
        {
            var normalDefenseOutput = CalculateDefense(model, defenseModificationValues, standCount, noShields);

            model.Defense++;
            var buffedDefenseOutput = CalculateDefense(model, defenseModificationValues, standCount, noShields);
            model.Defense--;

            var diffDefense = buffedDefenseOutput[0] - normalDefenseOutput[0];
            var diffResolve = buffedDefenseOutput[1] - normalDefenseOutput[1];

            diffDefense /= 2;
            diffResolve /= 2;

            normalDefenseOutput[0] += diffDefense;
            normalDefenseOutput[1] += diffResolve;

            return normalDefenseOutput;
        }

        /// <summary>
        /// if buff defense or evasion is true, we need to run both +1D and +1E and return whichever is the highest overall output
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="defenseModificationValues"></param>
        /// <param name="standCount"></param>
        /// <param name="noShields"></param>
        /// <returns></returns>
        private static double[] CalculateDefenseBuffed<T>(T model, List<int> defenseModificationValues, int standCount,
            bool noShields = false) where T : ConquestInput<T>
        {
            model.Defense++;
            var buffedDefense = CalculateDefense(model, defenseModificationValues, standCount, noShields);

            model.Defense--;
            model.Evasion++;
            var buffedEvasion = CalculateDefense(model, defenseModificationValues, standCount, noShields);

            model.Evasion--;

            var defenseMod = (buffedDefense[0] + buffedDefense[1]) / 2;
            var evasionMod = (buffedEvasion[0] + buffedEvasion[1]) / 2;

            return defenseMod > evasionMod ? buffedDefense : buffedEvasion;
        }

        /// <summary>
        /// Basic defense calculation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="defenseModificationValues"></param>
        /// <param name="standCount"></param>
        /// <param name="noShields"></param>
        /// <returns></returns>
        private static double[] CalculateDefense<T>(T model, List<int> defenseModificationValues,
            int standCount, bool noShields = false) where T : ConquestInput<T>
        {
            var returnOutput = new[] { 0.0d, 0.0d };
            var defense = model.Defense;
            if (model.IsBastion == 1) defense++;
            if (!noShields && model.IsShields == 1) defense++;

            var copyQ = GetResolveWoundArray(model.Resolve, standCount, (model.Wounds * model.Models) / standCount);
            var mainQ = new Queue<Tuple<int, int>>();
            double defenseScore = 0;
            double defenseResolve = 0;

            foreach (var defenseMod in defenseModificationValues)
            {
                //transfer everything from copyStack to mainStack
                while (copyQ.Count > 0)
                {
                    mainQ.Enqueue(copyQ.Dequeue());
                }

                //queue contains tuples in the form of [resolve tier, wounds in tier]
                //queue should be organized from highest resolve down to its base resolve
                while (mainQ.Count > 0)
                {
                    //looking at based off of the current defense Modification value and the current clash/volley score
                    //how many hits does it take to reduce this output object to 0 wounds?
                    //afterwards calculate the mean of all of these

                    var output = mainQ.Dequeue();
                    var analysisDefense = Math.Clamp(defense - defenseMod, 0, 6);

                    //evasion score is not modified by anything so use it if the modified defense is now lower
                    if (analysisDefense < model.Evasion) analysisDefense = model.Evasion;

                    var defenseProbability = 0.0;

                    if (model.IsBlessed == 1)
                    {
                        //with blessed, you get to reroll failed defense rolls OR attacks.  To add to output we get half the offense and half the defense
                        //example, if probability = 0.5 (50%) then with full reroll you get .5 * .5 = .25 and then add .5 + .25 for .75
                        //however to half it we would say .25/2 (.125) and add that to .5 for .625
                        var probabilityModWithFullReRoll = Probabilities[analysisDefense] * Probabilities[analysisDefense];
                        defenseProbability = (probabilityModWithFullReRoll / 2) + Probabilities[analysisDefense] + 1.0d;

                        if (model.Reroll6_Defense)
                        {
                            //you will do nothing because you cannot reroll a reroll, and this comment is here to cement that
                        }
                    }
                    else
                    {
                        defenseProbability = Probabilities[analysisDefense] + 1.0d; //so a [2] is 0.33 and probability would then be 1.33    
                        if (model.Reroll6_Defense)
                        {
                            //save% + (save% * 0.167) 
                            //for test - 12 attacks hit with defense 2 (33%)
                            //4 save
                            //2 of those would be a 6
                            //2 rerolls, 0.67 save so a total of 4.67 save out of 12 (38.91)
                            //%save (0.33) + (%save 0.33 * 0.167) = 38.511 - close enough

                            defenseProbability = Probabilities[analysisDefense] + (Probabilities[analysisDefense] * Probabilities[1]) + 1.0d;
                        }
                    }

                    //defense score means that the def probability (how many hits it takes on average to drop one model) is multiplied by
                    //how many wounds have to be chewed through with just pure hits
                    //ex 12 wounds with a D score of 2 is 1.33 hits to drop one model or 15.96 hits on average to destroy the unit

                    //healing is tricky.  We're going to just add it to each tier of models
                    var hits = defenseProbability * (output.Item2 + model.Healing); //keeping this separate because this value is needed below

                    defenseScore += hits;

                    //now how many hits does it take when you take into account the resolve score?
                    var resolveFailureProbability = 1 - Probabilities[output.Item1];

                    //so if defense score was 15.96 and they have a resolve of 2, that means 0.67 models will run for every hit (as 0.33 will stay)
                    //so it takes much less than 15.96 hits to run the unit off, it would be 15.96 - (6 * 0.66) or 12 (15.96 - 3.96)
                    defenseResolve += (hits - (6 * resolveFailureProbability)); //6 represents d6

                    copyQ.Enqueue(output); //put the item back in the copyQ so that we can do this all over again
                }
            }

            //get the average defense score and then the average defense resolve
            defenseScore /= (defenseModificationValues.Count);
            defenseResolve /= (defenseModificationValues.Count);

            //now get the mean value between those two values
            returnOutput[0] = defenseScore;
            returnOutput[1] = defenseResolve;
            return returnOutput;
        }

        private static int CalculateResolveBonus(int standCount)
        {
            if (standCount <= 3) return 0;
            if (standCount <= 6) return 1;
            if (standCount <= 9) return 2;
            return 3;
        }

        /// <summary>
        /// Returns a queue of tuples in the form of [resolve tier, wounds in tier] ordered from highest resolve down
        /// </summary>
        /// <param name="baseResolve"></param>
        /// <param name="standCount"></param>
        /// <param name="standWounds"></param>
        /// <returns></returns>
        private static Queue<Tuple<int, int>> GetResolveWoundArray(int baseResolve, int standCount, int standWounds)
        {
            //item1 = how many wounds are in that tier of resolve (it counts as a resolve score)
            //item2 = how many wounds you have to chew through to get through that tier
            var queue = new Queue<Tuple<int, int>>();
            while (standCount > 0)
            {
                var resolve = Math.Clamp(baseResolve + CalculateResolveBonus(standCount), 1, 6);
                var conditionReached = false;
                var stands = 0;
                while (!conditionReached)
                {
                    standCount--;
                    stands++;
                    if (standCount == 0 || standCount % 3 == 0) conditionReached = true;
                }

                var metric = new Tuple<int, int>(resolve, stands * standWounds);
                queue.Enqueue(metric);
            }

            return queue;
        }
    }
}
