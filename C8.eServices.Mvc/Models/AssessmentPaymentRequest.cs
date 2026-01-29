using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace C8.eServices.Mvc.Models
{
    public class AssessmentPaymentRequest : BaseModel
    {

        [Column(Order = 11)]
        [Display(Name = "RCSApplicationStatusId")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }


        [Column(Order = 12)]
        [Display(Name = "Application Reference")]
        [StringLength(500)]
        public string ApplicationReference { get; set; }


        [Column(Order = 13)]
        [Display(Name = "Amount Paid")]
      
        public decimal Amount { get; set; }



        [Column(Order = 14)]
        [Display(Name = "Vote Number")]
        [StringLength(100)]
        public string VoteNumber { get; set; }



        [Column(Order = 15)]
        [Display(Name = "Descritpion")]
        [StringLength(100)]
        public string Descritpion { get; set; }


        [Column(Order = 16)]
        [Display(Name = "Merchant Reference")]
        [StringLength(100)]
        public string MerchantReference { get; set; }

        [Column(Order = 17)]
        [Display(Name = "SubMerchant Name")]
        [StringLength(100)]
        public string SubMerchantName { get; set; }

        public string SmsNotify { get; set; }
        public string EmailNotify { get; set; }
        public string Code { get; set; }
        public bool UseOnce { get; set; }

    }
}