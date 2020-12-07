namespace ConquestController.Models.Input
{
    public class UnitOptionModel : IConquestBase, IOption
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

        /// <summary>
        /// returns a UnitOptionModel
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new UnitOptionModel
            {
                ArmyLimit = this.ArmyLimit,
                Category = this.Category,
                Faction = this.Faction,
                Name = this.Name,
                Notes = this.Notes,
                Points = this.Points,
                SelfOnly = this.SelfOnly,
                Tag = this.Tag,
                Unit = this.Unit,
                WarlordOnly = this.WarlordOnly
            };
        }
    }
}
