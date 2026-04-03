using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("MaintenanceJobCardSignatures")]
    public class MaintenanceJobCardSignature
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AllocatedUnitMaintenanceEHCId { get; set; }

        [ForeignKey("AllocatedUnitMaintenanceEHCId")]
        public virtual AllocatedUnitMaintenanceEHC AllocatedUnitMaintenance { get; set; }

        [Required]
        [StringLength(50)]
        public string OfficialNumber { get; set; }

        [Required]
        [StringLength(20)]
        public string ApprovalAction { get; set; } // "Approved" or "Rejected"

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }

        [Required]
        public string SignatureData { get; set; } // Base64 signature image

        public DateTime ApprovalDate { get; set; }

        public int SignedByCustomerId { get; set; }

        [ForeignKey("SignedByCustomerId")]
        public virtual Customer SignedByCustomer { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public int CreatedBySystemUserId { get; set; }

        [ForeignKey("CreatedBySystemUserId")]
        public virtual SystemUser CreatedBySystemUser { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public int? ModifiedBySystemUserId { get; set; }

        [ForeignKey("ModifiedBySystemUserId")]
        public virtual SystemUser ModifiedBySystemUser { get; set; }

        public DateTime ModifiedDateTime { get; set; }
    }
}
