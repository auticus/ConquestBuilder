namespace ConquestController.Models.Output
{
    /// <summary>
    /// Class that represents what gets written out in file format
    /// </summary>
    public sealed class AnalysisFileOutput
    {
        public double ClashOffense { get; set; }
        public double RangedOffense { get; set; }
        public double NormalizedOffense { get; set; }
        public double TotalDefense { get; set; }
        public double OutputScore { get; set; }
        public double OffenseEfficiency { get; set; }
        public double DefenseEfficiency { get; set; }
        public double Efficiency { get; set; }
    }
}
