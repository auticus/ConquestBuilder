namespace ConquestController.Models.Output
{
    public interface IConquestAnalysisOutput
    {
        double OffenseOutput { get; }
        double DefenseOutput { get; }
        double TotalOutput { get; }
    }
}
