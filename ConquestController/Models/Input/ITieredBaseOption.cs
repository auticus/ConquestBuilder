namespace ConquestController.Models.Input
{
    public interface ITieredBaseOption: IBaseOption
    {
        int Tier { get; set; }
    }
}
