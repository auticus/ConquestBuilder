namespace ConquestController.Models.Input
{
    public interface IMastery: IOption
    {
        /// <summary>
        /// What it originally cost at base value
        /// </summary>
        int BasePoints { get; set; }
        string Faction { get; set; }
        string Restrictions { get; set; }
    }
}
