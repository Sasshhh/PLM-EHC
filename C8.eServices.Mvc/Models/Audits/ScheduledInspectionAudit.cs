using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class ScheduledInspectionAudit :BaseModelAudit
    {
        [Column(Order = 10)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        [Display(Name = "PropertyLeaseApplication")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        [Column(Order = 11)]

        public int? DateToScheduleId { get; set; }
        [ForeignKey("DateToScheduleId")]
        [Display(Name = "DateToScheduleId")]
        public DateToSchedule DateToSchedule { get; set; }

        [Column(Order = 12)]
        public int? TimeSlotId { get; set; }
        [ForeignKey("TimeSlotId")]
        [Display(Name = "TimeSlotId")]
        public TimeSlot TimeSlot { get; set; }

        [Column(Order = 13)]
        public int? HousingSupervisorId { get; set; }
        [ForeignKey("HousingSupervisorId")]
        [Display(Name = "HousingSupervisorId")]
        public Customer HousingSupervisor { get; set; }

        [Column(Order = 14)]
        [Display(Name = "IsInspected")]
        public bool IsInspected { get; set; }

        [Column(Order = 15)]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        [Display(Name = "Human Settlement Application")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
    }
}