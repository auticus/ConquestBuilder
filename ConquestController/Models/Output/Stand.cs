namespace ConquestController.Models.Output
{
    public class Stand
    {
        public class OffenseScore
        {
            public double ClashOutput { get; set; }
            public double ImpactOutput { get; set; }
            public double RangedOutput { get; set; }
            public double MagicOutput { get; set; }
            public double TotalOutput => ClashOutput + ImpactOutput + RangedOutput + MagicOutput;

            public double NormalizedClashOutput => ClashOutput * NormalizedVector;
            public double NormalizedRangedOutput => RangedOutput * NormalizedVector;
            public double NormalizedImpactOutput => ImpactOutput * NormalizedVector;
            public double NormalizedMagicOutput => MagicOutput * NormalizedVector;

            public double NormalizedTotalOutput => NormalizedClashOutput + NormalizedRangedOutput +
                                                   NormalizedImpactOutput + NormalizedMagicOutput;

            public double NormalizedVector { get; set; }
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
            public double TotalOutput => (RawOutput + ResolveOutput) / 2;

            public double Efficiency { get; set; }
        }

        public class MovementScore
        {
            public int MovementRaw { get; set; }
            public double NormalizedMovement { get; set; }
        }

        public OffenseScore Offense { get; }
        public DefenseScore Defense { get; }
        public MovementScore Movement { get; }

        public double Efficiency { get; set; }

        public double OutputScore =>
            Offense.NormalizedTotalOutput + Defense.TotalOutput + Movement.NormalizedMovement;

        public Stand()
        {
            Offense = new OffenseScore();
            Defense = new DefenseScore();
            Movement = new MovementScore();
        }
    }//end Stand class
}
