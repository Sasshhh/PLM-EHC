using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace C8.eServices.Mvc.ViewModels
{
    public class UnitViewModel
    {
        public Units UnitInformation { get; set; }
        public UnitsEkurhuleniHousingCompany UnitsEkurhuleniHousingCompany { get; set; }
        public UnitsHumanSettlement01 UnitsHumanSettlement01 { get; set; }
        public ApplicationAllocatedProperty AllocatedUnit { get; set; }
        public string UnitDescription { get; set; }

        public int MatchedUnitId { get; set; }
    }
}