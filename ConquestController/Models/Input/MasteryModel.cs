namespace ConquestController.Models.Input
{
    public class MasteryModel : IMastery
    {
        public string Name { get; set; }
        public string Tag { get; set; }

        /// <summary>
        /// Overall point cost (the more you spam the same mastery the more expensive it gets
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// What it originally cost
        /// </summary>
        public int BasePoints { get; set; } 
        public int SelfOnly { get; set; }

        public string Faction { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }
        public string Restrictions { get; set; }
        public int WarlordOnly { get; set; }

        /// <summary>
        /// returns a MasteryModel
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new MasteryModel
            {
                Name = this.Name,
                Tag = this.Tag,
                Points = this.Points,
                BasePoints = this.BasePoints,
                SelfOnly = this.SelfOnly,
                Faction = this.Faction,
                Category = this.Category,
                Notes = this.Notes,
                Restrictions = this.Restrictions,
                WarlordOnly = this.WarlordOnly
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
