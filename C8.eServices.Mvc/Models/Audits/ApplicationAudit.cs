using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class ApplicationAudit : BaseTypeAudit
    {
        [Required]
        [Column( Order = 20 )]
        [MaxLength(25)]
        [Display( Name = "Abbreviation" )]
        public string ApplicationAbbreviation { get; set; }

        [Column( Order = 21 )]
        [Display( Name = "Is Online" )]
        public bool IsOnline { get; set; }

        [Column( Order = 22 )]
        [Display( Name = "Seed Date" )]
        public DateTime SeedDate { get; set; }

        [Column( Order = 23 )]
        [Display( Name = "Seed" )]
        public int Seed { get; set; }
    }
}