using ConquestController.Models.Output;

namespace ConquestController.Models.Input
{
    public class AnalysisInput<T> where T: ConquestInput<T>
    {
        public ConquestInput<T> Model { get; set; }
        public ConquestUnitOutput BaselineOutput { get; set; }
        public UnitOptionModel Option { get; set; }
        public SpellModel Spell { get; set; }
        public int AnalysisStandCount { get; set; }
        public int FrontageCount { get; set; }
        public bool ApplyFullyDeadly { get; set; }
    }
}