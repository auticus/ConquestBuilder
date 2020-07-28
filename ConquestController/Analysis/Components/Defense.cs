using System;
using System.Collections.Generic;
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
        public static double[] CalculateOutput(UnitInputModel model, List<int> defenseModificationValues, 
            int standCount, bool noShields = false)
        {
            var returnOutput = new[] {0.0d, 0.0d};
            var defense = model.Defense;
            if (model.IsBastion == 1) defense++;
            if (!noShields && model.IsShields == 1) defense++;

            var copyQ = GetResolveWoundArray(model.Resolve, standCount, (model.Wounds * model.Models)/standCount);
            var mainQ = new Queue<Tuple<int, int>>();
            double defenseScore = 0;
            double defenseResolve = 0;

            foreach (var defenseMod in defenseModificationValues)
            {
                //transfer everything from copyStack to mainStack
                while(copyQ.Count > 0)
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
                    var defenseProbability = Probabilities[analysisDefense] + 1.0d; //so a [2] is 0.33 and probability would then be 1.33
                    
                    //defense score means that the def probability (how many hits it takes on average to drop one model) is multiplied by
                    //how many wounds have to be chewed through with just pure hits
                    //ex 12 wounds with a D score of 2 is 1.33 hits to drop one model or 15.96 hits on average to destroy the unit
                    
                    var tally = defenseProbability * output.Item2; //keeping this separate because this value is needed below
                    defenseScore += tally;

                    //now how many hits does it take when you take into account the resolve score?
                    var resolveFailureProbability = 1 - Probabilities[output.Item1];

                    //so if defense score was 15.96 and they have a resolve of 2, that means 0.67 models will run for every hit (as 0.33 will stay)
                    //so it takes much less than 15.96 hits to run the unit off, it would be 15.96 - (6 * 0.66) or 12 (15.96 - 3.96)
                    defenseResolve += (tally - (6 * resolveFailureProbability)); //6 represents d6

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
