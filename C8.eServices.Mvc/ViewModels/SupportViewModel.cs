using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class SupportViewModel
    {
        public List<Support> Queries { get; set; }
        public Support Query { get; set; }
        public Support Status { get; set; }
        public Customer Customer { get; set; }
    }
}