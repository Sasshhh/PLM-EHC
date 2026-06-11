using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    /// <summary>
    /// UC026 — Tracks lease disputes through Capture → Review → Resolve → Close lifecycle.
    /// </summary>
    [Table("LeaseDisputes")]
    public class LeaseDispute : BaseModel
    {
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public virtual PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public virtual LeaseDetails LeaseDetails { get; set; }

        [StringLength(50)]
        public string DisputeReferenceNumber { get; set; } // EHC_DISP_###_YYYY

        // ── UC26A: Capture Fields ──
        [StringLength(200)]
        public string ReportedByName { get; set; }

        [StringLength(50)]
        public string ContactNumber { get; set; }

        [StringLength(200)]
        public string EmailAddress { get; set; }

        [StringLength(200)]
        public string ComplexName { get; set; }

        [StringLength(50)]
        public string BlockNumber { get; set; }

        [StringLength(50)]
        public string UnitNumber { get; set; }

        [StringLength(100)]
        public string Category { get; set; } // Financial & Billing / Compliance / Maintenance / Tenure / Notices / Administrative

        [StringLength(200)]
        public string SubCategory { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        // ── UC26B: Review Fields ──
        [StringLength(100)]
        public string RiskClassification { get; set; } // Low / Medium / High / Other

        [StringLength(2000)]
        public string ReviewComment { get; set; }

        [StringLength(50)]
        public string OfficialNumber { get; set; }

        // Legal referral (High risk)
        [StringLength(200)]
        public string LegalAgencyName { get; set; }

        [StringLength(500)]
        public string LegalAgencyAddress { get; set; }

        [StringLength(100)]
        public string LegalAgencyContact { get; set; }

        [StringLength(200)]
        public string LegalAgencyEmail { get; set; }

        [StringLength(2000)]
        public string LegalBriefDescription { get; set; }

        // ── UC26C-S1: Resolve Fields ──
        [StringLength(200)]
        public string ResolutionOutcomeType { get; set; } // 7 types per spec

        [StringLength(4000)]
        public string ResolutionSummary { get; set; }

        public bool? IsResolved { get; set; }

        [StringLength(2000)]
        public string NotResolvedReason { get; set; }

        public string RevenueManagerSignature { get; set; }
        public DateTime? RevenueManagerSignDate { get; set; }

        // ── UC26C-S2: Close Fields ──
        [StringLength(200)]
        public string ClosureOutcome { get; set; } // 5 types per spec

        [StringLength(4000)]
        public string ClosureSummary { get; set; }

        public bool? IsClosed { get; set; }

        [StringLength(200)]
        public string RejectionOption { get; set; } // 3 rejection types

        [StringLength(2000)]
        public string RejectionReason { get; set; }

        public string CEOSignature { get; set; }
        public DateTime? CEOSignDate { get; set; }

        // Status tracking
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }
    }
}
