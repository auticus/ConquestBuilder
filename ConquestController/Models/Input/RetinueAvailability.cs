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
        public Availability Tactical { get; set; }
        public Availability Combat { get; set; }
        public Availability Magic { get; set; }
    }
}
