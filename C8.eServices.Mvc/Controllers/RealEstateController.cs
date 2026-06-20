using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class RealEstateController : Controller
    {
        public UserManager<SystemIdentityUser> UserManager { get; private set; }
        private eServicesDbContext db = new eServicesDbContext();
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }

        public RealEstateController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public RealEstateController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
            Initialise();
        }

        public RealEstateController(eServicesDbContext context)
        {
            UserManager = new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(context));
        }

        private void Initialise()
        {
            try
            {
                IdentityManager = new IdentityManager(db);

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    SystemUser = IdentityManager.CurrentUser(User);
                }

                if (SystemUser != null)
                {
                    Customer = db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                        .Include(o => o.CustomerType)
                        .Include(o => o.Country)
                        .Include(o => o.IdentificationType)
                        .Include(o => o.TitleType)
                        .FirstOrDefault();

                    if (Customer != null)
                    {
                        Entity = db.Entities.Where(o => o.CustomerId == Customer.Id)
                            .Include(o => o.EntityType)
                            .FirstOrDefault();
                        CustomerId = Customer.Id;
                    }
                }

                if (Customer != null)
                {
                    Agent = db.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                }
            }
            catch (Exception ex)
            {
                // Silence initialisation errors or log them
            }
        }

        public ActionResult Inbox()
        {
            Initialise();
            // Point him to an empty dashboard/inbox
            // No real estate applications currently
            var applications = new List<PropertyLeaseApplication>();
            return View(applications);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
