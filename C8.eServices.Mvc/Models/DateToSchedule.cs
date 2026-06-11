using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class DateToSchedule : BaseModel
    {
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Schedule Date")]
        [Column(Order = 10)]
        public DateTime? ShecduleDate { get; set; }

        [Column(Order = 11)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        [Display(Name = "Property Lease Application")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 12)]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        [Display(Name = "Human Settlement Application")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }

        [Column(Order = 13)]
        public int? TenantComplaintId { get; set; }
        [ForeignKey("TenantComplaintId")]
        [Display(Name = "Tenant Complaint")]
        public TenantComplaint TenantComplaint { get; set; }
    }
}