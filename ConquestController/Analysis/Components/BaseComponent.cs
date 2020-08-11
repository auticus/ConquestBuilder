using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace ConquestController.Analysis.Components
{
    public abstract class BaseComponent
    {
        protected static int AuraDeathStands = 3; //the number of stands we are testing against
        protected static double DeadlyShotBladesDmg = 2.0;
        protected static double HalvedDeadlyShotBladesDmg = 1.5;

        //the 5th and 6th element are the same because a 6 always fails so that chance of failure exists even for that
        //the first element (0) is there to have a 0 element as this is used one-based, and a 0 does have a 0% chance.
        //The last element (7th) exists so that any +1s will also reflect a 6 result
        protected static readonly double[] Probabilities = {
            0.0, 0.1667, 0.3333, 0.5, 0.6667, 0.8333, 0.8333, 0.8333
        };

        protected static double IsFluidWeight = 1.5;
        protected static double IsFlyWeight = 2.0;
        protected static double IsFluidFlyWeight = 2.5;
        protected static double RangeModifier = 0.01; //for every inch of range on a ranged weapon add 1% to its output score
        protected static double ArcOfFireMultiplier = 2.0;

        protected static double CalculateMeanResolveFailures(double hits, List<int> resolveScores, bool isTerrifying)
        {
            //hits = 10, resolve = 2... that means that 3.33 will succeed and 6.66 will fail
            if (resolveScores.Count == 0) return 0;

            var total = 0.0d;
            foreach (var resolve in resolveScores)
            {
                var successes = isTerrifying ? hits * Probabilities[Math.Clamp(resolve - 1, 1, 6)] : hits * Probabilities[resolve];
                total += successes;
            }

            return total / resolveScores.Count;
        }
    }
}
