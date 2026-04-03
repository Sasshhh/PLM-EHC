using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class ComplaintExternalReferral : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [StringLength(200)]
        [Display(Name = "Agency Name")]
        public string AgencyName { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Column(Order = 12)]
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Column(Order = 13)]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Column(Order = 14)]
        [StringLength(100)]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Brief Description")]
        public string BriefDescription { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Referred By")]
        public int? ReferredById { get; set; }
        [ForeignKey("ReferredById")]
        public Customer ReferredBy { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Date Referred")]
        public DateTime? DateReferred { get; set; }

        public virtual ICollection<ComplaintInvestigation> Investigations { get; set; }
    }
}
