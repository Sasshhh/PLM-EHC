using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace C8.eServices.Mvc.Models
{
    public class WaterMeterInformation : BaseModel
    {
    
        [Display(Name = "Municipal Account number")]
        [Column(Order = 11)]
        [StringLength(21)]
        public string MunicipalAccountNumber { get; set; }

        [Display(Name = "Water Meter Number")]
        [Column(Order = 12)]
        [StringLength(21)]
        public string WaterMeterNo { get; set; }

        [Display(Name = "Water Meter Reading")]
        [Column(Order = 13)]
        [StringLength(21)]
        public string WaterMeterReading { get; set; }

        [Display(Name = "Date Taken ")]
        [Column(Order = 14)]
        public string WaterMeterReadingDateTaken { get; set; }

        [Column(Order = 15)]
        [Display(Name = "RCSApplicationStatusId")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [NotMapped]
        public string Status { get; set; }

        [NotMapped]
        public bool DeleteEnabled { get; set; }

        [NotMapped]
        public string DateString { get; set; }
    }
}