using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PropertyLeaseActionComments: BaseModel
    {
        [Column(Order = 10)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        [Display(Name = "Property Lease Application")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Required Comment")]
        public string RejectReason { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Exit Inspection")]
        public bool ExitInspection { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Account Validation")]
        public bool AccountValidation { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Property Eviction")]
        public bool PropertyEvictionValidation { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Lease Renewal Validation")]
        public bool LeaseRenewalValidation { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Property Review Validation")]
        public bool PropertyReviewValidation { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Revenue Review Validation")]
        public bool RevenueReviewValidation { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Tenant  Risk Assessment")]
        public bool RiskAssessmentTenant { get; set; }

        [Column(Order = 19)]
        public int? ClerkId { get; set; }
        [ForeignKey("ClerkId")]
        public Customer Clerk { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Lease Warning Letter")]
        public bool LeaseWarningLetter { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Warning Reason")]
        public string WarningReason { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Lease Termination Letter")]
        public bool LeaseTerminationLetter { get; set; }

        [Column(Order = 23)]
        public int? WarningDocRefId { get; set; }

        [Column(Order = 24)]
        public string RejectRenewalReasonCustomer { get; set; }
        
    }
}