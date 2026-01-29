using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{

    public class AssessmentPaymentTransaction : BaseModel
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


        [Column(Order = 17)]
        [Display(Name = "Merchant Reference")]
        [StringLength(100)]
        public string MerchantReference { get; set; }

        [Column(Order = 18)]
        [Display(Name = "SubMerchant Name")]
        [StringLength(100)]

        public string SubMerchantName { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Id")]
        public int TransactionId { get; set; }

   
        [Column(Order = 21)]
        [Display(Name = "Reference")]
        [MaxLength(64)]
        public string Reference { get; set; }



        [Column(Order = 24)]
        [Display(Name = "Status")]
        [MaxLength(16)]
        public string Status { get; set; }


        [Column(Order = 25)]
        [Display(Name = "Reference")]
        public int RetrievalReferenceNumber { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status IntStatus { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Masterpass Request")]
        public int ? AssessmentPaymentRequestId { get; set; }
        [ForeignKey("AssessmentPaymentRequestId")]
        public AssessmentPaymentRequest AssessmentPaymentRequest { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Responsibility Type")]
        public int ? ResponsibilityTypeId { get; set; }
        [ForeignKey("ResponsibilityTypeId")]
        public ResponsibilityType ResponsibilityType { get; set; }

        public string SmsNotify { get; set; }
        public string EmailNotify { get; set; }
        public string Code { get; set; }
        public bool UseOnce { get; set; }

    }
}