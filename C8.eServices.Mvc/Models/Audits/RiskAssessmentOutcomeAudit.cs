using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RiskAssessmentOutcomeAudit: BaseModelAudit
    {

        [Column(Order = 10)]
        [Display(Name = "Surname")]
        public string LastName { get; set; } // CSO Surname

        [Column(Order = 11)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } // CSO Name

        [Column(Order = 12)]
        [Display(Name = "Official Number")]
        public string OfficialNumber { get; set; } // CSO Official No

        [Column(Order = 13)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; } // "Recommended" or "Not Recommended"

        [Column(Order = 14)]
        [Display(Name = "Reason")]
        public string Reason { get; set; } // CSO Comments

        [Column(Order = 15)]
        [Display(Name = "Supporting Doccuments")]
        public string SupportingDoccuments { get; set; } // Changed to string (path) or File depending on your handler

        [DataType(DataType.Date)]
        [Column(Order = 16)]
        [Display(Name = "Date Stamp")]
        public DateTime? DateStamp { get; set; } // Date CSO submitted

        // ==========================================
        // TIER 2: REVENUE MANAGER (New)
        // ==========================================

        [Display(Name = "RM Official Number")]
        public string RM_OfficialNumber { get; set; }

        [Display(Name = "RM Outcome")]
        public string RM_Outcome { get; set; } // "Supported" or "Not Supported"

        [Display(Name = "RM Reason")]
        public string RM_Reason { get; set; }

        [Display(Name = "RM Date")]
        public DateTime? RM_DateStamp { get; set; }

        [Display(Name = "RM User")]
        public int? RM_SystemUserId { get; set; }
        [ForeignKey("RM_SystemUserId")]
        public SystemUser RM_SystemUser { get; set; }

        // ==========================================
        // TIER 3: CEO (New)
        // ==========================================

        [Display(Name = "CEO Official Number")]
        public string CEO_OfficialNumber { get; set; }

        [Display(Name = "CEO Outcome")]
        public string CEO_Outcome { get; set; } // "Approved" or "Rejected"

        [Display(Name = "CEO Reason")]
        public string CEO_Reason { get; set; }

        [Display(Name = "CEO Approval Date")]
        public DateTime? CEO_DateStamp { get; set; }

        [Display(Name = "CEO User")]
        public int? CEO_SystemUserId { get; set; }
        [ForeignKey("CEO_SystemUserId")]
        public SystemUser CEO_SystemUser { get; set; }

        public string SignatureBlob { get; set; } // Stores the Base64 signature string

        // ==========================================
        // LINKING FIELDS (Existing)
        // ==========================================

        public int? Lease_ID { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Tenant Lease Application")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Captured by")]
        public int? CapturedById { get; set; }
        [ForeignKey("CapturedById")]
        public SystemUser CapturedBy { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
    }
}