using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class RenewalRecommendation: BaseModel
    {
        [Column(Order = 10)]
        public int PropertyLeaseApplicationId { get; set; }
        [ForeignKey("PropertyLeaseApplicationId")]
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }

        [Column(Order = 10)]
        public int LeaseDetailsId { get; set; }
        [ForeignKey("LeaseDetailsId")]
        public LeaseDetails LeaseDetails { get; set; }

        [Column(Order = 10)]
        public int MonthsOfferd { get; set; }
    }
}