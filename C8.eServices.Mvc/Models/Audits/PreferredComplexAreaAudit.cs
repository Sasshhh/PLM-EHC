using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class PreferredComplexAreaAudit : BaseTypeAudit
    {
        [Display(Name = "Complex Name")]
        [Column(Order = 13)]
        public string ComplexName { get; set; }

        [Display(Name = "Housing Officer")]
        [Column(Order = 14)]
        public int? LettingOfficerId { get; set; }

        [ForeignKey("LettingOfficerId")]
        public Customer LettingOfficer { get; set; }

        [Display(Name = "Housing Officer")]
        [Column(Order = 15)]
        public int? HousingSuperId { get; set; }
        [ForeignKey("HousingSuperId")]
        public Customer HousingSuper { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Region")]
        public int? RegionTypeId { get; set; }
        [ForeignKey("RegionTypeId")]
        public RegionType RegionType { get; set; }

        [Column(Order = 17)]
        [Display(Name = "CCC")]
        public int? CCCTypeId { get; set; }
        [ForeignKey("CCCTypeId")]
        public CCCType CCCType { get; set; }
    }
}