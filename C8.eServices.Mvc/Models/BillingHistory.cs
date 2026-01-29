using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class BillingHistory
    {
        [Column( Order = 1 )]
        [Display( Name = "Year" )]
        public int Year { get; set; }

        [Column( Order = 2 )]
        [Display( Name = "Bill Date" )]
        [DisplayFormat( DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true )]
        public DateTime BillDate { get; set; }

        [Column( Order = 3 )]
        [Display( Name = "Account No" )]
        public Int64 AccountNumber { get; set; }

        [Column( Order = 4)]
        [Display(Name = "Identification No")]
        public string IdentificationNumber { get; set; }

        [Column( Order = 5 )]
        [Display( Name = "Balance" )]
        public Decimal BalanceBroughtForward { get; set; }

        [Column( Order = 6 )]
        [Display( Name = "Payments" )]
        public Decimal Payments { get; set; }

        [Column( Order = 7 )]
        [Display( Name = "Electricity" )]
        public Decimal Electricity { get; set; }

        [Column( Order = 8 )]
        [Display( Name = "Water" )]
        public Decimal Water { get; set; }

        [Column( Order = 9 )]
        [Display( Name = "Sewage" )]
        public Decimal Sewage { get; set; }

        [Column( Order = 10 )]
        [Display( Name = "Rates" )]
        public Decimal Rates { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "Vat" )]
        public Decimal Vat { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Other" )]
        public Decimal Other { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "Multiplier" )]
        public int Multiplier { get; set; }

        [Column( Order = 14 )]
        [Display( Name = "Total" )]
        public Decimal Total { get; set; }

        [Column( Order = 15 )]
        [Display( Name = "View Bill" )]
        public string DownloadLink { get; set; }

        [Column(Order = 16)]
        [Display(Name="Account Type")]
        public string AccountType { get; set; }
    }
}