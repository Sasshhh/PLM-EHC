using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.Models;
namespace C8.eServices.Mvc.ViewModels
{
    public class RegistrationViewModel
    {
        public RegisterViewModel Register { get; set; }
        public CustomerProfileViewModel Profile { get; set; }

        public List<RoundRobinLog> Error { get; set; }

    }
}