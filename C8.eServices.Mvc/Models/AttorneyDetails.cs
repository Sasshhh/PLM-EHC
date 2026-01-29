using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{

    public class AttorneyDetails
    {
        public int Id { get; set; }
        public string AttorneyCode { get; set; }
        public string AttorneyName { get; set; }
        public string AddressLine1 { get; set; }
        public string StreetName { get; set; }
        public string POBox { get; set; }
        public string Suburb { get; set; }
        public string AddressLine2 { get; set; }
        public string Postcode { get; set; }
        public string WorkNumber { get; set; }
        public string Status { get; set; }
        public List<string> StatusMessages { get; set; }

        public string CCCLocation { get; set; }
        public DateTime GeneratedDate { get; set; }

        public string FirmName { get; set; }
        public string FirmAddress { get; set; }
        public string FirmPostalCode { get; set; }
        public string Json { get; set; }
    }
}