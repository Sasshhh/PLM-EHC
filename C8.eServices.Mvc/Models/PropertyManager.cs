using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PropertyManager: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Property Manager Comments")]
        public string PropertyComments { get; set; }

        [Display(Name = "Property Lease Application")]
        [Column(Order = 11)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Approved")]
        public bool Approved { get; set; }
    }
}