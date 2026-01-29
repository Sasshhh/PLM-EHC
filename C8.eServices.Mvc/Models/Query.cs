using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Query : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Query Type")]
        public int? QueryTypeId { get; set; }
        [ForeignKey("QueryTypeId")]
        public QueryType QueryType { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Column(Order = 12)]
        [MaxLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display(Name = "Reference No")]
        public string ReferenceNumber { get; set; }

        //[Column(Order = 15)]
        //[Display(Name = "Resolved")]
        //public bool IsResolved { get; set; }
        [Column(Order = 15)]
        [Display(Name = "StatusId")]
        public int? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Resolution")]
        public string ResolutionComment { get; set; }

        [Column(Order = 17)]
        [MaxLength(100)]
        [Display(Name = "Contact No")]
        public string ContactNumber { get; set; }

        [Column(Order=18)]
        [Display(Name="Date Resolved")]
        public DateTime? ResolvedDateTime { get; set; }

        //public string Resolved { get { return IsResolved ? "Yes" : "No"; } }
    }
}