using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("RE_DepartmentalComments")]
    public class RE_DepartmentalComment : BaseModel
    {
        public int RE_ApplicationId { get; set; }
        [ForeignKey("RE_ApplicationId")]
        public virtual RE_Application RE_Application { get; set; }

        [Required]
        [MaxLength(250)]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Representative Name")]
        public string RepresentativeName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; } // "Supported", "Supported with Conditions", "Not Supported", "Request Additional Information"

        [Display(Name = "Comments")]
        public string Comments { get; set; }

        public int? SupportingDocumentFileId { get; set; }
        [ForeignKey("SupportingDocumentFileId")]
        public virtual File SupportingDocumentFile { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Stamp")]
        public DateTime? DateStamp { get; set; }
    }
}
