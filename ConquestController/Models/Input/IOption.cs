namespace ConquestController.Models.Input
{
    public interface IOption
    {
        string Name { get; set; }
        string Tag { get; set; }
        int Points { get; set; }
        int SelfOnly { get; set; }
        int WarlordOnly { get; set; }
        string Notes { get; set; }
        string Category { get; set; }
    }
}
