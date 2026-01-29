using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.ViewModels
{
    public class UserAdminViewModel
    {
        public List<ApplicationUserRole>  UserRole { get; set; }
   
        public IEnumerable<string> SelectedFeatures { get; set; }
        public IEnumerable<SelectListItem> Features { get; set; }
        public PropertyLeaseApplication propertyLeaseApplication { get; set; }


        public int ID { get; set; }
        public string Name { get; set; }
    }
}