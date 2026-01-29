using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class RatesRebateProperty : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public Property Property { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Rates Rebate")]
        public int RatesRebateId { get; set; }
        [ForeignKey("RatesRebateId")]
        public RatesRebate RatesRebate { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Customer" )]
        public int CustomerId { get; set; }
        [ForeignKey( "CustomerId" )]
        public Customer Customer { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }
    }
}