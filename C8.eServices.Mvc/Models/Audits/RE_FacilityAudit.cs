using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RE_FacilityAudit : BaseModelAudit
    {
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [Required]
        public int CCCId { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }
    }
}
