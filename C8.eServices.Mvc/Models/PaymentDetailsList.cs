using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
 
    public class PaymentDetailsList
    {
        public double Amt { get; set; }
        public string Date { get; set; }
        public DateTime ConvertedDate { get; set; }
        public string Ref { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
    }
}