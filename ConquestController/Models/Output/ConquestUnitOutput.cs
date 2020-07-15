using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models.Output
{
    public class ConquestUnitOutput
    {
        #region Sub Classes
        public class Stand
        {
            public class OffenseScore
            {
                public double ClashOutput { get; set; }
                public double ImpactOutput { get; set; }
                public double RangedOutput { get; set; }
                public double MagicOutput { get; set; }
                public double TotalOutput
                {
                    get
                    {
                        return ClashOutput + ImpactOutput + RangedOutput + MagicOutput;
                    }
                }

                public double NormalizedOutput { get; set; }
                public double Efficiency { get; set; } //output / point cost
            }

            public class DefenseScore
            {
                /// <summary>
                /// The defensive output 
                /// </summary>
                public double RawOutput { get; set; }

                /// <summary>
                /// The defensive output counting resolve failures as well
                /// </summary>
                public double ResolveOutput { get; set; }

                /// <summary>
                /// The mean between raw defensive output and the resolve output
                /// </summary>
                public double TotalOutput
                {
                    get
                    {
                        return (RawOutput + ResolveOutput) / 2;
                    }
                }

                public double Efficiency { get; set; }
                public double NormalizedOutput { get; set; }
            }

            public class MovementScore
            {
                public int MovementRaw { get; set; }
                public double NormalizedMovement { get; set; }
            }

            public OffenseScore Offense { get; }
            public DefenseScore Defense { get; }
            public MovementScore Movement { get; }

            public Stand()
            {
                Offense = new OffenseScore();
                Defense = new DefenseScore();
                Movement = new MovementScore();
            }
        }
        #endregion Sub Classes

        #region Constant Values
        //region to index the normalization vectors
        public const int OFFENSE = 0;
        public const int DEFENSE = 1;
        public const int MOVEMENT = 2;

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
        /// The normalization scores used to even out the raw scores
        /// </summary>
        public double[] OffensiveNormalizationVector { get; set; } //what we multiply offensive scores to even them out

        public Stand[] Stands { get; }
        public ConquestUnitOutput()
        {
            OffensiveNormalizationVector = new double[] { 0, 0, 0 };
            var standfull = new Stand();
            var stand_extra_1 = new Stand();
            var stand_extra_2 = new Stand();
            var stand_extra_3 = new Stand();
            var stand_support_1 = new Stand();
            var stand_support_2 = new Stand();
            var stand_support_3 = new Stand();

            Stands = new Stand[] { standfull, stand_extra_1, stand_extra_2, stand_extra_3, stand_support_1, stand_support_2, stand_support_3 };
        }
    }
}
