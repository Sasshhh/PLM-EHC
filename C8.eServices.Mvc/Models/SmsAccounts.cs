using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class SmsAccounts
    {

        public int SmsAccountId { get; set; }

        public int ApplicationId { get; set; }

        public int MaxFailureCount { get; set; }

    }
}