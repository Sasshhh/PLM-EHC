using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace C8.eServices.Mvc.Models
{
    public class LeaseReviewComment : BaseModel
    {

        [Column(Order = 2)]
        [Display(Name = "Property Lease Application")]
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 4)]
     
        [Display(Name = "Application Allocated Property")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 5)]
        [Display(Name = "Status")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }
        [Column(Order = 6)]
        [Display(Name = "Status")]
        public int? RCSActionTypeId { get; set; }
        [ForeignKey("RCSActionTypeId")]
        public RCSActionType RCSActionType { get; set; }
     

        [Column(Order = 8)]
        [MaxLength(100)]
        [Display(Name = "Review Comment")]
        public string Comment { get; set; }


    }


}