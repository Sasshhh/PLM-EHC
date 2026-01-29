using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class DocumentReferenceAudit : BaseModelAudit
    {
        [Column( Order = 10 )]
        [Display( Name = "Document" )]
        public int DocumentId { get; set; }
        [ForeignKey( "DocumentId" )]
        public Document Document { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "Application" )]
        public int ApplicationId { get; set; }
        [ForeignKey( "ApplicationId" )]
        public Application Application { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "Reference Type" )]
        public int ReferenceTypeId { get; set; }
        [ForeignKey( "ReferenceTypeId" )]
        public ReferenceType ReferenceType { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "Reference" )]
        public int ReferenceId { get; set; }
    }
}