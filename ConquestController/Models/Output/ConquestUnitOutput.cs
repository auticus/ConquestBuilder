using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ConquestController.Models.Output
{
    public class ConquestUnitOutput : IConquestAnalysisOutput
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

        public Guid ID { get; private set; }
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

        /// <summary>
        /// This record is a modified core record with an option added to it
        /// </summary>
        public bool HasOptionAdded { get; set; }

        /// <summary>
        /// This record is a modified core record with an option that does nothing to output score added to it
        /// </summary>
        public bool HasNoImpactOptionAdded { get; set; }

        /// <summary>
        /// Has the model been released yet or is it still proxy
        /// </summary>
        public int IsReleased { get; set; }

        /// <summary>
        /// Baseline output is the model or character without any options or spells etc on it - the baseline
        /// </summary>
        public bool IsBaselineOutput { get; set; }

        public List<ConquestUnitOutput> UpgradeOutputModifications { get; set; }

        private AnalysisFileOutput AnalysisOutputData { get; set; }

        public Stand[] Stands { get; private set; }
        public AnalysisOutput Analysis { get; private set; }
        public AnalysisSummary Summary { get; private set; }

        #region Interface Implementation
        public double OffenseOutput => AnalysisOutputData.NormalizedOffense;
        public double DefenseOutput => AnalysisOutputData.TotalDefense;
        public double TotalOutput => AnalysisOutputData.OutputScore;
        #endregion

        #region Constructor
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
            UpgradeOutputModifications = new List<ConquestUnitOutput>();

            Stands = new[] { standFull, standExtra1, standExtra2, standExtra3, standSupport1, standSupport2, standSupport3 };
            ID = Guid.NewGuid();
        }
        #endregion Constructor

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

        /// <summary>
        /// Publishes the calculated scores to an internal data cache that is used to publish out to file format later
        /// </summary>
        public void CreateOutputData()
        {
            AnalysisOutputData = new AnalysisFileOutput(); //creating this here intentionally so that if extensions forget to call this method it will fire a null error

            var full = Stands[FULL_OUTPUT];
            AnalysisOutputData.ClashOffense = full.Offense.NormalizedClashOutput;
            AnalysisOutputData.RangedOffense = full.Offense.NormalizedRangedOutput;
            AnalysisOutputData.NormalizedOffense = full.Offense.NormalizedTotalOutput;
            AnalysisOutputData.TotalDefense = full.Defense.TotalOutput;
            AnalysisOutputData.OutputScore = full.OutputScore;
            AnalysisOutputData.OffenseEfficiency = full.Offense.Efficiency;
            AnalysisOutputData.DefenseEfficiency = full.Defense.Efficiency;
        }

        public string PublishToCommaFormat()
        {
            var full = Stands[FULL_OUTPUT];

            return string.Join(",", Faction, Unit, Weight, Points, PointsAdditional, full.Movement.NormalizedMovement,
                AnalysisOutputData.ClashOffense, AnalysisOutputData.RangedOffense, AnalysisOutputData.NormalizedOffense,
                AnalysisOutputData.TotalDefense,
                AnalysisOutputData.OutputScore, AnalysisOutputData.OffenseEfficiency, AnalysisOutputData.DefenseEfficiency,
                AnalysisOutputData.Efficiency,
                HasOptionAdded ? 1 : 0, HasNoImpactOptionAdded ? 1 : 0, IsReleased, IsBaselineOutput ? 1 : 0,  full.Offense.NormalizedVector);
        }

        public override string ToString()
        {
            return Unit;
        }

        public ConquestUnitOutput Copy()
        {
            var output = new ConquestUnitOutput
            {
                Faction = this.Faction,
                FrontageCount = this.FrontageCount,
                HasNoImpactOptionAdded = this.HasNoImpactOptionAdded,
                HasOptionAdded = this.HasOptionAdded,
                IsReleased = this.IsReleased,
                Points = this.Points,
                PointsAdditional = this.PointsAdditional,
                StandCount = this.StandCount,
                Unit = this.Unit,
                Weight = this.Weight,
                Summary = this.Summary.Copy(),
                Analysis = this.Analysis.Copy(),
                IsBaselineOutput = this.IsBaselineOutput,
                Stands = new[]{
                    Stands[FULL_OUTPUT].Copy(), Stands[FULL_EXTRA_OUTPUT_4_6].Copy(), Stands[FULL_EXTRA_OUTPUT_7_9].Copy(),
                    Stands[FULL_EXTRA_OUTPUT_10].Copy(), Stands[SUPPORT_EXTRA_OUTPUT_4_6].Copy(),
                    Stands[SUPPORT_EXTRA_OUTPUT_7_9].Copy(), Stands[SUPPORT_EXTRA_OUTPUT_10].Copy()
                },
                ID = this.ID
            };

            return output;
        }
    }
}

