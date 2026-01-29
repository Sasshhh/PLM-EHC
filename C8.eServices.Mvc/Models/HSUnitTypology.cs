using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HSUnitTypology: BaseType
    {
        //[Column(Order = 13)]
        //[Display(Name = "Flat")]
        //public string Flat { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Unit Category")]
        public int? HSUnitCategoryId { get; set; }
        [ForeignKey("HSUnitCategoryId")]
        public HSUnitCategory HSUnitCategory { get; set; }

        //[Column(Order = 14)]
        //[Display(Name = "Flat Type")]
        //public string FlatType { get; set; }
    }
}