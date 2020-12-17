namespace ConquestController.Models.Input
{
    /// <summary>
    /// Represents magic items, heirlooms, etc
    /// </summary>
    public class ItemModel : IBaseOption
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

        /// <summary>
        /// Returns an ItemModel
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new ItemModel
            {
                Category = this.Category,
                Faction = this.Faction,
                Name = this.Name,
                Notes = this.Notes,
                Points = this.Points,
                Tag = this.Tag,
                SelfOnly = this.SelfOnly,
                WarlordOnly = this.WarlordOnly,
                Restrictions = this.Restrictions
            };
        }

        public override string ToString()
        {
            return $"[{Category}] - {Name}";
        }
    }
}
