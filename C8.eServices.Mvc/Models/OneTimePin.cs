using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class OneTimePin : BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "One Time Pin")]
        public string OTP { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Is Verified")]
        public bool IsVerified { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Is Abandoned")]
        public bool IsAbandoned { get; set; }
    }
}