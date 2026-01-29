using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class MasterpassRequest : BaseModel
    {
        public decimal Amount { get; set; }
        public string MerchantReference { get; set; }
        public string ShortDescription { get; set; }
        public string SubMerchantName { get; set; }
        public string TerminalId { get; set; }
        public string SmsNotify { get; set; }
        public string EmailNotify { get; set; }
        public string Code { get; set; }
        public bool UseOnce { get; set; }
    }
}