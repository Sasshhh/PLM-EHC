using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Models
{
    public class SystemIdentityUser : IdentityUser
    {
        [Required]
        public int SystemUserId { get; set; }

        public string UnconfirmedEmail { get; set; }

        public string ServiceNo { get; set; }
        public bool RoundRobinIsActive { get; set; }
        public bool isInternalUser { get; set; }
        public bool isActiveDirectoryUser { get; set; }
        public bool isDeleted { get; set; }
        public int? CCCId { get; set; }
        [ForeignKey("CCCId")]
        public CCC CCC { get; set; }

        [ForeignKey("SystemUserId")]
        public virtual SystemUser SystemUser { get; set; }

        internal bool IsInRole()
        {
            throw new NotImplementedException();
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<SystemIdentityUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

    }
}