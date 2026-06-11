using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("ServiceRequests")]
    public class ServiceRequest : BaseModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique ticket reference: EHC_SR_###_YYYY
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Request Reference #")]
        public string RequestReferenceNumber { get; set; }

        // Reported By / Tenant Details
        [Required]
        [StringLength(100)]
        [Display(Name = "Reported By - Name")]
        public string ReportedByName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Reported By - Surname")]
        public string ReportedBySurname { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact #")]
        public string ContactNumber { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "E-mail Address")]
        public string EmailAddress { get; set; }

        // Location Details
        [Display(Name = "Complex Name")]
        public int ComplexId { get; set; }

        [StringLength(50)]
        [Display(Name = "Block Number")]
        public string BlockNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Room Type")]
        public string RoomType { get; set; }

        // Category and Priority
        [Display(Name = "Category")]
        public int ServiceRequestCategoryId { get; set; }

        [Display(Name = "Priority")]
        public int? ServiceRequestPriorityId { get; set; }

        // Description
        [Required]
        [Display(Name = "Detailed Description")]
        public string DetailedDescription { get; set; }

        // Status and Tracking
        [Display(Name = "Status")]
        public int StatusId { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// SLA response deadline based on priority (BR34)
        /// </summary>
        public DateTime? ResponseDeadline { get; set; }

        /// <summary>
        /// SLA resolution deadline based on priority (BR34) and category (BR23/BR24)
        /// </summary>
        public DateTime? ResolutionDeadline { get; set; }

        /// <summary>
        /// Whether the SLA escalation at 50% has been triggered (BR34)
        /// </summary>
        public bool EscalationTriggered { get; set; }

        /// <summary>
        /// Date the request was resolved
        /// </summary>
        public DateTime? DateResolved { get; set; }

        /// <summary>
        /// Date the request was closed
        /// </summary>
        public DateTime? DateClosed { get; set; }

        /// <summary>
        /// The Customer (user) who created this service request
        /// Only the creator can edit or delete
        /// </summary>
        public int? CreatedByCustomerId { get; set; }

        /// <summary>
        /// The Letting Officer assigned to handle this service request
        /// Determined by PreferredComplexArea.LettingOfficerId at submission time
        /// </summary>
        public int? AssignedToId { get; set; }

        /// <summary>
        /// Date this service request was assigned to a Letting Officer
        /// </summary>
        public DateTime? DateAssigned { get; set; }

        /// <summary>
        /// Reason for deletion (required when deleting)
        /// </summary>
        public string DeletionReason { get; set; }

        // Navigation Properties
        [ForeignKey("ServiceRequestCategoryId")]
        public virtual ServiceRequestCategory Category { get; set; }

        [ForeignKey("ServiceRequestPriorityId")]
        public virtual ServiceRequestPriority Priority { get; set; }

        [ForeignKey("ComplexId")]
        public virtual PreferredComplexArea Complex { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }

        [ForeignKey("CreatedByCustomerId")]
        public virtual Customer CreatedByCustomer { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual Customer AssignedTo { get; set; }

        public virtual ICollection<ServiceRequestDocument> Documents { get; set; }

        public virtual ICollection<ServiceRequestAuditLog> AuditLogs { get; set; }
    }
}
