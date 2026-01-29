using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class Section49ST:BaseModel
    {
        [Column(Order = 10)]
        public string Area { get; set; }
        [Column(Order = 11)]
        public string Postal1 { get; set; }
        [Column(Order = 12)]
        public string Postal2 { get; set; }
        [Column(Order = 13)]
        public string Postal3 { get; set; }
        [Column(Order = 14)]
        public string PostalCode { get; set; }
        [Column(Order = 15)]
        public string PIN { get; set; }

        [Column(Order = 16)]
        public string SchemeName { get; set; }
        [Column(Order = 17)]
        public string SchemeNumber { get; set; }

        [Column(Order = 18)]
        public string Address { get; set; }
        [Column(Order = 19)]
        public string UseCode { get; set; }
        [Column(Order = 20)]
        public string UseCodeDescription { get; set; }
        [Column(Order = 21)]
        public string Cat { get; set; }
        [Column(Order = 22)]
        public string RatingCategoryCode { get; set; }
        [Column(Order = 23)]
        public string RateCodeDescription { get; set; }
        [Column(Order = 24)]
        public string Extent { get; set; }
        [Column(Order = 25)]
        public string PostalAccount { get; set; }
        [Column(Order = 26)]
        public string VenusCode { get; set; }
        [Column(Order = 27)]
        public string Value { get; set; }
        [Column(Order = 28)]
        public string OwnerName { get; set; }
        [Column(Order = 29)]
        public string PostalOwner { get; set; }
        [Column(Order = 30)]
        public string PostalEmail { get; set; }
        [Column(Order = 31)]
        public string Suburb { get; set; }
        [Column(Order = 32)]
        public string Erf { get; set; }
        [Column(Order = 33)]
        public string Portion { get; set; }
        [Column(Order = 34)]
        public string UnitNr { get; set; }
        [Column(Order = 35)]
        public string PropertyType { get; set; }
        [Column(Order = 36)]
        public string Property { get; set; }
        [Column(Order = 37)]
        public string EXCLUDE { get; set; }

        [Column(Order = 38)]
        public string PDFLink { get; set; }
        [Column(Order = 39)]
        public DateTime PDFGeneratedOn { get; set; }




    }
}