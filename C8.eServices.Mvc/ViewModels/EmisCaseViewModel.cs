using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class EmisCaseViewModel
    {
        public Customer Customer { get; set; }
        public List<EmisCase> EmisCases { get; set; }
        public EmisCase EmisCase { get; set; }
    }
}