namespace ConquestController.Models.Input
{
    public class SpellModel : IOption
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int Points { get; set; }
        public int SelfOnly { get; set; }
        public string Faction { get; set; }
        public string School { get; set; }
        public int Range { get; set; }
        public int Difficulty { get; set; }
        public int IsScaling { get; set; }
        public int HitsCaused { get; set; }
        public string EffectCausedPer { get; set; }
        public string Effect { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
