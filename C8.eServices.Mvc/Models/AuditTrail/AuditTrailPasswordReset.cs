using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.AuditTrail
{
    [Table("AuditTrail_PasswordResets")]
    public class AuditTrailPasswordReset
    {
        [Key]
        public int Id { get; set; }

        public int TargetSystemUserId { get; set; }
        [ForeignKey("TargetSystemUserId")]
        public virtual SystemUser TargetSystemUser { get; set; }

        public int? TargetDepartmentId { get; set; }

        public int ResetBySystemUserId { get; set; }
        [ForeignKey("ResetBySystemUserId")]
        public virtual SystemUser ResetBySystemUser { get; set; }

        [Required]
        [StringLength(50)]
        public string ResetType { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }

        public DateTime ResetDateTime { get; set; }
    }
}
