using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class FirmDetails : BaseModel
    {
        [Column(Order = 10)]
        [MaxLength(100)]
        [Display(Name = "Law Firm Name")]
        public string LawFirmName { get; set; }

        [Column(Order = 11)]
        [MaxLength(100)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Column(Order = 11)]
        [MaxLength(10)]
        [Display(Name = "Cell Number")]
        public string CellNumber { get; set; }


        [Column(Order = 12)]
        [MaxLength(10)]
        [Display(Name = "Work Number")]
        public string WorkNumber { get; set; }

        [Column(Order = 13)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display(Name = "Street Name")]
        public string PhysicalAddress { get; set; }

        [Column(Order = 15)]
        [MaxLength(100)]
        [Display(Name = "City")]
        public string City { get; set; }

        [Column(Order = 16)]
        [MaxLength(100)]
        [Display(Name = "FirmAddressType")]
        public string FirmAddressType { get; set; }

        [Column(Order = 16)]
        [MaxLength(100)]
        [Display(Name = "Floor/Unit")]
        public string FloorOrUnit { get; set; }

        [Column(Order = 16)]
        [MaxLength(100)]
        [Display(Name = "Building/Complex")]
        public string BuildingOrComplex { get; set; }

    }
}