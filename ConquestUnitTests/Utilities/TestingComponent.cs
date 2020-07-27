using ConquestController.Analysis.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestUnitTests.Utilities
{
    public class TestingComponent : BaseComponent
    {
        public static double TestMeanResolve(int hits, List<int> resolveScores, bool isTerrifying)
        {
            return CalculateMeanResolveFailures(hits, resolveScores, isTerrifying);
        }
    }
}
