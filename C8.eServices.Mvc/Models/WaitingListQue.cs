using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class WaitingListQue: BaseModel
    {
        public int LeaseID { get; set; }
        public bool IsMatched { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 23)]
        [Display(Name = "Date Matched")]
        public DateTime QueueDate { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Re-List Date")]
        public DateTime? ReListDate { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Waiting List No.")]
        public int? Position { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Re-Listed")]
        public bool IsReListed { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }


    }
}