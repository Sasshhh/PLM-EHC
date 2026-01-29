using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class LeaseDetailsViewModel
    {
        public int PropertyLeaseApplicationId { get; set; }
        public string EkurhulreniEHCUnitId { get; set; }
        public int Id { get; set; }
        public string ReferenceNumber { get; set; }
        public string CompletionStatus { get; set; }
        public string StatusName { get; set; }
        public string TenantFullName { get; set; }
        public string Data { get; set; }
        public string TenantType { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime Date { get; set; }
        public int WarningLetterCount { get; set; }
    }
}