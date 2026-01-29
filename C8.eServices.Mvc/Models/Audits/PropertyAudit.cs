using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models.Audits
{
    public class PropertyAudit : BaseModelAudit
    {
        [Column(Order = 10)]
        [MaxLength(30)]
        [Display( Name = "Rates No" )]
        //[RegularExpression( @"^[0-9]{8}$", ErrorMessage = "Please only numbers no longer than 8 digits." )]
        public string RatesNumber { get; set; }

        [Column(Order = 11)]
        [MaxLength(25)]
        [Display( Name = "Door No" )]
        public string PhysicalAddress1 { get; set; }

        [Column(Order = 12)]
        [MaxLength(100)]
        [Display( Name = "Block/ Complex Name" )]
        public string PhysicalAddress2 { get; set; }

        [Column(Order = 13)]
        [MaxLength(100)]
        [Display( Name = "Street No" )]
        public string PhysicalAddress3 { get; set; }

        [Column(Order = 14)]
        [MaxLength(100)]
        [Display( Name = "Street Name" )]
        public string PhysicalAddress4 { get; set; }

        [Column(Order = 15)]
        [MaxLength(100)]
        [Display( Name = "Suburb" )]
        public string PhysicalAddress5 { get; set; }

        [Column(Order = 16)]
        [MaxLength(25)]
        [Display( Name = "Postal Code" )]
        public string PhysicalAddressCode { get; set; }

        [Column(Order = 17)]
        [MaxLength(250)]
        [Display( Name = "Description" )]
        public string PropertyDescription { get; set; }

        [Column( Order = 18 )]
        [Display( Name = "Status" )]
        public int StatusId { get; set; }
        [ForeignKey( "StatusId" )]
        public Status Status { get; set; }

        [Column(Order = 19)]
        [Display(Name = "Market Value")]
        public Decimal MarketValue { get; set; }

        [Column(Order = 20)]
        [Display(Name = "Rating Category Code")]
        public string RatingCategoryCode { get; set; }

        [Column(Order = 21)]
        public string SectionType { get; set; }

        [Display( Name = "Block/ Complex" )]
        public string BlockComplex
        {
            get { return string.Format( "{0} {1}", PhysicalAddress1, PhysicalAddress2 ); }
        }

        [Display( Name = "Street & Suburb" )]
        public string StreetSuburb
        {
            get { return string.Format( "{0} {1}", PhysicalAddress3, PhysicalAddress4 ); }
        }

        public string PropertyFullAddress
        {
            get { return string.Format( "{0} , {1}", BlockComplex, StreetSuburb ); }
        }
    }
}