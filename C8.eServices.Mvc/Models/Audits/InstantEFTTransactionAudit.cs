using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class InstantEFTTransactionAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Service Acc No")]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }

        [Column(Order = 11)]
        [Display(Name = "EFT No")]
        public int InstantEFTId { get; set; }
        [ForeignKey("InstantEFTId")]
        public InstantEFT InstantEFT { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Amount Paid To Municipal Account")]
        // [Range(typeof(decimal), "50", "79228162514264337593543950335")]
        public decimal Amount { get; set; }
    }
}