using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using C8.eServices.Mvc.Models;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;

namespace C8.eServices.Mvc.Helpers
{
    public class BaseHelper
    {
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Agent Agent { get; set; }
        public Customer Customer { get; set; }
        public void Initialise(eServicesDbContext _context)
        {
            IdentityManager = new IdentityManager(_context);
            var user = HttpContext.Current.User;

            if (user != null && user.Identity.IsAuthenticated)
            {
                IdentityManager.CurrentUser(user);
                SystemUser = IdentityManager.CurrentUser(user);
                _context.CurrentSystemUser = SystemUser;

                if (SystemUser != null)
                {
                    var customer = _context.Customers.AsNoTracking().FirstOrDefault(c => c.SystemUserId == SystemUser.Id);
                    if (customer != null)
                    {
                        Customer = customer;
                        var agent = _context.Agents.AsNoTracking().FirstOrDefault(a => a.CustomerId == customer.Id);
                        if (agent != null)
                            Agent = agent;
                    }
                }
            }
        }
    }
}