using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class WaitingListQueueHuman : BaseModel
    {
        [Column(Order = 22)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

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
        [Display(Name = "New In")]
        public bool IsNew { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Disapproved")]
        public bool IsDisapproved { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Transfer")]
        public bool IsTransfer { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Matched")]
        public bool IsMatched { get; set; }
    }
}