namespace ConquestController.Models.Input
{
    public class RetinueModel : IOption
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int Points { get; set; }
        public int SelfOnly { get; set; }

        public string Faction { get; set; }
        public string NotAllowed { get; set; }
        public int Tier { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }
        public int WarlordOnly { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
