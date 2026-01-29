using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models.Audits;
using Microsoft.AspNet.Identity.EntityFramework;


namespace C8.eServices.Mvc.Models
{
    public class AppUserRoleAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        public int ApplicationUserRoleId { get; set; }
        [ForeignKey("ApplicationUserRoleId")]
        public ApplicationUserRole ApplicationUserRole { get; set; }



        [Column(Order = 11)]
        public string RoleId { get; set; }
        [ForeignKey("RoleId")]
        public IdentityRole IdentityRole { get; set; }

    }
}