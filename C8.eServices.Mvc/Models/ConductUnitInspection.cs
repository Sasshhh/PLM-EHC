using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class ConductUnitInspection : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Lease Details")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }


        [Column(Order = 11)]
        [Display(Name = "Property Lease Application")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Inspection Comment")]
        public string InspectionComment { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Human Settlement Application")]
        public int? HumanSettlementApplicationId { get; set; }
        [ForeignKey("HumanSettlementApplicationId")]
        public HumanSettlementApplication HumanSettlementApplication { get; set; }
    }
}