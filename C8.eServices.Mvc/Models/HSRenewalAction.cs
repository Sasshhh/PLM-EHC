using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HSRenewalAction : BaseModel
    {
        [Column(Order = 10)]
        public int HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        [Display(Name = "Human Setlement Application")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 11)]
        public int ActionByUserId { get; set; }
        [ForeignKey("ActionByUserId")]
        [Display(Name = "By User")]
        public SystemUser ActionByUser { get; set; }

        [Column(Order = 12)]
        [Display(Name = "By User")]
        public string Comment { get; set; }

        [Column(Order = 13)]
        [Display(Name = "By User")]
        public bool HLO { get; set; }

        [Column(Order = 14)]
        [Display(Name = "By User")]
        public bool SHS { get; set; }

        [Column(Order = 15)]
        [Display(Name = "By User")]
        public bool IsApproved { get; set; }
    }
}