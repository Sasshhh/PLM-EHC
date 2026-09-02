using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models
{
    [Table("RE_FacilityUnits")]
    public class RE_FacilityUnit : BaseModel
    {
        [Required]
        public int FacilityId { get; set; }
        [ForeignKey("FacilityId")]
        public virtual RE_Facility Facility { get; set; }

        [Required]
        public int FacilityCategoryId { get; set; }
        [ForeignKey("FacilityCategoryId")]
        public virtual RE_FacilityCategory FacilityCategory { get; set; }

        [Required]
        [MaxLength(150)]
        public string UnitType { get; set; }

        [Required]
        public decimal UnitSize { get; set; }

        [Required]
        public int MaxUnits { get; set; }
    }
}
