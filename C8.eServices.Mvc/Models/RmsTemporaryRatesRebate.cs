using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class RmsTemporaryRatesRebate
    {
        [Column( Order = 1 )]
        public int Id { get; set; }

        [Column( Order = 2 )]
        [StringLength( 11 )]
        public string AccountNumber { get; set; }

        [Column( Order = 3 )]
        [StringLength( 8 )]
        public string RateNumber { get; set; }

        [Column( Order = 4 )]
        [StringLength( 2 )]
        public string RebateCategoryCode { get; set; }

        [Column( Order = 5 )]
        [StringLength( 255 )]
        public string RebateCategoryDescription { get; set; }

        [Column( Order = 6 )]
        public DateTime RebateFromDateTime { get; set; }

        [Column( Order = 7 )]
        public DateTime RebateToDateTime { get; set; }

        [Column( Order = 8 )]
        [StringLength( 3 )]
        public string Status { get; set; }

        [Column( Order = 9 )]
        [StringLength( 255 )]
        public string ErrorDescription { get; set; }

        [Column( Order = 10 )]
        public DateTime AuthorisedDateTime { get; set; }

        [Column( Order = 11 )]
        public DateTime RmsProcessedDateTime { get; set; }
    }
}