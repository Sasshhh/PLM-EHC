using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    [Table("RE_DepartmentalCommentAudits")]
    public class RE_DepartmentalCommentAudit : BaseModelAudit
    {
        public int RE_ApplicationId { get; set; }

        [Required]
        [MaxLength(250)]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Representative Name")]
        public string RepresentativeName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; }

        [Display(Name = "Comments")]
        public string Comments { get; set; }

        public int? SupportingDocumentFileId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date Stamp")]
        public DateTime? DateStamp { get; set; }
    }
}
