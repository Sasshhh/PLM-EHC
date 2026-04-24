using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("PaymentTransgressionAuditLogs")]
    public class PaymentTransgressionAuditLog : BaseModel
    {
        [Key]
        public int Id { get; set; }

        public int PaymentTransgressionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        public string Details { get; set; }

        public int? PerformedByCustomerId { get; set; }

        public DateTime PerformedAt { get; set; }

        [ForeignKey("PaymentTransgressionId")]
        public virtual PaymentTransgression PaymentTransgression { get; set; }

        [ForeignKey("PerformedByCustomerId")]
        public virtual Customer PerformedBy { get; set; }
    }
}
