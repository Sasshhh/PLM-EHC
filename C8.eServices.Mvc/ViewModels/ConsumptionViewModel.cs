using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class ConsumptionViewModel
    {
        public string MeterType { get; set; }
        public string AccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MeterNumber { get; set; }
        public List<MeterConsumption> MeterConsumptions { get; set; }
    }
}