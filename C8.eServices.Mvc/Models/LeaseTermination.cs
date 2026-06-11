using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LeaseTermination: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "PropertyLeaseApplication")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "LeaseDetails")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Reason For Termination")]
        public string ReasonForTermination { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Lease Reference Number")]
        public string LeaseReferenceNumber { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Termination Date")]
        public DateTime TerminationDate { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 16)]
        [Display(Name = "HumanSettlementApplication")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        // UC023 — Termination Reference Number (auto-generated, unique)
        [Column(Order = 17)]
        [Display(Name = "Termination Reference Number")]
        public string TerminationReferenceNumber { get; set; }

        // UC023-S2b — Expanded CSO capture fields
        [Column(Order = 18)]
        [Display(Name = "Clause Reference")]
        public string ClauseReference { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Notice Period")]
        public string NoticePeriod { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Official Number")]
        public string OfficialNumber { get; set; }

        // UC023-S3 — Eviction Reference Number (generated on legal referral)
        [Column(Order = 21)]
        [Display(Name = "Eviction Reference Number")]
        public string EvictionReferenceNumber { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Revenue Manager Signature")]
        public string RevenueManagerSignature { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Revenue Manager Official Number")]
        public string RevenueManagerOfficialNumber { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Revenue Manager Signature Date")]
        public DateTime? RevenueManagerSignDate { get; set; }

        [Column(Order = 25)]
        [Display(Name = "CEO Signature")]
        public string CEOSignature { get; set; }

        [Column(Order = 26)]
        [Display(Name = "CEO Official Number")]
        public string CEOOfficialNumber { get; set; }

        [Column(Order = 27)]
        [Display(Name = "CEO Signature Date")]
        public DateTime? CEOSignDate { get; set; }

        // UC028 Vacating Info
        [Column(Order = 28)]
        [Display(Name = "Access Card Number")]
        public string AccessCardNumber { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Key Number")]
        public string KeyNumber { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Other Possessions")]
        public string OtherPossessions { get; set; }

        // UC029 Refund / Deductions
        [Column(Order = 31)]
        [Display(Name = "Maintenance Cost")]
        public decimal? MaintenanceCost { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Approved Deductions")]
        public decimal? ApprovedDeductions { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Nett Refund Amount")]
        public decimal? NettRefundAmount { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Financial Officer Official Number")]
        public string FinancialOfficerOfficialNumber { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Deposit Refund Recommended")]
        public string DepositRefundRecommended { get; set; }

        [Column(Order = 36)]
        [Display(Name = "Financial Officer Reason")]
        public string FinancialOfficerReason { get; set; }

        // UC030 RM Review
        [Column(Order = 37)]
        [Display(Name = "Revenue Manager Refund Support")]
        public bool? RevenueManagerRefundSupport { get; set; }

        [Column(Order = 38)]
        [Display(Name = "Revenue Manager Refund Official Number")]
        public string RevenueManagerRefundOfficialNumber { get; set; }

        [Column(Order = 39)]
        [Display(Name = "Revenue Manager Refund Reason")]
        public string RevenueManagerRefundReason { get; set; }

        // UC030 CEO Authorization
        [Column(Order = 40)]
        [Display(Name = "CEO Refund Response")]
        public string CEORefundResponse { get; set; }

        [Column(Order = 41)]
        [Display(Name = "CEO Refund Official Number")]
        public string CEORefundOfficialNumber { get; set; }

        [Column(Order = 42)]
        [Display(Name = "CEO Refund Signature")]
        public string CEORefundSignature { get; set; }

        [Column(Order = 43)]
        [Display(Name = "CEO Refund Sign Date")]
        public DateTime? CEORefundSignDate { get; set; }

        [Column(Order = 44)]
        [Display(Name = "CEO Refund Reason")]
        public string CEORefundReason { get; set; }
    }
}