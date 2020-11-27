using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConquestController.Models.Input
{
    public interface IConquestGameElementOption : IConquestBaseInput
    {
        List<IConquestBaseInput> Options { get; }
        ObservableCollection<IOption> ActiveOptions { get; }
        int AdditionalPoints { get; set; }
        int LeaderPoints { get; set; }
        int StandardPoints { get; set; }
    }
}
