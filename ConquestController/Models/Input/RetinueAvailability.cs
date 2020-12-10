using System.Collections.Generic;

namespace ConquestController.Models.Input
{
    public class RetinueAvailability
    {
        public enum Availability
        {
            NotAvailable = 0,
            Restricted,
            Available
        }

        public Dictionary<string, Availability> RetinueAvailabilities { get; }

        public RetinueAvailability()
        {
            RetinueAvailabilities = new Dictionary<string, Availability>();
        }
    }
}
