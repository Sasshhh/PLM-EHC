using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class PaymentHistoryViewModel
    {
        public List<PaymentHistory> PaymentHistories { get; set; }
        public Customer Customer { get; set; }
    }
}