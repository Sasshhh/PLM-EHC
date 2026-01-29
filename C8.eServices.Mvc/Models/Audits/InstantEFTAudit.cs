using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace C8.eServices.Mvc.Models.Audits
{
    public class InstantEFTAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Column(Order = 11)]
        [Display(Name = "EFT Amount")]
        // [Range(typeof(decimal), "50", "79228162514264337593543950335")]
        public decimal Amount { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Batch")]
        public bool? IsBatch { get; set; }

        [Column(Order = 14)]
        [Display(Name = "EFT Reference Number")]
        public Int64 ReferenceNumber { get; set; }


        [Column(Order = 15)]
        [MaxLength(50)]
        [Display(Name = "EFT Reference")]
        public string ReferenceAlpha { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Message")]
        [MaxLength(500)]
        public string Message { get; set; }


        //[Display(Name = "Credit Account No")]
        //public string CreditBankAccountNumber { get; set; }

        //[Column(Order = 11)]
        //[Display(Name = "Bank Acc No")]
        //public int BankAccountId { get; set; }
        //[ForeignKey("BankAccountId")]
        //public BankAccount BankAccount { get; set; }

        //[Column(Order = 12)]
        //[Display(Name = "Service Acc No")]
        //public int AccountId { get; set; }
        //[ForeignKey("AccountId")]
        //public Account Account { get; set; }



        //[Column(Order = 14)]
        //[Display(Name = "Debit Order Day")]
        //public int StrikeDay { get; set; }



        //[Column(Order = 16)]
        //[Display(Name = "Start Date")]
        //public DateTime StartDateTime { get; set; }

        //[Column(Order = 17)]
        //[Display(Name = "Recurring")]
        //public bool IsRecurring { get; set; }


    }
}