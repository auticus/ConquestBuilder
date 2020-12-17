namespace ConquestController.Models.Input
{
    public interface IMastery: IBaseOption
    {
        /// <summary>
        /// What it originally cost at base value
        /// </summary>
        int BasePoints { get; set; }
        string Restrictions { get; set; }
    }
}
