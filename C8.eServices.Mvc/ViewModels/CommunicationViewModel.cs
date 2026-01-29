using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class CommunicationViewModel
    {
        public Comment Comments { get; set; }
        public Communication Communications { get; set; }
        public int RCSApplicationID { get; set; }

        public int RefundApplicationID { get; set; }
    }
}