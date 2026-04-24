using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class ComplaintAuditLog : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [Display(Name = "Complaint")]
        public int TenantComplaintId { get; set; }
        [ForeignKey("TenantComplaintId")]
        public TenantComplaint TenantComplaint { get; set; }

        [Column(Order = 11)]
        [Required]
        [StringLength(100)]
        [Display(Name = "Action")]
        public string Action { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Details")]
        public string Details { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Performed By")]
        public int? PerformedByCustomerId { get; set; }
        [ForeignKey("PerformedByCustomerId")]
        public Customer PerformedBy { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Performed At")]
        public DateTime PerformedAt { get; set; }
    }
}
