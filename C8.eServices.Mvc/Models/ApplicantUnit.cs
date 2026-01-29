using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class ApplicantUnit: BaseModel
    {
        public int LeaseID { get; set; }
        public int MatchedID { get; set; }
        [ForeignKey("MatchedID")]
        public MatchedUnits Matched { get; set; }
        public double DepositPaid { get; set; }
        public double OutstandingDepopsitAmount { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
    }
}