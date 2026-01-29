using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Region : BaseModel
    {
        [Column(Order = 12)]
        [Display(Name = "Region")]
        public int? RegionTypeId { get; set; }
        [ForeignKey("RegionTypeId")]
        public RegionType RegionType { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Region Name")]
        [MaxLength(50)]
        public string RegionName { get; set; }

        [Column(Order = 14)]
        [Display(Name = "CCC Prefix")]
        [MaxLength(50)]
        public string Prefix { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Area Manager")]
        public int? AreaManagerId { get; set; }
        [ForeignKey("AreaManagerId")]
        public Customer AreaManager { get; set; }



    }
}