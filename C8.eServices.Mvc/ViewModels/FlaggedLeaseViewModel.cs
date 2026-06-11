using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class FlaggedLeaseViewModel
    {
        public LeaseDetails Lease { get; set; }
        public string FlaggedReason { get; set; }
    }
}
