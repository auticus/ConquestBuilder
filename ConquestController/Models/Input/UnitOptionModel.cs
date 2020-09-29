namespace ConquestController.Models.Input
{
    public class UnitOptionModel : IConquestInput, IOption
    { 
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public string Tag { get; set; }
        public int SelfOnly { get; set; }
        public int ArmyLimit { get; set; }
        public string Notes { get; set; }
        public int Grouping { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
