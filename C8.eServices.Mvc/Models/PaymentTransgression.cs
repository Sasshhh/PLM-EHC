using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("PaymentTransgressions")]
    public class PaymentTransgression : BaseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CaseReferenceNumber { get; set; }

        // Tenant Information
        [Required]
        [StringLength(50)]
        public string OfficialNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string TenancyReferenceNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string TenantName { get; set; }

        [Required]
        [StringLength(100)]
        public string TenantSurname { get; set; }

        [StringLength(100)]
        public string TenantEmail { get; set; }

        [StringLength(20)]
        public string TenantCellphone { get; set; }

        // Property Details
        public int ComplexId { get; set; }

        [StringLength(50)]
        public string BlockNumber { get; set; }

        [StringLength(50)]
        public string UnitNumber { get; set; }

        [StringLength(50)]
        public string AccountNumber { get; set; }

        // Financial Information
        public decimal? LastPaymentAmount { get; set; }

        public DateTime? LastPaymentDate { get; set; }

        public decimal TotalAmountDue { get; set; }

        // Transgression Details
        public int PaymentTransgressionCategoryId { get; set; }

        public int PaymentTransgressionTypeId { get; set; }

        public int PaymentTransgressionSeverityId { get; set; }

        [Required]
        public string DetailedDescription { get; set; }

        // Letter Information
        [StringLength(100)]
        public string LetterType { get; set; } // Payment Transgression Notice, Written Warning Letter, Final Written Warning Letter

        public DateTime? LetterGeneratedDate { get; set; }

        public DateTime? LetterSentDate { get; set; }

        [StringLength(500)]
        public string LetterFilePath { get; set; }

        // Status
        public int StatusId { get; set; }

        // Tracking
        public DateTime DateSubmitted { get; set; }

        public int? AssignedToCustomerId { get; set; }

        // Navigation Properties
        [ForeignKey("PaymentTransgressionCategoryId")]
        public virtual PaymentTransgressionCategory Category { get; set; }

        [ForeignKey("PaymentTransgressionTypeId")]
        public virtual PaymentTransgressionType Type { get; set; }

        [ForeignKey("PaymentTransgressionSeverityId")]
        public virtual PaymentTransgressionSeverity Severity { get; set; }

        [ForeignKey("ComplexId")]
        public virtual PreferredComplexArea Complex { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }

        [ForeignKey("AssignedToCustomerId")]
        public virtual Customer AssignedTo { get; set; }

        public virtual ICollection<PaymentTransgressionDocument> Documents { get; set; }

        public virtual ICollection<PaymentTransgressionAuditLog> AuditLogs { get; set; }
    }
}
