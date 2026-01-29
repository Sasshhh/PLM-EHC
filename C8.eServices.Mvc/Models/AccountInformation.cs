using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class AccountInformation : BaseModel
    {
        [Column(Order = 10)]
        [MaxLength(32)]
        [Display(Name = "Account No")]
        public string AccountNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Disconnection Require Amount")]
        public decimal DisconnectionRequireAmount { get; set; }

        [Column(Order = 12)]
        [MaxLength(1)]
        [Display(Name = "Ownership")]
        public string Ownership { get; set; }

        [Column(Order = 13)]
        [MaxLength(32)]
        [Display(Name = "Disconnection Status")]
        public string DisconnectionStatus { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Disconnection Date")]
        public DateTime DisconnectionDate { get; set; }

        [Column(Order = 15)]
        [MaxLength(32)]
        [Display(Name = "Indigent Status")]
        public string IndigentStatus { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Advance Amount")]
        public decimal AdvanceAmount { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Total Balance")]
        public decimal TotalBalance { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Current Balance")]
        public decimal CurrentBalance { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Date Updated")]
        public DateTime DateUpdated { get; set; }
    }
}