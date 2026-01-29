using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class HumanViewModel : BaseModel
    {
        public int HumanSettlementApplicationId { get; set; }
        public int? HumanSettlementLeaseMasterId { get; set; }
        public int? HumanSettlementLeaseId { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public string ApplicantFullName { get; set; }
        public string ApplicationType { get; set; }
        public string StatusKey { get; set; }
        public string StastusName { get; set; }
        public string ViewName { get; set; }
        public DateTime DateTimeCreated { get; set; }
        //public string DateTimeCreated { get; set; }
    }
}