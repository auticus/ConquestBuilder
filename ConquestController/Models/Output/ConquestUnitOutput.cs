using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models.Output
{
    public class ConquestUnitOutput
    {
        #region Constant Values
        public const int BASE_STAND_COUNT = 3;

        //stand array indexes
        public const int FULL_OUTPUT = 0;
        public const int FULL_EXTRA_OUTPUT_4_6 = 1;  //stand getting its full attacks alongside the base 3
        public const int FULL_EXTRA_OUTPUT_7_9 = 2;
        public const int FULL_EXTRA_OUTPUT_10 = 3;
        public const int SUPPORT_EXTRA_OUTPUT_4_6 = 4;
        public const int SUPPORT_EXTRA_OUTPUT_7_9 = 5;
        public const int SUPPORT_EXTRA_OUTPUT_10 = 6;
        #endregion

        public string Faction { get; set; }
        public string Unit { get; set; }

        public string Weight { get; set; }
        public int Points { get; set; }

        /// <summary>
        /// How many total stands in the regiment
        /// </summary>
        public int StandCount { get; set; }

        /// <summary>
        /// How many stands make up this regiment's front?
        /// </summary>
        public int FrontageCount { get; set; }

        /// <summary>
        /// Points spent for additional stands
        /// </summary>
        public int PointsAdditional { get; set; }

        public Stand[] Stands { get; }
        public AnalysisOutput Analysis { get; }
        public AnalysisSummary Summary { get; }
        public ConquestUnitOutput()
        {
            var standFull = new Stand();
            var standExtra1 = new Stand();
            var standExtra2 = new Stand();
            var standExtra3 = new Stand();
            var standSupport1 = new Stand();
            var standSupport2 = new Stand();
            var standSupport3 = new Stand();

            Analysis = new AnalysisOutput();
            Summary = new AnalysisSummary();

            Stands = new[] { standFull, standExtra1, standExtra2, standExtra3, standSupport1, standSupport2, standSupport3 };
        }

        /// <summary>
        /// Apply all of the score given to every stand within the output model
        /// </summary>
        /// <param name="rawMovement"></param>
        /// <param name="normalizedMovement"></param>
        public void ApplyMovementScoresToAllStands(int rawMovement, double normalizedMovement)
        {
            foreach (var stand in Stands)
            {
                stand.Movement.MovementRaw = rawMovement;
                stand.Movement.NormalizedMovement = normalizedMovement;
            }
        }

        public string FullStandToCommaFormat()
        {
            var full = Stands[FULL_OUTPUT];

            return string.Join(",", Faction, Unit, Weight, Points, PointsAdditional, full.Movement.NormalizedMovement,
                full.Offense.NormalizedClashOutput, full.Offense.NormalizedRangedOutput,
                full.Offense.NormalizedTotalOutput,
                full.Defense.TotalOutput, full.OutputScore, full.Offense.Efficiency, full.Defense.Efficiency,
                full.Efficiency);
        }
    }
}

