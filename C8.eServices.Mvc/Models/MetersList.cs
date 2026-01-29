using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MetersList
    {
        public string Service { get; set; }
        public string ServiceType { get; set; }
        public string MeterNumber { get; set; }
        public double AmountLevied { get; set; }
        public double VatLevied { get; set; }
        public double? UnprocessedMeterReading { get; set; }
        public int? UnprocessedMeterReadingDate { get; set; }
        public bool hasUnprocessedMeter { get; set; }
        public double MeterReading { get; set; }
        public int MeterReadingDate { get; set; }
        public double PreviousMeterReading { get; set; }
        public int PreviousMeterReadingDate { get; set; }
        public bool hasPreviousMeter { get; set; }
        public double MeterConsumption { get; set; }
        public string Status { get; set; }
    }
}