using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace C8.eServices.Mvc.Models
{
    public class BatchAccount:BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        //[ForeignKey("CustomerId")]
        //public Customer Customer { get; set; }
        [Column(Order = 11)]
        [Display(Name = "Service Acc ID")]
        public int AccountId { get; set; }
        //[ForeignKey("AccountId")]
        //public Account Account { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Linked  Account ID")]
        public int LinkedAccountId { get; set; }
        //[ForeignKey("LinkedAccountId")]
        //public LinkedAccount LinkedAccount { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Service Acc No")]
        public Int64 NEW_ACCOUNT { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal PAYABLE { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime DUE_DATE { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Run Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime NEW_RUNDATE { get; set; }

    }
}