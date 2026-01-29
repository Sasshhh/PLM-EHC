using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.ViewModels
{
    public class ProfileViewModel
    {
        public bool IsManagingAgent { get; set; }
        public bool IsSameAddress { get; set; }

        public Agent Agent { get; set; }

        public Customer Customer { get; set; }

        public Entity Entity { get; set; }

        public bool IsNew { get; set; }
    }
}