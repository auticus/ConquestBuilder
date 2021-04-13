namespace ConquestController.Models.Input
{
    public interface ISpell :IBaseOption
    {
        /// <summary>
        /// If TRUE means this spell was taken by the Learned In the Occult Mastery
        /// </summary>
        bool LearnedInTheOccult { get; set; }
        int Range { get; set; }
        int HitsCaused { get; set; }
        int Difficulty { get; set; }

    }
}
