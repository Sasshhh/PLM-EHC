using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PaymentHistory : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Metro Account No")]
        public long MetroAccountNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDateTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column(Order = 12)]
        [Display(Name = "Recon Date")]
        public DateTime ReconDateTime { get; set; }

        [Column(Order = 13)]
        [MaxLength(16)]
        [Display(Name = "Bank Account No")]
        public string BankAccountNumber { get; set; }

        [Column(Order = 14)]
        [MaxLength(8)]
        [Display(Name = "Bank Branch Code")]
        public string BankBranchCode { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Amount")]
        public decimal AmountPaid { get; set; }

        [Column(Order = 16)]
        [MaxLength(32)]
        [Display(Name = "Payment Type")]
        public string PaymentType { get; set; }
    }
}