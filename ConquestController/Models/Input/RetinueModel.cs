namespace ConquestController.Models.Input
{
    public class RetinueModel : ITieredBaseOption
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
            return $"[{Category} Retinue] {Name}";
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

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if ((obj is RetinueModel) == false) return false;

            var retinue = (RetinueModel) obj;

            return (retinue.Category == this.Category &&
                    retinue.Tag == this.Tag &&
                    retinue.Points == this.Points &&
                    retinue.Name == this.Name &&
                    retinue.Tier == this.Tier);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
