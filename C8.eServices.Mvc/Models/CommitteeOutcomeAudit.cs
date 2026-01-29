using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace C8.eServices.Mvc.Models.Audits
{
    public class CommitteeOutcomeAudits : BaseModelAudit
    {
        [Column(Order = 10)]
        [Display(Name = "Committee Outcome")]
        public string Committee_Outcome { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Represented by")]
        public string RepresentedBy { get; set; }

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
        [Display(Name = "Recommendations")]
        public int RecommendationID { get; set; }


        [Column(Order = 16)]
        [Display(Name = "Reason")]
        public string Reason { get; set; }


        [Column(Order = 17)]
        [Display(Name = "SupportingDocuments")]

        public string SupportingDocuments { get; set; }


        [Column(Order = 18)]
        [Display(Name = "DateStamp")]
        public DateTime DateStamp { get; set; }


    }
}