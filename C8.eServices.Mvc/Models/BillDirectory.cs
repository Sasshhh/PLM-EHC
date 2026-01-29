using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class BillDirectory:BaseModel
    {
        [Column(Order = 10)]
        [Display(Name = "Account Number")]
        [MaxLength(30)]
        public string AccountNumber { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Bill Date")]
        [MaxLength(30)]
        public string BillDate { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Bill Location")]
        [MaxLength(250)]
        public string BillLocation { get; set; }
    }
}