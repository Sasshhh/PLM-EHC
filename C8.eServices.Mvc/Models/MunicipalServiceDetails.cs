using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MunicipalServiceDetails
    {
        public string AccountNumber { get; set; }
        public string RatesServiceExist { get; set; }
        public string RefuseServiceExist { get; set; }
        public string SewerServiceExist { get; set; }
        public List<MetersList> MetersList { get; set; }
        public List<string> StatusMessages { get; set; }
        public string Status { get; set; }
    }


}