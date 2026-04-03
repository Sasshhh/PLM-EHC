using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("MaintenanceJobCardTasks")]
    public class MaintenanceJobCardTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AllocatedUnitMaintenanceEHCId { get; set; }

        [ForeignKey("AllocatedUnitMaintenanceEHCId")]
        public virtual AllocatedUnitMaintenanceEHC AllocatedUnitMaintenance { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Activity { get; set; }

        [StringLength(200)]
        public string MaterialUsed { get; set; }

        public decimal? QuantityUsed { get; set; }

        [StringLength(200)]
        public string LabourUsed { get; set; }

        public decimal? TotalCosts { get; set; }

        [StringLength(1000)]
        public string TaskComments { get; set; }

        public int? SupportingDocumentId { get; set; }

        [ForeignKey("SupportingDocumentId")]
        public virtual Document SupportingDocument { get; set; }

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
