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
    }
}
