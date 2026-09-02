using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.AuditTrail
{
    [Table("AuditTrail_UserLogins")]
    public class AuditTrailUserLogin
    {
        [Key]
        public int Id { get; set; }

        public int SystemUserId { get; set; }
        [ForeignKey("SystemUserId")]
        public virtual SystemUser SystemUser { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string EventType { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        public DateTime EventDateTime { get; set; }

        public bool IsSuccessful { get; set; }

        [StringLength(500)]
        public string FailureReason { get; set; }
    }
}
