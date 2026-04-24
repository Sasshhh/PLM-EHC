using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("ServiceRequestDocuments")]
    public class ServiceRequestDocument : BaseModel
    {
        [Key]
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        [StringLength(100)]
        public string FileType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedDate { get; set; }

        [ForeignKey("ServiceRequestId")]
        public virtual ServiceRequest ServiceRequest { get; set; }
    }
}
