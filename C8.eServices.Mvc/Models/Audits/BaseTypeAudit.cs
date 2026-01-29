using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class BaseTypeAudit : BaseModelAudit
    {
        [Required]
        [Column( Order = 10 )]
        [MaxLength(100)]
        [Display( Name = "Name" )]
        public string Name { get; set; }

        [Column( Order = 11 )]
        [MaxLength(250)]
        [Display( Name = "Description" )]
        public string Description { get; set; }

        [Column( Order = 12 )]
        [MaxLength(250)]
        [Display( Name = "Key" )]
        public string Key { get; set; }
    }
}