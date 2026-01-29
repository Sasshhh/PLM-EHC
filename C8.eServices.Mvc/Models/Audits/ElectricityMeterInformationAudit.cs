using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using C8.eServices.Mvc.Models.Audits;

namespace C8.eServices.Mvc.Models.Audits
{
    public class ElectricityMeterInformationAudit:BaseModelAudit
    {

        [Display(Name = "Municipal Account number")]
        [Column(Order = 11)]
        [StringLength(21)]
        public string MunicipalAccountNumber { get; set; }

        [Display(Name = "Electricity Meter Number")]
        [Column(Order = 12)]
        [StringLength(21)]
        public string ElectricityMeterNo { get; set; }

        [Display(Name = "Electricity Meter Reading")]
        [Column(Order = 13)]
        [StringLength(21)]
        public string ElectricityMeterReading { get; set; }

        [Display(Name = "Date Taken ")]
        [Column(Order = 14)]
        public DateTime? ElectricityMeterReadingDateTaken { get; set; }

        [Display(Name = "Pre-Paid Electricity Meter No")]
        [Column(Order = 15)]
        [StringLength(20)]
        public string PrepaidElectricityMeterNo { get; set; }

        [Column(Order = 16)]
        [Display(Name = "RCSApplicationStatusId")]
        public int? RCSApplicationStatusId { get; set; }
        [ForeignKey("RCSApplicationStatusId")]
        public RCSApplicationStatus RCSApplicationStatus { get; set; }

        [Display(Name = "Electricity Meter Type")]
        [Column(Order = 17)]
        [StringLength(20)]
        public string ElectricityMeterType{ get; set; }

        [NotMapped]
        public string Status { get; set; }

        [NotMapped]
        public bool DeleteEnabled { get; set; }

        [NotMapped]
        public string DateString { get; set; }
    }
}