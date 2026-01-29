using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class MasterpassHistoryDTO : BaseModel
    {

        [Required]
        [Column(Order = 20)]
        [Display(Name = "Id")]
        public int TransactionId { get; set; }

        [Required]
        [Column(Order = 21)]
        [Display(Name = "Reference")]
        [MaxLength(64)]
        public string Reference { get; set; }

        [Required]
        [Column(Order = 22)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column(Order = 23)]
        [Display(Name = "Currency")]
        [MaxLength(4)]
        public string CurrencyCode { get; set; }

        [Required]
        [Column(Order = 24)]
        [Display(Name = "Status")]
        [MaxLength(16)]
        public string Status { get; set; }

        [Required]
        [Column(Order = 25)]
        [Display(Name = "Reference")]
        public int RetrievalReferenceNumber { get; set; }

        [Required]
        [Column(Order = 26)]
        [Display(Name = "Reference")]
        [MaxLength(8)]
        public string AuthCode { get; set; }

        [Required]
        [Column(Order = 27)]
        [Display(Name = "Bank Response")]
        [MaxLength(4)]
        public string BankResponse { get; set; }

        [Required]
        [Column(Order = 28)]
        [Display(Name = "Code")]
        [MaxLength(32)]
        public string Code { get; set; }

        [Required]
        [Column(Order = 29)]
        [Display(Name = "Card Type")]
        [MaxLength(16)]
        public string CardType { get; set; }

        [Required]
        [Column(Order = 30)]
        [Display(Name = "Bin Last 4")]
        [MaxLength(16)]
        public string BinLast4 { get; set; }

        [Required]
        [Column(Order = 31)]
        [Display(Name = "Account Type")]
        [MaxLength(16)]
        public string AccountType { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status IntStatus { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Masterpass Request")]
        public int MasterpassRequestId { get; set; }
        [ForeignKey("MasterpassRequestId")]
        public MasterpassRequest MasterpassRequest { get; set; }
    }
}