using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class StatusAudit : BaseTypeAudit
    {
        [Column(Order = 20)]
        [Display(Name = "StatusType")]
        public int StatusTypeId { get; set; }
        [ForeignKey("StatusTypeId")]
        public StatusType StatusType { get; set; }
    }
}