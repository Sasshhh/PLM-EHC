using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace C8.eServices.Mvc.Models.Audits
{
    public class PropertyLeaseRenewalOfferAudit :BaseModelAudit
    {
        [Column(Order = 10)]
        public int? PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        [Display(Name = "Property Lease Application")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Months Offer")]
        public int MonthsOffer { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Accepted")]
        public bool? IsAccepted { get; set; }
    }
}