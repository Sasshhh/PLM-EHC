using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Section49Letter:BaseType
    {
        [Column(Order = 20)]
        [Display(Name = "Receipt View")]
        public string LetterTemplate { get; set; }

    }
}