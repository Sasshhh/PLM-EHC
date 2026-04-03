using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    public class ComplaintEvidence : BaseModel
    {
        [Column(Order = 10)]
        [Required]
        [Display(Name = "Complaint")]
        public int TenantComplaintId { get; set; }
        [ForeignKey("TenantComplaintId")]
        public TenantComplaint TenantComplaint { get; set; }

        [Column(Order = 11)]
        [Required]
        [StringLength(255)]
        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Column(Order = 12)]
        [Required]
        [StringLength(100)]
        [Display(Name = "File Type")]
        public string FileType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "File Path")]
        public string FilePath { get; set; }

        [Column(Order = 14)]
        [Display(Name = "File Size (bytes)")]
        public long? FileSize { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Uploaded By")]
        public int? UploadedById { get; set; }
        [ForeignKey("UploadedById")]
        public Customer UploadedBy { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Upload Date")]
        public DateTime? UploadDate { get; set; }

        [Column(Order = 17)]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
