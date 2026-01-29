using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class LeaseCaptureAddressContact: BaseModel
    {
        
        [Column(Order = 10)]
        [Display(Name = "Lease Capture Sheet")]
        public int LeaseCaptureSheetId { get; set; }
        [ForeignKey("LeaseCaptureSheetId")]
        public LeaseCaptureSheet LeaseCaptureSheet { get; set; }

        [Column(Order = 11)]
        [Display(Name = "Physical Address")]
        public string DomPhysicalAddress { get; set; }

        [Column(Order = 12)]
        [Display(Name = "Suburb")]
        public string DomSuburb { get; set; }

        [Column(Order = 13)]
        [Display(Name = "Town")]
        public string DomTown { get; set; }

        [Column(Order = 14)]
        [Display(Name = "Street Code")]
        public string DomStreetCode { get; set; }

        [Column(Order = 15)]
        [Display(Name = "Suburb")]
        public string InvSuburb { get; set; }

        [Column(Order = 16)]
        [Display(Name = "Town")]
        public string InvTown { get; set; }

        [Column(Order = 17)]
        [Display(Name = "Street Code")]
        public string InvPostalCode { get; set; }

        [Column(Order = 18)]
        [Display(Name = "Telephone No.")]
        public string InvTelephone { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Telephone No.(Alternative)")]
        public string InvTelephone2 { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Cellular No.")]
        public string InvCellularNo { get; set; }
        [Column(Order = 21)]
        [Display(Name = "Cellular No. (Alternative)")]
        public string InvCellularNo2 { get; set; }

        [Column(Order = 22)]
        [Display(Name = "Fax No.")]
        public string InvFax { get; set; }

        [Column(Order = 23)]
        [Display(Name = "E-mail")]
        public string InvEmail { get; set; }

        [Column(Order = 24)]
        [Display(Name = "Suburb")]
        public string LeaNegSuburb { get; set; }

        [Column(Order = 25)]
        [Display(Name = "Town")]
        public string LeaNegTown { get; set; }

        [Column(Order = 26)]
        [Display(Name = "Street Code")]
        public string LeaNegPostalCode { get; set; }

        [Column(Order = 27)]
        [Display(Name = "Telephone No.")]
        public string LeaNegTelephone { get; set; }

        [Column(Order = 28)]
        [Display(Name = "Telephone No.(Alternative)")]
        public string LeaNegTelephone2 { get; set; }

        [Column(Order = 29)]
        [Display(Name = "Cellular No.")]
        public string LeaNegCellularNo { get; set; }
        [Column(Order = 30)]
        [Display(Name = "Cellular No. (Alternative)")]
        public string LeaNegCellularNo2 { get; set; }

        [Column(Order = 31)]
        [Display(Name = "Fax No.")]
        public string LeaNegFax { get; set; }

        [Column(Order = 32)]
        [Display(Name = "E-mail")]
        public string LeaNegEmail { get; set; }

        [Column(Order = 33)]
        [Display(Name = "Postal Address")]
        public string LeaNegPostalAddress { get; set; }













    }
}