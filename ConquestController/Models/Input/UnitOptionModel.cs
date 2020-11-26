namespace ConquestController.Models.Input
{
    public class UnitOptionModel : IConquestBaseInput, IOption
    { 
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }
        public string Tag { get; set; }
        public int SelfOnly { get; set; }
        public int ArmyLimit { get; set; }
        public string Notes { get; set; }
        public string Category { get; set; }
        public int WarlordOnly { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
