using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class DebitOrderRegistrationAudit: BaseModelAudit
    {


        [Column(Order = 10)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Bank Number")]
        public string BankNumber { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Debit check action date")]
        public DateTime DebitCheckActionDate { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Leasing Officer")]
        public string LeasingOfficer { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Signature")]
        public bool LeasingSignature { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Revenue Officer")]
        public string RevenueOfficer { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Signature")]
        public bool RevenueSignature { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Key No")]
        public string KeyNo { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Meter No")]
        public string MenterNumber { get; set; }

        [Column(Order = 20)]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Column(Order = 21)]
        [Display(Name = "Unit No")]
        public string UnitNumber { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Signature")]
        public bool TenantSignature { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Name of Applicant")]
        public string Name { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Name of Applicant")]
        public string RentalAmount { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Cell no.")]
        public string CellNo { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Comment")]
        public string RejectionComment { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Rejected")]
        public bool Rejected { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Revenue Officer")]
        public int? RevenueOfficerId { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Leasing Officer")]
        public int? LeasingOfficerId { get; set; }
    }
}