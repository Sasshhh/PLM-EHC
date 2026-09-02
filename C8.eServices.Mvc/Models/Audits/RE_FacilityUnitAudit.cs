using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class RE_FacilityUnitAudit : BaseModelAudit
    {
        [Required]
        public int FacilityId { get; set; }

        [Required]
        public int FacilityCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string UnitType { get; set; }

        [Required]
        public decimal UnitSize { get; set; }

        [Required]
        public int MaxUnits { get; set; }
    }
}
