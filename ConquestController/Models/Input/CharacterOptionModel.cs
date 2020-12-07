namespace ConquestController.Models.Input
{
    public class CharacterOptionModel : IConquestBase
    {
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Upgrade { get; set; }
        public int Points { get; set; }
        public string Tag { get; set; }
        public int SelfOnly { get; set; }
        public int ArmyLimit { get; set; }
        public int IsRegimentUpgrade { get; set; } //means if taken, they need to apply it to a regiment (like dweghom mnemancer upgrade)
    }
}
