using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace C8.eServices.Mvc.Models
{
    public class CommitteeOutcome : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Committee Name")]
        public string Committee_Name { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Column(Order = 13)]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Official Number")]
        public string OfficialNumber { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Recommendation")]
        public int? RecommendationId { get; set; }
        [ForeignKey("RecommendationId")]
        public Recommendation Recommendation { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Display(Name = "Stamp Date")]
        [Column(Order = 17)]
        public DateTime DateStamp { get; set; }


        [Column(Order = 18)]
        [Display(Name = "Supporting Documents")]
        public string SupportingDocuments { get; set; }


        [Column(Order = 19)]
        [Display(Name = "PropertyLeaseApplication")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 20)]
        [Display(Name = "LeaseDetails")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }



    }
}