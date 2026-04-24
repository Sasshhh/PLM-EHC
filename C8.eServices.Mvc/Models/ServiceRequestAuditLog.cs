using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("ServiceRequestAuditLogs")]
    public class ServiceRequestAuditLog : BaseModel
    {
        [Key]
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        public string Details { get; set; }

        public int? PerformedByCustomerId { get; set; }

        public DateTime PerformedAt { get; set; }

        [ForeignKey("ServiceRequestId")]
        public virtual ServiceRequest ServiceRequest { get; set; }

        [ForeignKey("PerformedByCustomerId")]
        public virtual Customer PerformedBy { get; set; }
    }
}
