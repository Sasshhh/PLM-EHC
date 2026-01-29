using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class BillViewModel
    {
        public string AccountNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Index { get; set; }

        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        public LeaseDetails LeaseDetail { get; set; }
        public MatchedUnits MatchedUnit { get; set; }
        public ApplicationAllocatedProperty ApplicationAllocatedProperty { get; set; }
    }

    public class PLMList
    {
        public List<BillViewModel> billViewModels { get; set; }

        public PLMList()
        {
            billViewModels = new List<BillViewModel>();
        }
    }
}