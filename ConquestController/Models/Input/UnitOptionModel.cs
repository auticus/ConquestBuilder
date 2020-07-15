namespace ConquestController.Models.Input
{
    public class UnitOptionModel : IConquestInput
    { 
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Upgrade { get; set; }
        public int Points { get; set; }
        public string Tag { get; set; }
        public int ArmyLimit { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return Upgrade;
        }
    }
}
