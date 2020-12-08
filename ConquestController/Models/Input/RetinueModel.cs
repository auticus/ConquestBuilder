namespace ConquestController.Models.Input
{
    public class RetinueModel : IRetinue
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int Points { get; set; }
        public int SelfOnly { get; set; }

        public string Faction { get; set; }
        public int Tier { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }
        public int WarlordOnly { get; set; }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns a RetinueModel
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new RetinueModel
            {
                Category = this.Category,
                Faction = this.Faction,
                Name = this.Name,
                Notes = this.Notes,
                Points = this.Points,
                SelfOnly = this.SelfOnly,
                Tag = this.Tag,
                WarlordOnly = this.WarlordOnly,
                Tier = this.Tier
            };
        }
}
}
