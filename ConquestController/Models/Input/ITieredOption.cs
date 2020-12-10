namespace ConquestController.Models.Input
{
    public interface ITieredOption: IOption
    {
        int Tier { get; set; }
    }
}
