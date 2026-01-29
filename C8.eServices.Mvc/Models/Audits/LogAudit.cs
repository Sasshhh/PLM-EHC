using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class LogAudit : BaseModelAudit
    {
        [Column( Order = 10 )]
        public string LogEntry { get; set; }

        [Column( Order = 11 )]
        [Display( Name = "LogType" )]
        public int LogTypeId { get; set; }
        [ForeignKey( "LogTypeId" )]
        public LogType LogType { get; set; }

        [Column( Order = 12 )]
        [Display( Name = "ReferenceId" )]
        public int ReferenceId { get; set; }

        [Column( Order = 13 )]
        [Display( Name = "ReferenceType" )]
        public int ReferenceTypeId { get; set; }
        [ForeignKey( "ReferenceTypeId" )]
        public ReferenceType ReferenceType { get; set; }
    }
}