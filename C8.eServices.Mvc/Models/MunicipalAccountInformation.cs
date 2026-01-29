using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace C8.eServices.Mvc.Models
{
    public class MunicipalAccountInformation : BaseModel
    {
        [Column(Order = 11)]
        [Display(Name = "Services on Property")]

        public bool ServicesOnProperty { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Water Account No")]
        [StringLength(20)]
        public string WaterAccountNo { get; set; }

        [Display(Name = "Water Meter No")]
        [Column(Order = 13)]
        [StringLength(20)]
        public string WaterMeterNo { get; set; }

        [Display(Name = "Electricity Account No")]
        [Column(Order = 14)]
        [StringLength(20)]
        public string ElectricityAccountNo { get; set; }

        [Display(Name = "Electricity Meter No")]
        [Column(Order = 15)]
        [StringLength(20)]
        public string ElectricityMeterNo { get; set; }

        [Display(Name = "Pre-Paid Electricity Meter No")]
        [Column(Order = 16)]
        [StringLength(20)]
        public string PrepaidElectricityMeterNo { get; set; }


        [Display(Name = "Rates and Taxes")]
        [Column(Order = 17)]
        [StringLength(20)]
        public string RatesandTaxes { get; set; }

        [Display(Name = "Refuse")]
        [Column(Order = 18)]
        [StringLength(20)]
        public string Refuse { get; set; }

        [Display(Name = "Sewer")]
        [Column(Order = 19)]
        [StringLength(20)]
        public string Sewer { get; set; }

        [Display(Name = "Water")]
        [Column(Order = 20)]
        [StringLength(20)]
        public string Water { get; set; }


        [Display(Name = "Electricity")]
        [Column(Order = 21)]
        [StringLength(21)]
        public string Electricity { get; set; }

        [Display(Name = "Electricity Meter Reading")]
        [Column(Order = 22)]
        [StringLength(21)]
        public string ElectricityMeterReading { get; set; }

        [Display(Name = "Date Taken ")]
        [Column(Order = 23)]
        public DateTime? ElectricityMeterReadingDateTaken { get; set; }
   

        [Display(Name = "Water Meter Reading")]
        [Column(Order = 24)]
        [StringLength(21)]
        public string WaterMeterReading { get; set; }

        [Display(Name = "Date Taken ")]
        [Column(Order = 25)]
        public DateTime? WaterMeterReadingDateTaken { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Property Rates")]

        public bool RatesandTaxeschk { get; set; }
        [Display(Name = "Refuse")]
        [Column(Order = 27)]
    
        public bool Refusechk { get; set; }

        [Display(Name = "Sewer")]
        [Column(Order = 28)]
      
        public bool Sewerchk { get; set; }






    }
}