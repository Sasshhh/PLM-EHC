using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class InspectionSchedule: BaseModel
    {
        [Column(Order = 10)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        [Display(Name = "Property Lease Application")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        public int? DateToScheduleId { get; set; }
        [ForeignKey("DateToScheduleId")]
        [Display(Name = "DateToSchedule")]
        public DateToSchedule DateToSchedule { get; set; }

        [Column(Order = 12)]
        public int? TimeSlotId { get; set; }
        [ForeignKey("TimeSlotId")]
        [Display(Name = "TimeSlot")]
        public TimeSlot TimeSlot { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Is Inspected")]
        public bool IsInspected { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        [Column(Order = 16)]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        [Display(Name = "Human Settlement Application")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
    }
}