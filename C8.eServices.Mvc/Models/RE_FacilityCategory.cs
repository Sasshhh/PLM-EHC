using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("RE_FacilityCategories")]
    public class RE_FacilityCategory : BaseModel
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        [Required]
        public decimal TariffPerSqm { get; set; }

        // DPRE-Q11 (26 Nov 2026): annual escalation % set by the administrator per facility category.
        // Copied to the lease register record when a lease is created from an application.
        public decimal? EscalationPercentage { get; set; }
    }
}
