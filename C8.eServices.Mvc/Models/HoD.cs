using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class HoD: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "ApplicationReferenceNumber")]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "First Name")]
        public string HoDFirstName { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Surnane")]
        public string HoDLastName { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; }

        [Column(Order = 14)]
        [Display(Name = "HoD Commments")]
        public string HoDCommments { get; set; }

        [Column(Order = 15)]
        [Display(Name = "LeaseDetails")]
        public int? LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }




    }
}