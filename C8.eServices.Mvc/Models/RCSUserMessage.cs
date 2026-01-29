using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class RCSUserMessage : BaseType
    {
        [Column(Order = 20)]
        [Display(Name = "Title")]
        [MaxLength(500)]
        public string Title { get; set; }
        [Column(Order = 21)]
        [Display(Name = "Body")]
        [MaxLength(500)]
        public string Body { get; set; }

    }
}