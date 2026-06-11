using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PropertyLeaseRenewalOffer: BaseModel
    {
        [Column(Order = 10)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        [Display(Name = "Property Lease Application")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Months Offer")]
        public int MonthsOffer { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Accepted")]
        public bool? IsAccepted { get; set; }

        // ── Proposed new lease dates (calculated at CSO step, NOT yet applied to LeaseDetails) ──
        [Column(Order = 13)]
        [Display(Name = "Proposed End Date")]
        public DateTime? ProposedEndDate { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Proposed Renewal Notice Date")]
        public DateTime? ProposedRenewalNotice { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Proposed Termination Notice Date")]
        public DateTime? ProposedTerminationNotice { get; set; }

        // ── CSO decision trail ──
        [Column(Order = 16)]
        [Display(Name = "CSO Outcome")]
        public string CSO_Outcome { get; set; }   // e.g. "Renew for 12 months" / "Renew for 24 months" / "Not Renew"

        [Column(Order = 17)]
        [Display(Name = "CSO Comment")]
        public string CSO_Comment { get; set; }

        [Column(Order = 18)]
        [Display(Name = "CSO Decision Date")]
        public DateTime? CSO_Date { get; set; }

        [Column(Order = 19)]
        [Display(Name = "CSO User")]
        public int? CSO_SystemUserId { get; set; }
        [ForeignKey("CSO_SystemUserId")]
        public SystemUser CSO_SystemUser { get; set; }

        // ── Revenue Manager decision trail ──
        [Column(Order = 20)]
        [Display(Name = "RM Outcome")]
        public string RM_Outcome { get; set; }    // "Approved" / "Rejected"

        [Column(Order = 21)]
        [Display(Name = "RM Comment")]
        public string RM_Comment { get; set; }

        [Column(Order = 22)]
        [Display(Name = "RM Decision Date")]
        public DateTime? RM_Date { get; set; }

        [Column(Order = 23)]
        [Display(Name = "RM User")]
        public int? RM_SystemUserId { get; set; }
        [ForeignKey("RM_SystemUserId")]
        public SystemUser RM_SystemUser { get; set; }

        // ── Director/CEO decision trail ──
        [Column(Order = 24)]
        [Display(Name = "CEO Outcome")]
        public string CEO_Outcome { get; set; }   // "Approved" / "Rejected"

        [Column(Order = 25)]
        [Display(Name = "CEO Comment")]
        public string CEO_Comment { get; set; }

        [Column(Order = 26)]
        [Display(Name = "CEO Decision Date")]
        public DateTime? CEO_Date { get; set; }

        [Column(Order = 27)]
        [Display(Name = "CEO User")]
        public int? CEO_SystemUserId { get; set; }
        [ForeignKey("CEO_SystemUserId")]
        public SystemUser CEO_SystemUser { get; set; }

        // ── Customer response ──
        [Column(Order = 28)]
        [Display(Name = "Customer Decline Reason")]
        public string CustomerDeclineReason { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Customer Response Date")]
        public DateTime? CustomerResponseDate { get; set; }
    }
}