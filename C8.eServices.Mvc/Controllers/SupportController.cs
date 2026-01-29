using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    public class SupportController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        BaseHelper _base = new BaseHelper();
        public SupportController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public SupportController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public SupportController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }

        #region Report Init

        private void Initialise()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    IdentityManager = new IdentityManager(context);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            context.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                context.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = context.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #endregion

        // GET: Support
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None)]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);

                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);

                    if (cust == null)
                        return RedirectToAction("Index", "Profile");

                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

                    ViewBag.Accounts = cxt.LinkedAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).Include(o => o.Account).ToList();
                    ViewBag.QueryTypes = new SelectList(cxt.QueryTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");

                    foreach (var a in ViewBag.Accounts)
                    {
                        a.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("Id={0}", a.Id)));
                    }

                    ViewBag.Accounts = new SelectList(ViewBag.Accounts, "Account.Id", "Account.AccountNumber");

                    SupportViewModel dovm = new SupportViewModel()
                    {
                        Customer = cust,
                        Queries = cxt.SupportQueries.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList(),
                        Query = new Support()
                    };

                    return View(dovm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index(SupportViewModel vm)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                   
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);
                    
                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

                   
                    SupportViewModel svm = new SupportViewModel()
                    {
                        Customer = cust,
                        Queries = cxt.SupportQueries.Include(o => o.Account).Include(o => o.QueryType).Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList()
                    };
                    QueryType queryTypeKey = cxt.QueryTypes.FirstOrDefault(x=>x.Key== QueryTypeKeys.DeactiveAccount);
                    // AJC Check if User Already Sent A Request For Deactivation And Dont let them send another.
                    var Status = cxt.Status.FirstOrDefault(x => x.Key == StatusKeys.SupportQueryResolved).Id;
                    //var DeactivateRequest = svm.Queries.Where(x=>x.QueryTypeId==queryTypeKey.Id);
                    var DeactivateRequest = cxt.SupportQueries.Include(o => o.Account).Include(o => o.QueryType).FirstOrDefault(o => o.CustomerId == cust.Id && o.QueryTypeId == queryTypeKey.Id && o.IsActive && !o.IsDeleted);
                    var QueryType= cxt.QueryTypes.FirstOrDefault(x => x.Id == vm.Query.QueryTypeId);
                    if (DeactivateRequest != null && QueryType.Key== QueryTypeKeys.DeactiveAccount)
                    {
                        ViewBag.Accounts = new SelectList(cxt.LinkedAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).Include(o => o.Account).ToList(), "Account.Id", "Account.AccountNumber");
                        ViewBag.QueryTypes = new SelectList(cxt.QueryTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                        TempData["Success"] = false;
                        ViewBag.QueryMessage = "A Deactivate account request has already been sent.";
                        return View(svm);
                    }

                    if (vm.Query.Id == 0)
                    {
                        vm.Query.CustomerId = cust.Id;
                        vm.Query.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SupportQueryPending).Id;
                        var CaseRef = "Siya-Ref:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + vm.Query.Id + vm.Query.QueryTypeId;
                        vm.Query.CaseNumber = CaseRef;
                        //If i do not have an account sets account id to 1
                        if(vm.Query.AccountId==null)
                        {
                            vm.Query.AccountId =1;
                        }
                        cxt.SupportQueries.Add(vm.Query);
                        cxt.SaveChanges();
                        TempData["Success"] = true;
                        ViewBag.QueryMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Support ticket successfully created.";
                        svm.Query = cxt.SupportQueries.Include(o => o.QueryType).Include(o => o.Account).FirstOrDefault(o => o.Id == vm.Query.Id && o.IsActive && !o.IsDeleted);
                        SendSupportTicket(vm.Query);
                        SendSupportEmailSms(vm.Query);
                    }

                    else
                    {
                        var q = cxt.SupportQueries.Include(o => o.QueryType).Include(o => o.Account).FirstOrDefault(o => o.Id == vm.Query.Id && o.IsActive && !o.IsDeleted);

                        if (ModelState.IsValid)
                        {
                            var CaseRef = "Siya-Ref:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + q.Id+q.QueryTypeId;
                            q.AccountId = vm.Query.Account.Id;
                            q.QueryTypeId = vm.Query.QueryTypeId;
                            q.Description = vm.Query.Description;
                            q.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SupportQueryPending).Id;
                            q.CaseNumber = CaseRef;
                            q.IsActive = true;
                            q.IsDeleted = false;
                            cxt.Entry(q).State = EntityState.Modified;
                            cxt.SaveChanges();
                            SendSupportTicket(q);
                            SendSupportEmailSms(q);
                        }
                        TempData["Success"] = true;
                        ViewBag.QueryMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Support ticket successfully created.";
                        svm.Query = new Support();
                    }

                    ViewBag.Accounts = new SelectList(cxt.LinkedAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).Include(o => o.Account).ToList(), "Account.Id", "Account.AccountNumber");
                    ViewBag.QueryTypes = new SelectList(cxt.QueryTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");

                    return View(svm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        private void SendSupportTicket(Support q)
        {
            try
            {
                //Confirmation email
                var email = new Email();
                var body = new StringBuilder();

                body.AppendLine(string.Format("Query Support Ticket created on {0}.<br/><br/>", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")));
                body.AppendLine(string.Format("<b>Account No.:</b> {0}<br/>", q.Account.AccountNumber));
                body.AppendLine(string.Format("<b>Query Type:</b> {0}<br/>", q.QueryType.Name));
                body.AppendLine(string.Format("<b>Description:</b> {0}<br/>", q.Description));
                if (SystemUser.EmailAddress != null)
                {
                    body.AppendLine(string.Format("<b>Customer Email Address:</b> {0}<br/>", SystemUser.EmailAddress));
                }
                if (SystemUser.MobileNumber != null)
                {
                    body.AppendLine(string.Format("<b>Customer Mobile Number:</b> {0}<br/>", SystemUser.MobileNumber));
                }
                //siyakhokha@ekurhuleni.gov.za
                email.GenerateEmail(
                     "siyakhokha@ekurhuleni.gov.za",
                    "Ticket: "+q.CaseNumber+" - " + q.QueryType.Name,
                    body.ToString(),
                    SystemUser.Id.ToString(CultureInfo.InvariantCulture),
                    false,
                    AppSettingKeys.EservicesDefaultEmailTemplate,
                    SystemUser.FullName);


            }
            catch (Exception x)
            {

                throw x;
            }
        }
        private void SendSupportEmailSms(Support q)
        {
            string response;
      
                    //_base.Initialise(context);
                    //var systemUser = context.SystemUsers.FirstOrDefault(su => su.Id == _base.SystemUser.Id);
                    var user = UserManager.FindById(User.Identity.GetUserId());
           
            if (user != null)
            {
                if (SystemUser.EmailAddress != null && !SystemUser.EmailAddress.Trim().Equals(string.Empty))
                {
                    //Confirmation email

                    try
                    {
                        //Confirmation email
                        var email = new Email();
                        var body2 = new StringBuilder();

                        body2.AppendLine(string.Format("Query Support Ticket created on {0}.<br/><br/>", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")));
                        body2.AppendLine(string.Format("<b>Account No.:</b> {0}<br/>", q.Account.AccountNumber));
                        body2.AppendLine(string.Format("<b>Query Type:</b> {0}<br/>", q.QueryType.Name));
                        body2.AppendLine(string.Format("<b>Description:</b> {0}<br/>", q.Description));
                        body2.AppendLine(string.Format("<b>Feel confident that our support team is currently investigating your issue and will respond to you as soon as it has been resolved.</b> <br/>"));
                        //body2.AppendLine(string.Format("<b>Feel confident that our support team is currently investigating your issue and will respond to you as soon as it has been resolved. </b> {0}<br/>"));
                        email.GenerateEmail(
                            SystemUser.EmailAddress,
                            "Siyakhokha Support Ticket",
                            body2.ToString(),
                            SystemUser.Id.ToString(CultureInfo.InvariantCulture),
                            false,
                            AppSettingKeys.EservicesDefaultEmailTemplate,
                            SystemUser.FullName);


                    }
                    catch (Exception x)
                    {

                        throw x;
                    }

                }
                
               try
                    {
                    if (SystemUser.MobileNumber != null && !SystemUser.MobileNumber.Trim().Equals(string.Empty))
                    {
                        var bodysms = new StringBuilder(); 
                        bodysms.AppendLine(string.Format("Query Support Ticket created on {0}.\\n", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt")));
                        bodysms.AppendLine(string.Format("Account No.: {0}\\n", q.Account.AccountNumber));
                        bodysms.AppendLine(string.Format("Query Type: {0}\\n", q.QueryType.Name));
                        //bodysms.AppendLine(string.Format("Description: {0}\n", q.Description));
                        bodysms.AppendLine(string.Format("Feel confident that our support team is currently investigating your issue and will respond to you as soon as it has been resolved. \\n"));
                        var sms = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                        sms.GenerateSMS(SystemUser.MobileNumber, bodysms.ToString(),
    SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, SystemUser.FullName);
                        //SmsHelper.Send(SystemUser.MobileNumber, bodysms.ToString());
                    }

                    response = "Success";
                    ViewBag.EmailAddress = SystemUser.EmailAddress == null || SystemUser.EmailAddress.Trim() == string.Empty ? SystemUser.MobileNumber : SystemUser.EmailAddress;

                }
                catch (Exception smserr)
                {

                }
         }
            else
            {
                response = "Failed";
                /* AddErrors(result);*/
            }
        }
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadQuery(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    Support a = cxt.SupportQueries.Include(o => o.Account).Include(o => o.QueryType).FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);

                    var json = Json(new { data = a }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadQueries(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    List<Support> list = cxt.SupportQueries.Include(o => o.Account).Include(o => o.QueryType).Include(o => o.Status).Where(o => o.CustomerId == Id && o.IsActive && !o.IsDeleted).OrderByDescending(o => o.CreatedDateTime).ToList();

                    foreach (var q in list)
                    {
                        q.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", q.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }
    }
}