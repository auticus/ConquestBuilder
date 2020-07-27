namespace ConquestController.Models.Output
{
    public class RangedOutput
    {
        public int DefenseValue { get; set; }
        public double ObscureHits { get; set; }
        public double ObscureAimedHits { get; set; }
        public double FullHits { get; set; }
        public double FullAimedHits { get; set; }

        public double SumOfOutput()
        {
            return ObscureHits + ObscureAimedHits + FullHits + FullAimedHits;
        }
    }
}
