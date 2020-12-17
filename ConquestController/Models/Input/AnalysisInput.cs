using ConquestController.Models.Output;

namespace ConquestController.Models.Input
{
    public class AnalysisInput
    {
        public IConquestGamePiece Model { get; set; }
        public IConquestAnalysisOutput BaselineOutput { get; set; }
        public IBaseOption BaseOption { get; set; }
        public ISpell Spell { get; set; }
        public int AnalysisStandCount { get; set; }
        public int FrontageCount { get; set; }
        public bool ApplyFullyDeadly { get; set; }
    }
}