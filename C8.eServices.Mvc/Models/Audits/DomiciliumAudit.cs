using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class DomiciliumAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [MaxLength(25)]
        [Display(Name = "Door No")]
        public string Address1 { get; set; }

        [Column(Order = 11)]
        [MaxLength(100)]
        [Display(Name = "Block/ Complex Name")]
        public string Address2 { get; set; }

        [Column(Order = 12)]
        [MaxLength(100)]
        [Display(Name = "Street No")]
        public string Address3 { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        [Display(Name = "Stree Name")]
        public string Address4 { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display(Name = "Suburb")]
        public string Address5 { get; set; }

        [Column(Order = 15)]
        [MaxLength(25)]
        [Display(Name = "Postal Code")]
        public string AddressCode { get; set; }
    }
}