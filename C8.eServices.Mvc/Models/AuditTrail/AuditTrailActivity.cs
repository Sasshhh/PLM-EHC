using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.AuditTrail
{
    [Table("AuditTrail_Activities")]
    public class AuditTrailActivity
    {
        [Key]
        public int Id { get; set; }

        public int? SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public virtual SystemUser SystemUser { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; }

        [StringLength(200)]
        public string Controller { get; set; }

        [StringLength(200)]
        public string Action { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(200)]
        public string EntityName { get; set; }

        public int? EntityId { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        public bool IsAdminAction { get; set; }

        public DateTime ActivityDateTime { get; set; }
    }
}
