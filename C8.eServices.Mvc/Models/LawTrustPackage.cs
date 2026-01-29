using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LawTrustPackage
    {
        public int package_id { get; set; }
        public string workflow_mode { get; set; }
        public string workflow_type { get; set; }

        public string Error { get; set; }

        public string documentid { get; set; }

        public string token { get; set; }
    }
}