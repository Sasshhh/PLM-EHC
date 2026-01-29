using C8.eServices.Mvc.ApiServices;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class CustomerLinkedAccountController : Controller
    {
        //eServicesDbContext _context = new eServicesDbContext();
        ValueAssistDbContext _vassist = new ValueAssistDbContext();
        BaseHelper _base = new BaseHelper();

        public CustomerLinkedAccountController()
        {
            var context = new eServicesDbContext();
            IdentityManager = new IdentityManager(context);
            UserManager =
                new UserManager<SystemIdentityUser>(
                new UserStore<SystemIdentityUser>(context));
        }

        public IdentityManager IdentityManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        private int _systemUserId = -1;

        #region CustomerLinkedAccount Populate Linked Account View Model
        // GET: CustomerLinkedAccount
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        private CustomerLinkedAccountViewModel CreateLinkedAccountVm(int? customerId, int? agentId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var custLinkedAccVm = new CustomerLinkedAccountViewModel();
                    var accounts = new List<Account>();
                    string accountsNotification = null;

                    if (customerId != null)
                    {
                        custLinkedAccVm.CustomerId = Convert.ToInt32(customerId.Value);
                        custLinkedAccVm.AgentId = (agentId != null) ? Convert.ToInt32(agentId.Value) : default(int);

                        var customer = context.Customers.Include(c => c.CustomerType)
                        .SingleOrDefault(c => c.Id == customerId);
                        var agent = context.Agents.SingleOrDefault(a => a.Id == agentId);

                        if (customer == null) throw new Exception("Invalid Customer");
                        var ca = CustomerAccountApi.GetAccounts(customer.IdentificationNumber);

                        if (agent != null)
                            custLinkedAccVm.AgentCustomerId = agent.CustomerId;
                        if (context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId) != null
                            && customer.CustomerType.Key != CustomerTypeKeys.Entity)
                            custLinkedAccVm.IsEntityAgent = true;
                        var currentAccounts = context.LinkedAccounts.Include(la => la.Customer).Include(la => la.Status)
                            .Include(la => la.Account)
                            .Where(la => la.CustomerId == customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked)
                            .ToList();
                        var custAccounts = new List<CustomerAccount>();
                        var cAccounts = new List<Account>();
                        var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                        var referenceType = (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent || customer.CustomerType.Key == CustomerTypeKeys.Entity)
                                            ? context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.LinkedAccountsManagingAgent)) :
                                            context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);
                        var documentStatus = context.Status.FirstOrDefault(a => a.Key.Equals(StatusKeys.DocumentPending));

                        if (application == null) throw new Exception("Invalid Application");
                        if (referenceType == null) throw new Exception("Invalid Reference type");
                        if (documentStatus == null) throw new Exception("Invalid Status");

                        custLinkedAccVm.CustomerEmail = context.Customers.Find(customerId).EmailAddress;

                        switch (customer.CustomerType.Key)
                        {
                            case CustomerTypeKeys.Entity:
                            case CustomerTypeKeys.ManagingAgent:
                                var entity = (custLinkedAccVm.IsEntityAgent) ? context.Entities.SingleOrDefault(e => e.Id == context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId).EntityId) :
                                    context.Entities.SingleOrDefault(e => e.CustomerId == customer.Id);
                                if (entity != null)
                                {
                                    // TODO: Get accounts according to entity/ agent.
                                    custAccounts = context.CustomerAccounts.Where(o => o.IDNo.ToString() == customer.IdentificationNumber).ToList();
                                    cAccounts = (from a in custAccounts
                                                 select new Account()
                                                 {
                                                     AccountNumber = a.AccountNo.ToString(),
                                                     AccountHolder = a.Surname + ' ' + a.Initials,
                                                     ReferenceNumber = a.IDNo.ToString(),
                                                     Description = string.Format("{0} - {1}", a.AccountNo, a.StandAddr),
                                                     IsActive = true
                                                 }).ToList();
                                }
                                break;
                            default:
                                if(ca.Count>0)
                                {
                                    cAccounts= (from a in ca
                                                select new Account()
                                                {
                                                    AccountNumber = a.AccountNumber.ToString(),
                                                    AccountHolder = a.AccountHolder,
                                                    ReferenceNumber = customer.IdentificationNumber.ToString(),
                                                    Description = a.Description,
                                                    IsActive = true
                                                }).ToList();

                                }
                                else { 
                                custAccounts = context.CustomerAccounts.Where(o => o.IDNo.ToString() == customer.IdentificationNumber).ToList();
                                cAccounts = (from a in custAccounts
                                             select new Account()
                                             {
                                                 AccountNumber = a.AccountNo.ToString(),
                                                 AccountHolder = a.Surname + ' ' + a.Initials,
                                                 ReferenceNumber = a.IDNo.ToString(),
                                                 Description = string.Format("{0} - {1}", a.AccountNo, a.StandAddr),
                                                 IsActive = true
                                             }).ToList();
                                }
                                break;
                        }

                        accounts = cAccounts.DistinctBy(a => a.AccountNumber)
                                   .Where(va => currentAccounts.All(a => a.Account.AccountNumber != va.AccountNumber))
                                   .ToList();
                        accountsNotification = (!cAccounts.Any()) ? "No associated Municipal accounts found." :
                            (!accounts.Any()) ? "All associated Municipal accounts have already been linked." :
                            "Associated Municipal accounts found.";

                        ViewBag.CurrentAccounts = currentAccounts;

                        var dvm = new DocumentsViewModel
                        {
                            ApplicationReferenceNumber = DateTime.Now.ToString("yyyyMMddHHmmss"),
                            ApplicationId = application.Id,
                            CustomerId = custLinkedAccVm.CustomerId,
                            ReferenceTypeId = referenceType.Id,
                            ReferenceId = 0,
                            StatusId = documentStatus.Id,
                            Application = context.Applications.Find(application.Id),
                            ReferenceType = context.ReferenceTypes.Find(referenceType.Id),
                            IsUploadView = true,
                            DocumentCheckLists = context.DocumentCheckLists.Where(o => o.ApplicationId == application.Id && o.ReferenceTypeId == referenceType.Id).Include(o => o.DocumentType).ToList(),
                            Documents = null
                        };

                        custLinkedAccVm.Document = dvm;
                    }

                    ViewBag.AccountsMessage = accountsNotification ?? "An error occurred when trying to retrieve associated Municipal accounts";
                    ViewBag.Accounts = new SelectList(accounts, "AccountNumber", "Description");
                    return custLinkedAccVm;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region CustomerLinkedAccount Create Linked Account GET
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index(int? customerId, int? agentId)
        {
            using (var context = new eServicesDbContext())
            {
                PopulateEmailRecipientType();
                agentId = agentId ?? 0;

                _base.Initialise(context);
                _systemUserId = _base.SystemUser.Id;

                var customer = context.Customers.FirstOrDefault(c => c.Id == customerId);
                if (customer == null) throw new Exception("Invalid customer");

                //Checks if current customer profile belongs to the system user
                if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, ViewCodeKeys.CreateCustomerLinkedAccount))
                {
                    SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                    return RedirectToAction("Index", "Error");
                }

                return View("~/Views/CustomerLinkedAccount/CustomerLinkedAccount.cshtml", CreateLinkedAccountVm(customerId, agentId));
            }
        }
        #endregion

        //change here in this controller.
        #region CustomerLinkedAccount Create Linked Account POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index(CustomerLinkedAccountViewModel vm, FormCollection collection)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    var valueAssistAccounts = new List<Account>();
                    var email = new Email();
                    var isAgentCustomerAccount = false;
                    var customer = context.Customers.Include(c => c.CustomerType).FirstOrDefault(c => c.Id == vm.CustomerId);
                    var systemUser = context.Customers.FirstOrDefault(c => c.SystemUserId == _systemUserId);
                    if (customer == null) throw new Exception("Invalid customer");
                    var ca = CustomerAccountApi.GetAccounts(customer.IdentificationNumber);
                    //Checks if current customer profile belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, customer, ViewCodeKeys.CreateCustomerLinkedAccount))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }

                    // JK.20190618a - Checks if there is a customer profile for the agent.
                    if (systemUser != null)
                        vm.Agent = context.Agents.FirstOrDefault(a => a.CustomerId == systemUser.Id);
                    else vm.Agent = null;

                    var agentId = (vm.Agent != null) ? vm.Agent.Id : 0;

                    if (vm.Agent != null)
                        vm.AgentCustomerId = vm.Agent.CustomerId;
                    if (context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId) != null
                        && customer.CustomerType.Key != CustomerTypeKeys.Entity)
                        vm.IsEntityAgent = true;

                    var linkedAccountType = context.LinkedAccountTypes.FirstOrDefault(la => la.Key == vm.LinkedAccountType);
                    var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.LinkedAccounts));
                    var referenceType = (customer.CustomerType.Key == CustomerTypeKeys.ManagingAgent) ? context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.LinkedAccountsManagingAgent)) :
                                         context.ReferenceTypes.FirstOrDefault(d => d.Key == ReferenceTypeKeys.LinkedAccounts);

                    if (linkedAccountType == null) throw new Exception("Invalid Linked account type");
                    if (application == null) throw new Exception("Invalid Application");

                    vm.AccountNumber = (linkedAccountType.Key == LinkedAccountTypeKeys.Associated)
                                        ? Request.Form["Accounts"] : Request.Form["CustomerAccNum"];

                    var isValidAccount = context.CustomerAccounts.Any(a => a.AccountNo.ToString() == vm.AccountNumber);
                    var statusCustLinkedAccount = new Status();
                    var custLinkedAccExists = context.LinkedAccounts.Any(la => la.Account.AccountNumber == vm.AccountNumber
                                                && la.CustomerId == customer.Id && la.IsActive && !la.IsDeleted);

                    if (custLinkedAccExists && context.Accounts.Any(o => o.AccountNumber == vm.AccountNumber))
                        statusCustLinkedAccount = context.LinkedAccounts.Include(o => o.Status).Include(o => o.Account).FirstOrDefault(la => la.Account.AccountNumber == vm.AccountNumber
                                                  && la.CustomerId == customer.Id && la.IsActive && !la.IsDeleted).Status;
               

                    var custAccounts = context.CustomerAccounts.Where(o => o.IDNo.ToString() == customer.IdentificationNumber).ToList();

                    //
                    // JK.20190622a - TODO: Switch to the links to the customer/ account api.
                    switch (customer.CustomerType.Key)
                    {
                        case CustomerTypeKeys.Entity:
                        case CustomerTypeKeys.ManagingAgent:
                            var entity = (vm.IsEntityAgent) ? context.Entities.SingleOrDefault(e => e.Id == context.EntityAgents.FirstOrDefault(ea => ea.AgentId == agentId).EntityId) :
                                context.Entities.SingleOrDefault(e => e.CustomerId == customer.Id);
                            if (entity != null)
                            {
                                if(ca.Count > 0 && (vm.LinkedAccountType!= context.LinkedAccountTypes.FirstOrDefault(o => o.Key == LinkedAccountTypeKeys.Unassociated).Key))
                                {
                                    valueAssistAccounts = (from a in ca
                                                           select new Account()
                                                           {
                                                               AccountNumber = a.AccountNumber.ToString(),
                                                               AccountHolder = a.AccountHolder,
                                                               ReferenceNumber = customer.IdentificationNumber.ToString(),
                                                               Description = a.Description,
                                                               IsActive = true
                                                           }).ToList();

                                }
                                else
                                {
                                    valueAssistAccounts = (from a in custAccounts
                                                           select new Account()
                                                           {
                                                               AccountNumber = a.AccountNo.ToString(),
                                                               AccountHolder = a.Surname + ' ' + a.Initials,
                                                               ReferenceNumber = a.IDNo.ToString(),
                                                               Description = string.Format("{0} - {1}", a.AccountNo, a.StandAddr),
                                                               IsActive = true
                                                           }).ToList();
                                }
                      
                            }
                            break;
                        default:
                            if (ca.Count > 0 && (vm.LinkedAccountType != context.LinkedAccountTypes.FirstOrDefault(o => o.Key == LinkedAccountTypeKeys.Unassociated).Key))
                            {
                                valueAssistAccounts = (from a in ca
                                                       select new Account()
                                                       {
                                                           AccountNumber = a.AccountNumber.ToString(),
                                                           AccountHolder = a.AccountHolder,
                                                           ReferenceNumber = customer.IdentificationNumber.ToString(),
                                                           Description = a.Description,
                                                           IsActive = true
                                                       }).ToList();

                            }
                            else
                            {
                                valueAssistAccounts = (from a in custAccounts
                                                       select new Account()
                                                       {
                                                           AccountNumber = a.AccountNo.ToString(),
                                                           AccountHolder = a.Surname + ' ' + a.Initials,
                                                           ReferenceNumber = a.IDNo.ToString(),
                                                           Description = string.Format("{0} - {1}", a.AccountNo, a.StandAddr),
                                                           IsActive = true
                                                       }).ToList();
                            }
                     
                            break;
                    }

                    // JK.20190428 - Demo
                    //if (linkedAccountType.Key == LinkedAccountTypeKeys.Associated && valueAssistAccounts.All(va => va.AccountNumber != vm.AccountNumber))
                    //{
                    //    return RedirectToAction("Index", "Error");
                    //}

                    if ((!custLinkedAccExists && isValidAccount) || (statusCustLinkedAccount != null && statusCustLinkedAccount.Key == StatusKeys.LinkedAccountUnlinked))
                    {
                        if (ModelState.IsValid)
                        {
                            var emailNotification = false;

                            var linkedAccount = new LinkedAccount();
                            {
                                //add logic for selecting recipient type and second email.
                                var account = new Account();
                                // JK.20190622a - Add integration to venus service here to grab accounts.
                                Account valueAssistAcc = null;
                                var accountActive = context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.AccountActive));
                                var accountExists = context.Accounts.FirstOrDefault(a => a.AccountNumber == vm.AccountNumber);
                                var documentPending = context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.DocumentPending));
                                var linkedAccountActive = context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.LinkedAccountActive));
                                var recipientType = context.RecipientTypes.Find(vm.LinkedAccount.RecipientTypeId);
                                var accList= new List<Account>();
                                if (valueAssistAcc == null)
                                {
                                    var caList = context.CustomerAccounts.Where(a => a.AccountNo.ToString() == vm.AccountNumber).ToList();
                                    if (ca.Count > 0 && (vm.LinkedAccountType != context.LinkedAccountTypes.FirstOrDefault(o => o.Key == LinkedAccountTypeKeys.Unassociated).Key))
                                    {
                                        accList = (from a in ca
                                                   select new Account()
                                                   {
                                                       AccountNumber = a.AccountNumber.ToString(),
                                                       AccountHolder = a.AccountHolder,
                                                       ReferenceNumber = customer.IdentificationNumber.ToString(),
                                                       Description = a.Description,
                                                       IsActive = true
                                                   }).ToList();

                                    }
                                    else
                                    {
                                        accList = (from a in caList
                                                   select new Account()
                                                   {
                                                       AccountNumber = a.AccountNo.ToString(),
                                                       AccountHolder = a.Surname + ' ' + a.Initials,
                                                       ReferenceNumber = a.IDNo.ToString(),
                                                       Description = string.Format("{0} - {1}", a.AccountNo, a.StandAddr),
                                                       IsActive = true
                                                   }).ToList();
                                    }
                                    valueAssistAcc = accList.FirstOrDefault(a => a.AccountNumber == vm.AccountNumber);
                                }

                                if (valueAssistAcc == null) throw new Exception("Invalid account");
                                if (accountActive == null) throw new Exception("Invalid status");
                                if (documentPending == null) throw new Exception("Invalid status");
                                if (linkedAccountActive == null) throw new Exception("Invalid status");

                                if (accountExists == null)
                                {
                                    account.AccountHolder = valueAssistAcc.AccountHolder;
                                    account.Description = valueAssistAcc.Description;
                                    account.IsRMSAccount = true;
                                    account.AccountNumber = vm.AccountNumber;
                                    account.StatusId = accountActive.Id;
                                    context.Accounts.Add(account);
                                    context.SaveChanges();
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(accountExists.Description))
                                        accountExists.Description = valueAssistAcc.Description;
                                    account = accountExists;
                                }

                                // Check if there is already a linked account and reactivate it.
                                var la = context.LinkedAccounts.Include(o => o.Status).Include(o => o.Account).FirstOrDefault(o => o.AccountId == account.Id && o.CustomerId == customer.Id);

                                if (la != null)
                                {
                                    la.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.LinkedAccountActive).Id;
                                    la.IsActive = true;
                                    la.IsDeleted = false;
                                    context.Entry(la).State = EntityState.Modified;
                                    context.SaveChanges();
                                    linkedAccount = la;
                                }

                                if (la == null)
                                {
                                    linkedAccount.Account = account;
                                    linkedAccount.CustomerId = customer.Id;
                                    linkedAccount.RecipientTypeId = vm.LinkedAccount.RecipientTypeId;

                                    if (vm.Agent != null)
                                    {
                                        if (vm.Agent.CustomerId != vm.CustomerId)
                                            isAgentCustomerAccount = true;
                                    }

                                    // JK.20190707a - Check if the account is associated with the customer and skip documents step
                                    if ((ca.Where(r => r.AccountNumber.ToString() == vm.AccountNumber).Any())|| (context.CustomerAccounts.Where(o => o.AccountNo.ToString() == vm.AccountNumber && o.IDNo.ToString() == customer.IdentificationNumber).Any()))
                                    {
                                        linkedAccount.StatusId = linkedAccountActive.Id;
                                    } else
                                        linkedAccount.StatusId = (linkedAccountType.Key == LinkedAccountTypeKeys.Associated && !isAgentCustomerAccount)
                                                             ? linkedAccountActive.Id : documentPending.Id;

                                    linkedAccount.LinkedAccountTypeId = linkedAccountType.Id;
                                    linkedAccount.IsActive = true;
                                    linkedAccount.IsDeleted = false;
                                    linkedAccount.IsLocked = false;
                                    context.LinkedAccounts.Add(linkedAccount);
                                    context.SaveChanges();
                                }
                                else
                                {
                                    la.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.LinkedAccountActive).Id;
                                    la.IsActive = true;
                                    la.IsDeleted = false;
                                    context.Entry(la).State = EntityState.Modified;
                                    context.SaveChanges();
                                    linkedAccount = la;
                                }

                                if (recipientType.Key == RecepientTypeKeys.SecondaryEmail)
                                {
                                    var secondaryEmails = new Email().ExtractEmails(Request.Form["SecondaryEmails"]);

                                    if (secondaryEmails.Any())
                                    {
                                        foreach (var mail in secondaryEmails)
                                        {
                                            AddLinkedEmail(linkedAccount, customer, mail, this);
                                        }

                                        emailNotification = context.LinkedEmails.Include(le => le.Status)
                                                    .Any(le => le.LinkedAccountId == linkedAccount.Id
                                                    && le.Status.Key == StatusKeys.PendingEmailVerification);
                                    }
                                }

                                if (recipientType.Key == RecepientTypeKeys.Mobile)
                                {
                                    // JK.20190622a - TODO: Iterate through the entire list of these phone controls and save.
                                    var mobileNumber = Request.Form["phone[1][number]"];

                                    if (mobileNumber.Trim().Length > 0)
                                    {

                                        AddLinkedMobile(linkedAccount, customer, mobileNumber, this);


                                        emailNotification = context.LinkedEmails.Include(le => le.Status)
                                                    .Any(le => le.LinkedAccountId == linkedAccount.Id
                                                    && le.Status.Key == StatusKeys.LinkedMobilePending);
                                    }
                                }
                            }
                            
                            //send email notification 
                            const string subject = "Siyakhokha: Linked Account";
                            var body = string.Empty;
                            var notification = new StringBuilder(string.Empty);
                            string message ="";
                            string emailTemplate;
                            if (customer.EmailAddress != null && customer.EmailAddress != "")
                            {
                                if (linkedAccountType.Key == LinkedAccountTypeKeys.Associated && !isAgentCustomerAccount)
                                {
                                    emailTemplate = AppSettingKeys.LinkedAccountsAssociatedEmailTemplate;
                                    var activeMessage =
                                        context.AppSettings.FirstOrDefault(
                                            a => a.Key == AppSettingKeys.LinkedAccountsActiveMessage);
                                    if (activeMessage == null) throw new Exception("Invalid appsetting");

                                    message = activeMessage.Value;
                                }
                                else
                                {
                                    body = LinkedAccountHelper.LinkedAccountNotification(emailNotification);

                                    message = body;
                                    emailTemplate = AppSettingKeys.LinkedAccountsUnassociatedEmailTemplate;
                                }

                                email.GenerateEmail(customer.EmailAddress, subject, body, linkedAccount.CustomerId.ToString(CultureInfo.InvariantCulture), false, emailTemplate, recipientName: customer.FullName, referenceNumber: linkedAccount.Account.AccountNumber);

                            }
                            else if(customer.CellPhoneNumber != null || customer.CellPhoneNumber != "")
                            {

                                if (linkedAccountType.Key == LinkedAccountTypeKeys.Associated && !isAgentCustomerAccount)
                                {
                            
                                    var bodysms = " Dear " + customer.FullName+ "\\n"+
                                       "Account: " + linkedAccount.Account.AccountNumber + " is now successfully linked to your profile.\\n" +
                                       "Regards, \\nSiyakhokha Team" ;
                                    var sms = new CesarSMS();
                                    var cxt = new eServicesDbContext();
                                    var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                    sms.GenerateSMS(customer.CellPhoneNumber, bodysms.ToString(),
            customer.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, customer.SystemUser.FullName);
                                    //SmsHelper.Send(customer.CellPhoneNumber, bodysms.ToString());


                                }
                                else
                                {
                                    var bodysms = " Dear " + customer.FullName + "\\n" +
                                  "You have requested to link a new account : " + linkedAccount.Account.AccountNumber + " to your profile.\\nThis request has been sent to the Siyakhokha Administration team for approval. \\n" +
                                  "Regards, \\nSiyakhokha Team";
                                    var sms = new CesarSMS();
                                    var cxt = new eServicesDbContext();
                                    var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                    sms.GenerateSMS(customer.CellPhoneNumber, bodysms.ToString(),
            customer.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, customer.FullName);
                                    //SmsHelper.Send(customer.CellPhoneNumber, bodysms.ToString());

                                }
                              

                            }


                            DocumentHelper.ManageDocumentReferences(linkedAccount.Id, DocumentTypeKeys.IdDocument);

                            ViewBag.CompletedAction = (emailNotification)
                            ? notification.Append("Linked Municipal Account Created.").Append("<br/>").Append("Activation emails sent to linked email addresses.").Append("<br/>").Append(message).Append("<br/>").ToString()
                            : notification.Append("Linked Municipal Account Created.").Append("<br/>").Append(message).Append("<br/>").ToString();

                            TempData["Success"] = true;
                            ViewBag.LinkedAccountId = linkedAccount.Id;
                            ViewBag.ReferenceTypeId = referenceType.Id;
                            ViewBag.ApplicationId = application.Id;
                            ViewBag.AccountStatus = linkedAccount.Status.Key;
                            ModelState.Clear();
                            PopulateEmailRecipientType();
                            return View("~/Views/CustomerLinkedAccount/CustomerLinkedAccount.cshtml", CreateLinkedAccountVm(vm.CustomerId, agentId));
                        }
                    }

                    TempData["Success"] = false;
                    ViewBag.CompletedAction = "Municipal Account already linked to a customer profile";
                    PopulateEmailRecipientType();

                    return View("~/Views/CustomerLinkedAccount/CustomerLinkedAccount.cshtml", CreateLinkedAccountVm(vm.CustomerId, agentId));
                }
                catch (Exception x)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region CustomerLinkedAccount Edit Linked Account GET
        [HttpGet]
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult EditLinkedAccount(int id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    //populate linked account vm
                    var emailList = new List<string>();
                    var mobileList = new List<string>();
                    var linkedAccount = context.LinkedAccounts.Include(l => l.RecipientType)
                                .Include(l => l.LinkedAccountType)
                                .Include(l => l.Customer)
                                .Include(l => l.Status)
                                .Include(l => l.Customer.CustomerType)
                                .SingleOrDefault(l => l.Id == id);

                    if (linkedAccount == null) throw new Exception("Invalid Linked Municipal account");

                    //Checks if current linked account belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedAccount, ViewCodeKeys.UpdateCustomerLinkedAccount))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }

                    var vm = LinkedAccountHelper.PopulateCustomerLinkedModel(linkedAccount, emailList, mobileList);
                    PopulateEmailRecipientType(linkedAccount.RecipientType);
                    this.PopulateDocuments(linkedAccount);

                    ViewBag.RecipientTypeId = new SelectList(context.RecipientTypes.Where(rt => rt.IsActive && !rt.IsDeleted).ToList(), "Id", "Name", linkedAccount.RecipientType);
                    ViewBag.RecipientType = linkedAccount.RecipientType.Key;
                    ViewBag.EmailAddresses = emailList.ToArray();
                    ViewBag.MobileNumbers = mobileList.ToArray();

                    return View(vm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion
        #region CustomerLinkedAccount Edit Linked Account POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult EditLinkedAccount(CustomerLinkedAccountViewModel vm, FormCollection collection)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    if (ModelState.IsValid)
                    {
                        var linkedAccount = context.LinkedAccounts
                            .Include(l => l.Customer)
                            .Include(l => l.Customer.CustomerType)
                            .Include(l => l.Account)
                            .Include(l => l.RecipientType)
                            .Include(l => l.Status)
                            .Include(l => l.LinkedAccountType)
                            .SingleOrDefault(l => l.Id == vm.Id);

                        if (linkedAccount == null) throw new Exception("Invalid Linked Municipal account");

                        //Checks if current linked account belongs to the system user
                        if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedAccount, ViewCodeKeys.UpdateCustomerLinkedAccount))
                        {
                            SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                            return RedirectToAction("Index", "Error");
                        }

                        var emailList = new List<string>();
                        var mobileList = new List<string>();
                        var newEmails = false;

                        var recipTypeId = Convert.ToInt32(Request.Form["RecipientTypeId"]);
                        var recipientType = context.RecipientTypes.FirstOrDefault(rt => rt.Id == recipTypeId);
                        if (recipientType == null) throw new Exception("Invalid Recipient type");
                        var currentLinkedEmails = context.LinkedEmails.Where(le => le.LinkedAccountId == vm.Id && le.IsActive
                                                   && le.IsDeleted == false);
                        var secondaryEmails = new Email().ExtractEmails(Request.Form["SecondaryEmails"]);

                        linkedAccount.RecipientTypeId = recipientType.Id;
                        TempData["Success"] = true;

                        if (recipientType.Key == RecepientTypeKeys.ViewOnly)
                            secondaryEmails.Clear();

                        //If emails removed by customer or recipient type changed.
                        foreach (var mail in currentLinkedEmails)
                        {
                            var currentEmailExists = secondaryEmails.Any(le => string.Compare(le, mail.EmailAddress, StringComparison.OrdinalIgnoreCase) == 0);

                            if (!currentEmailExists)
                            {
                                mail.IsActive = false;
                                mail.IsDeleted = true;
                                mail.IsLocked = false;
                            }
                        }

                        if (recipientType.Key == RecepientTypeKeys.SecondaryEmail)
                        {
                            if (!secondaryEmails.Any())
                            {
                                TempData["Success"] = false;
                            }
                            else
                            {
                                //Add new secondary emails for customer
                                foreach (var mail in secondaryEmails)
                                {
                                    //Prevent duplicate linked emails
                                    var emailExists = currentLinkedEmails.Any(le => le.EmailAddress == mail);

                                    if (!emailExists)
                                    {
                                        AddLinkedEmail(linkedAccount, linkedAccount.Customer, mail, this);
                                    }
                                }

                                newEmails = context.LinkedEmails.Include(le => le.Status)
                                    .Any(le => le.LinkedAccountId == linkedAccount.Id
                                               && le.Status.Key == StatusKeys.PendingEmailVerification);
                            }
                        }

                        if (TempData["Success"].Equals(true))
                        {
                            context.Entry(linkedAccount).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        var cvm = LinkedAccountHelper.PopulateCustomerLinkedModel(linkedAccount, emailList, mobileList);
                        this.PopulateDocuments(linkedAccount);

                        ViewBag.CompletedAction = (TempData["Success"].Equals(false)) ? "Recipient type emails requires at least one email." : (newEmails == false) ? "Linked Municipal Account Updated." : "Linked Account Municipal Updated. Activation emails sent to linked email addresses.";
                        ViewBag.RecipientTypeId = new SelectList(context.RecipientTypes.Where(rt => rt.IsActive && !rt.IsDeleted).ToList(), "Id", "Name", linkedAccount.RecipientType);
                        ViewBag.RecipientType = linkedAccount.RecipientType.Key;
                        ViewBag.EmailAddresses = emailList.ToArray();
                        ViewBag.MobileNumbers = mobileList.ToArray();
                        return View(cvm);
                    }

                    PopulateEmailRecipientType();
                    TempData["Success"] = false;
                    ModelState.Clear();
                    ViewBag.CompletedAction = "Failed to update linked Municipal account.";

                    return View();
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region CustomerLinkedAccount Display Linked Account
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult DisplayLinkedAccount(int id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var emailList = new List<string>();
                    var mobileList = new List<string>();
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;

                    var linkedaccount = context.LinkedAccounts.Include(la => la.RecipientType)
                    .Include(la => la.Status)
                    .Include(la => la.Account)
                    .Include(la => la.Customer)
                    .Include(la => la.Customer.CustomerType)
                    .Include(la => la.LinkedAccountType)
                    .SingleOrDefault(la => la.Id == id);

                    if (linkedaccount == null) throw new Exception("Invalid Linked account");

                    //Checks if current linked account belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedaccount, ViewCodeKeys.ViewCustomerLinkedAccount))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }

                    var customerLinkedAccountVm = LinkedAccountHelper.PopulateCustomerLinkedModel(linkedaccount, emailList, mobileList);
                    this.PopulateDocuments(linkedaccount);

                    object obj = new { id };
                    ViewBag.ReturnUrl = Url.Action("DisplayLinkedAccount", "CustomerLinkedAccount", SecureActionLinkExtension.Encrypt(obj));

                    return View(customerLinkedAccountVm);
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion

        #region CustomerLinkedAccount Unlink Account
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult UnLinkAccount(int id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    _systemUserId = _base.SystemUser.Id;
                    var linkedAccount = context.LinkedAccounts
                                .Include(i => i.Status)
                                .FirstOrDefault(i => i.Id == id);

                    if (linkedAccount == null) throw new Exception("Invalid Linked Municipal account");

                    //Checks if current linked account belongs to the system user
                    if (_systemUserId != -1 && !SecurityHelper.VerifySystemUserOwnership(_systemUserId, linkedAccount, ViewCodeKeys.UpdateCustomerLinkedAccount))
                    {
                        SecurityHelper.LogError(new Exception("Malicious Activity"), null);
                        return RedirectToAction("Index", "Error");
                    }

                    var systemUserCustomer = context.Customers.FirstOrDefault(ac => ac.SystemUserId == _systemUserId);
                    var agent = context.Agents.FirstOrDefault(a => a.CustomerId == systemUserCustomer.Id);
                    var agentId = (agent != null) ? agent.Id : 0;
                    var unlinkStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountUnlinked);

                    if (unlinkStatus == null) throw new Exception("Invalid Status");

                    linkedAccount.IsDeleted = true;
                    linkedAccount.IsActive = false;
                    linkedAccount.IsLocked = false;
                    linkedAccount.StatusId = unlinkStatus.Id;

                    TempData["Success"] = true;
                    ViewBag.CompletedAction = "Municipal Account Unlinked";
                    context.SaveChanges();

                    object obj = new { customerId = linkedAccount.CustomerId, agentId = agentId };
                    return RedirectToAction("Index", "CustomerLinkedAccount", SecureActionLinkExtension.Encrypt(obj));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region CustomerLinkedAccount Populate Email Recipient Type
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        private void PopulateEmailRecipientType(RecipientType currentType = null)
        {
            var context = new eServicesDbContext();
            var recipientQuery = context.RecipientTypes.Where(rt => rt.IsActive && !rt.IsDeleted).ToList();
            ViewBag.RecipientTypes = (currentType != null) ? new SelectList(recipientQuery, "Id", "Name", currentType.Id)
                : new SelectList(recipientQuery, "Id", "Name");
        }
        #endregion


        #region CustomerLinkedAccount Add Single Linked Email
        public void AddLinkedEmail(LinkedAccount linkedAccount, Customer customer, string emailAddress, Controller controller)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var email = new Email();

                    var linkedAccountStatus = context.Status.FirstOrDefault(s => s.Id == linkedAccount.StatusId);

                    if (linkedAccountStatus == null)
                        throw new Exception("Invalid status");

                    var emailStatus = (VerifyEmail(emailAddress)) ? context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.EmailVerified)) :
                    context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.PendingEmailVerification));

                    if (emailStatus == null)
                        throw new Exception("Invalid status");

                    var linkedEmail = new LinkedEmail
                    {
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        EmailAddress = emailAddress,
                        LinkedAccountId = linkedAccount.Id,
                        RecipientTypeId = linkedAccount.RecipientTypeId,
                        StatusId = emailStatus.Id
                    };

                    context.LinkedEmails.Add(linkedEmail);
                    context.SaveChanges();

                    if (emailStatus.Key == StatusKeys.PendingEmailVerification)
                    {
                        var message = LinkedAccountHelper.LinkedEmailNotification(linkedAccountStatus);

                        //send activation email
                        const string lesubject = "Siyakhokha: Linked Account Activation Email";
                        var body =
                            string.Format(
                                "You have requested to link a new email address: " + emailAddress +
                                " to linked account: <strong>" +
                                linkedAccount.Account.AccountNumber + "</strong><br/>" + message +
                                "</strong><br/><br/>Please click on the link below to activate this email address: <br/><a href=\"{0}\" title=\"Activation Email\">{0}</a>",
                                controller.Url.Action("ConfirmActivationEmail", "Account",
                                    SecureActionLinkExtension.Encrypt(new { token = linkedEmail.Id }), controller.Request.Url.Scheme));

                        email.GenerateEmail(emailAddress, lesubject, body,
                            linkedAccount.CustomerId.ToString(CultureInfo.InvariantCulture), false,
                            AppSettingKeys.EservicesDefaultEmailTemplate, customer.FullName);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region CustomerLinkedAccount Add Single Linked Mobile
        public void AddLinkedMobile(LinkedAccount linkedAccount, Customer customer, string mobileNumber, Controller controller)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var email = new Email();

                    var linkedAccountStatus = context.Status.FirstOrDefault(s => s.Id == linkedAccount.StatusId);

                    if (linkedAccountStatus == null)
                        throw new Exception("Invalid status");

                    var mobileStatus = mobileNumber.Length < 13 ? context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.LinkedMobileActive)) :
                    context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.LinkedMobilePending));

                    if (mobileStatus == null)
                        throw new Exception("Invalid status");

                    var linkedMobile = new LinkedMobile
                    {
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        MobileNumber = mobileNumber,
                        LinkedAccountId = linkedAccount.Id,
                        RecipientTypeId = linkedAccount.RecipientTypeId,
                        StatusId = mobileStatus.Id
                    };

                    context.LinkedMobiles.Add(linkedMobile);
                    context.SaveChanges();
                    var sms = new CesarSMS();
                    var cxt = new eServicesDbContext();
                    var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                    sms.GenerateSMS(mobileNumber, string.Format("Welcome to Siyakhokha, your number, {0}, has been linked to account {1} for bill notifications. Regards, Siyakhokha Team.", mobileNumber, linkedAccount.Account.AccountNumber),
customer.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, customer.SystemUser.FullName);
                    //SmsHelper.Send(mobileNumber, string.Format("Welcome to Siyakhokha, your number, {0}, has been linked to account {1} for bill notifications. Regards, Siyakhokha Team.", mobileNumber, linkedAccount.Account.AccountNumber));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region CustomerLinkedAccount Add Bulk Linked Emails
        public void AddLinkedEmail(List<LinkedAccount> linkedAccounts, Customer customer, string emailAddress, Controller controller)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var email = new Email();
                    var linkedAccountsNumbers = string.Empty;
                    var linkedEmailsId = new List<int>();
                    var emailStatus = (VerifyEmail(emailAddress)) ? context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.EmailVerified)) :
                                      context.Status.FirstOrDefault(x => x.Key.Equals(StatusKeys.PendingEmailVerification));

                    if (emailStatus == null)
                        throw new Exception("Invalid status");
                    var linkedAccountStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.LinkedAccountPending);

                    if (linkedAccountStatus == null)
                        throw new Exception("Invalid status");

                    var lastRecord = linkedAccounts.Last();

                    foreach (var linkedAccount in linkedAccounts)
                    {
                        var linkedEmail = new LinkedEmail
                        {
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            EmailAddress = emailAddress,
                            LinkedAccountId = linkedAccount.Id,
                            RecipientTypeId = linkedAccount.RecipientTypeId,
                            StatusId = emailStatus.Id
                        };

                        linkedAccountsNumbers += string.Format("{0} {1}", linkedAccount.Account.AccountNumber
                            , (!linkedAccount.Equals(lastRecord)) ? "," : "");

                        context.LinkedEmails.Add(linkedEmail);
                        context.SaveChanges();
                        linkedEmailsId.Add(linkedEmail.Id);
                    }

                    if (emailStatus.Key == StatusKeys.PendingEmailVerification)
                    {
                        var message = LinkedAccountHelper.LinkedEmailNotification(linkedAccountStatus);

                        //send activation email
                        const string lesubject = "Siyakhokha: Linked Account Activation Email";
                        var body =
                            string.Format(
                                "You have requested to link a new email address: " + emailAddress +
                                " to linked accounts: <strong>" +
                                linkedAccountsNumbers + "</strong><br/>" + message +
                                "</strong><br/><br/>Please click on the link below to activate this email address: <br/><a href=\"{0}\" title=\"Activation Email\">{0}</a>",
                                controller.Url.Action("ConfirmActivationEmail", "Account",
                                    SecureActionLinkExtension.Encrypt(new { token = linkedEmailsId.First() }), controller.Request.Url.Scheme));

                        email.GenerateEmail(emailAddress, lesubject, body,
                            customer.Id.ToString(CultureInfo.InvariantCulture), false,
                            AppSettingKeys.EservicesDefaultEmailTemplate, customer.FullName);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion

        #region CustomerLinkedAccount Verify Customer's Email
        public bool VerifyEmail(string email)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    bool isVerified;
                    var store = new UserStore<SystemIdentityUser>(context);
                    var userManager = new UserManager<SystemIdentityUser>(store);
                    var userEmail = userManager.FindByEmail(email);

                    if (userEmail == null)
                    {
                        isVerified = context.LinkedEmails.Include(le => le.Status)
                                     .Any(le => le.Status.Key == StatusKeys.EmailVerified
                                      && le.EmailAddress == email);
                    }
                    else
                    {
                        isVerified = userEmail.EmailConfirmed;
                    }

                    return isVerified;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        #endregion


        #region CustomerLinkedController Verify Account Number POST
        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult VerifyAccount(string accountNo)
        {
            using (var context = new eServicesDbContext())
            {

                var CurrUser = User.Identity.GetUserName();
                var customer = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                var CurrCustomer= context.Customers.SingleOrDefault(c => c.SystemUserId == customer.Id);
                if (customer == null) throw new Exception("Invalid Customer");
                //var ca = CustomerAccountApi.GetAccounts(customer.IdentificationNumber);
                var custAccounts = context.CustomerAccounts.Where(o => o.AccountNo.ToString() == accountNo).ToList();
                var cAccounts = new List<Account>();
                //if (ca.Count > 0)
                //{
                //    cAccounts = (from a in ca
                //                 select new Account()
                //                 {
                //                     AccountNumber = a.AccountNumber.ToString(),
                //                     AccountHolder = a.AccountHolder,
                //                     ReferenceNumber = customer.IdentificationNumber.ToString(),
                //                     Description = a.Description,
                //                     IsActive = true
                //                 }).ToList();

                //}
                //else
                //{
                     custAccounts = context.CustomerAccounts.Where(o => o.AccountNo.ToString() == accountNo).ToList();
                     cAccounts = (from a in custAccounts
                                     select new Account()
                                     {
                                         AccountNumber = a.AccountNo.ToString(),
                                         AccountHolder = a.Surname + ' ' + a.Initials,
                                         ReferenceNumber = a.IDNo.ToString(),
                                         Description = string.Format("{0} - {1}", a.AccountNo, a.StandAddr),
                                         IsActive = true
                                     }).ToList();
              //  }
                var accounts = cAccounts.Where(a => a.AccountNumber == accountNo);
                var getaccs = context.Accounts.Where(a => a.AccountNumber == accountNo).ToList().FirstOrDefault();
                if (getaccs != null)
                {
                    int accid = getaccs.Id;
                    var alreadyexists = context.LinkedAccounts.Where(a => a.AccountId == accid && a.IsActive && !a.IsDeleted);
                    var linkedToCustomer = context.LinkedAccounts.Where(a => a.AccountId == accid && a.IsActive && !a.IsDeleted && a.CustomerId==CurrCustomer.Id);
                   
                    if (linkedToCustomer.Any()) return Json(new { taken1 = true }, JsonRequestBehavior.AllowGet);
                    else if (alreadyexists.Any()) return Json(new { taken = true }, JsonRequestBehavior.AllowGet);

                }

                if (accounts.Any())
                    return Json(new
                    {
                        isValid = true,
                        accountHolder = accounts.First().AccountHolder
                        ,
                        accountType = (accounts.Select(a => new { IsRms = a.IsRMSAccount }).Any(a => a.IsRms)) ? "Venus" : "Venus"
                    }
                        , JsonRequestBehavior.AllowGet);
            }

            return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);

        }
        #endregion
    }
}