namespace ConquestController.Models.Input
{
    public class MasteryModel : IOption
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public int Points { get; set; }
        public int SelfOnly { get; set; }

        public string Faction { get; set; }
        public string Category { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
