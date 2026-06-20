using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.ApiServices;
using System.Web.Routing;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using C8.eServices.Mvc.Keys;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Infrastructure;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        //eServicesDbContext _context = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public ProfileController()
        {
            var context = new eServicesDbContext();

            IdentityManager = new IdentityManager(context);
            UserManager =
                new UserManager<SystemIdentityUser>(
                new UserStore<SystemIdentityUser>(context));
        }

        public IdentityManager IdentityManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        public bool ViewSuccess { get; set; }
        public bool SendEmailRequest { get; set; }
        private int _systemUserId = -1;

        #region Profile Index GET
        //GET: /Profile/
        [Authorize(Roles = "Senior Housing Specialist, Regional Manager,Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]
        public ActionResult Index()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    int customerId;
                    int agentId;
                    var customer = new Customer();

                    _base.Initialise(context);
                    var systemUserId = _base.SystemUser.Id;

                    #region IAM User Redirect
                    if (_base.SystemUser.IsIAMRegistered && User.IsInRole("Housing Liaison Officer")) return RedirectToAction("CaptureMaster", "HumanSettlementApplication");
                    if (_base.SystemUser.IsIAMRegistered && User.IsInRole("Senior Housing Specialist")) return RedirectToAction("HumanSettlementApplications", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Master=" + true + "&All=" + false + "&New=" + false) });
                    if (_base.SystemUser.IsIAMRegistered && User.IsInRole("Regional Manager")) return RedirectToAction("HumanSettlementApplications", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Master=" + true + "&All=" + false + "&New=" + false) });
                    if (_base.SystemUser.IsIAMRegistered && User.IsInRole("Caretaker")) return RedirectToAction("HumanSettlementApplications", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Master=" + true + "&All=" + false + "&New=" + false) });
                    #endregion
                  
                    if (systemUserId != -1)
                    {
                        customer = context.Customers.SingleOrDefault(x => x.SystemUserId == systemUserId);
                    }

                    if (customer == null)
                    {
                        customerId = 0;
                        agentId = 0;
                    }
                    else
                    {
                        customerId = customer.Id;
                        var agent = context.Agents.SingleOrDefault(x => x.CustomerId == customerId
                                    && x.IsActive && x.IsDeleted == false);
                        agentId = agent != null ? agent.Id : 0;
                    }
                             
                    object obj = new { customerId = customerId, agentId = agentId };
                    return RedirectToAction("Index2", SecureActionLinkExtension.Encrypt(obj));
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion
        #region GetUsersInRole
        public SystemIdentityUser GetUsersInRole(string roleId)
        {
            return UserManager.Users.Where(o => o.UserName == roleId).FirstOrDefault();
        }
        #endregion

        #region Profile Index2 Init GET
        [EncryptedActionParameter]
        [Authorize(Roles = "Senior Housing Specialist, Regional Manager,Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]


        public ActionResult Index2(int? agentId, int? customerId) // 
        {
            using (var context = new eServicesDbContext())
            {
               
                try
                {
                    var resName = User.Identity.GetUserName();
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    var customerVm = new CustomerProfileViewModel { ShowUpdateLink = true };
                    var customer = context.Customers.Include(c => c.SystemUser)
                   .Include(c => c.IdentificationType)
                   .Include(c => c.Status)
                   .Include(c => c.Country)
                   .SingleOrDefault(c => c.Id == customerId);

                    ViewBag.CusIDNo=context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                                                                        && e.IsActive && e.IsDeleted == false);
                    if (customer != null)
                    {
                        customerVm.Customer = customer;
                        customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customer.Id));
                        var isEntity = context.Entities.FirstOrDefault(e => e.CustomerId == customerId
                                                                        && e.IsActive && e.IsDeleted == false);

                        if (isEntity != null)
                            customerVm.Entity = isEntity;
                    }



                    //completely new systemuser
                    if ((customerId == 0))
                    {
                        var c = new Customer();
                        c.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", c.Id));
                        c.IsDeceased = false;
                        customerVm.Customer = c;
                        return View("Index", customerVm);
                    }

                    //profile info exists
                    if ((customerId != 0))
                    {
                        customerVm.IsManagingAgent = false;
                    }

                    if (null != customerVm.Customer)
                    {
                        //Checks if current customer profile belongs to the system user
                        if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customerVm.Customer, ViewCodeKeys.UpdateAgentCustomerProfile))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                            return RedirectToAction("Index", "Error");
                        }
                    }
                    //agent
                    if ((agentId > 0))
                    {
                        customerVm.Agent = context.Agents.Find(agentId);
                        customerVm.Agent.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Agent.Id));
                        customerVm.Agent.Id = agentId.Value;
                        customerVm.Customer = context.Customers.Include(c => c.Status).SingleOrDefault(x => x.Id == customerVm.Agent.CustomerId);
                        if (customerVm.Customer != null)
                            customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Customer.Id));
                        customerVm.IsManagingAgent = true;

                        var isEntityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId);

                        if (isEntityAgent != null)
                            customerVm.Entity = context.Entities.Find(isEntityAgent.EntityId);

                        return View("IndexAgent", customerVm);
                    }
                    //var store = new UserStore<SystemIdentityUser>(context);
                    //var UserManager = new UserManager<SystemIdentityUser>(store);
                    //UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
                    //var user = new SystemIdentityUser();
                    string us = customerVm.Customer.SystemUser.UserName;
                    var user = GetUsersInRole(us);

                    //var user =  UserManager.FindByName(customerVm.Customer.SystemUser.UserName);
                    
                    //var identityuser = context.systemIdentity
                    if(user.isInternalUser == true)
                    {
                        if (UserManager.IsInRole(user.Id, "System Administrators"))
                        {
                            
                            return RedirectToAction("Index", "Home");
                        }
                        if (UserManager.IsInRole(user.Id, "Internal Registration"))
                        {
                         
                            return RedirectToAction("InternalRegister", "Account");
                        }
                        if (UserManager.IsInRole(user.Id, "Area Manager"))
                        {
                        
                            return RedirectToAction("Index", "AreaManager");
                        }
                        if (UserManager.IsInRole(user.Id, "Acknowledge RCS Application"))
                        {
                        
                            return RedirectToAction("AcknowledgementDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Submit Figures"))
                        {
                         
                            return RedirectToAction("AssessmentFiguresDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Issue Certificate"))
                        {
                            
                            return RedirectToAction("RCCertificateDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Billing"))
                        {
                           
                            return RedirectToAction("BillingDepartmentDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Rates"))
                        {
                        
                            return RedirectToAction("RatesDepartmentDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Credit Control"))
                        {
                            
                            return RedirectToAction("CreditDepartmentDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Sundry Account"))
                        {
                           
                            return RedirectToAction("SundryDepartmentDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Issue Refunds Collection"))
                        {
                     
                            return RedirectToAction("IssueRefundDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Acknowledge Refund Application"))
                        {
                       
                            return RedirectToAction("RefundAcknowledgementDashboard", "DepartmentsApprovals");
                        }
                        if (UserManager.IsInRole(user.Id, "Community Development Officer"))
                        {
                       
                            return RedirectToAction("PropertyLeaseTenantTraining", "PropertyLeaseApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Housing Supervisor"))
                        {
                       
                            return RedirectToAction("PropertyLeaseInspections", "PropertyLeaseApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Property Manager"))
                        {
                       
                            return RedirectToAction("PropertyLeaseAgreements", "PropertyLeaseApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Revenue Manager"))
                        {
                       
                            return RedirectToAction("PropertyLeaseAgreements", "PropertyLeaseApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Revenue Officer"))
                        {
                       
                            return RedirectToAction("PropertyLeaseDeposit", "PropertyLeaseApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Letting Officer"))
                        {
                       
                            return RedirectToAction("UpdateLeaseDetails", "PropertyLeaseApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Housing Liaison Officer"))
                        {
                          
                            return RedirectToAction("CaptureMaster", "HumanSettlementApplication");
                        }
                        if (UserManager.IsInRole(user.Id, "Senior Housing Specialist"))
                        {
                         
                            return RedirectToAction("HumanSettlementApplications", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Master=" + true + "&All=" + false + "&New=" + false) });
                        }
                        if (UserManager.IsInRole(user.Id, "Regional Manager"))
                        {

                            return RedirectToAction("HumanSettlementApplications", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Master=" + true + "&All=" + false + "&New=" + false) });
                        }
                        if (UserManager.IsInRole(user.Id, "Caretaker"))
                        {

                            return RedirectToAction("HumanSettlementApplications", "HumanSettlementApplication", new { q = new AesCrypto().Encrypt("Master=" + true + "&All=" + false + "&New=" + false) });
                        }
                        if (UserManager.IsInRole(user.Id, "Rates Clearance Senior Clerk"))
                        {
                            //return RedirectToAction(secur)
                            return RedirectToAction("Index", "Customer", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { id = 0 })));
                            return RedirectToAction("Index", "DepartmentsApprovals");
                        }
                        if (user.UserName == "SashenMoodley")
                        {

                           
                            return RedirectToAction("CaptureWalkIn", "Capture");
                        }
                        if (UserManager.IsInRole(user.Id, "Back Office System Administrator"))
                        {

                        
                            return RedirectToAction("AdminInbox", "AreaManager");
                        }
                    }

                    return View("Index", customerVm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region Profile Index2 Init GET
        [EncryptedActionParameter]
        [Authorize(Roles = "Senior Housing Specialist, Regional Manager,Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]

        // new index3 view if system user is exisiting in customers
        public ActionResult Index3(int? agentId, int? customerId) 
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var resName = User.Identity.GetUserName();
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    var customerVm = new CustomerProfileViewModel { ShowUpdateLink = true };
                    var customer = context.Customers.Include(c => c.SystemUser)
                   .Include(c => c.IdentificationType)
                   .Include(c => c.Status)
                   .Include(c => c.Country)
                   .SingleOrDefault(c => c.Id == customerId);

                    ViewBag.CusIDNo = context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                                                                          && e.IsActive && e.IsDeleted == false);
                    if (customer != null)
                    {
                        customerVm.Customer = customer;
                        customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customer.Id));
                        var isEntity = context.Entities.FirstOrDefault(e => e.CustomerId == customerId
                                                                        && e.IsActive && e.IsDeleted == false);

                        if (isEntity != null)
                            customerVm.Entity = isEntity;
                    }


                    var status = context.Status.FirstOrDefault(s => s.Key.Equals(StatusKeys.CustomerPendingApproval));
                    if (status == null) throw new Exception(string.Format("Invalid/ missing status key {0}", StatusKeys.CustomerActive));
                    if (customer != null && customer.Status.Key == status.Key)
                    {
                        customerVm.Agent = context.Agents.Find(agentId);
                        customerVm.Agent.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Agent.Id));
                        customerVm.Agent.Id = agentId.Value;
                        customerVm.Customer = context.Customers.Include(c => c.Status).SingleOrDefault(x => x.Id == customerVm.Agent.CustomerId);
                        if (customerVm.Customer != null)
                            customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Customer.Id));
                        customerVm.IsManagingAgent = true;

                        var isEntityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId);

                        if (isEntityAgent != null)
                            customerVm.Entity = context.Entities.Find(isEntityAgent.EntityId);

                        return View("IndexAgent", customerVm);
                    }


                    //completely new systemuser
                    if ((customerId == 0))
                    {
                        var c = new Customer();
                        c.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", c.Id));
                        c.IsDeceased = false;
                        customerVm.Customer = c;
                     
                        return View("Index", customerVm);
                    }

                    //profile info exists
                    if ((customerId != 0))
                    {
                        customerVm.IsManagingAgent = false;
                    }

                    if (null != customerVm.Customer)
                    {
                        //Checks if current customer profile belongs to the system user
                        if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customerVm.Customer, ViewCodeKeys.UpdateAgentCustomerProfile))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                            return RedirectToAction("Index", "Error");
                        }
                    }
                    //agent
                    if ((agentId > 0))
                    {
                        customerVm.Agent = context.Agents.Find(agentId);
                        customerVm.Agent.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Agent.Id));
                        customerVm.Agent.Id = agentId.Value;
                        customerVm.Customer = context.Customers.Include(c => c.Status).SingleOrDefault(x => x.Id == customerVm.Agent.CustomerId);
                        if (customerVm.Customer != null)
                            customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Customer.Id));
                        customerVm.IsManagingAgent = true;

                        var isEntityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId);
                        var DepartmentKey = context.SystemUsers.Include(r => r.Department).FirstOrDefault(e => e.UserName == resName && e.IsActive && e.IsDeleted == false).Department?.Key;
                        var Entities = context.ApplicationEntities.ToList();
                        if (isEntityAgent != null)
                            customerVm.Entity = context.Entities.Find(isEntityAgent.EntityId);

                        if (DepartmentKey == Entities.FirstOrDefault(x => x.Key == ApplicationEntityKeys.EkurhuleniHousingCompany).Key)
                        {
                            return RedirectToAction("Inbox", "PropertyLeaseApplication");
                        }
                        else if (DepartmentKey == Entities.FirstOrDefault(x => x.Key == ApplicationEntityKeys.HumanSettlmentDevelopment).Key)
                        {
                            return RedirectToAction("Inbox", "HumanSettlementApplication");
                        }
                        else if (DepartmentKey == Entities.FirstOrDefault(x => x.Key == ApplicationEntityKeys.RealEstateDevelopment).Key)
                        {
                            return RedirectToAction("Inbox", "RealEstate");
                        }

                        return RedirectToAction("Inbox", "PropertyLeaseApplication");

                        return View("Index", "RCSApplication");
                        return View("IndexAgent", customerVm);
                    }

                    return View("Index", customerVm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        //Dashboard for profile when logging in
        public ActionResult Dashboard()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    int customerId;
                    int agentId;
                    var customer = new Customer();

                    _base.Initialise(context);

                    // JK.20160624 - Check if user is not null.
                    if (_base.SystemUser == null)
                        return RedirectToAction("Login", "Account");

                    var systemUserId = _base.SystemUser.Id;

                    if (systemUserId != -1)
                    {
                        customer = context.Customers.SingleOrDefault(x => x.SystemUserId == systemUserId);
                    }

                    if (customer == null)
                    {
                        customerId = 0;
                        agentId = 0;
                    }
                    else
                    {
                        customerId = customer.Id;
                        var agent = context.Agents.SingleOrDefault(x => x.CustomerId == customerId
                                    && x.IsActive && x.IsDeleted == false);
                        agentId = agent != null ? agent.Id : 0;
                    }

                    object obj = new { customerId = customerId, agentId = agentId };
                    return RedirectToAction("Index3", SecureActionLinkExtension.Encrypt(obj));
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }

        #region Profile Index GET
        //GET: /Profile/
 [Authorize(Roles = "Senior Housing Specialist, Regional Manager,Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]
        public ActionResult AccountRequests()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    int customerId;
                    int agentId;
                    var customer = new Customer();

                    _base.Initialise(context);
                    var systemUserId = _base.SystemUser.Id;

                    if (systemUserId != -1)
                    {
                        customer = context.Customers.SingleOrDefault(x => x.SystemUserId == systemUserId);
                    }

                    if (customer == null)
                    {
                        customerId = 0;
                        agentId = 0;
                    }
                    else
                    {
                        customerId = customer.Id;
                        var agent = context.Agents.SingleOrDefault(x => x.CustomerId == customerId
                                    && x.IsActive && x.IsDeleted == false);
                        agentId = agent != null ? agent.Id : 0;
                    }

                    object obj = new { customerId = customerId, agentId = agentId };
                    return RedirectToAction("Index4", SecureActionLinkExtension.Encrypt(obj));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region Profile AccountRequest Init GET
        [EncryptedActionParameter]
 [Authorize(Roles = "Senior Housing Specialist, Regional Manager,Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]

        public ActionResult Index4(int? agentId, int? customerId) // 
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var resName = User.Identity.GetUserName();
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    var customerVm = new CustomerProfileViewModel { ShowUpdateLink = true };
                    var customer = context.Customers.Include(c => c.SystemUser)
                   .Include(c => c.IdentificationType)
                   .Include(c => c.Status)
                   .Include(c => c.Country)
                   .SingleOrDefault(c => c.Id == customerId);

                    ViewBag.CusIDNo = context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                                                                          && e.IsActive && e.IsDeleted == false);
                    if (customer != null)
                    {
                        customerVm.Customer = customer;
                        customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customer.Id));
                        var isEntity = context.Entities.FirstOrDefault(e => e.CustomerId == customerId
                                                                        && e.IsActive && e.IsDeleted == false);

                        if (isEntity != null)
                            customerVm.Entity = isEntity;
                    }

                    //completely new systemuser
                    if ((customerId == 0))
                    {
                        var c = new Customer();
                        c.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", c.Id));
                        c.IsDeceased = false;
                        customerVm.Customer = c;
                        return View("AccountRequests", customerVm);
                    }

                    //profile info exists
                    if ((customerId != 0))
                    {
                        customerVm.IsManagingAgent = false;
                    }

                    if (null != customerVm.Customer)
                    {
                        //Checks if current customer profile belongs to the system user
                        if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customerVm.Customer, ViewCodeKeys.UpdateAgentCustomerProfile))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                            return RedirectToAction("Index", "Error");
                        }
                    }
                    //agent
                    if ((agentId > 0))
                    {
                        customerVm.Agent = context.Agents.Find(agentId);
                        customerVm.Agent.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Agent.Id));
                        customerVm.Agent.Id = agentId.Value;
                        customerVm.Customer = context.Customers.Include(c => c.Status).SingleOrDefault(x => x.Id == customerVm.Agent.CustomerId);
                        if (customerVm.Customer != null)
                            customerVm.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("id={0}", customerVm.Customer.Id));
                        customerVm.IsManagingAgent = true;

                        var isEntityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId);

                        if (isEntityAgent != null)
                            customerVm.Entity = context.Entities.Find(isEntityAgent.EntityId);

                        return View("AccountRequests", customerVm);
                    }

                    return View("AccountRequests", customerVm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region LoadCustomer (Integration)

        [HttpPost]
        //[ValidateAntiForgeryToken] - TODO: Need to look into this
 [Authorize(Roles = "Senior Housing Specialist, Regional Manager,Housing Liaison Officer,Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer")]
        public ActionResult LoadCustomer(Int64 identityOrAccountNo)
        {
            try
            {
                if (identityOrAccountNo > 0)
                {
                    using (var cxt = new eServicesDbContext())
                    {
                        _base.Initialise(cxt);
                        _systemUserId = _base.SystemUser.Id;

                        var custAcc = cxt.CustomerAccounts.FirstOrDefault(o => o.IDNo == identityOrAccountNo || o.AccountNo == identityOrAccountNo);
                        var cust = cxt.Customers.SingleOrDefault(o => o.SystemUserId == _systemUserId);
                        int c = 0;
                        int p = 0;
                        if (cust == null && custAcc != null)
                        {
                            cust = new Customer()
                            {
                                SystemUserId = _systemUserId,
                                FirstName = custAcc.Initials,
                                LastName = custAcc.Surname,
                                IdentificationNumber = custAcc.IDNo.ToString(),
                                IdentificationTypeId = cxt.IdentificationTypes.SingleOrDefault(o => o.Key == IdentificationTypeKey.SouthAfricanID).Id,
                                HomePhoneNumber = custAcc.HomeTel,
                                WorkPhoneNumber = custAcc.WorkTel,
                                CellPhoneNumber = custAcc.CellNo,
                                EmailAddress = custAcc.EmailAddr.Trim() == string.Empty ? _base.SystemUser.EmailAddress : custAcc.EmailAddr,
                                PhysicalAddress1 = custAcc.StreetNo,
                                PhysicalAddress2 = custAcc.StreetAddr,
                                PhysicalAddress3 = custAcc.PostalAddress1,
                                PhysicalAddress4 = custAcc.PostalAddress2,
                                PhysicalAddress5 = custAcc.PostalAddress3,
                                PhysicalAddressCode = int.TryParse(custAcc.PostalCode, out c) ? c : 0,
                                PostalAddress1 = custAcc.SecondaryPostalAddress1,
                                PostalAddress2 = custAcc.SecondaryPostalAddress2,
                                PostalAddress3 = custAcc.SecondaryPostalAddress3,
                                PostalAddress4 = custAcc.SecondaryPostalAddress4,
                                PostalAddress5 = custAcc.SecondaryPostalAddress5,
                                PostalAddressCode = int.TryParse(custAcc.SecondaryPostalCode, out p) ? p : 0,
                                TitleTypeId = cxt.TitleTypes.SingleOrDefault(o => o.Key == TitleTypeKeys.Mister).Id,
                                CustomerTypeId = cxt.CustomerTypes.SingleOrDefault(o => o.Key == CustomerTypeKeys.Individual).Id,
                                StatusId = cxt.Status.SingleOrDefault(o => o.Key == StatusKeys.CustomerActive).Id,
                                IsDeceased = false,
                                IsActive = true
                            };

                            cxt.Customers.Add(cust);
                            cxt.SaveChanges();
                        }
                        else if(cust == null && custAcc == null)
                        {
                            var result = "Failed";
                            return Json(result, JsonRequestBehavior.AllowGet);
                            
                           
                        }
                        else ViewBag.CustomerMessage = "A customer profile already exists for this login.";

                        object obj = new { agentId = 0, customerId = cust.Id, viewId = 0 };
                        TempData["Success"] = true;
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Successfully retrieved customer information .";
                        var redirectUrl = new UrlHelper(Request.RequestContext).Action("ManageProfile", "Profile", SecureActionLinkExtension.Encrypt(obj));

                        return Json(new { Url = redirectUrl, status = "OK" });
                    }
                }

                return Json(new { status = "Error" });
            }
            catch (Exception x)
            {
                System.Diagnostics.Debug.WriteLine(x.ToString());
                return View("_Error");
            }
        }

        #endregion LoadCustomer


        #region Profile Load Accounts
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult LoadAccounts(int? Id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);
                _systemUserId = _base.SystemUser.Id;

                var customer = context.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.Id == Id.Value);
                if (customer == null) throw new Exception("Invalid customer");

                if (!context.AgentCustomers.Any(ac => ac.CustomerId == Id.Value))
                {
                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, ViewCodeKeys.IndividualCustomerProfile))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }
                }

                var query = context.LinkedAccounts.Include(x => x.Account).Include(x => x.Customer)
                .Include(x => x.Customer.Status)
                .Include(x => x.Status)
                .Where(x => x.CustomerId == Id.Value && x.Customer.IsActive
                && x.Customer.Status.Key == StatusKeys.CustomerActive && x.IsActive && x.IsDeleted == false);

                var accounts = query.ToList();

                foreach (var acc in accounts)
                {
                    acc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", acc.Id)));
                }

                var json = Json(new { data = accounts }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;

                return json;
            }
        }
        #endregion
        #region Profile Load Account Requests
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult LoadAccountReqs(int? Id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);
                _systemUserId = _base.SystemUser.Id;

                var customer = context.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.Id == Id.Value);
                if (customer == null) throw new Exception("Invalid customer");

                if (!context.AgentCustomers.Any(ac => ac.CustomerId == Id.Value))
                {
                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, ViewCodeKeys.IndividualCustomerProfile))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }
                }

                var query = context.Requests.Include(x => x.Comment).Include(x => x.Customer)
                .Include(x => x.Status)
                .Where(x => x.CustomerId == Id.Value && x.Customer.IsActive
                 && (x.Status.Key == StatusKeys.LinkedAccountPending || x.Status.Key == StatusKeys.IncentivePolicyApplicationDeclined) && x.IsActive && x.IsDeleted == false);
                //&& x.Status.Key == StatusKeys.LinkedAccountPending
                var accountReqs = query.OrderByDescending(o => o.Id).ToList();

                foreach (var acc in accountReqs)
                {
                    acc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", acc.Id)));
                }

                var json = Json(new { data = accountReqs }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;

                return json;
            }
        }
        #endregion
        #region Profile Load Account Requests Received
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult LoadAccountReqRecs(int? Id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);
                _systemUserId = _base.SystemUser.Id;

                var customer = context.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.Id == Id.Value);
                if (customer == null) throw new Exception("Invalid customer");

                if (!context.AgentCustomers.Any(ac => ac.CustomerId == Id.Value))
                {
                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, ViewCodeKeys.IndividualCustomerProfile))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }
                }

                var query = context.Requests.Include(x => x.Comment).Include(x => x.Customer)
                .Include(x => x.Status)
                .Where(x => x.RecipientCustomerId == Id.Value && x.Customer.IsActive
                && x.Status.Key == StatusKeys.LinkedAccountPending && x.IsActive && x.IsDeleted == false);

                var accountReqs = query.OrderByDescending(o => o.Id).ToList();

                foreach (var acc in accountReqs)
                {
                    acc.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", acc.Id)));
                }

                var json = Json(new { data = accountReqs }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;

                return json;
            }
        }
        #endregion
        #region Profile Load Customers
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult LoadCustomers(int? Id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);
                _systemUserId = _base.SystemUser.Id;
                var agent = context.Agents.Include(c => c.Customer).FirstOrDefault(a => a.Id == Id.Value);

                if (agent == null) throw new Exception("Invalid Agent");

                //Checks if current customer profile belongs to the system user
                if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, agent.Customer, ViewCodeKeys.IndividualCustomerProfile))
                {
                    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    return RedirectToAction("Index", "Error");

                }

                var customers = (from cus in context.Customers
                                 join ag in context.AgentCustomers on cus.Id equals ag.CustomerId
                                 where ag.AgentId == Id
                                 select cus).ToList();
                List<Customer> cList = customers;

                foreach (var c in cList)
                {
                    c.DataList = new List<string>
                    {
                        HttpUtility.UrlEncode(
                            SecureActionLinkExtension.Encrypt(string.Format("customerId={0}??agentId={1}??viewId=1",
                                c.Id, Id))),
                        HttpUtility.UrlEncode(
                            SecureActionLinkExtension.Encrypt(string.Format("customerId={0}??agentId={1}??viewId=1",
                                c.Id, Id))),
                        HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", c.Id))),
                        c.FullName
                    };
                    c.Status = context.Status.FirstOrDefault(s => s.Id == c.StatusId && s.IsActive && !s.IsDeleted);
                }

                var json = Json(new { data = cList }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;

                return json;
            }
        }
        #endregion


        #region Profile Linked Accounts
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult LinkedAccounts(int? customerId, int? agentId)
        {
            agentId = agentId ?? 0;
            object obj = new { customerId, agentId };
            return RedirectToAction("Index", "CustomerLinkedAccount", SecureActionLinkExtension.Encrypt(obj));
        }
        #endregion


        #region Profile Linked Account Details
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult LinkedAccountsDetails(int? Id)
        {
            return RedirectToAction("EditLinkedAccount", "CustomerLinkedAccount", SecureActionLinkExtension.Encrypt(new { Id }));
        }
        #endregion

        #region Delink Accounts

        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult DelinkAccount(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    var la = cxt.LinkedAccounts.FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
                    la.StatusId = cxt.Status.FirstOrDefault(o=>o.Key == StatusKeys.LinkedAccountUnlinked).Id;
                    cxt.Entry(la).State = EntityState.Modified;
                    cxt.SaveChanges();

                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == _base.SystemUser.Id && o.IsActive && !o.IsDeleted);
                    var agent = cxt.Agents.FirstOrDefault(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted);

                    object obj = new { customerId = cust.Id, agentId = agent != null ? agent.Id : 0 };
                    return RedirectToAction("Index2", SecureActionLinkExtension.Encrypt(obj));
                }

                return null;
            }
        }

        #endregion


        #region Profile Add Customer Link
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult AddCustomer(int? id)
        {
            return RedirectToAction("ManageProfile", "Profile", new { agentId = id });
        }
        #endregion


        #region Profile Manage Profile GET
        //View Codes
        //1 - Edit Agent Customer Details
        //2 - Create Third Party Profile (Rates Rebate)
        //3 - Add Agent Customer
        //4 - Create Customer Profile, Individual (Rates Rebate)
        [HttpGet]
        [EncryptedActionParameter]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult ManageProfile(int? agentId, int? customerId, int? viewId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    Agent agent = null;
                    Customer customer = null;
                    EntityAgent entityAgent = null;
                    Entity entity = null;
                    DocumentsViewModel dvm = null;
                    ReferenceType documentReferenceType = null;
                    var customerDetails = new List<object>();
                    var customerDocuments = new List<Document>();
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                    var customerTypes = context.CustomerTypes.Where(l => l.IsActive && l.IsDeleted == false);
                    var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                            .Include(d => d.ReferenceType)
                                            .Where(dc => dc.ApplicationId == application.Id &&
                                            dc.ReferenceTypeId == documentReferenceType.Id);
                    var resName = User.Identity.GetUserName();
                    ViewBag.CusManIDNo = context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                                                                          && e.IsActive && e.IsDeleted == false);
                    if (application == null) throw new Exception("Invalid Application");
                    if (documentReferenceType == null) throw new Exception("Invalid Document reference type");

                    if (agentId != null && agentId != 0)
                    {
                        agent = context.Agents.Find(agentId);
                        entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);
                    }

                    if (customerId != null && customerId != 0)
                    {
                        customer = context.Customers.Include(c => c.CustomerType)
                                   .Include(c => c.IdentificationType)
                                   .Include(c => c.Country)
                                   .Include(c => c.SystemUser)
                                   .Include(c => c.Status)
                                   .Include(c => c.TitleType)
                                   .Include(c => c.SystemUser)
                                   .SingleOrDefault(c => c.Id == customerId);

                        if (customer == null) throw new Exception("Invalid customer");

                        entity = (customer.CustomerType.Key == CustomerTypeKeys.Entity) ? context.Entities.FirstOrDefault(e => e.CustomerId == customerId)
                        : (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent && entityAgent != null) ? context.Entities.FirstOrDefault(e => e.Id == entityAgent.EntityId)
                        : null;

                        customerDetails.Add(new
                        {
                            customerTypeId = customer.CustomerType.Key,
                            entityTypeId = (entity != null) ? entity.EntityTypeId : 0,
                            identificationTypeId = customer.IdentificationType.Key,
                            countryOfIssueTypeId = customer.CountryOfIssueTypeId ?? 0,
                            titleTypeId = customer.TitleTypeId,
                            emailAddress = customer.EmailAddress,
                            isDeceased = customer.IsDeceased,
                            gender = customer.Gender,
                            identificationNo=customer.IdentificationNumber
                        });

                        customerDocuments = context.Documents.Include(d => d.File)
                                            .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                        && d.IsActive
                        && d.IsDeleted == false).ToList();

                        foreach (var doc in customerDocuments)
                        {
                            doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                        }

                        dvm = new DocumentsViewModel
                        {
                            ApplicationId = application.Id,
                            CustomerId = customer.Id,
                            Documents = customerDocuments,
                            IsUploadView = false,
                            DocumentCheckLists = documentCheckLists.ToList(),
                        };
                    }

                    var pvm = new CustomerProfileViewModel
                    {
                        IsManagingAgent = agent != null,
                        Agent = agent,
                        Customer = customer,
                        Entity = entity,
                        CustomerTypes = context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(),
                        CustomerDocuments = customerDocuments,
                        Document = dvm,
                        DocumentsVerified = customerDocuments.Any(c => c.IsLocked != null && (bool)c.IsLocked)
                    };

                    if (viewId == ViewCodeKeys.CreateAgentCustomerProfile || viewId == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile)
                    {
                        customerTypes = customerTypes.Where(l => l.Key != CustomerTypeKeys.ManagingAgent);
                        documentReferenceType =
                            context.ReferenceTypes.FirstOrDefault(
                                r => r.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent);
                        if (null == documentReferenceType) throw new Exception(string.Format("Invalid/ Missing Reference Type Key ({0})", ReferenceTypeKeys.LinkedAccountsManagingAgent));
                    }

                    if (null != pvm.Customer)
                    {
                        //Checks if current customer profile belongs to the system user
                        if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, pvm.Customer, ViewCodeKeys.IndividualCustomerProfile))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                            return RedirectToAction("Index", "Error");

                        }
                    }

                    ViewBag.ReferenceTypeId = documentReferenceType.Id;
                    ViewBag.ApplicationId = application.Id;
                    ViewBag.CustomerDetails = customerDetails;
                    ViewBag.ViewCode = viewId;
                    ViewBag.ViewSuccess = true;
                    ViewBag.CustomerTypeId = new SelectList(customerTypes.ToList(), "Key", "Name");
                    ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                    ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(i => i.IsActive && !i.IsDeleted).ToList(), "Key", "Name");
                    ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).ToList().OrderBy(c => c.Name), "Id", "Name");
                    ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");

                    return View(pvm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region Profile Manage Profile POST
        //View Codes
        //1 - Edit Agent Customer Details
        //2 - Create Third Party Profile (Rates Rebate)
        //3 - Add Agent Customer
        //4 - Create Customer Profile, Individual (Rates Rebate)
        [HttpPost]
        [ValidateAntiForgeryToken]
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult ManageProfile(CustomerProfileViewModel customerviewmodel, string CustomerTypeId, string EntityTypeId, string IdentificationTypeId,
            string CountryOfIssueTypeId, string TitleTypeId, string IsDeceased, string emailAddress, int? ViewCode)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var captchaHelper = new CaptchaHelper();
                    var captchaResponse = Request.Params["g-recaptcha-response"];

                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    var customer = customerviewmodel.Customer;
                    bool recaptchaval = true;
                    var customerId = customer.Id;
                    var customerSystemUser = context.Customers.AsNoTracking().FirstOrDefault(sc => sc.SystemUserId == _systemUserId);
                    var sysUserOwnershipCode = (ViewCode == ViewCodeKeys.CreateAgentCustomerProfile) ? ViewCodeKeys.CreateAgentCustomerProfile : ViewCodeKeys.IndividualCustomerProfile;

                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, sysUserOwnershipCode))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }

                    if (/*captchaHelper.ValidateCaptcha(captchaResponse)*/recaptchaval == true)
                    {
                        var customerType = context.CustomerTypes.FirstOrDefault(ct => ct.Key == CustomerTypeId);
                        var customerActiveStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.CustomerActive);
                        var customerPendingStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.CustomerPendingDocuments);
                        var identificationType = context.IdentificationTypes.FirstOrDefault(it => it.Key == IdentificationTypeKey.SouthAfricanID);
                        var agentStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.AgentActive);
                        var entityStatus = context.Status.FirstOrDefault(st => st.Key == StatusKeys.EntityActive);

                        if (customerType == null) throw new Exception("Invalid Customer type");
                        if (customerActiveStatus == null) throw new Exception("Invalid Status");
                        if (customerPendingStatus == null) throw new Exception("Invalid Status");
                        if (identificationType == null) throw new Exception("Invalid Identification type");
                        if (agentStatus == null) throw new Exception("Invalid status");
                        if (entityStatus == null) throw new Exception("Invalid status");

                        var titleTypeId = Convert.ToInt32(TitleTypeId);
                        var countryOfIssueTypeId = (identificationType.Key == IdentificationTypeKey.SouthAfricanID) ? (int?)null : Convert.ToInt32(CountryOfIssueTypeId);
                        var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                        var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.CustomerProfile));
                        var entityTypeId = (EntityTypeId != "") ? Convert.ToInt32(EntityTypeId) : 0;

                        // JK.20190619a - Check what triggers this to be false.
                        ViewSuccess = true;

                        if (application == null) throw new Exception("Invalid Application");
                        if (referenceType == null) throw new Exception("Invalid Reference type");

                        customer.EmailAddress = emailAddress;
                        customer.IsDeceased = Convert.ToBoolean(IsDeceased);
                        customer.CustomerTypeId = customerType.Id;
                        customer.IdentificationTypeId = identificationType.Id;
                        customer.TitleTypeId = titleTypeId;
                        customer.CountryOfIssueTypeId = countryOfIssueTypeId;
                        customer.StatusId = customerPendingStatus.Id;
                        customer.IsActive = true;
                        customer.IsDeleted = false;
                        customer.IsLocked = false;

                        var agent = (ViewCode == ViewCodeKeys.CreateAgentCustomerProfile || ViewCode == ViewCodeKeys.UpdateAgentCustomerProfile) ? context.Agents.AsNoTracking().FirstOrDefault(a => a.CustomerId == customerSystemUser.Id) :
                                     context.Agents.AsNoTracking().FirstOrDefault(a => a.CustomerId == customer.Id);

                        if (agent != null)
                        {
                            customerviewmodel.Agent = agent;
                            customerviewmodel.IsManagingAgent = true;
                        }
                        else
                        {
                            customerviewmodel.Agent = new Agent();
                        }

                        switch (customerType.Key)
                        {
                            case CustomerTypeKeys.ManagingAgent:
                                SaveCustomer(customer, customerSystemUser, ViewCode);

                                SaveAgent(customerviewmodel.Agent, customer, agentStatus);

                                if (customer.Id != 0)
                                {
                                    //customerviewmodel.Entity.Id = SaveEntity(customerviewmodel.Entity, customer, entityTypeId, entityStatus);
                                    //SaveEntityAgent(customerviewmodel.Entity, customerviewmodel.Agent);
                                }

                                SendApprovalNotification(customer);
                                break;
                            case CustomerTypeKeys.Entity:
                                SaveCustomer(customer, customerSystemUser, ViewCode);

                                if (customer.Id != 0)
                                    //SaveEntity(customerviewmodel.Entity, customer, entityTypeId, entityStatus);

                                SendApprovalNotification(customer);
                                break;
                            default:
                                if (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile ||
                                    ViewCode == ViewCodeKeys.CreateAgentCustomerProfile)
                                {
                                    customer.StatusId = customerPendingStatus.Id;
                                    SaveCustomer(customer, customerSystemUser, ViewCode);

                                    SendApprovalNotification(customer);
                                }
                                else
                                {
                                    customer.StatusId = customerActiveStatus.Id;
                                    SaveCustomer(customer, customerSystemUser, ViewCode);
                                }
                                break;
                        }

                        context.SaveChanges();

                        if (customerviewmodel.IsManagingAgent && (ViewCode == ViewCodeKeys.CreateAgentCustomerProfile || ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile) && customer.Id > 0)
                        {
                            SaveAgentCustomer(customerviewmodel.Agent, customer);
                        }

                        if (ViewSuccess)
                        {
                            var customerDocuments = context.Documents.Include(d => d.File)
                                                    .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == referenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false).ToList();

                            if (!customerDocuments.Any())
                            {
                                var success = (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? Url.Action("Create", "RatesRebate", SecureActionLinkExtension.Encrypt(new { customerId = customer.Id })) : Url.Action("Index2", SecureActionLinkExtension.Encrypt(new { agentId = customerviewmodel.Agent.Id, customerId = (customerviewmodel.Agent.Id != 0) ? customerviewmodel.Agent.CustomerId : customer.Id }));
                                if (success != null)
                                {
                                    if (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile ||
                                        ViewCode == ViewCodeKeys.CreateAgentCustomerProfile ||
                                        ViewCode == ViewCodeKeys.UpdateAgentCustomerProfile)
                                    {
                                        referenceType =
                                            context.ReferenceTypes.FirstOrDefault(
                                                r =>
                                                    r.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent && r.IsActive &&
                                                    !r.IsDeleted);
                                        if (null == referenceType) throw new Exception(string.Format("Invalid/ Missing Reference key ({0})", ReferenceTypeKeys.LinkedAccountsManagingAgent));
                                    }
                                    var returnUrl = success.ToString(CultureInfo.InvariantCulture);
                                    return RedirectToAction("Register", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = customer.Id, customerId = customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = customerviewmodel.Agent.Id, returnUrl = returnUrl })));
                                }
                            }
                            else
                            {
                                return (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? RedirectToAction("Create", "RatesRebate", SecureActionLinkExtension.Encrypt(new { customerId = customer.Id })) : RedirectToAction("Index2", SecureActionLinkExtension.Encrypt(new { agentId = customerviewmodel.Agent.Id, customerId = customer.Id }));
                            }
                        }
                        else
                        {
                            return RedirectToAction("Capture", "Capture");
                            //C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("refNo=" + item.Id.ToString() + "&" + "id=
                            AesCrypto aes = new AesCrypto();
                            string para = aes.Encrypt("agentId=0&customerId=0&viewId=0");
                            return RedirectToAction("ManageProfile", "Profile");
                            //RedirectToAction( ManageProfile(0, 0, 0);
                            ViewBag.CustomerTypeId = new SelectList(context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Key", "Name");
                            ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                            ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Key", "Name");
                            ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).OrderBy(c => c.Name).ToList(), "Id", "Name");
                            ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");
                            ViewBag.CustomerDetails = new List<object>();
                            ViewBag.ViewCode = ViewCode;
                            ViewBag.ViewSuccess = ViewSuccess;
                            ViewBag.ReturnUrl = (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? Url.Action("Create", "RatesRebate", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { customerId = customer.Id }))) : Url.Action("Index2", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { agentId = (customerviewmodel.Agent != null) ? customerviewmodel.Agent.Id : 0, customerId = (customer.Id != 0) ? customer.Id : context.Agents.Find(customerviewmodel.Agent.Id).CustomerId })));
                            return View(customerviewmodel);
                        }
                    }
                    else
                    {
                        return RedirectToAction("Capture", "Capture");
                   
                        Agent agent = null;
                        int viewId = 0;
                        EntityAgent entityAgent = null;
                        Entity entity = null;
                        DocumentsViewModel dvm = null;
                        ReferenceType documentReferenceType = null;
                        //var customerId = customer.id;
                        var customerDetails = new List<object>();
                        var customerDocuments = new List<Document>();
                        var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                        documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                        var customerTypes = context.CustomerTypes.Where(l => l.IsActive && l.IsDeleted == false);
                        var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                                                .Include(d => d.ReferenceType)
                                                .Where(dc => dc.ApplicationId == application.Id &&
                                                dc.ReferenceTypeId == documentReferenceType.Id);
                        var resName = User.Identity.GetUserName();
                        ViewBag.CusManIDNo = context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                                                                              && e.IsActive && e.IsDeleted == false);
                        var cus = customer.Id;
                        if ( customerId != 0)
                        {
                            customer = context.Customers.Include(c => c.CustomerType)
                                       .Include(c => c.IdentificationType)
                                       .Include(c => c.Country)
                                       .Include(c => c.SystemUser)
                                       .Include(c => c.Status)
                                       .Include(c => c.TitleType)
                                       .Include(c => c.SystemUser)
                                       .SingleOrDefault(c => c.Id == customerId);

                            if (customer == null) throw new Exception("Invalid customer");

                            entity = (customer.CustomerType.Key == CustomerTypeKeys.Entity) ? context.Entities.FirstOrDefault(e => e.CustomerId == customerId)
                            : (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent && entityAgent != null) ? context.Entities.FirstOrDefault(e => e.Id == entityAgent.EntityId)
                            : null;

                            customerDetails.Add(new
                            {
                                customerTypeId = customer.CustomerType.Key,
                                entityTypeId = (entity != null) ? entity.EntityTypeId : 0,
                                identificationTypeId = customer.IdentificationType.Key,
                                countryOfIssueTypeId = customer.CountryOfIssueTypeId ?? 0,
                                titleTypeId = customer.TitleTypeId,
                                emailAddress = customer.EmailAddress,
                                isDeceased = customer.IsDeceased,
                                gender = customer.Gender,
                                identificationNo = customer.IdentificationNumber
                            });

                            customerDocuments = context.Documents.Include(d => d.File)
                                                .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                            && d.IsActive
                            && d.IsDeleted == false).ToList();

                            foreach (var doc in customerDocuments)
                            {
                                doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                            }

                            dvm = new DocumentsViewModel
                            {
                                ApplicationId = application.Id,
                                CustomerId = customer.Id,
                                Documents = customerDocuments,
                                IsUploadView = false,
                                DocumentCheckLists = documentCheckLists.ToList(),
                            };
                        }
                        var pvm = new CustomerProfileViewModel
                        {
                            IsManagingAgent = agent != null,
                            Agent = agent,
                            Customer = customer,
                            Entity = entity,
                            CustomerTypes = context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(),
                            CustomerDocuments = customerDocuments,
                            Document = dvm,
                            DocumentsVerified = customerDocuments.Any(c => c.IsLocked != null && (bool)c.IsLocked)
                        };

                        if (viewId == ViewCodeKeys.CreateAgentCustomerProfile || viewId == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile)
                        {
                            customerTypes = customerTypes.Where(l => l.Key != CustomerTypeKeys.ManagingAgent);
                            documentReferenceType =
                                context.ReferenceTypes.FirstOrDefault(
                                    r => r.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent);
                            if (null == documentReferenceType) throw new Exception(string.Format("Invalid/ Missing Reference Type Key ({0})", ReferenceTypeKeys.LinkedAccountsManagingAgent));
                        }

                        if (null != pvm.Customer)
                        {
                            //Checks if current customer profile belongs to the system user
                            if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, pvm.Customer, ViewCodeKeys.IndividualCustomerProfile))
                            {
                                SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                                return RedirectToAction("Index", "Error");

                            }
                        }

                        ViewBag.ReferenceTypeId = documentReferenceType.Id;
                        ViewBag.ApplicationId = application.Id;
                        ViewBag.CustomerDetails = customerDetails;
                        ViewBag.ViewCode = viewId;
                        ViewBag.ViewSuccess = true;
                        ViewBag.CustomerTypeId = new SelectList(customerTypes.ToList(), "Key", "Name");
                        ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                        ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(i => i.IsActive && !i.IsDeleted).ToList(), "Key", "Name");
                        ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).ToList().OrderBy(c => c.Name), "Id", "Name");
                        ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");
                        return View(pvm);



                        return RedirectToAction("ManageProfile", "Profile");
                        //RedirectToAction( ManageProfile(0, 0, 0);
                        ViewBag.CustomerTypeId = new SelectList(context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Key", "Name");
                        ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                        ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Key", "Name");
                        ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).OrderBy(c => c.Name).ToList(), "Id", "Name");
                        ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");
                        ViewBag.CustomerDetails = new List<object>();
                        ViewBag.ViewCode = ViewCode;
                        ViewBag.ViewSuccess = ViewSuccess;
                        ViewBag.ReturnUrl = (ViewCode == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile || ViewCode == ViewCodeKeys.IndividualCustomerProfile) ? Url.Action("Create", "RatesRebate", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { customerId = customer.Id }))) : Url.Action("Index2", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { agentId = (customerviewmodel.Agent != null) ? customerviewmodel.Agent.Id : 0, customerId = (customer.Id != 0) ? customer.Id : context.Agents.Find(customerviewmodel.Agent.Id).CustomerId })));
                        return View(customerviewmodel);
                    }

                    // else
                    // {
                    //Agent agent = null;
                    //int agentId = 0;
                    //var Custagent = context.Agents.SingleOrDefault(x => x.CustomerId == customer.Id
                    //  && x.IsActive && x.IsDeleted == false);
                    //agentId = Custagent != null ? Custagent.Id : 0;
                    //return RedirectToAction("ManageProfile", new { id = 99 });
                    //Entity entity = new Entity();
                    //DocumentsViewModel dvm = new DocumentsViewModel();
                    ////ReferenceType documentReferenceType = new ReferenceType();
                    //var customerDetails = new List<object>();
                    //var applic= context.Applications.Where(x => x.Key == ApplicationKeys.LinkedAccounts).FirstOrDefault();
                    ////var application = context.Applications.Where(x => x.Key == ApplicationKeys.LinkedAccounts).FirstOrDefault();
                    //var customerDocuments = new List<Document>();
                    ////var application = context.Applications.FirstOrDefault(a => a.Key == ApplicationKeys.LinkedAccounts);
                    //ReferenceType documentReferenceType = context.ReferenceTypes.SingleOrDefault(r => r.Key == ReferenceTypeKeys.CustomerProfile);
                    //var customerTypes = context.CustomerTypes.Where(l => l.IsActive && l.IsDeleted == false);
                    //var documentCheckLists = context.DocumentCheckLists.Include(d => d.DocumentType)
                    //                        .Include(d => d.ReferenceType)
                    //                        .Where(dc => dc.ApplicationId == applic.Id &&
                    //                        dc.ReferenceTypeId == documentReferenceType.Id);
                    //var resName = User.Identity.GetUserName();
                    //ViewBag.CusManIDNo = context.SystemUsers.FirstOrDefault(e => e.UserName == resName
                    //                                                      && e.IsActive && e.IsDeleted == false);
                    //if (applic == null) throw new Exception("Invalid Application");
                    //if (documentReferenceType == null) throw new Exception("Invalid Document reference type");
                    //var Custagent = context.Agents.SingleOrDefault(x => x.CustomerId == customer.Id
                    //        && x.IsActive && x.IsDeleted == false);
                    //agentId = Custagent != null ? Custagent.Id : 0;
                    //Agent agent = new Agent();
                    //agent = Custagent;
                    //EntityAgent entityAgent = new EntityAgent();
                    //if (agent.Id != null && agentId != 0)
                    //{
                    //    agent = context.Agents.Find(agentId);
                    //    entityAgent = context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agent.Id);
                    //}

                    //if (customer.Id != null && customer.Id != 0)
                    //{
                    //    customer = context.Customers.Include(c => c.CustomerType)
                    //               .Include(c => c.IdentificationType)
                    //               .Include(c => c.Country)
                    //               .Include(c => c.SystemUser)
                    //               .Include(c => c.Status)
                    //               .Include(c => c.TitleType)
                    //               .Include(c => c.SystemUser)
                    //               .SingleOrDefault(c => c.Id == customer.Id);

                    //    if (customer == null) throw new Exception("Invalid customer");

                    //    entity = (customer.CustomerType.Key == CustomerTypeKeys.Entity) ? context.Entities.FirstOrDefault(e => e.CustomerId == customer.Id)
                    //    : (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent && entityAgent != null) ? context.Entities.FirstOrDefault(e => e.Id == entityAgent.EntityId)
                    //    : null;

                    //    customerDetails.Add(new
                    //    {
                    //        customerTypeId = customer.CustomerType.Key,
                    //        entityTypeId = (entity != null) ? entity.EntityTypeId : 0,
                    //        identificationTypeId = customer.IdentificationType.Key,
                    //        countryOfIssueTypeId = customer.CountryOfIssueTypeId ?? 0,
                    //        titleTypeId = customer.TitleTypeId,
                    //        emailAddress = customer.EmailAddress,
                    //        isDeceased = customer.IsDeceased,
                    //        gender = customer.Gender,
                    //        identificationNo = customer.IdentificationNumber
                    //    });

                    //    customerDocuments = context.Documents.Include(d => d.File)
                    //                        .Where(d => d.CustomerId == customer.Id && d.ReferenceType.Id == documentReferenceType.Id
                    //    && d.IsActive
                    //    && d.IsDeleted == false).ToList();

                    //    foreach (var doc in customerDocuments)
                    //    {
                    //        doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                    //    }

                    //    dvm = new DocumentsViewModel
                    //    {
                    //        ApplicationId = applic.Id,
                    //        CustomerId = customer.Id,
                    //        Documents = customerDocuments,
                    //        IsUploadView = false,
                    //        DocumentCheckLists = documentCheckLists.ToList(),
                    //    };
                    //}

                    //var pvm = new CustomerProfileViewModel
                    //{
                    //    IsManagingAgent = agent != null,
                    //    Agent = agent,
                    //    Customer = customer,
                    //    Entity = entity,
                    //    CustomerTypes = context.CustomerTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(),
                    //    CustomerDocuments = customerDocuments,
                    //    Document = dvm,
                    //    DocumentsVerified = customerDocuments.Any(c => c.IsLocked != null && (bool)c.IsLocked)
                    //};

                    ////if (viewId == ViewCodeKeys.CreateAgentCustomerProfile || viewId == ViewCodeKeys.RatesRebateThirdPartyCustomerProfile)
                    ////{
                    ////    customerTypes = customerTypes.Where(l => l.Key != CustomerTypeKeys.ManagingAgent);
                    ////    documentReferenceType =
                    ////        context.ReferenceTypes.FirstOrDefault(
                    ////            r => r.Key == ReferenceTypeKeys.LinkedAccountsManagingAgent);
                    ////    if (null == documentReferenceType) throw new Exception(string.Format("Invalid/ Missing Reference Type Key ({0})", ReferenceTypeKeys.LinkedAccountsManagingAgent));
                    ////}

                    //if (null != pvm.Customer)
                    //{
                    //    //Checks if current customer profile belongs to the system user
                    //    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, pvm.Customer, ViewCodeKeys.IndividualCustomerProfile))
                    //    {
                    //        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    //        return RedirectToAction("Index", "Error");

                    //    }
                    //}

                    //ViewBag.ReferenceTypeId = documentReferenceType.Id;
                    //ViewBag.ApplicationId = applic.Id;
                    //ViewBag.CustomerDetails = customerDetails;
                    //ViewBag.ViewCode = 0;
                    //ViewBag.ViewSuccess = true;
                    //ViewBag.CustomerTypeId = new SelectList(customerTypes.ToList(), "Key", "Name");
                    //ViewBag.EntityTypeId = new SelectList(context.EntityTypes.Where(et => et.IsActive && et.IsDeleted == false).ToList(), "Id", "Name");
                    //ViewBag.IdentificationTypeId = new SelectList(context.IdentificationTypes.Where(i => i.IsActive && !i.IsDeleted).ToList(), "Key", "Name");
                    //ViewBag.CountryOfIssueTypeId = new SelectList(context.Countries.Where(o => o.IsActive && o.IsDeleted == false).ToList().OrderBy(c => c.Name), "Id", "Name");
                    //ViewBag.TitleTypeId = new SelectList(context.TitleTypes.Where(o => o.IsActive && o.IsDeleted == false).ToList(), "Id", "Name");

                    //  }

                    return View();
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }

        }
        #endregion


        public JsonResult GetAttorneyDetails(string id)
        {
            AttorneyDetails result = new AttorneyDetails();
            string Err;
            try
            {
                AttorneyDetailsApi API = new AttorneyDetailsApi();
                result = API.AttorneyDetails(id);

                dynamic data = JObject.Parse(result.Json);
                //if(result.AttorneyCode == null || result.AttorneyCode == "")
                //{
                //    result.Status = "Attorney Code Not Found";
                //}
                var tester = Json(result, JsonRequestBehavior.AllowGet);
                //var js = JObject.Parse(result.Json);
                //var testd = Json(js, JsonRequestBehavior.AllowGet);
                //var test = testd.Data;
                //var NextTest = Json(test, JsonRequestBehavior.AllowGet);
                //var stat = data.StatusMessages;
                //var statt = data.StatusMessages.slice(1);
                //result.StatusMessages[0]
                //var test3 = test.AttorneyCode;
                return tester;
                //var AttorneyCodeDuplicate = db.Customers.Where(x => x.AttorneyCode == id).ToList();
                //if (AttorneyCodeDuplicate.Count > 0)
                //{
                //    result = "Duplicate";
                //}
                //else
                //{
                //    result = "Unique";
                //    //var AttorneyDetails = RCSAttorneyApi.GetAttorneyDetails(id.Trim()).ToList();
                //    //result = AttorneyDetails;
                //}
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                object result2 = "Error";
                return Json(result2, JsonRequestBehavior.AllowGet);
                Err = "Error";
            }
            return Json(Err, JsonRequestBehavior.AllowGet);
        }


        #region Profile Verify Entity
        [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public JsonResult VerifyEntity(string custType, string entityRegNo)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                //entity
                var entity = context.Entities.FirstOrDefault(e => e.EntityRegistrationNumber == entityRegNo);

                if (entity != null)
                {
                    return Json(entity, JsonRequestBehavior.AllowGet);
                }

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Profile Save Customer
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        private void SaveCustomer(Customer customer, Customer currentSystemUser, int? viewCode)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    var systemUser = User.Identity.GetUserId();
                    var accc = new AccountController();
                    var systemUserId = _base.SystemUser.Id;
                        //IdentityManager.CurrentUser(TempData["Claims"] as List<Claim>, accc.GetUserName()).Id;

                    if (viewCode == 0 || viewCode == ViewCodeKeys.IndividualCustomerProfile)
                    {
                        customer.SystemUserId = systemUserId;

                        if (currentSystemUser != null) customer.Id = currentSystemUser.Id;
                    }

                    if (customer.Id != 0)
                    {
                        //AJC 2019/11/15 Update system user email and mobile number on update of customer.
                        var sysUser = context.SystemUsers.Where(x => x.Id == customer.SystemUserId).FirstOrDefault();
                        sysUser.EmailAddress = customer.EmailAddress;
                        sysUser.MobileNumber = customer.CellPhoneNumber;
                        //

                        var customerReference = context.Customers.AsNoTracking().First(c => c.Id == customer.Id);

                        customer.StatusId = customerReference.StatusId;
                        customer.CreatedBySystemUserId = customerReference.CreatedBySystemUserId;
                        customer.CreatedDateTime = customerReference.CreatedDateTime;
                        context.Entry(customer).State = EntityState.Modified;
                        context.Entry(sysUser).State = EntityState.Modified;
                    }
                    else
                    {
                        //var custIdExists = context.Customers.Any(c => c.IdentificationNumber == customer.IdentificationNumber);

                        //if (custIdExists && viewCode == ViewCodeKeys.CreateAgentCustomerProfile)
                        //{
                        //    TempData["IDError"] = "Customer profile exists for id number : " + customer.IdentificationNumber +
                        //        ".<br/>For more details please contact the administrator.";
                        //    ViewSuccess = false;
                        //}
                        //else if (custIdExists)
                        //{
                        //    TempData["IDError"] = "Customer profile exists for id number : " + customer.IdentificationNumber +
                        //        ".<br/>New profile will be used. For more details please contact the administrator.";
                        //    ViewSuccess = false;
                        //    SendEmailRequest = true;
                        //    context.Customers.Add(customer);
                        //}
                       
                            SendEmailRequest = true;
                            context.Customers.Add(customer);
                        
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        #region Profile Save Agent
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        private void SaveAgent(Agent agent, Customer customer, Status agentStatus)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    agent.CustomerId = customer.Id;
                    agent.CustomerTypeId = customer.CustomerTypeId;
                    agent.IdentificationTypeId = customer.IdentificationTypeId;
                    agent.TitleTypeId = customer.TitleTypeId;
                    agent.CountryOfIssueTypeId = customer.CountryOfIssueTypeId;
                    agent.StatusId = agentStatus.Id;
                    agent.IdentificationNumber = customer.IdentificationNumber;
                    agent.FirstName = customer.FirstName;
                    agent.LastName = customer.LastName;
                    agent.EmailAddress = customer.EmailAddress;
                    agent.HomePhoneNumber = customer.HomePhoneNumber;
                    agent.CellPhoneNumber = customer.CellPhoneNumber;
                    agent.PhysicalAddress1 = customer.PhysicalAddress1;
                    agent.PhysicalAddress2 = customer.PhysicalAddress2;
                    agent.PhysicalAddress3 = customer.PhysicalAddress3;
                    agent.PhysicalAddress4 = customer.PhysicalAddress4;
                    agent.PhysicalAddress5 = customer.PhysicalAddress5;
                    agent.PhysicalAddressCode = customer.PhysicalAddressCode;
                    agent.PostalAddress1 = customer.PostalAddress1;
                    agent.PostalAddress2 = customer.PostalAddress2;
                    agent.PostalAddress3 = customer.PostalAddress3;
                    agent.PostalAddress4 = customer.PostalAddress4;
                    agent.PostalAddress5 = customer.PostalAddress5;
                    agent.PostalAddressCode = customer.PostalAddressCode;

                    agent.IsActive = true;
                    agent.IsDeleted = false;
                    agent.IsLocked = false;

                    if (agent.Id != 0)
                    {
                        context.Entry(agent).State = EntityState.Modified;
                    }
                    else
                    {
                        if (!context.Agents.Any(a => a.CustomerId == agent.CustomerId))
                        {
                            context.Agents.Add(agent);
                        }
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        #region Save Entity
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        private int SaveEntity(Entity entity, Customer customer, int entityTypeId, Status entityStatus)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    entity.CustomerId = customer.Id;
                    entity.EntityTypeId = entityTypeId;
                    entity.StatusId = entityStatus.Id;
                    entity.IsActive = true;
                    entity.IsDeleted = false;

                    var currentEntity = context.Entities.AsNoTracking().FirstOrDefault(e => e.EntityRegistrationNumber == entity.EntityRegistrationNumber);

                    if (currentEntity == null)
                    {
                        if (!context.Entities.Any(e => e.CustomerId == entity.CustomerId))
                            context.Entities.Add(entity);
                    }
                    else
                    {
                        entity.Id = currentEntity.Id;
                        currentEntity.CreatedBySystemUserId = currentEntity.CreatedBySystemUserId;
                        currentEntity.CreatedDateTime = currentEntity.CreatedDateTime;
                    }

                    context.SaveChanges();
                    return entity.Id;
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        #region Save Agent Customer
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        private void SaveAgentCustomer(Agent agent, Customer customer)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    if (!context.AgentCustomers.Any(ac => ac.CustomerId == customer.Id))
                    {
                        var agentcustomer = new AgentCustomer
                        {
                            AgentId = agent.Id,
                            CustomerId = customer.Id,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false
                        };

                        context.AgentCustomers.Add(agentcustomer);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw ex;
                }
            }
        }
        #endregion

        #region Profile Save Entity Agent
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        private void SaveEntityAgent(Entity entity, Agent agent)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    if (!context.EntityAgents.Any(ea => ea.AgentId == agent.Id))
                    {
                        var entityagent = new EntityAgent
                        {
                            EntityId = entity.Id,
                            AgentId = agent.Id,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false
                        };

                        context.EntityAgents.Add(entityagent);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion


        #region Profile Send Customer Profile Approval Notification
        public void SendApprovalNotification(Customer customer)
        {
            try
            {
                if (SendEmailRequest)
                {
                    if (customer.EmailAddress != null && customer.EmailAddress != "")
                    {
                        var email = new Email();
                        const string customerSubject = "Property Lease Management: Customer Profile";
                        const string customerBody = " Your profile will be reviewed by the Property Lease Management administration team. Once approved, you will be able to create plm applications. " +
                                       "Please make sure that all required documents have been uploaded on your profile.";

                        email.GenerateEmail(customer.EmailAddress, customerSubject, customerBody, customer.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, customer.FullName);

                    }
                    else if (customer.EmailAddress == null && customer.CellPhoneNumber != null)
                    {
                        var sms = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                        sms.GenerateSMS(customer.SystemUser.MobileNumber, "Your profile will be reviewed and approved before you can start capturing plm application. Please supply all documents for a quick response. Thanks Property Lease Management Team.",
    customer.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, customer.SystemUser.FullName);
                        //SmsHelper.Send(customer.CellPhoneNumber, "Your profile will be reviewed and approved to start linking accounts. Please supply all documents for a quick response. Thanks Siyakhokha Team.");

                    }
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }
        }
        #endregion

        //private Exception HandleDbUpdateException(DbUpdateException dbu)
        //{
        //    var builder = new StringBuilder("A DbUpdateException was caught while saving changes. ");

        //    try
        //    {
        //        foreach (var result in dbu.Entries)
        //        {
        //            builder.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        builder.Append("Error parsing DbUpdateException: " + e.ToString());
        //    }

        //    string message = builder.ToString();
        //    return new Exception(message, dbu);
        //}

        #region Profile Render Customer (ROR)
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public ActionResult RenderCustomer(int Id)
        {
            using (var context = new eServicesDbContext())
            {
                var vm = new CustomerProfileViewModel
                {
                    Customer = context.Customers.Find(Id),
                    Entity = context.Entities.SingleOrDefault(o => o.CustomerId == Id),
                    Agent = context.Agents.SingleOrDefault(o => o.CustomerId == Id)
                };

                vm.IsManagingAgent = vm.Agent != null;

                var obj = new
                {
                    status = vm.Customer != null ? "Success" : "Failure",
                    view = RenderHelper.PartialView(this, "_CustomerPartial", vm)
                };

                return Json(obj);
            }
        }
        #endregion

        #region Profile Get Customer Object
 [Authorize(Roles = "Clerks,Administrators,Customers,Super Administrators,Submit Figures,Rates,Issue Certificate,Internal Registration,Area Manager,Credit Control,Sundry Account,Acknowledge RCS Application,Billing,Acknowledge Refund Application,Issue Refunds collection,Back Office System Administrator,System Administrators,Lease Official,Property Manager,Housing Supervisor,Finance Administrator,Revenue Manager,Community Development Officer,Letting Officer,Revenue Officer" )]
        public JsonResult GetCustomer(int id)
        {
            using (var context = new eServicesDbContext())
            {
                var customer = context.Customers.Find(id);
                return null != customer ? Json(customer) : null;
            }
        }
        #endregion
    }
}