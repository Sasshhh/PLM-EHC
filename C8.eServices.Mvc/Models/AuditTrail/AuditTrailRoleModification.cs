using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.AuditTrail
{
    [Table("AuditTrail_RoleModifications")]
    public class AuditTrailRoleModification
    {
        [Key]
        public int Id { get; set; }

        public int TargetSystemUserId { get; set; }
        [ForeignKey("TargetSystemUserId")]
        public virtual SystemUser TargetSystemUser { get; set; }

        public int? TargetDepartmentId { get; set; }

        public int ModifiedBySystemUserId { get; set; }
        [ForeignKey("ModifiedBySystemUserId")]
        public virtual SystemUser ModifiedBySystemUser { get; set; }

        [Required]
        [StringLength(50)]
        public string ModificationType { get; set; }

        [StringLength(200)]
        public string PreviousRoleName { get; set; }

        [StringLength(200)]
        public string NewRoleName { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }

        public DateTime ModificationDateTime { get; set; }
    }
}
