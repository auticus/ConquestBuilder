namespace ConquestController.Models.Output
{
    public class AnalysisSummary
    {
        public double MeanScore { get; set; }
        public double MeanOffenseEfficiency { get; set; }
        public double MeanDefenseEfficiency { get; set; }
        public double MeanEfficiency { get; set; }

        public AnalysisSummary Copy()
        {
            var output = new AnalysisSummary()
            {
                MeanScore = this.MeanScore,
                MeanOffenseEfficiency =  this.MeanOffenseEfficiency,
                MeanDefenseEfficiency = this.MeanDefenseEfficiency,
                MeanEfficiency = this.MeanEfficiency
            };

            return output;
        }
    }
}
