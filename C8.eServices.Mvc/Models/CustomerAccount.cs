using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class CustomerAccount
    {
        public Int64 Id { get; set; }
        public Int64 AccountNo { get; set; }
        public string UnitNo { get; set; }
        public string OwnerTenant { get; set; }
        public string GrpDebType { get; set; }
        public string DebType { get; set; }
        public string Surname { get; set; }
        public string Initials { get; set; }
        public int Cycle { get; set; }
        public string StreetNo { get; set; }
        public string StandAddr { get; set; }
        public string PostalAddress1 { get; set; }
        public string PostalAddress2 { get; set; }
        public string PostalAddress3 { get; set; }
        public string PostalAddress4 { get; set; }
        public string PostalAddress5 { get; set; }
        public string PostalCode { get; set; }
        public string WorkTel { get; set; }
        public string HomeTel { get; set; }
        public Int64 IDNo { get; set; }
        public string CellNo { get; set; }
        public string DebIndex { get; set; }
        public string IndexDesc { get; set; }
        public string EmailInd { get; set; }
        public string SmsInd { get; set; }
        public string FaxInd { get; set; }
        public string FaxNo { get; set; }
        public string StreetAddr { get; set; }
        public string EmailAddr { get; set; }
        public string AcctType { get; set; }
        public string EmailAddr2 { get; set; }
        public string EmailAddr3 { get; set; }
        public bool? StatementViaPost { get; set; }
        public string SecondaryPostalAddress1 { get; set; }
        public string SecondaryPostalAddress2 { get; set; }
        public string SecondaryPostalAddress3 { get; set; }
        public string SecondaryPostalAddress4 { get; set; }
        public string SecondaryPostalAddress5 { get; set; }
        public string SecondaryPostalCode { get; set; }
        public bool? StatementViaSecondaryPost { get; set; }
        public string SecondaryEmailAddress { get; set; }
        public bool? StatementViaSecondaryEmail { get; set; }
    }
}