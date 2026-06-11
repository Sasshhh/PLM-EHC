using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class TenantComplaint : BaseModel
    {
        [Column(Order = 10)]
        [StringLength(50)]
        [Display(Name = "Case Reference Number")]
        public string CaseReferenceNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Official Number/Tenancy Reference")]
        public string OfficialNumber { get; set; }

        // Complainant (Reporter) Details
        [Column(Order = 12)]
        [Required]
        [Display(Name = "Complainant Complex")]
        public int? ComplainantComplexId { get; set; }
        [ForeignKey("ComplainantComplexId")]
        public PreferredComplexArea ComplainantComplex { get; set; }

        [Column(Order = 13)]
        [Required]
        [StringLength(100)]
        [Display(Name = "Complainant First Name")]
        public string ComplainantFirstName { get; set; }

        [Column(Order = 14)]
        [Required]
        [StringLength(100)]
        [Display(Name = "Complainant Surname")]
        public string ComplainantSurname { get; set; }

        [Column(Order = 15)]
        [StringLength(100)]
        [Display(Name = "Complainant Email")]
        public string ComplainantEmail { get; set; }

        [Column(Order = 16)]
        [StringLength(20)]
        [Display(Name = "Complainant Cellphone")]
        public string ComplainantCellphone { get; set; }

        [Column(Order = 17)]
        [StringLength(20)]
        [Display(Name = "Complainant Unit Number")]
        public string ComplainantUnitNumber { get; set; }

        [Column(Order = 18)]
        [StringLength(20)]
        [Display(Name = "Complainant Block Number")]
        public string ComplainantBlockNumber { get; set; }

        // Complainee (Respondent) Details
        [Column(Order = 19)]
        [Required]
        [Display(Name = "Respondent Complex")]
        public int? RespondentComplexId { get; set; }
        [ForeignKey("RespondentComplexId")]
        public PreferredComplexArea RespondentComplex { get; set; }

        [Column(Order = 20)]
        [Required]
        [StringLength(20)]
        [Display(Name = "Respondent Block Number")]
        public string RespondentBlockNumber { get; set; }

        [Column(Order = 21)]
        [Required]
        [StringLength(20)]
        [Display(Name = "Respondent Unit Number")]
        public string RespondentUnitNumber { get; set; }

        [Column(Order = 22)]
        [StringLength(100)]
        [Display(Name = "Respondent First Name")]
        public string RespondentFirstName { get; set; }

        [Column(Order = 23)]
        [StringLength(100)]
        [Display(Name = "Respondent Surname")]
        public string RespondentSurname { get; set; }

        // Complaint Details
        [Column(Order = 24)]
        [Required]
        [Display(Name = "Category")]
        public int ComplaintCategoryId { get; set; }
        [ForeignKey("ComplaintCategoryId")]
        public ComplaintCategory ComplaintCategory { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Type")]
        public int? ComplaintTypeId { get; set; }
        [ForeignKey("ComplaintTypeId")]
        public ComplaintType ComplaintType { get; set; }

        [Column(Order = 26)]
        [Required]
        [Display(Name = "Detailed Description")]
        public string DetailedDescription { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Status")]
        public int StatusId { get; set; }
        [ForeignKey("StatusId")]
        public Status Status { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Submitted By Customer")]
        public int? SubmittedByCustomerId { get; set; }
        [ForeignKey("SubmittedByCustomerId")]
        public Customer SubmittedByCustomer { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Submitted By System User")]
        public int? SubmittedByUserId { get; set; }

        [Column(Order = 30)]
        [Display(Name = "Date Submitted")]
        public DateTime? DateSubmitted { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Assigned To")]
        public int? AssignedToId { get; set; }
        [ForeignKey("AssignedToId")]
        public Customer AssignedTo { get; set; }

        [Column(Order = 32)]
        [Display(Name = "Date Assigned")]
        public DateTime? DateAssigned { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Warning Letters Sent")]
        public int WarningLetterCount { get; set; }

        [Column(Order = 34)]
        [Display(Name = "Last Warning Date")]
        public DateTime? LastWarningDate { get; set; }

        [Column(Order = 35)]
        [Display(Name = "Lease Termination Triggered")]
        public bool LeaseTerminationTriggered { get; set; }

        [Column(Order = 36)]
        [Display(Name = "Lease Termination Date")]
        public DateTime? LeaseTerminationDate { get; set; }

        [Column(Order = 37)]
        [Display(Name = "SLA Escalation Triggered")]
        public bool EscalationTriggered { get; set; }

        [Column(Order = 38)]
        [Display(Name = "Escalation Date")]
        public DateTime? EscalationDate { get; set; }

        // Navigation Properties
        public virtual ICollection<ComplaintEvidence> Evidence { get; set; }
        public virtual ICollection<ComplaintInvestigation> Investigations { get; set; }
        public virtual ICollection<ComplaintAuditLog> AuditLogs { get; set; }
    }
}
