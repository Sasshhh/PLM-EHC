using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace C8.eServices.Mvc.Models
{
    public class Payment
    {
        [Column( Order = 10 )]
        [Display( Name = "Customer" )]
        public string FullName { get; set; }
        [Column( Order = 11 )]
        [Display( Name = "ID No." )]
        [Key]
        public string IdentificationNumber { get; set; }
        [Column( Order = 12 )]
        [Display( Name = "Account No." )]
        public string AccountNumber { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "Bill Date" )]
        [DisplayFormat( ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}" )]
        public DateTime BillDate { get; set; }

        [Column( Order = 14 )]
        [Display( Name = "Bill Year" )]
        public int BillYear { get; set; }
        [Column( Order = 15 )]
        [Display( Name = "Bill Month" )]
        public int BillMonth { get; set; }

        [Column( Order = 16 )]
        [Display( Name = "Amount" )]
        [DataType( DataType.Currency )]
        public decimal Amount { get; set; }

        [Display( Name = "Code" )]
        [NotMapped]
        public string Code { get; set; }

        [Display( Name = "Status" )]
        [NotMapped]
        public int Status { get; set; }
    }
}