namespace ConquestController.Models.Input
{
    public interface IConquestBase
    {
        public string Faction { get; set; }
        public string Unit { get; set; } //game name of the unit 
    }
}
