using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class ClerkRole : BaseModel
    {
        [Column(Order = 10)]
        //[RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "The FirstName field should consist of characters only")]
        public string RoleName { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Clerk")]
        public int? ClerkRegistrationId { get; set; }
        [ForeignKey("ClerkRegistrationId")]
        public ClerkRegistration ClerkRegistration { get; set; }
    }
}