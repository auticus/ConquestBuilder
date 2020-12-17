namespace ConquestController.Models.Input
{
    /// <summary>
    /// IPerkOptions are BaseOptions that also have a Perk field
    /// </summary>
    public interface IPerkOption: IBaseOption
    {
        public string Perk { get; set; }
    }
}