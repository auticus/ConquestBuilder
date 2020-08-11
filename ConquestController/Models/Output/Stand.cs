using ConquestController.Analysis.Components;

namespace ConquestController.Models.Output
{
    public class Stand
    {
        public class OffenseScore
        {
            public double ClashOutput { get; set; }
            public double ImpactOutput { get; set; }
            public double RangedOutput { get; set; }
            public double TotalOutput => ClashOutput + ImpactOutput + RangedOutput;

            public double NormalizedClashOutput => ClashOutput * NormalizedVector;
            public double NormalizedRangedOutput => RangedOutput * NormalizedVector;
            public double NormalizedImpactOutput => ImpactOutput * NormalizedVector;

            public double NormalizedTotalOutput => NormalizedClashOutput + NormalizedRangedOutput +
                                                   NormalizedImpactOutput;

            public double NormalizedVector { get; set; }
            public double Efficiency { get; set; } //output / point cost

            public OffenseScore Copy()
            {
                var output = new OffenseScore()
                {
                    ClashOutput = this.ClashOutput,
                    ImpactOutput = this.ImpactOutput,
                    RangedOutput = this.RangedOutput,
                    NormalizedVector = this.NormalizedVector,
                    Efficiency = this.Efficiency
                };

                return output;
            }
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

            public DefenseScore Copy()
            {
                var output = new DefenseScore()
                {
                    RawOutput = this.RawOutput,
                    ResolveOutput = this.ResolveOutput,
                    Efficiency = this.Efficiency
                };

                return output;
            }
        }

        public class MagicScore
        {
            public double Output { get; set; }
            public double Efficiency { get; set; }
            public double NormalizationVector { get; set; }
            public double NormalizationOutput => Output * NormalizationVector;

            public MagicScore Copy()
            {
                var output = new MagicScore()
                {
                    Output = this.Output,
                    Efficiency = this.Efficiency,
                    NormalizationVector = this.NormalizationVector
                };

                return output;
            }
        }

        public class MovementScore
        {
            public int MovementRaw { get; set; }
            public double NormalizedMovement { get; set; }

            public MovementScore Copy()
            {
                var output = new MovementScore()
                {
                    MovementRaw = this.MovementRaw,
                    NormalizedMovement = this.NormalizedMovement
                };

                return output;
            }
        }

        public OffenseScore Offense { get; private set; }
        public DefenseScore Defense { get; private set; }
        public MovementScore Movement { get; private set; }
        public MagicScore Magic { get; private set; }

        public double Efficiency { get; set; }

        public double OutputScore =>
            Offense.NormalizedTotalOutput + Defense.TotalOutput + Movement.NormalizedMovement + Magic.NormalizationOutput;

        public Stand()
        {
            Offense = new OffenseScore();
            Defense = new DefenseScore();
            Movement = new MovementScore();
            Magic = new MagicScore();
        }

        public Stand Copy()
        {
            var output = new Stand()
            {
                Efficiency = this.Efficiency
            };

            output.Offense = this.Offense.Copy();
            output.Defense = this.Defense.Copy();
            output.Movement = this.Movement.Copy();
            output.Magic = this.Magic.Copy();

            return output;
        }
    }//end Stand class
}
