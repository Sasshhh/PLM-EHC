using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class RatesAccount
    {
        [Column(Order = 1)]
        public string RatesNumber { get; set; }
        [Column(Order = 2)]
        public string ServiceUnitCode { get; set; }

        [Column(Order = 3)]
        public string AccountNumber { get; set; }
        [Column(Order = 4)]
        public string OwnerFirstName { get; set; }
        [Column(Order = 5)]
        public string OwnerLastName { get; set; }
        [Column(Order = 6)]
        public string OwnerIdentificationNumber { get; set; }
        [Column(Order=7)]
        public string CityIndicator { get; set; }

    };
}