using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class PropertyLeaseRenewalOfferAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        public int? PropertyLeaseApplicationId { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Months Offer")]
        public int MonthsOffer { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Accepted")]
        public bool? IsAccepted { get; set; }

        // Proposed dates
        public DateTime? ProposedEndDate { get; set; }
        public DateTime? ProposedRenewalNotice { get; set; }
        public DateTime? ProposedTerminationNotice { get; set; }

        // CSO trail
        public string CSO_Outcome { get; set; }
        public string CSO_Comment { get; set; }
        public DateTime? CSO_Date { get; set; }
        public int? CSO_SystemUserId { get; set; }

        // RM trail
        public string RM_Outcome { get; set; }
        public string RM_Comment { get; set; }
        public DateTime? RM_Date { get; set; }
        public int? RM_SystemUserId { get; set; }

        // CEO trail
        public string CEO_Outcome { get; set; }
        public string CEO_Comment { get; set; }
        public DateTime? CEO_Date { get; set; }
        public int? CEO_SystemUserId { get; set; }

        // Customer response
        public string CustomerDeclineReason { get; set; }
        public DateTime? CustomerResponseDate { get; set; }
    }
}