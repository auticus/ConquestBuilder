using ConquestController.Analysis.Components;
using ConquestUnitTests.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ConquestUnitTests
{
    public class ComponentTests
    {
        [Fact]
        public void ResolveTest()
        {
            var allResolve = new List<int>() { 2 };
            var expectedAnswer = 6.667;
            var hits = 10;
            var isTerrifying = false;

            var meanResolve = TestingComponent.TestMeanResolve(hits, allResolve, isTerrifying);

            Assert.True(meanResolve == expectedAnswer);
        }
    }
}
