using System.Collections.Generic;
using System.Web.Mvc;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class RE_CaptureViewModel
    {
        public RE_Application Application { get; set; }
        
        // Dropdown data
        public IEnumerable<SelectListItem> CCCList { get; set; }
        public IEnumerable<SelectListItem> ApplicantTypeList { get; set; }
        public IEnumerable<SelectListItem> PurposeOfLeaseList { get; set; }
        public IEnumerable<SelectListItem> BankAccountTypeList { get; set; }
        public IEnumerable<SelectListItem> FacilityList { get; set; }
        public IEnumerable<SelectListItem> FacilityUnitList { get; set; }
    }
}
