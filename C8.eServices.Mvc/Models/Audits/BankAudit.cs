using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class BankAudit : BaseTypeAudit
    {
        [Required]
        [Column(Order = 20)]
        [Display(Name = "Branch Code")]
        [MaxLength(6)]
        public string UniBranchCode { get; set; }
    }
}