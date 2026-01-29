using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using C8.eServices.Mvc.DataAccessLayer;

namespace C8.eServices.Mvc.Helpers
{
    public class ApplicationHelper
    {
        /// <summary>
        /// Seeds the application number in the application table.
        /// </summary>
        /// <param name="applicationKey">The application key.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Application not present or invalid key.</exception>
        public static string SeedApplicationNumber(string applicationKey)
        {
            var _context = new eServicesDbContext();
            var application = _context.Applications.SingleOrDefault(o => o.Key == applicationKey);
            var applicationNumber = string.Empty;

            if (application == null)
                throw new Exception("Application not present or invalid key.");

            if (!application.SeedDate.ToString("yyyyMMdd").Equals(DateTime.Now.ToString("yyyyMMdd")))
            {
                application.SeedDate = DateTime.Now;
                application.Seed = 1;
            }
            else
                application.Seed++;

            _context.Applications.AddOrUpdate(application);
            _context.SaveChanges();

            applicationNumber = string.Format("{0}{1}{2}", application.ApplicationAbbreviation, application.SeedDate.ToString("yyyyMMdd"), application.Seed);

            return applicationNumber;
        }

        /// <summary>
        /// L.M.20160814 - Checks if User has authority to approve application
        /// </summary>
        /// <returns>True = user can approve, False= user don't have rights to approve</returns>
        public static bool CanApprove()
        {
            bool canApprove;
            try
            {
                canApprove = HttpContext.Current.User.IsInRole("Clerks") 
                          || HttpContext.Current.User.IsInRole("Administrators");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return canApprove;
        }
    }
}