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
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Globalization;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class DebitOrderController : Controller
    {
        public DebitOrderController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public DebitOrderController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public DebitOrderController(eServicesDbContext db)
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

        //Get All Debit Orders For Back Office
        //[Authorize(Roles = "Financial Clerk" + "," + "Super Administrators" )]
        public ActionResult GADO()
        {
            using (var cxt = new eServicesDbContext())
            {
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                //Switch to debitorder status

                return View();

            }
        }
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult GADOLoad()
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                DateTime golive = new DateTime(2019, 07, 01);
                var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer).Where(o=>o.CreatedDateTime>golive).ToList();
                var json = Json(new { data = currentDebitOrders }, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = Int32.MaxValue;

                return json;
            }
        }

        // GET: DebitOrder
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                ViewBag.limit = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RecurringDebitOrdersStrikeDayEdit).Value;
                ViewBag.limitMonthly = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsMonthly).IsActive;
                ViewBag.limitDaily = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsDaily).IsActive;
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Customer == null)
                    return RedirectToAction("Index", "Profile");

                var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                            .Include(o => o.Account)
                            .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked && la.StatusId==4)
                            .ToList();
                var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Where(o => o.CustomerId == Customer.Id).ToList();
                var bankAccounts = cxt.BankAccounts.Where(o => o.IsActive && !o.IsDeleted && o.IsAccountHolder==true &&o.AccountStatus=="Verified" && o.CustomerId == Customer.Id);

                this.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", this.Customer.Id));

                DebitOrderViewModel dovm = new DebitOrderViewModel()
                {
                    Customer = this.Customer
                };

                ViewBag.BankAccounts = new SelectList(bankAccounts.ToList(), "Id", "HiddenBankAccountNumber");
                ViewBag.CurrentAccounts = new SelectList(currentAccounts.ToList(), "Account.Id", "Account.Description");
                ViewBag.CurrentDebitOrders = currentDebitOrders;

                if (bankAccounts.Count() == 0 || currentAccounts.Count() == 0)
                    ViewBag.DebitOrderWarning = "Please add bank accounts and link municipality accounts to your profile before creating debit orders.";

                return View(dovm);
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index(DebitOrderViewModel vm)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);

                    // Setup supporting data.
                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

                    DebitOrderViewModel dovm = new DebitOrderViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                   // vm.DebitOrder.AccountId = 6;
                    if (vm.DebitOrder.Id == 0)
                    {
                        var vDate = DateTime.Now.Day;
                        var vMonth = DateTime.Now.Month;
                        var vYear = DateTime.Now.Year;
                        AppSetting DuplicatesMonthly = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsMonthly);
                        AppSetting DuplicatesDaily = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsDaily);
                        AppSetting DuplicatesAmount = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsAmount);
                        // Add new debit order.
                        if (vm.DebitOrder.IsRecurring == true)
                        {
                            //check if recurring debit order is a duplicate
                            //var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();DbFunctions.TruncateTime
                            List<DebitOrder> RecurringDuplicatOrig = new List<DebitOrder>();
                            if(DuplicatesAmount.IsActive)
                            {
                                if (DuplicatesDaily.IsDeleted)
                                {
                                    RecurringDuplicatOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.Amount==vm.DebitOrder.Amount && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsDeleted)
                                {
                                    RecurringDuplicatOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.Amount == vm.DebitOrder.Amount && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsActive || DuplicatesDaily.IsActive)
                                {
                                    RecurringDuplicatOrig.Clear();
                                }

                            }
                            else
                            {
                                if (DuplicatesDaily.IsDeleted)
                                {
                                    RecurringDuplicatOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsDeleted)
                                {
                                    RecurringDuplicatOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsActive || DuplicatesDaily.IsActive)
                                {
                                    RecurringDuplicatOrig.Clear();
                                }
                            }
                           

                            //var RecurringDuplicatOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.ToShortDateString()== vDate).OrderByDescending(x => x.Id).ToList();
                            if (RecurringDuplicatOrig.Count() != 0)
                            { 
                            var RecurringDuplicate = RecurringDuplicatOrig.First();

                            if (RecurringDuplicate != null)
                            {
                                RecurringDuplicate.IsDeleted = true;

                                cxt.Entry(RecurringDuplicate).State = EntityState.Modified;
                                cxt.SaveChanges();
                            }
                            else
                            {

                            }
                            }

                        }
                        else if (vm.DebitOrder.IsRecurring == false)
                        {

                            //var dup = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id).FirstOrDefault();
                            //var dupOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.ToShortDateString() == vDate).OrderByDescending(x => x.Id).ToList();
                            List<DebitOrder> dupOrig = new List<DebitOrder>();
                            if(DuplicatesAmount.IsActive)
                            {
                                if (DuplicatesDaily.IsDeleted)
                                {
                                    dupOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.Amount == vm.DebitOrder.Amount && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsDeleted)
                                {
                                    dupOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.Amount == vm.DebitOrder.Amount && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsActive || DuplicatesDaily.IsActive)
                                {
                                    dupOrig.Clear();
                                }
                            }
                            else
                            {
                                if (DuplicatesDaily.IsDeleted)
                                {
                                    dupOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsDeleted)
                                {
                                    dupOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                                }
                                if (DuplicatesMonthly.IsActive || DuplicatesDaily.IsActive)
                                {
                                    dupOrig.Clear();
                                }
                            }
                          

                            if ( dupOrig.Count() != 0)
                            { 
                            var dup = dupOrig.First();
                            if (dup != null)
                            {
                                dup.IsDeleted = true;

                                cxt.Entry(dup).State = EntityState.Modified;
                                cxt.SaveChanges();

                            }
                            else
                            {

                            }
                            }

                        }
                        vm.DebitOrder.CustomerId = cust.Id;
                        vm.DebitOrder.IsBatch = false;
                        vm.DebitOrder.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                        cxt.DebitOrders.Add(vm.DebitOrder);
                        DebitOrderStatus DebitOrderStatus = new DebitOrderStatus();
                        DebitOrderStatus.DebitOrderId = vm.DebitOrder.Id;
                        DebitOrderStatus.StatusId = vm.DebitOrder.StatusId;
                        DebitOrderStatus.BatchNumber = 0;//0 referes to all none batch payments
                        DebitOrderStatus.BatchReference = "Individual Debit Order";
                        DebitOrderStatus.IsActive = true;
                        cxt.DebitOrderStatuses.Add(DebitOrderStatus);
                        cxt.SaveChanges();
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order successfully created.";
                        dovm.DebitOrder = cxt.DebitOrders.SingleOrDefault(o => o.Id == vm.DebitOrder.Id && o.IsActive && !o.IsDeleted);
                    }
                    else
                    {
                        // Update debit order.
                        var order = cxt.DebitOrders.SingleOrDefault(o => o.Id == vm.DebitOrder.Id && o.IsActive && !o.IsDeleted);
                       // var DebitOrderStatus = cxt.DebitOrderStatuses.Include(dso => dso.DebitOrder).Where(dso => dso.DebitOrderId == vm.DebitOrder.Id && dso.IsActive && !dso.IsDeleted).OrderByDescending(dso=>dso.Id).ToList().First();
                        if (order != null && order.CustomerId != cust.Id)
                        {
                            ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order already created by another user.";
                            return View(dovm);
                        }

                        if (ModelState.IsValid)
                        {
                            //order.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                            //order.BankAccountId = vm.DebitOrder.BankAccountId;
                           // order.AccountId = vm.DebitOrder.AccountId;
                            order.Amount = vm.DebitOrder.Amount;
                            //order.IsRecurring = vm.DebitOrder.IsRecurring;
                            order.StrikeDay = vm.DebitOrder.StrikeDay;
                            //order.StartDateTime = vm.DebitOrder.StartDateTime;
                            //order.StrikeDay = vm.DebitOrder.StrikeDay;
                            cxt.Entry(order).State = EntityState.Modified;

                            
                           //// DebitOrderStatus.DebitOrderId = vm.DebitOrder.Id;
                           // DebitOrderStatus.StatusId = vm.DebitOrder.StatusId;
                           // cxt.Entry(DebitOrderStatus).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }

                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit Order successfully updated.";
                        dovm.DebitOrder = new DebitOrder();
                    }
                    //var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                    //       .Include(o => o.Account)
                    //       .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked)
                    //       .ToList();
                    ViewBag.BankAccounts = new SelectList(cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified").Include(o => o.BankAccountType).ToList<BankAccount>(), "Id", "HiddenBankAccountNumber");
                    ViewBag.BankAccountTypes = new SelectList(cxt.BankAccountTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                    ViewBag.CurrentAccounts = new SelectList(cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                           .Include(o => o.Account).Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.StatusId == 4).ToList(), "Account.Id", "Account.Description");
                    ViewBag.limit = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RecurringDebitOrdersStrikeDayEdit).Value;

                    return View(dovm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadOrder(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    //DebitOrder order = cxt.DebitOrders.Include(o => o.BankAccount).Include(o => o.Account).FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
                    DebitOrder order = cxt.DebitOrders.Include(o => o.BankAccount).Include(o => o.Account).FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
                    //List<DebitOrderStatus> list = cxt.DebitOrderStatuses.Include(o => o.DebitOrder).Include(o => o.DebitOrder.BankAccount).Include(o => o.DebitOrder.BankAccount.Bank).Include(o => o.DebitOrder.Account).Include(o => o.Status).Where(o => o.DebitOrder.CustomerId == Id && o.IsActive && !o.IsDeleted && o.DebitOrder.IsBatch == true && o.DebitOrder.IsActive == true && !o.DebitOrder.IsDeleted).OrderByDescending(o => o.Id).ToList();

                    var json = Json(new { data = order }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadOrders(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    List<DebitOrder> list = cxt.DebitOrders.Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Account).Include(o => o.Status).Where(o => o.CustomerId == Id && o.IsBatch==false && o.IsActive && !o.IsDeleted).OrderByDescending(o => o.Id).ToList();
                   // List<DebitOrderStatus> list = cxt.DebitOrderStatuses.Include(o => o.DebitOrder).Include(o => o.DebitOrder.BankAccount).Include(o => o.DebitOrder.BankAccount.Bank).Include(o => o.DebitOrder.Account).Include(o => o.Status).Where(o => o.DebitOrder.CustomerId == Id && o.IsActive && !o.IsDeleted && o.DebitOrder.IsBatch == false && o.DebitOrder.IsActive == true && !o.DebitOrder.IsDeleted).OrderByDescending(o => o.Id).ToList();

                    foreach (var order in list)
                    {
                        order.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", order.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteOrder(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    var order = cxt.DebitOrders.SingleOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
                    try { 
                    var orderStats = cxt.DebitOrderStatuses.Include(dso=>dso.DebitOrder).Where(dso => dso.DebitOrder.Id == Id && dso.IsActive && !dso.IsDeleted).OrderByDescending(dso=>dso.Id).ToList().First();
                        // var DebitOrderStatus = cxt.DebitOrderStatuses.Include(dso => dso.DebitOrder).Where(dso => dso.DebitOrderId == vm.DebitOrder.Id && dso.IsActive && !dso.IsDeleted).OrderByDescending(dso=>dso.Id).ToList().First();
                   
                    if (order != null)
                    {
                        order.IsDeleted = true;
                            order.IsActive = false;
                        cxt.Entry(order).State = EntityState.Modified;

                        orderStats.IsDeleted = true;
                            orderStats.IsActive = false;
                       cxt.Entry(orderStats).State = EntityState.Modified;
                        cxt.SaveChanges();
                    }
                    }
                    catch (Exception e)
                    {
                        var mess = e;
                    }
                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id && o.IsActive && !o.IsDeleted);

                    DebitOrderViewModel dovm = new DebitOrderViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                    return RedirectToAction("Index", "DebitOrder");
                }

                return null;
            }
        }

        #region Check if payment is duplicate to Warn Customer
        [HttpPost]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult CheckDuplicatePayments(int AccId, bool isRecc, decimal Amt)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var CurrUser = User.Identity.GetUserName();
                    var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
                    if (customer == null) throw new Exception("Invalid Customer");
                    var vDate = DateTime.Now.Day;
                    var vMonth = DateTime.Now.Month;
                    var vYear = DateTime.Now.Year;
                    
                    AppSetting DuplicatesMonthly = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsMonthly);
                    AppSetting DuplicatesDaily = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsDaily);
                    AppSetting DuplicatesAmount = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DOMultiplePaymentsAmount);
                    //var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();DbFunctions.TruncateTime
                    if (isRecc == true)
                    {
                        //check if recurring debit order is a duplicate
                        //var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();DbFunctions.TruncateTime
                        List<DebitOrder> RecurringDuplicatOrig = new List<DebitOrder>();
                        if(DuplicatesAmount.IsActive)
                        {
                            if (DuplicatesDaily.IsActive)
                            {
                                RecurringDuplicatOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.Amount == Amt && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();
                             
                            }
                            if (DuplicatesMonthly.IsActive)
                            {
                                RecurringDuplicatOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.Amount == Amt && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                            }
                        }
                        else { 
                        if (DuplicatesDaily.IsActive)
                        {
                            RecurringDuplicatOrig = context.DebitOrders.Where(x => x.AccountId == AccId  && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                        }
                        if (DuplicatesMonthly.IsActive)
                        {
                            RecurringDuplicatOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                        }
                        }
                        //var RecurringDuplicatOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted && x.CreatedDateTime.Value.ToShortDateString()== vDate).OrderByDescending(x => x.Id).ToList();
                        if (RecurringDuplicatOrig.Count() != 0)
                        {
                            var RecurringDuplicate = RecurringDuplicatOrig.First();

                            if (RecurringDuplicate != null)
                            {
                                if (DuplicatesAmount.IsActive)
                                {
                                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { isValidNoAmt = true }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    else if (isRecc == false)
                    {

                        //var dup = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id).FirstOrDefault();
                        //var dupOrig = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.ToShortDateString() == vDate).OrderByDescending(x => x.Id).ToList();
                        List<DebitOrder> dupOrig = new List<DebitOrder>();
                        if (DuplicatesAmount.IsActive)
                        {
                            if (DuplicatesDaily.IsActive)
                            {
                                dupOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.Amount == Amt && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                            }
                            if (DuplicatesMonthly.IsActive && DuplicatesDaily.IsDeleted)
                            {
                                dupOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.Amount == Amt && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                            }
                        }
                        else { 
                            if (DuplicatesDaily.IsActive)
                        {
                            dupOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth && x.CreatedDateTime.Value.Day == vDate).OrderByDescending(x => x.Id).ToList();

                        }
                        if (DuplicatesMonthly.IsActive && DuplicatesDaily.IsDeleted)
                        {
                            dupOrig = context.DebitOrders.Where(x => x.AccountId == AccId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id && x.CreatedDateTime.Value.Year == vYear && x.CreatedDateTime.Value.Month == vMonth).OrderByDescending(x => x.Id).ToList();

                        }
                        }
                        if (dupOrig.Count() != 0)
                        {
                            var dup = dupOrig.First();
                            if (dup != null)
                            {

                                if (DuplicatesAmount.IsActive)
                                {
                                    return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { isValidNoAmt = true }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }

            }

        }
        #endregion

        #region All Debit Order PAyments GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewDOPayments()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return View("_Error");
            }
        }
        #endregion

        #region Load All Debit Order Payments GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadDoPayments(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var debitOrders = new List<DebitOrder>();
            var rows = new List<object>();
            var skip = (pageNumber - 1) * pageSize;
            const string goLiveDate = "2019/07/01";
            var startDateTime = (string.IsNullOrEmpty(startDate)) ? DateTime.ParseExact(goLiveDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(startDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDateTime = (string.IsNullOrEmpty(endDate)) ? DateTime.ParseExact(DateTime.Now.ToString("yyyyMMdd"), "yyyyMMdd", CultureInfo.InvariantCulture) : DateTime.ParseExact(endDate.Replace(@"/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            var recurringText = "";
            var batchText = "";
            var paymentText = "";
            var emailText = "";
            var mobileNoText = "";
            try
            {
                using (var context = new eServicesDbContext())
                {
                    //Gets system users with no customer profile o=>o.CreatedDateTime>golive
                    // var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer).Where(o=>o.CreatedDateTime>golive).ToList();
                    var debitOrdersModel = context.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Status).Include(o => o.Customer)
                        .Where(s => (DbFunctions.TruncateTime(s.CreatedDateTime) >= startDateTime && DbFunctions.TruncateTime(s.CreatedDateTime) <= endDateTime));

                    Status queryPending = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending);
                    Status querySent = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent);
                    Status querySuccess = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess);
                    Status queryFailed = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderFailed);


                    //Filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var isNumericAccount = double.TryParse(searchText, out double n);
                        var isString = double.TryParse(searchText, out double str);

                        //|| (searchText.ToUpper() == "SINGLE") || (searchText.ToUpper() == "RECURRING") || searchText.ToUpper() == "ONCE-OFF" || searchText.ToUpper() == "ONCEOFF" || searchText.ToUpper() == "ONCE OFF"
                        if (searchText.ToUpper() == "BATCH" || searchText.ToUpper() == "BATCH PAYMENT")
                        {
                            searchText = "true";
                            debitOrders = debitOrdersModel.Where(s => (s.IsBatch.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "SINGLE" || searchText.ToUpper() == "SINGLE PAYMENT")
                        {
                            searchText = "false";
                            debitOrders = debitOrdersModel.Where(s => (s.IsBatch.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() =="RECURRING" || searchText.ToUpper() == "RECURRING PAYMENT" || searchText.ToUpper() == "RECURRING DEBIT ORDER")
                        {
                            searchText = "true";
                            debitOrders = debitOrdersModel.Where(s => (s.IsRecurring.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "ONCE-OFF" || searchText.ToUpper() == "ONCE-OFF PAYMENT" || searchText.ToUpper() == "ONCE-OFF DEBIT ORDER" || searchText.ToUpper() == "ONCEOFF" || searchText.ToUpper() == "ONCE OFF")
                        {
                            searchText = "false";
                            debitOrders = debitOrdersModel.Where(s => (s.IsRecurring.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "ACTIVE" || searchText.ToUpper() == "ACTIVE DEBIT ORDER" ||searchText.ToUpper() == "ACTIVE PAYMENT")
                        {
                            searchText = "true";
                            debitOrders = debitOrdersModel.Where(s => (s.IsActive.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "CANCELLED" || searchText.ToUpper() == "CANCELLED DEBIT ORDER" || searchText.ToUpper() == "CANCELLED PAYMENT")
                        {
                            searchText = "false";
                            debitOrders = debitOrdersModel.Where(s => (s.IsActive.ToString() ?? "").Contains(searchText)).ToList();
                        }
                //        // if (status=="SUCCESS")
                //        {
                //            instantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess).Id;
                //        }
                //else if (status == "FAILED")
                //        {
                //            instantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderFailed).Id;
                //        }
                //        else
                //        {
                //            instantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderFailed).Id;
                //        }
                        else if (searchText.ToUpper() == "PENDING")
                        {
                            int searchId = queryPending.Id;
                            debitOrders = debitOrdersModel.Include(o=>o.Status).Where(s =>(s.Status.Id==searchId)).ToList();
                        }
                        else if (searchText.ToUpper() == "SENT" || searchText.ToUpper() == "PROCESSING")
                        {
                            int searchId = querySent.Id;
                            debitOrders = debitOrdersModel.Include(o => o.Status).Where(s => (s.Status.Id == searchId)).ToList();
                        }
                        else if (searchText.ToUpper() == "SUCCESS")
                        {
                            int searchId = querySuccess.Id;
                            debitOrders = debitOrdersModel.Include(o => o.Status).Where(s => (s.Status.Id == searchId)).ToList();
                        }
                        else if (searchText.ToUpper() == "FAILED")
                        {
                            int searchId = queryFailed.Id;
                            debitOrders = debitOrdersModel.Include(o => o.Status).Where(s => (s.Status.Id == searchId)).ToList();
                        }
                        else if (isNumericAccount)
                        {
                            debitOrders = debitOrdersModel.Where(s => (s.Account.AccountNumber ?? "").Contains(searchText) ||
                                                                (s.Customer.IdentificationNumber ?? "").Contains(searchText) ||
                                                                (s.Amount.ToString() ?? "").Contains(searchText) ||

                                                                (s.Customer.SystemUser.MobileNumber ?? "").Contains(searchText)).ToList();
                        }
                        else if (!isNumericAccount)
                        {
                            debitOrders = debitOrdersModel.Where(s =>  (s.Customer.FirstName ?? "").Contains(searchText) ||
                                       (s.Customer.LastName ?? "").Contains(searchText) ||
                                       (s.Status.Name ?? "").Contains(searchText) ||
                                       (s.CreatedDateTime.ToString() ?? "").Contains(searchText) ||
                                       (s.Customer.SystemUser.EmailAddress ?? "").Contains(searchText)).ToList();
                        }
                        else
                        {
                            debitOrders = debitOrdersModel.Where(s => (s.Account.AccountNumber ?? "").Contains(searchText) ||
                                                                 (s.Customer.FirstName ?? "").Contains(searchText) ||
                                                                 (s.Customer.LastName ?? "").Contains(searchText) ||
                                                                 (s.Customer.IdentificationNumber ?? "").Contains(searchText) ||
                                                                 (s.Amount.ToString() ?? "").Contains(searchText) ||
                                                                 (s.IsRecurring.ToString() ?? "").Contains(searchText) ||
                                                                 (s.IsBatch.ToString() ?? "").Contains(searchText) ||
                                                                 (s.Status.Name ?? "").Contains(searchText) ||
                                                                 (s.CreatedDateTime.ToString() ?? "").Contains(searchText) ||
                                                                 (s.Customer.SystemUser.EmailAddress ?? "").Contains(searchText) ||
                                                                 (s.Customer.SystemUser.MobileNumber ?? "").Contains(searchText)).ToList();
                        }

                        debitOrders = debitOrders.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                        total = debitOrdersModel.Count();
                    }
                    else
                    {
                        debitOrders = debitOrdersModel.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                        total = debitOrdersModel.Count();
                    }

                    foreach (var debitOrder in debitOrders)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,

                        if(debitOrder.IsRecurring){recurringText = "Recurring";}
                        else{ recurringText = "Once-Off";}
                        if (debitOrder.IsBatch==true)
                        {batchText = "Batch";}
                       else { batchText = "Single"; }
                        if (debitOrder.IsActive)
                        {paymentText = "Active"; }
                        else { paymentText = "Cancelled"; }

                        var custSystem = context.SystemUsers.FirstOrDefault( s=>s.Id==debitOrder.Customer.SystemUserId);
                        if (!string.IsNullOrEmpty(custSystem.EmailAddress))
                        {
                            emailText = custSystem.EmailAddress;
                        }
                        if (!string.IsNullOrEmpty(debitOrder.Customer.EmailAddress))
                        {
                            emailText = debitOrder.Customer.EmailAddress;
                        }
                        else
                        {
                            emailText = "N/A";
                        }

                        if (!string.IsNullOrEmpty(custSystem.MobileNumber))
                        {
                            mobileNoText = custSystem.MobileNumber;
                        }
                        if (!string.IsNullOrEmpty(debitOrder.Customer.CellPhoneNumber))
                        {
                            mobileNoText = debitOrder.Customer.CellPhoneNumber;
                        }
                        else
                        {
                            mobileNoText = "N/A";
                        }



                        rows.Add(new
                        {
                            
                            municipalAccount = debitOrder.Account.AccountNumber,
                            customerName = debitOrder.Customer.FullName,
                            customerId = debitOrder.Customer.IdentificationNumber,
                            amount = debitOrder.Amount.ToString("0.00"),
                            recurring = recurringText,
                            batch = batchText,
                            payment = paymentText,
                            status = debitOrder.Status.Name,
                            createdDate = String.Format("{0:yyyy/MM/dd}", debitOrder.CreatedDateTime),
                            emailAddress = emailText,
                            mobileNo = mobileNoText

                        });
                    }

                    return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



        /*__________________________________________Batch Debit Order Payments___________________________________________________________________________*/
        //Bulk Batch Payment
        // GET: DebitOrder
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult IndexBatchPayment()
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Customer == null)
                    return RedirectToAction("Index", "Profile");

                var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                            .Include(o => o.Account)
                            .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked && la.StatusId == 4)
                            .ToList();
                var currentDebitOrders = cxt.DebitOrders.Include(o => o.Account).Include(o => o.BankAccount).Where(o => o.CustomerId == Customer.Id).ToList();
                var bankAccounts = cxt.BankAccounts.Where(o => o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified" && o.CustomerId == Customer.Id);

                this.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", this.Customer.Id));

                DebitOrderViewModel dovm = new DebitOrderViewModel()
                {
                    Customer = this.Customer
                };

                ViewBag.BankAccounts = new SelectList(bankAccounts.ToList(), "Id", "HiddenBankAccountNumber");
                ViewBag.CurrentAccounts = new SelectList(currentAccounts.ToList(), "Account.Id", "Account.Description");
                ViewBag.CurrentDebitOrders = currentDebitOrders;

                if (bankAccounts.Count() == 0 || currentAccounts.Count() == 0)
                    ViewBag.DebitOrderWarning = "Please add bank accounts and link municipality accounts to your profile before creating debit orders.";

                return View(dovm);
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult IndexBatchPayment(DebitOrderViewModel vm)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);

                    // Setup supporting data.
                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

                    DebitOrderViewModel dovm = new DebitOrderViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                    // vm.DebitOrder.AccountId = 6;
                    if (vm.DebitOrder.Id == 0)
                    {
                        // Add new debit order.
                        if (vm.DebitOrder.IsRecurring == true)
                        {
                            //check if recurring debit order is a duplicate
                            var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();

                            if (RecurringDuplicate != null)
                            {
                                RecurringDuplicate.IsDeleted = true;

                                cxt.Entry(RecurringDuplicate).State = EntityState.Modified;
                                cxt.SaveChanges();
                            }
                            else
                            {

                            }

                        }
                        else if (vm.DebitOrder.IsRecurring == false)
                        {

                            var dup = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id).FirstOrDefault();
                            if (dup != null)
                            {
                                dup.IsDeleted = true;

                                cxt.Entry(dup).State = EntityState.Modified;
                                cxt.SaveChanges();

                            }
                            else
                            {

                            }

                        }
                        vm.DebitOrder.CustomerId = cust.Id;
                        vm.DebitOrder.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                        cxt.DebitOrders.Add(vm.DebitOrder);
                        cxt.SaveChanges();
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order successfully created.";
                        dovm.DebitOrder = cxt.DebitOrders.SingleOrDefault(o => o.Id == vm.DebitOrder.Id && o.IsActive && !o.IsDeleted);
                    }
                    else
                    {
                        // Update debit order.
                        var order = cxt.DebitOrders.SingleOrDefault(o => o.Id == vm.DebitOrder.Id && o.IsActive && !o.IsDeleted);
                        if (order != null && order.CustomerId != cust.Id)
                        {
                            ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order already created by another user.";
                            return View(dovm);
                        }

                        if (ModelState.IsValid)
                        {
                            order.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                            order.BankAccountId = vm.DebitOrder.BankAccountId;
                            order.AccountId = vm.DebitOrder.AccountId;
                            order.Amount = vm.DebitOrder.Amount;
                            order.IsRecurring = vm.DebitOrder.IsRecurring;
                            order.StrikeDay = vm.DebitOrder.StrikeDay;
                            order.StartDateTime = vm.DebitOrder.StartDateTime;
                            order.StrikeDay = vm.DebitOrder.StrikeDay;
                            cxt.Entry(order).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }

                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Bank account successfully updated.";
                        dovm.DebitOrder = new DebitOrder();
                    }
                    //var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                    //       .Include(o => o.Account)
                    //       .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked)
                    //       .ToList();
                    ViewBag.BankAccounts = new SelectList(cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified").Include(o => o.BankAccountType).ToList<BankAccount>(), "Id", "HiddenBankAccountNumber");
                    ViewBag.BankAccountTypes = new SelectList(cxt.BankAccountTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                    ViewBag.CurrentAccounts = new SelectList(cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                           .Include(o => o.Account).Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.StatusId == 4).ToList(), "Account.Id", "Account.Description");

                    return View(dovm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }


        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadBatchOrders(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
    

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    List<DebitOrderStatus> list = cxt.DebitOrderStatuses.Include(o => o.DebitOrder).Include(o=>o.DebitOrder.BankAccount).Include(o => o.DebitOrder.BankAccount.Bank).Include(o => o.DebitOrder.Account).Include(o => o.Status).Where(o => o.DebitOrder.CustomerId == Id  && o.IsActive && !o.IsDeleted && o.DebitOrder.IsBatch == true && o.DebitOrder.IsActive == true && !o.DebitOrder.IsDeleted).OrderByDescending(o => o.Id).ToList();

                    foreach (var order in list)
                    {
                        order.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", order.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadAccountBatch(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                BillingDbContext EkuContext = new BillingDbContext();

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    List<BatchAccount> list = new List<BatchAccount>();

                    try {

                        var clientIdParameter = new SqlParameter("@CustId", Id);

                        var result = EkuContext.Database
                            .SqlQuery<BatchAccount>("getBatchAccounts @CustId", clientIdParameter)
                            .ToList();

                        foreach (var order in result)
                    {
                        order.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", order.Id));
                    }

                    var json = Json(new { data = result }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                    }
                    catch (Exception err)
                    {
                        var mess = err;
                        var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                        json.MaxJsonLength = Int32.MaxValue;

                        return json;
                       
                    }


                }

                return null;
            }
        }

        //[EncryptedActionParameter]
        [Authorize]
        [HttpPost]
        public JsonResult BulkPayment(string[] ControllerDebitAmount, int CusId, int BId,  string [] MAccounts, string bat, string dStrike)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.Id == CusId);
                    if (customer == null) throw new Exception("Invalid Customer");
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(context);

                    // Setup supporting data.
                    var cust = context.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    //cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));
                    BillingDbContext EkuContext = new BillingDbContext();
                    List<BatchAccount> list = new List<BatchAccount>();
                    if (CusId > 0)
                    {
                            var clientIdParameter = new SqlParameter("@CustId", CusId);
                            var result = EkuContext.Database
                                .SqlQuery<BatchAccount>("getBatchAccounts @CustId", clientIdParameter)
                                .ToList();
                        //var jj = 0;
                        //    foreach (var order in result)
                        //    {
                        //    if (MAccounts.Length > jj)
                        //    {
                        //        if (order.NEW_ACCOUNT.ToString() == MAccounts[jj])
                        //        {
                        //            list.Add(order);
                        //        }
                        //    }
                        //    jj++;
                        //    }

                            foreach(var MAcc in MAccounts)
                              {
                                    foreach (var order in result)
                                    {
                                     
                                     if (order.NEW_ACCOUNT.ToString() == MAcc)
                                        {
                                        list.Add(order);
                                        }
                                    
                               
                              }
                        }
                        
                        var i = 0;
                        //string batchRef = bat.Substring(1,(bat.Length-1));
                        string batchRef = bat;
                        Int64 batchNum =Convert.ToInt64(batchRef);
                        Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                        string init = initials.Replace(cust.FullName, "$1");
                        string CusInitials = init+cust.LastName+ batchRef;
                        foreach (var bdo in list)
                        {
                            DebitOrder BatDebitOrder = new DebitOrder();
                            DebitOrderStatus BatDebitOrderStatus = new DebitOrderStatus();
                            BatDebitOrder.BankAccountId = BId;
                            BatDebitOrder.AccountId = bdo.AccountId;
                            string arym = ControllerDebitAmount[i];
                            arym = arym.Replace('.', ',');
                            decimal amt = decimal.Parse(arym);
                            BatDebitOrder.Amount = amt;
                            BatDebitOrder.StrikeDay =Convert.ToInt32(dStrike);
                            BatDebitOrder.CustomerId = cust.Id;
                            BatDebitOrder.StartDateTime = DateTime.Now;
                            BatDebitOrder.IsRecurring = false;
                            BatDebitOrder.IsActive = true;
                            BatDebitOrder.IsBatch = true;
                            BatDebitOrder.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;

                            context.DebitOrders.Add(BatDebitOrder);

                            BatDebitOrderStatus.DebitOrderId = BatDebitOrder.Id;
                            BatDebitOrderStatus.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                            BatDebitOrderStatus.BatchNumber = batchNum;
                            BatDebitOrderStatus.BatchReference = CusInitials;
                            BatDebitOrderStatus.IsActive = true;
                            context.DebitOrderStatuses.Add(BatDebitOrderStatus);
                            context.SaveChanges();
                            i++;
                        }

                        var test = 0;
                        //ViewBag.BankAccounts = new SelectList(context.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified").Include(o => o.BankAccountType).ToList<BankAccount>(), "Id", "HiddenBankAccountNumber");
                        //ViewBag.BankAccountTypes = new SelectList(context.BankAccountTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
                        //ViewBag.CurrentAccounts = new SelectList(context.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                        //       .Include(o => o.Account).Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.StatusId == 4).ToList(), "Account.Id", "Account.Description");

                        return Json(new { isValid = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (Exception e)
                {
                    return Json(new { isValid = false }, JsonRequestBehavior.AllowGet);
                }
               
            }
                   }

        public ActionResult BatchDailySequence()
        {
            using (var context = new eServicesDbContext())
            {
                int limiter = 0;
                try
                {
                    var CurrUser = User.Identity.GetUserName();
                    var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
                    AppSetting query = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DebitOrderBatchSequence);
                    //var query = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DebitOrderBatchSequence);
                    var SeqLimit = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DebitOrderBatchSequenceLimit);
                    var BatchCounter = query.Value;
                     limiter = Convert.ToInt16(SeqLimit.Value);
                    if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                    {
                        var lastRef = query.Value;
                        BatchCounter = lastRef.ToString();
                        int nextSeq = Convert.ToInt16(query.Value) + 1;
                        string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                        query.Value = nextVal;
                        context.Entry(query).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    else
                    {
                        int nextSeq = 1;
                        string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                        BatchCounter = nextVal;
                        nextSeq = 2;
                        nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                        query.Value = nextVal;
                        query.ModifiedDateTime = DateTime.Now.Date;
                        context.Entry(query).State = EntityState.Modified;
                        context.SaveChanges();
                        
                    }
                    Session["BatchCount"] = BatchCounter;
                }
                catch (Exception e)
                {
                    //int nextSeq = 1;
                    var mess = e;
                   // string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                   // Session["BatchCount"] = nextVal;
                    return null;
                }

                return null;
            }
        }
    }
}