using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Models.Audits
{
    public class ApplicationRoleAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        public int ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public Application Application { get; set; }

        [Column(Order = 11)]
        public string RoleId { get; set; }
        [ForeignKey("RoleId")]
        public IdentityRole IdentityRole { get; set; }
    }
}