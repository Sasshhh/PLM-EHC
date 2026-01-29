using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class GenerateSolarRCCPayload
    {
        public string AccountNumber { get; set; }
        public string ClearanceCertificate { get; set; }
        public List<string> StatusMessages { get; set; }
    }
}