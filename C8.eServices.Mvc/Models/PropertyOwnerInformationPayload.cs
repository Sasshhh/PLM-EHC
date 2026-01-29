using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class PropertyOwnerInformationPayload
    {
        public string AccountNo { get; set; }
        public string PhysicalAddress1 { get; set; }
        public string PhysicalAddress2 { get; set; }
        public string PhysicalAddress3 { get; set; }
        public string PhysicalAddress4 { get; set; }
        public string PostalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IDNumber { get; set; }
        public string PassportNo { get; set; }
        public string HomeTelNo { get; set; }
        public string WorkTelNo { get; set; }
        public string CellPhoneNo { get; set; }
        public string EmailAddr { get; set; }
        public List<string> StatusMessages { get; set; }
    }
}