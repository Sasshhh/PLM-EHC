using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class REAC: BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "PropertyLeaseID")]
        public string PropertyLeaseID { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Outcome")]
        public string Outcome { get; set; }

        [Column(Order = 12)]
        [Display(Name = "REAC Commments")]
        public string REACCommments { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Surname")]
        public string LastName { get; set; }

        [Column(Order = 14)]
        [Display(Name = "First Name")]
        public string FistName { get; set; }



    }
}