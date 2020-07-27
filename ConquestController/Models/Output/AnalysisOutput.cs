namespace ConquestController.Models.Output
{
    public class AnalysisOutput
    {
        public double DeviationScore { get; set; }
        public double DeviationScorePercent { get; set; }
        public double DeviationOffenseEfficiency { get; set; }
        public double DeviationOffenseEfficiencyPercent { get; set; }

        public double DeviationDefenseEfficiency { get; set; }
        public double DeviationDefenseEfficiencyPercent { get; set; }

        public double DeviationEfficiency { get; set; }
        public double DeviationEfficiencyPercent { get; set; }
    }
}
