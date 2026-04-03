using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class ComplaintInvestigation : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [Display(Name = "Complaint")]
        public int TenantComplaintId { get; set; }
        [ForeignKey("TenantComplaintId")]
        public TenantComplaint TenantComplaint { get; set; }

        // Appointment Details
        [Column(Order = 11)]
        [Display(Name = "Appointment Date")]
        public DateTime? AppointmentDate { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Appointment Time")]
        public TimeSpan? AppointmentTime { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Scheduled By")]
        public int? ScheduledById { get; set; }
        [ForeignKey("ScheduledById")]
        public Customer ScheduledBy { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Date Scheduled")]
        public DateTime? DateScheduled { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Respondent Confirmed")]
        public bool RespondentConfirmed { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Date Confirmed")]
        public DateTime? DateConfirmed { get; set; }

        // Investigation Outcome
        [Column(Order = 17)]
        [StringLength(50)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; } // Resolved, Referral, Unresolved

        [Column(Order = 18)]
        [Display(Name = "Outcome Details")]
        public string OutcomeDetails { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Outcome Date")]
        public DateTime? OutcomeDate { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Investigated By")]
        public int? InvestigatedById { get; set; }
        [ForeignKey("InvestigatedById")]
        public Customer InvestigatedBy { get; set; }

        // Referral Details (if applicable)
        [Column(Order = 21)]
        [Display(Name = "External Agency")]
        public int? ExternalReferralId { get; set; }
        [ForeignKey("ExternalReferralId")]
        public ComplaintExternalReferral ExternalReferral { get; set; }

        // Supporting Documents
        public virtual ICollection<ComplaintInvestigationDocument> Documents { get; set; }
    }
}
