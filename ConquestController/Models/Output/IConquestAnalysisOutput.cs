using System.Collections.Generic;

namespace ConquestController.Models.Output
{
    public interface IConquestAnalysisOutput
    {
        double OffenseOutput { get; }
        double DefenseOutput { get; }
        double TotalOutput { get; }
        int Points { get; set; }
        bool HasNoImpactOptionAdded { get; set; }
        bool HasOptionAdded { get; set; }
        string Unit { get; set; }
        IList<IConquestAnalysisOutput> UpgradeOutputModifications { get; }
        Stand[] Stands { get; }
        AnalysisOutput Analysis { get; }
        AnalysisSummary Summary { get; }


        string PublishToCommaFormat();
        void CreateOutputData();
    }
}
