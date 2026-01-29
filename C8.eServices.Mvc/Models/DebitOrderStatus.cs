using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class DebitOrderStatus : BaseModel
    {

        [Column(Order = 10)]
        [Display(Name = "Debit Order")]
        public int DebitOrderId { get; set; }
        [ForeignKey("DebitOrderId")]
        public DebitOrder DebitOrder { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Batch Number")]
        public Int64 BatchNumber { get; set; }


        [Column(Order = 13)]
        [MaxLength(50)]
        [Display(Name = "Batch Reference")]
        public string BatchReference { get; set; }
    }
}