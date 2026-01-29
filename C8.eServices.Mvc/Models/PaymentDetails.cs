using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PaymentDetails
    {

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 


   
            public string CaudAccountNo { get; set; }
            public List<PaymentDetailsList> PaymentDetailsList { get; set; }
            public List<string> StatusMessages { get; set; }
        public string Status { get; set; }
    }

}