using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class RCSApplicationHistoryLog: BaseModel
    {

        [Column(Order = 10)]
        [Display(Name = "User")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public Customer User { get; set; }


        [Column(Order = 11)]
        [Display(Name = "RCSApplication Id")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }
        [Column(Order = 12)]
        [Display(Name = "Refund Application Id")]
        public int? RefundApplicationId { get; set; }
        [ForeignKey("RefundApplicationId")]
        public RefundApplication RefundApplication { get; set; }
        [Column(Order = 13)]
        [Display(Name = "Action")]
        public string AuditAction { get; set; }
        [Column(Order = 14)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }






    }
}