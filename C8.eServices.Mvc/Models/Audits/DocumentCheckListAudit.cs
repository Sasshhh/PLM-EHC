using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class DocumentCheckListAudit : BaseModelAudit    
    {
        [Column(Order = 10)]
        [Display(Name = "Document Type")]
        public int DocumentTypeId { get; set; }
        [ForeignKey("DocumentTypeId")]
        public DocumentType DocumentType { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Application")]
        public int ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Reference")]
        public int ReferenceTypeId { get; set; }
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType ReferenceType { get; set; }


        //[Column(Order = 13)]
        //[Display(Name = "RCS Application")]
        //public int? RCSApplicationStatusId { get; set; }
        //[ForeignKey("RCSApplicationStatusId")]
        //public RCSApplicationStatus RCSApplicationStatus { get; set; }
    }
}