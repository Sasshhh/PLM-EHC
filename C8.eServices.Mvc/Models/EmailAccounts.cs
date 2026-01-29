using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.Models
{
    public class EmailAccounts
    {
        public int EmailAccountId { get; set; }

        public int ApplicationId { get; set; }

        public int MaxFailureCount { get; set; }

    }
}