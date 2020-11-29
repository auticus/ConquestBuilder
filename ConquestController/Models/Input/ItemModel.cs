namespace ConquestController.Models.Input
{
    /// <summary>
    /// Represents magic items, heirlooms, etc
    /// </summary>
    public class ItemModel : IOption
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int Points { get; set; }
        public int SelfOnly { get; set; }
        public int WarlordOnly { get; set; }

        public string Faction { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }
        public string Restrictions { get; set; }

        public override string ToString()
        {
            return $"[{Category}] - {Name}";
        }
    }
}
