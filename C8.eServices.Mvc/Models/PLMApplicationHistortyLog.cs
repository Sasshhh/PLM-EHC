using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PLMApplicationHistortyLog: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "User")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Customer User { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Action")]
        public string AuditAction { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

    }
}