namespace ConquestController.Models.Input
{
    public class SpellModel : ISpell
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int Points { get; set; }
        public int SelfOnly { get; set; }
        public string Faction { get; set; }
        public string Category { get; set; }
        public int Range { get; set; }
        public int Difficulty { get; set; }
        public int IsScaling { get; set; }
        public int HitsCaused { get; set; }
        public string EffectCausedPer { get; set; }
        public int WarlordOnly { get; set; }
        public string Notes { get; set; }
        public bool LearnedInTheOccult { get; set; }

        /// <summary>
        /// Returns a SpellModel
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new SpellModel
            {
                Category = this.Category,
                Difficulty = this.Difficulty,
                EffectCausedPer = this.EffectCausedPer,
                Faction = this.Faction,
                HitsCaused = this.HitsCaused,
                IsScaling = this.IsScaling,
                Name = this.Name,
                Notes = this.Notes,
                Points = this.Points,
                Range = this.Range,
                Tag = this.Tag,
                SelfOnly = this.SelfOnly,
                WarlordOnly = this.WarlordOnly,
                LearnedInTheOccult = this.LearnedInTheOccult
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
