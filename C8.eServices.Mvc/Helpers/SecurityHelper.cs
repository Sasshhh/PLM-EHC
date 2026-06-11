using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Elmah;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PdfSharp.Pdf.Content.Objects;
using WebGrease.Css.Ast.Selectors;

namespace C8.eServices.Mvc.Helpers
{
    
    public static class SecurityHelper
    {
        //private static eServicesDbContext _context = new eServicesDbContext();
        private static IdentityManager IdentityManager;
        private static BaseHelper _base;
        /// <summary>
        /// Checks if current object belongs to the logged in profile/ managing agent customer profile
        /// </summary>
        /// <param name="currentSystemUserId">The System User</param>
        /// <param name="obj">The Object</param>
        /// <param name="viewcode"> The view code => [View Code keys]</param>
        /// <returns></returns>
        public static bool VerifySystemUserOwnership(int currentSystemUserId, object obj, int viewcode)
        {
            using (eServicesDbContext _context = new eServicesDbContext())
            {
                var isVerified = false;
                var currentCustomer = _context.Customers.FirstOrDefault(c => c.SystemUserId == currentSystemUserId);
                //Bypass the check if Back Office user or current system user dont have customer profile.
                if (null == currentCustomer || HasSuperiorRights(AccessLevelKeys.AccessLevelThree)) return true;

                var agent = _context.Agents.FirstOrDefault(a => a.CustomerId == currentCustomer.Id);
                var isAgent = _context.Agents.Any(a => a.CustomerId == currentCustomer.Id);
                //List<RatesRebate> customerRatesRebates = _context.RatesRebates.Where(r => r.ApplicantCustomerId == currentCustomer.Id).ToList();

                #region Initilize Customer Type
                //var individual = _context.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.Individual);
                //if (individual == null) throw new Exception("Invalid/ Missing customer type.");

                //var managingAgent = _context.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.ManagingAgent);
                //if (managingAgent == null) throw new Exception("Invalid/ Missing customer type.");

                //var business = _context.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.Entity);
                //if (business == null) throw new Exception("Invalid/ Missing customer type.");

                #endregion

                if (currentSystemUserId <= 0 || viewcode <= 0 || obj == null) return false;


                switch (viewcode)
                {
                    case ViewCodeKeys.CreateCustomerLinkedAccount:
                    case ViewCodeKeys.IndividualCustomerProfile:
                    case ViewCodeKeys.UpdateAgentCustomerProfile:
                        {
                            var customer = (Customer)obj;

                            if (customer.Id == 0) return true;

                            //Check if current user is a customer
                            if ((_context.Customers.Any(c => c.SystemUserId == currentSystemUserId)))
                            {
                                if (!isAgent)
                                {
                                    isVerified = currentCustomer.Id == customer.Id;
                                }
                                else
                                {
                                    var isCustomerAgent = _context.AgentCustomers.Any(a => a.CustomerId == agent.CustomerId);

                                    if (!isCustomerAgent)
                                    {
                                        isVerified = currentCustomer.Id == customer.Id;
                                    }

                                    if (_context.AgentCustomers.Any(c => c.CustomerId == customer.Id))
                                    {
                                        isVerified = true;
                                    }

                                }
                            }
                        }
                        break;
                    case ViewCodeKeys.CreateAgentCustomerProfile:
                        {
                            var customer = (Customer)obj;

                            if (isAgent && customer.Id == 0)
                            {
                                isVerified = true;
                            }
                        }
                        break;
                    case ViewCodeKeys.UpdateCustomerLinkedAccount:
                    case ViewCodeKeys.ViewCustomerLinkedAccount:
                    case ViewCodeKeys.DownloadBills:
                        {
                            var linkedAccount = (LinkedAccount)obj;

                            if (isAgent)
                            {
                                if (_context.LinkedAccounts.Any(l => l.CustomerId == currentCustomer.Id && l.CustomerId == linkedAccount.CustomerId))
                                {
                                    isVerified = true;
                                }
                                else
                                {
                                    if (_context.AgentCustomers.Any(a => a.CustomerId == linkedAccount.CustomerId))
                                    {
                                        isVerified = true;
                                    }
                                }
                            }
                            else
                            {
                                if (_context.LinkedAccounts.Any(l => l.CustomerId == currentCustomer.Id && l.CustomerId == linkedAccount.CustomerId))
                                {
                                    isVerified = true;
                                }
                            }
                        }
                        break;
                    //case ViewCodeKeys.UpdateCustomerDocuments:
                    //    {
                    //        var document = (Document)obj;
                    //        if (isAgent)
                    //        {
                    //            if (customerRatesRebates.Any(c => c.OwnerCustomerId == document.CustomerId || c.ApplicantCustomerId == document.CustomerId)
                    //                || _context.Documents.Any(l => l.CustomerId == currentCustomer.Id && l.CustomerId == document.CustomerId))
                    //            {
                    //                isVerified = true;
                    //            }
                    //            else
                    //            {
                    //                if (_context.AgentCustomers.Any(a => a.CustomerId == document.CustomerId))
                    //                {
                    //                    isVerified = true;
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (customerRatesRebates.Any(c => c.OwnerCustomerId == document.CustomerId || c.ApplicantCustomerId == document.CustomerId)
                    //                || _context.Documents.Any(l => l.CustomerId == currentCustomer.Id && l.CustomerId == document.CustomerId))
                    //            {
                    //                isVerified = true;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //case ViewCodeKeys.ViewUploadedFile:
                    //    {
                    //        var file = (File)obj;
                    //        var doc = _context.Documents.FirstOrDefault(d => d.FileId == file.Id);

                    //        //if file is not linked to the document, skip the check
                    //        if (null == doc) return true;

                    //        if (isAgent)
                    //        {
                    //            if (customerRatesRebates.Any(c => c.OwnerCustomerId == doc.CustomerId || c.ApplicantCustomerId == doc.CustomerId)
                    //                || _context.Documents.Any(l => l.CustomerId == currentCustomer.Id && l.CustomerId == doc.CustomerId))
                    //            {
                    //                isVerified = true;
                    //            }
                    //            else
                    //            {
                    //                if (_context.AgentCustomers.Any(a => a.CustomerId == doc.CustomerId))
                    //                {
                    //                    isVerified = true;
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (customerRatesRebates.Any(c => c.OwnerCustomerId == doc.CustomerId || c.ApplicantCustomerId == doc.CustomerId)
                    //              || _context.Documents.Any(l => l.CustomerId == currentCustomer.Id && l.CustomerId == doc.CustomerId))
                    //            {
                    //                isVerified = true;
                    //            }
                    //        }
                    //    }
                    //    break;
                    //Rates Rates Security check
                    //case ViewCodeKeys.ViewRatesRebates:
                    //    {
                    //        var ratesRabate = (RatesRebate)obj;

                    //        if (ratesRabate.InOwnCapacity)
                    //        {
                    //            if (
                    //                _context.RatesRebates.Any(
                    //                    rr =>
                    //                        rr.OwnerCustomerId == currentCustomer.Id &&
                    //                        rr.OwnerCustomerId == ratesRabate.OwnerCustomerId))
                    //            {
                    //                isVerified = true;
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (
                    //                _context.RatesRebates.Any(
                    //                    rr =>
                    //                        rr.ApplicantCustomerId == currentCustomer.Id &&
                    //                        rr.ApplicantCustomerId == ratesRabate.ApplicantCustomerId))
                    //            {
                    //                isVerified = true;
                    //            }
                    //        }
                    //    }
                    //    break;
                    // Incentive Policy Application
                    case ViewCodeKeys.UpdateCustomerIncentivePolicyApplication:
                        {
                            var incentivePolicy = (IncentivePolicy)obj;
                            if (incentivePolicy.CustomerId == currentCustomer.Id)
                            {
                                isVerified = true;
                            }

                        }
                        break;
                }

                return isVerified;
            }
        }

        public static void LogError(Exception ex, string msg)
        {
            // log error to Elmah
            if (msg != null)
            {
                // log exception with contextual information that's visible when 
                // clicking on the error in the Elmah log
                var annotatedException = new Exception(msg, ex);
                ErrorSignal.FromCurrentContext().Raise(annotatedException, HttpContext.Current);
            }
            else
            {
                ErrorSignal.FromCurrentContext().Raise(ex, HttpContext.Current);
            }
        }

        public static bool HasSuperiorRights(string accessLevel)
        {
            bool hasRight = false;
            try
            {
                switch (accessLevel)
                {
                    case AccessLevelKeys.AccessLevelOne:
                        hasRight = HttpContext.Current.User.Identity.IsAuthenticated &&
                                   HttpContext.Current.User.IsInRole("Super Administrators");
                        break;
                    case AccessLevelKeys.AccessLevelTwo:
                        hasRight = HttpContext.Current.User.Identity.IsAuthenticated &&
                                   HttpContext.Current.User.IsInRole("Administrators") ||
                                   HttpContext.Current.User.IsInRole("Super Administrators");
                        break;
                    case AccessLevelKeys.AccessLevelThree:
                        hasRight = HttpContext.Current.User.Identity.IsAuthenticated &&
                                   HttpContext.Current.User.IsInRole("Administrators") ||
                                   HttpContext.Current.User.IsInRole("Super Administrators") ||
                                   HttpContext.Current.User.IsInRole("Clerks");
                        break;
                }


            }
            catch (Exception ex)
            {
                LogError(ex, "Error");
            }
            return hasRight;

        }

        /// <summary>
        /// 2-argument overload for IsAuthorized (Compatibility for Views)
        /// </summary>
        public static bool IsAuthorized(System.Security.Principal.IPrincipal user, string applicationKey)
        {
            var claims = new List<Claim>();
            if (user is ClaimsPrincipal principal)
            {
                claims = principal.Claims.ToList();
            }
            return IsAuthorized(user, applicationKey, user.Identity.Name, claims);
        }

        /// <summary>
        /// Restrict access to applications 
        /// </summary>
        /// <param name="user">Current User</param>
        /// <param name="applicationKey">The Application Key</param>
        /// <returns></returns>
        public static bool IsAuthorized(System.Security.Principal.IPrincipal user, string applicationKey, string username, List<Claim> Claims)
        {
            var isauthorized = false;
            try
            {
                using (var _eservices = new eServicesDbContext())
                {
                    if (user != null)
                    {
                        
                        var UserId = user.Identity.GetUserId();
                        SystemUser systemUser = new IdentityManager().CurrentUser(Claims, username ?? UserId);
                        var store = new UserStore<SystemIdentityUser>(_eservices);
                        var UserManager = new UserManager<SystemIdentityUser>(store);
                        var thisUser = UserManager.FindByName(systemUser.UserName.Trim());
                        SystemIdentityUser aUser = new IdentityManager().CurrentSystemUser(thisUser.Id);
                        var application =
                            _eservices.Applications.SingleOrDefault(a => a.Key == applicationKey && a.IsActive && !a.IsDeleted);
                        if (application == null)
                            throw new Exception(string.Format("Invalid/ missing application key - {0}", applicationKey));

                        var canAccessApplication = _eservices.ApplicationUserRoles.Any(s => s.SystemUserId == aUser.SystemUser.Id && s.ApplicationId == application.Id);
                        if (aUser != null)
                        {
                            var customer =
                                _eservices.Customers.FirstOrDefault(c => c.SystemUserId == aUser.SystemUser.Id);
                            if (customer != null)
                            {
                                var CustomerType =
                                _eservices.CustomerTypes.FirstOrDefault(r => r.Id == customer.CustomerTypeId);
                                var isEntity = _eservices.Entities.Any(e => e.CustomerId == customer.Id);
                                if (isEntity && applicationKey == ApplicationKeys.IncentivePolicy)
                                {
                                    isauthorized = true;
                                }
                                if (CustomerType.Key == CustomerTypeKeys.Individual)
                                {
                                    isauthorized = true;
                                }
                                if(systemUser.IsIAMRegistered)
                                    isauthorized = true;
                            }
                            if (HasSuperiorRights(AccessLevelKeys.AccessLevelOne))
                            {
                                isauthorized = true;
                            }
                            else if (canAccessApplication && HasSuperiorRights(AccessLevelKeys.AccessLevelThree))
                            {
                                isauthorized = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SecurityHelper.LogError(ex, "Error");
            }
            return isauthorized;
        }
        /// <summary>
        /// Check if the user is on the Domain
        /// </summary>
        /// <returns></returns>
        public static bool IsInternalUser()
        {
            var exists = false;
            var userName = Environment.UserName;
            try
            {
                using (var domainContext = new PrincipalContext(ContextType.Domain, "Martin_west")) //TODO: Read domain name from Db
                {
                    var user = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, userName);
                    if (user != null)
                    {
                        exists = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Exception could occur if machine is not on a domain
            }
            return exists;

        }


        public static String UserDepartment()
        {
            try
            {
                using(eServicesDbContext _context = new eServicesDbContext())
                {
                    _base = new BaseHelper();
                    _base.Initialise(_context);
                    SystemUser systemUser = _base.SystemUser;

                    ApplicationEntity applicationEntity = _context.ApplicationEntities.Find(systemUser.DepartmentId);

                    if (applicationEntity == null)
                        return String.Empty;

                    return applicationEntity.Key;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
           
        }
    }

}