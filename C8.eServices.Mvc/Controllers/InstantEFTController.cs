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
using System.Net;
using System.Globalization;

namespace C8.eServices.Mvc.Controllers
{
    public class InstantEFTController : Controller
    {
        public InstantEFTController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public InstantEFTController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public InstantEFTController(eServicesDbContext db)
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
        public int? EFTDetailSum { get; set; }
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
        #region Original Methods
        //// GET: InstantEFTs
        //public ActionResult Index()
        //{
        //    var instantEFTs = db.InstantEFTs.Include(i => i.CreatedBySystemUser).Include(i => i.Customer).Include(i => i.ModifiedBySystemUser).Include(i => i.Status);
        //    return View(instantEFTs.ToList());
        //}

        //// GET: InstantEFTs/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    InstantEFT instantEFT = db.InstantEFTs.Find(id);
        //    if (instantEFT == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(instantEFT);
        //}

        //// GET: InstantEFTs/Create
        //public ActionResult Create()
        //{
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber");
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
        //    ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
        //    return View();
        //}

        //// POST: InstantEFTs/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,CustomerId,Amount,StatusId,IsBatch,ReferenceNumber,ReferenceAlpha,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] InstantEFT instantEFT)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.InstantEFTs.Add(instantEFT);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instantEFT.CreatedBySystemUserId);
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", instantEFT.CustomerId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instantEFT.ModifiedBySystemUserId);
        //    ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", instantEFT.StatusId);
        //    return View(instantEFT);
        //}

        //// GET: InstantEFTs/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    InstantEFT instantEFT = db.InstantEFTs.Find(id);
        //    if (instantEFT == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instantEFT.CreatedBySystemUserId);
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", instantEFT.CustomerId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instantEFT.ModifiedBySystemUserId);
        //    ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", instantEFT.StatusId);
        //    return View(instantEFT);
        //}

        //// POST: InstantEFTs/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,CustomerId,Amount,StatusId,IsBatch,ReferenceNumber,ReferenceAlpha,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] InstantEFT instantEFT)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(instantEFT).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instantEFT.CreatedBySystemUserId);
        //    ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", instantEFT.CustomerId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instantEFT.ModifiedBySystemUserId);
        //    ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", instantEFT.StatusId);
        //    return View(instantEFT);
        //}

        //// GET: InstantEFTs/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    InstantEFT instantEFT = db.InstantEFTs.Find(id);
        //    if (instantEFT == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(instantEFT);
        //}

        //// POST: InstantEFTs/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    InstantEFT instantEFT = db.InstantEFTs.Find(id);
        //    db.InstantEFTs.Remove(instantEFT);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);

        //}

        #endregion
        //methods taken from Debit Order
        #region Single EFT Skeleton
        //[Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        //public ActionResult Index()
        //{
        //    using (var cxt = new eServicesDbContext())
        //    {
        //        //ViewBag.limit = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RecurringDebitOrdersStrikeDayEdit).Value;
        //        Initialise();
        //        BaseHelper _base = new BaseHelper();
        //        _base.Initialise(cxt);

        //        if (Customer == null)
        //            return RedirectToAction("Index", "Profile");

        //        var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
        //                    .Include(o => o.Account)
        //                    .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked && la.StatusId == 4)
        //                    .ToList();
        //        var currentInstantEFTs = cxt.InstantEFTs.Include(o => o.Customer).Include(o => o.Status).Where(o => o.CustomerId == Customer.Id).ToList();
        //        var bankAccounts = cxt.BankAccounts.Where(o => o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified" && o.CustomerId == Customer.Id);

        //        this.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", this.Customer.Id));

        //        InstantEFTViewModel ievm = new InstantEFTViewModel()
        //        {
        //            Customer = this.Customer
        //        };

        //        ViewBag.BankAccounts = new SelectList(bankAccounts.ToList(), "Id", "HiddenBankAccountNumber");
        //        ViewBag.CurrentAccounts = new SelectList(currentAccounts.ToList(), "Account.Id", "Account.Description");
        //        ViewBag.CurrentInstantEFTs = currentInstantEFTs;

        //        if (bankAccounts.Count() == 0 || currentAccounts.Count() == 0)
        //            ViewBag.DebitOrderWarning = "Please add bank accounts and link municipality accounts to your profile before creating debit orders.";

        //        return View(ievm);
        //    };
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        //public ActionResult Index(InstantEFTViewModel vm)
        //{
        //    using (var cxt = new eServicesDbContext())
        //    {
        //        try
        //        {
        //            Initialise();
        //            BaseHelper _base = new BaseHelper();
        //            _base.Initialise(cxt);

        //            // Setup supporting data.
        //            var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
        //            cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

        //            InstantEFTViewModel dovm = new InstantEFTViewModel()
        //            {
        //                Customer = cust,
        //                CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
        //                BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
        //            };

        //            // vm.DebitOrder.AccountId = 6;
        //            if (vm.InstantEFT.Id == 0)
        //            {
        //                // Add new debit order.
        //                if (vm.InstantEFT.IsRecurring == true)
        //                {
        //                    //check if recurring debit order is a duplicate
        //                    var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();

        //                    if (RecurringDuplicate != null)
        //                    {
        //                        RecurringDuplicate.IsDeleted = true;
        //                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Current Debit order successfully Update.";

        //                        cxt.Entry(RecurringDuplicate).State = EntityState.Modified;
        //                        cxt.SaveChanges();
        //                    }
        //                    else
        //                    {

        //                    }

        //                }
        //                else if (vm.DebitOrder.IsRecurring == false)
        //                {

        //                    //var dup = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id).FirstOrDefault();
        //                    //if (dup != null)
        //                    //{
        //                    //    dup.IsDeleted = true;

        //                    //    cxt.Entry(dup).State = EntityState.Modified;
        //                    //    cxt.SaveChanges();

        //                    //}
        //                    //else
        //                    //{

        //                    //}

        //                }
        //                vm.DebitOrder.CustomerId = cust.Id;
        //                vm.DebitOrder.IsBatch = false;
        //                vm.DebitOrder.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
        //                cxt.DebitOrders.Add(vm.DebitOrder);
        //                DebitOrderStatus DebitOrderStatus = new DebitOrderStatus();
        //                DebitOrderStatus.DebitOrderId = vm.DebitOrder.Id;
        //                DebitOrderStatus.StatusId = vm.DebitOrder.StatusId;
        //                DebitOrderStatus.BatchNumber = 0;//0 referes to all none batch payments
        //                DebitOrderStatus.BatchReference = "Individual Debit Order";
        //                DebitOrderStatus.IsActive = true;
        //                cxt.DebitOrderStatuses.Add(DebitOrderStatus);
        //                cxt.SaveChanges();
        //                ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order successfully created.";
        //                dovm.DebitOrder = cxt.DebitOrders.SingleOrDefault(o => o.Id == vm.DebitOrder.Id && o.IsActive && !o.IsDeleted);
        //            }
        //            else
        //            {
        //                // Update debit order.
        //                var order = cxt.DebitOrders.SingleOrDefault(o => o.Id == vm.DebitOrder.Id && o.IsActive && !o.IsDeleted);
        //                // var DebitOrderStatus = cxt.DebitOrderStatuses.Include(dso => dso.DebitOrder).Where(dso => dso.DebitOrderId == vm.DebitOrder.Id && dso.IsActive && !dso.IsDeleted).OrderByDescending(dso=>dso.Id).ToList().First();
        //                if (order != null && order.CustomerId != cust.Id)
        //                {
        //                    ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order already created by another user.";
        //                    return View(dovm);
        //                }

        //                if (ModelState.IsValid)
        //                {
        //                    //order.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
        //                    //order.BankAccountId = vm.DebitOrder.BankAccountId;
        //                    // order.AccountId = vm.DebitOrder.AccountId;
        //                    order.Amount = vm.DebitOrder.Amount;
        //                    //order.IsRecurring = vm.DebitOrder.IsRecurring;
        //                    order.StrikeDay = vm.DebitOrder.StrikeDay;
        //                    //order.StartDateTime = vm.DebitOrder.StartDateTime;
        //                    //order.StrikeDay = vm.DebitOrder.StrikeDay;
        //                    cxt.Entry(order).State = EntityState.Modified;


        //                    //// DebitOrderStatus.DebitOrderId = vm.DebitOrder.Id;
        //                    // DebitOrderStatus.StatusId = vm.DebitOrder.StatusId;
        //                    // cxt.Entry(DebitOrderStatus).State = EntityState.Modified;
        //                    cxt.SaveChanges();
        //                }

        //                ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Bank account successfully updated.";
        //                dovm.DebitOrder = new DebitOrder();
        //            }
        //            //var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
        //            //       .Include(o => o.Account)
        //            //       .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked)
        //            //       .ToList();
        //            ViewBag.BankAccounts = new SelectList(cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified").Include(o => o.BankAccountType).ToList<BankAccount>(), "Id", "HiddenBankAccountNumber");
        //            ViewBag.BankAccountTypes = new SelectList(cxt.BankAccountTypes.Where(o => o.IsActive && !o.IsDeleted).ToList(), "Id", "Name");
        //            ViewBag.CurrentAccounts = new SelectList(cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
        //                   .Include(o => o.Account).Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted && o.StatusId == 4).ToList(), "Account.Id", "Account.Description");
        //            ViewBag.limit = cxt.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RecurringDebitOrdersStrikeDayEdit).Value;

        //            return View(dovm);
        //        }
        //        catch (Exception x)
        //        {

        //            throw;
        //        }
        //    }
        //}

        //[EncryptedActionParameter]
        //[Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        //public ActionResult LoadOrder(int? Id)
        //{
        //    using (var cxt = new eServicesDbContext())
        //    {
        //        Initialise();
        //        BaseHelper _base = new BaseHelper();
        //        _base.Initialise(cxt);

        //        if (Id != null && Id > 0)
        //        {
        //            //DebitOrder order = cxt.DebitOrders.Include(o => o.BankAccount).Include(o => o.Account).FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
        //            DebitOrder order = cxt.DebitOrders.Include(o => o.BankAccount).Include(o => o.Account).FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
        //            //List<DebitOrderStatus> list = cxt.DebitOrderStatuses.Include(o => o.DebitOrder).Include(o => o.DebitOrder.BankAccount).Include(o => o.DebitOrder.BankAccount.Bank).Include(o => o.DebitOrder.Account).Include(o => o.Status).Where(o => o.DebitOrder.CustomerId == Id && o.IsActive && !o.IsDeleted && o.DebitOrder.IsBatch == true && o.DebitOrder.IsActive == true && !o.DebitOrder.IsDeleted).OrderByDescending(o => o.Id).ToList();

        //            var json = Json(new { data = order }, JsonRequestBehavior.AllowGet);
        //            json.MaxJsonLength = Int32.MaxValue;

        //            return json;
        //        }

        //        return null;
        //    }
        //}

        //[EncryptedActionParameter]
        //[Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        //public ActionResult LoadOrders(int? Id)
        //{
        //    using (var cxt = new eServicesDbContext())
        //    {
        //        Initialise();
        //        BaseHelper _base = new BaseHelper();
        //        _base.Initialise(cxt);

        //        if (Id != null && Id > 0)
        //        {
        //            List<DebitOrder> list = cxt.DebitOrders.Include(o => o.BankAccount).Include(o => o.BankAccount.Bank).Include(o => o.Account).Include(o => o.Status).Where(o => o.CustomerId == Id && o.IsBatch == false && o.IsActive && !o.IsDeleted).OrderByDescending(o => o.Id).ToList();
        //            // List<DebitOrderStatus> list = cxt.DebitOrderStatuses.Include(o => o.DebitOrder).Include(o => o.DebitOrder.BankAccount).Include(o => o.DebitOrder.BankAccount.Bank).Include(o => o.DebitOrder.Account).Include(o => o.Status).Where(o => o.DebitOrder.CustomerId == Id && o.IsActive && !o.IsDeleted && o.DebitOrder.IsBatch == false && o.DebitOrder.IsActive == true && !o.DebitOrder.IsDeleted).OrderByDescending(o => o.Id).ToList();

        //            foreach (var order in list)
        //            {
        //                order.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", order.Id));
        //            }

        //            var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
        //            json.MaxJsonLength = Int32.MaxValue;

        //            return json;
        //        }

        //        return null;
        //    }
        //}

        //[EncryptedActionParameter]
        //[Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        //public ActionResult DeleteOrder(int? Id)
        //{
        //    using (var cxt = new eServicesDbContext())
        //    {
        //        Initialise();
        //        BaseHelper _base = new BaseHelper();
        //        _base.Initialise(cxt);

        //        if (Id != null && Id > 0)
        //        {
        //            var order = cxt.DebitOrders.SingleOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);
        //            try
        //            {
        //                var orderStats = cxt.DebitOrderStatuses.Include(dso => dso.DebitOrder).Where(dso => dso.DebitOrder.Id == Id && dso.IsActive && !dso.IsDeleted).OrderByDescending(dso => dso.Id).ToList().First();
        //                // var DebitOrderStatus = cxt.DebitOrderStatuses.Include(dso => dso.DebitOrder).Where(dso => dso.DebitOrderId == vm.DebitOrder.Id && dso.IsActive && !dso.IsDeleted).OrderByDescending(dso=>dso.Id).ToList().First();

        //                if (order != null)
        //                {
        //                    order.IsDeleted = true;
        //                    order.IsActive = false;
        //                    cxt.Entry(order).State = EntityState.Modified;

        //                    orderStats.IsDeleted = true;
        //                    orderStats.IsActive = false;
        //                    cxt.Entry(orderStats).State = EntityState.Modified;
        //                    cxt.SaveChanges();
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                var mess = e;
        //            }
        //            var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id && o.IsActive && !o.IsDeleted);

        //            DebitOrderViewModel dovm = new DebitOrderViewModel()
        //            {
        //                Customer = cust,
        //                CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
        //                BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
        //            };

        //            return RedirectToAction("Index", "DebitOrder");
        //        }

        //        return null;
        //    }
        //}
#endregion

        /*__________________________________________Batch Debit Order Payments___________________________________________________________________________*/
        //Bulk Batch Payment
        // GET: DebitOrder
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult IndexBatchEFTPayment()
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
                var currentInstantEFTs = cxt.InstantEFTs.Include(o => o.Customer).Include(o => o.Status).Where(o => o.CustomerId == Customer.Id).ToList();
                var bankAccounts = cxt.BankAccounts.Where(o => o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified" && o.CustomerId == Customer.Id);

                this.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", this.Customer.Id));

                InstantEFTViewModel dovm = new InstantEFTViewModel()
                {
                    Customer = this.Customer
                };

                ViewBag.BankAccounts = new SelectList(bankAccounts.ToList(), "Id", "HiddenBankAccountNumber");
                ViewBag.CurrentAccounts = new SelectList(currentAccounts.ToList(), "Account.Id", "Account.Description");
                ViewBag.CurrentInstantEFTs = currentInstantEFTs;

                if (bankAccounts.Count() == 0 || currentAccounts.Count() == 0)
                    ViewBag.InstantEFTWarning = "Please link municipal accounts to your profile before creating Bulk Instant EFT.";

                return View(dovm);
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult IndexBatchEFTPayment(InstantEFTViewModel vm)
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

                    InstantEFTViewModel dovm = new InstantEFTViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                    // vm.DebitOrder.AccountId = 6;
                    if (vm.InstantEFT.Id == 0)
                    {
                        //// Add new debit order.
                        //if (vm.InstantEFT.IsRecurring == true)
                        //{
                        //    //check if recurring debit order is a duplicate
                        //    var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();

                        //    if (RecurringDuplicate != null)
                        //    {
                        //        RecurringDuplicate.IsDeleted = true;

                        //        cxt.Entry(RecurringDuplicate).State = EntityState.Modified;
                        //        cxt.SaveChanges();
                        //    }
                        //    else
                        //    {

                        //    }

                        //}
                        //else if (vm.DebitOrder.IsRecurring == false)
                        //{

                        //    var dup = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id).FirstOrDefault();
                        //    if (dup != null)
                        //    {
                        //        dup.IsDeleted = true;

                        //        cxt.Entry(dup).State = EntityState.Modified;
                        //        cxt.SaveChanges();

                        //    }
                        //    else
                        //    {

                        //    }

                        //}
                        vm.InstantEFT.CustomerId = cust.Id;
                        vm.InstantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess).Id;
                        cxt.InstantEFTs.Add(vm.InstantEFT);
                        cxt.SaveChanges();
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Instant Eft successfully created.";
                        dovm.InstantEFT = cxt.InstantEFTs.SingleOrDefault(o => o.Id == vm.InstantEFT.Id && o.IsActive && !o.IsDeleted);
                    }
                    else
                    {
                        // Update debit order.
                        var order = cxt.InstantEFTs.SingleOrDefault(o => o.Id == vm.InstantEFT.Id && o.IsActive && !o.IsDeleted);
                        if (order != null && order.CustomerId != cust.Id)
                        {
                            ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Instant Eft already created by another user.";
                            return View(dovm);
                        }

                        if (ModelState.IsValid)
                        {
                            order.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                            order.Amount = vm.InstantEFT.Amount;
                            cxt.Entry(order).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }

                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Instant EFT successfully updated.";
                        dovm.InstantEFT = new InstantEFT();
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
                    Status StatusK = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess);
                    List<InstantEFT> list = cxt.InstantEFTs.Include(o => o.Status).Where(o => o.CustomerId == Id && o.IsActive && !o.IsDeleted && o.IsActive == true && !o.IsDeleted && o.StatusId == StatusK.Id).OrderByDescending(o => o.Id).ToList();
                    //&& o.StatusId== StatusK.Id
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

                    try
                    {

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
        #region Profile EFT Details
        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult EFTDetails(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                if (Id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                InstantEFT instantEFT = cxt.InstantEFTs.Find(Id);
                if (instantEFT == null)
                {
                    return HttpNotFound();
                }
                //ViewBag.EFTDetSum = Id;
                return View(instantEFT);
            }
        }
        //[EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadEFTDetails(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {


                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);
                
                //Id = 3;
                if (Id != null && Id > 0)
                {
                    List<InstantEFTTransaction> list = cxt.InstantEFTTransactions.Include(o => o.InstantEFT).Include(o => o.Account).Include(o => o.InstantEFT.Status).Where(o => o.InstantEFTId == Id && o.IsActive && !o.IsDeleted).OrderByDescending(o => o.Id).ToList();

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
        #endregion

        //[EncryptedActionParameter]
        [Authorize]
        [HttpPost]
        public JsonResult BulkPayment(string[] ControllerDebitAmount, int CusId, string[] MAccounts, string bat, string refer,string eftTotal)
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

                        foreach (var MAcc in MAccounts)
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
                        //string batchRef = bat.Substring(1, (bat.Length - 1));
                        string batchRef = bat;
                        //if (list.Count > 1)
                        //{
                        //    batchRef = batchRef + "1";
                        //}
                        //else
                        //{
                        //    batchRef = batchRef + "0";
                        //}
                            Int64 batchNum = Convert.ToInt64(batchRef);
                        Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                        string init = initials.Replace(cust.FullName, "$1");
                        string CusInitials = init + cust.LastName;
                        int bulkId = 0;

                        //create Single Bulk reference Record
                        InstantEFT BulkInstantEFT = new InstantEFT();
                        string arym = eftTotal;
                        arym = arym.Replace('.', ',');
                        decimal amt = decimal.Parse(arym);
                        BulkInstantEFT.Amount = amt;
                        //BatDebitOrder.StrikeDay = Convert.ToInt32(dStrike);
                        BulkInstantEFT.CustomerId = cust.Id;
                        BulkInstantEFT.IsActive = true;
                        //check if more then one account
                        if (list.Count > 1)
                        {
                            BulkInstantEFT.IsBatch = true;
                            BulkInstantEFT.ReferenceAlpha = CusInitials + "-" + refer;
                        }
                        else
                        {
                            BulkInstantEFT.IsBatch = false;
                            BulkInstantEFT.ReferenceAlpha = CusInitials + "- Single EFT";
                        }

                        BulkInstantEFT.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                        BulkInstantEFT.ReferenceNumber = batchNum;

                        context.InstantEFTs.Add(BulkInstantEFT);
                        
                        context.SaveChanges();
                        bulkId = BulkInstantEFT.Id;
                        foreach (var bdo in list)
                        {
                            //create individual Line Items for Bulk Payment
                            InstantEFTTransaction BulkInstantEFTTransaction = new InstantEFTTransaction();
                            //BatDebitOrder.BankAccountId = BId;
                            BulkInstantEFTTransaction.InstantEFTId = bulkId;
                            BulkInstantEFTTransaction.AccountId = bdo.AccountId;
                            BulkInstantEFTTransaction.IsActive = true;
                            string arymT = ControllerDebitAmount[i];
                            arymT = arymT.Replace('.', ',');
                            decimal amtT = decimal.Parse(arymT);
                            BulkInstantEFTTransaction.Amount = amtT;
                            context.InstantEFTTransactions.Add(BulkInstantEFTTransaction);
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
        //Generate Daily Sequenece for EFT
        public ActionResult BatchDailySequence()
        {
            using (var context = new eServicesDbContext())
            {
                int limiter = 0;
                try
                {
                    //var CurrUser = User.Identity.GetUserName();
                    //var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                    //var customer = context.Customers.Include(c => c.CustomerType).SingleOrDefault(c => c.SystemUserId == customerSys.Id);
                    AppSetting query = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.EFTBatchSequence);
                    //var query = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.DebitOrderBatchSequence);
                    var SeqLimit = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.EFTBatchSequenceLimit);
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
                    Session["EFTBatchCount"] = BatchCounter;
                    ViewBag.BatSeq = BatchCounter;
                }
                catch (Exception e)
                {
                    var mess = e;
                    return null;
                }

                return null;
            }
        }
        
        //Generate Daily Sequenece for EFT
        //public ActionResult BatchDailySequence()
        //{
        //    using (var context = new eServicesDbContext())
        //    {
        //        try
        //        {
        //            var CurrUser = User.Identity.GetUserName();
        //            var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
        //            var customer = context.Customers.Include(c => c.CustomerType)
        //                     .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
        //            var query = context.InstantEFTs.Include(x => x.Customer)
        //        .Include(x => x.Status)
        //        .Where(x =>x.IsActive && x.IsDeleted == false).OrderByDescending(x => x.Id).ToList().First();
        //            var BatchCounter = "0001";
        //            if (query.CreatedDateTime.Value.Date == DateTime.Now.Date)
        //            {
        //                var lastRef = query.ReferenceNumber.ToString();
        //                string lastSeq = lastRef.Substring(8, 4);
        //                int SeqNo = Convert.ToInt32(lastSeq) + 1;
        //                if (SeqNo.ToString().Length == 1)
        //                {
        //                    BatchCounter = "000" + SeqNo;
        //                }
        //                else if (SeqNo.ToString().Length == 2)
        //                {
        //                    BatchCounter = "00" + SeqNo;
        //                }
        //                else if (SeqNo.ToString().Length == 3)
        //                {
        //                    BatchCounter = "0" + SeqNo;
        //                }
        //                else if (SeqNo.ToString().Length == 4)
        //                {
        //                    BatchCounter = SeqNo.ToString();
        //                }

        //            }
        //            else
        //            {
        //                BatchCounter = "0001";
        //            }
        //            Session["EFTBatchCount"] = BatchCounter;
        //            ViewBag.BatSeq = BatchCounter;
        //        }
        //        catch (Exception e)
        //        {
        //            Session["EFTBatchCount"] = "1";
        //            return null;
        //        }

        //        return null;
        //    }
        //}

        [HttpGet]
        public ActionResult ReturnLink(string q)
        {
            using (var cxt = new eServicesDbContext())
            {
                AesCrypto aes = new AesCrypto();
                var decrypted = aes.Decrypt(q);
                var values = decrypted.Split('|');
                var reference1 = values[0];
                var status = values[1];
                var message = values[2];

                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                Int64 refNo = Convert.ToInt64(reference1);
                InstantEFT instantEFT = cxt.InstantEFTs.Include(o => o.Status).SingleOrDefault( o=>o.ReferenceNumber==refNo);
                if (instantEFT == null)
                {
                    return HttpNotFound();
                }

                if (status=="SUCCESS")
                {
                    instantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess).Id;
                }
                else if(status == "FAILED")
                {
                    instantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderFailed).Id;
                }
                else
                {
                    instantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderFailed).Id;
                }
                instantEFT.Message = message;

                cxt.Entry(instantEFT).State = EntityState.Modified;

                cxt.SaveChanges();

                return View(instantEFT);
            }
        }
     
       
        public ActionResult TestReturn()
        {
            return View();
        }

        //__________________________________________SMART BILL EFT_______________________________________________//
        [EncryptedActionParameter]
        public ActionResult SmartBillEFTPayment(string municipalAccount, string amount, string systemUId, string customerId)
        {
            using (var cxt = new eServicesDbContext())
            {
                //Initialise();
                //BaseHelper _base = new BaseHelper();
                //_base.Initialise(cxt);
                municipalAccount = municipalAccount.Replace("z", string.Empty);
                amount = amount.Replace("z", string.Empty);
                systemUId = systemUId.Replace("z", string.Empty);
                customerId= customerId.Replace("z", string.Empty);
                var custId = Convert.ToInt32(customerId);
               string sbAmountt = amount.Replace("z", string.Empty);
                if (!string.IsNullOrEmpty(municipalAccount))
                ViewBag.SBMunAccount =municipalAccount;
                else
                    ViewBag.SBMunAccount = "";

                if (!string.IsNullOrEmpty(sbAmountt))
                    ViewBag.SBAmt = Convert.ToDecimal(sbAmountt);
                else
                    ViewBag.SBAmt = "";
                if (!string.IsNullOrEmpty(customerId))
                    ViewBag.SBSysId = Convert.ToInt32(systemUId);
                else
                    ViewBag.SBSysId = "";

                if (!string.IsNullOrEmpty(customerId))
                    ViewBag.SBCustId = Convert.ToInt32(customerId);
                else
                    ViewBag.SBCustId = "";


                if (!string.IsNullOrEmpty(customerId))
                    Customer = cxt.Customers.FirstOrDefault(x=>x.Id== custId);
          
                if (Customer == null)
                    return RedirectToAction("Index", "Profile");

                var currentAccounts = cxt.LinkedAccounts.Include(o => o.Customer).Include(o => o.Status)
                            .Include(o => o.Account)
                            .Where(la => la.CustomerId == Customer.Id && la.Status.Key != StatusKeys.LinkedAccountUnlinked && la.StatusId == 4)
                            .ToList();
                var currentInstantEFTs = cxt.InstantEFTs.Include(o => o.Customer).Include(o => o.Status).Where(o => o.CustomerId == Customer.Id).ToList();
                var bankAccounts = cxt.BankAccounts.Where(o => o.IsActive && !o.IsDeleted && o.IsAccountHolder == true && o.AccountStatus == "Verified" && o.CustomerId == Customer.Id);

                this.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", this.Customer.Id));

                InstantEFTViewModel dovm = new InstantEFTViewModel()
                {
                    Customer = this.Customer
                };

                ViewBag.BankAccounts = new SelectList(bankAccounts.ToList(), "Id", "HiddenBankAccountNumber");
                ViewBag.CurrentAccounts = new SelectList(currentAccounts.ToList(), "Account.Id", "Account.Description");
                ViewBag.CurrentInstantEFTs = currentInstantEFTs;

                if (bankAccounts.Count() == 0 || currentAccounts.Count() == 0)
                    ViewBag.InstantEFTWarning = "Please link municipal accounts to your profile before creating Bulk Instant EFT.";

                return View(dovm);
            };
        }
        [HttpPost]
        public JsonResult SBBulkPayment(string[] ControllerDebitAmount, int CusId, string[] MAccounts, string bat, string refer, string eftTotal, int sysId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.Id == CusId);
                    if (customer == null) throw new Exception("Invalid Customer");
                //  Initialise();
                //BaseHelper _base = new BaseHelper();
                   // _base.Initialise(context);

                    // Setup supporting data.
                    var cust = context.Customers.FirstOrDefault(o => o.SystemUserId == sysId);
                    //cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));
                    BillingDbContext EkuContext = new BillingDbContext();
                    List<BatchAccount> list = new List<BatchAccount>();
                    if (CusId > 0)
                    {
                        var clientIdParameter = new SqlParameter("@CustId", CusId);
                        var result = EkuContext.Database
                            .SqlQuery<BatchAccount>("getBatchAccounts @CustId", clientIdParameter)
                            .ToList();

                        foreach (var MAcc in MAccounts)
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
                        //string batchRef = bat.Substring(1, (bat.Length - 1));
                        string batchRef = bat;
                        //if (list.Count > 1)
                        //{
                        //    batchRef = batchRef + "1";
                        //}
                        //else
                        //{
                        //    batchRef = batchRef + "0";
                        //}
                        Int64 batchNum = Convert.ToInt64(batchRef);
                        Regex initials = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
                        string init = initials.Replace(cust.FullName, "$1");
                        string CusInitials = init + cust.LastName;
                        int bulkId = 0;

                        //create Single Bulk reference Record
                        InstantEFT BulkInstantEFT = new InstantEFT();
                        string arym = eftTotal;
                        arym = arym.Replace('.', ',');
                        decimal amt = decimal.Parse(arym);
                        BulkInstantEFT.Amount = amt;
                        //BatDebitOrder.StrikeDay = Convert.ToInt32(dStrike);
                        BulkInstantEFT.CustomerId = cust.Id;
                        BulkInstantEFT.IsActive = true;
                        //check if more then one account
                        if (list.Count > 1)
                        {
                            BulkInstantEFT.IsBatch = true;
                            BulkInstantEFT.ReferenceAlpha = CusInitials + "-" + refer;
                        }
                        else
                        {
                            BulkInstantEFT.IsBatch = false;
                            BulkInstantEFT.ReferenceAlpha = CusInitials + "- Smart Bill EFT";
                        }

                        BulkInstantEFT.StatusId = context.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                        BulkInstantEFT.ReferenceNumber = batchNum;
                       // BulkInstantEFT.CreatedBySystemUserId = sysId;
                        //BulkInstantEFT.ModifiedBySystemUserId = sysId;
                        context.InstantEFTs.Add(BulkInstantEFT);

                        context.SaveChanges();
                        bulkId = BulkInstantEFT.Id;
                        foreach (var bdo in list)
                        {
                            //create individual Line Items for Bulk Payment
                            InstantEFTTransaction BulkInstantEFTTransaction = new InstantEFTTransaction();
                            //BatDebitOrder.BankAccountId = BId;
                            BulkInstantEFTTransaction.InstantEFTId = bulkId;
                            BulkInstantEFTTransaction.AccountId = bdo.AccountId;
                            BulkInstantEFTTransaction.IsActive = true;
                            string arymT = ControllerDebitAmount[i];
                            arymT = arymT.Replace('.', ',');
                            decimal amtT = decimal.Parse(arymT);
                            BulkInstantEFTTransaction.Amount = amtT;
                            //BulkInstantEFTTransaction.CreatedBySystemUserId = sysId;
                           // BulkInstantEFTTransaction.ModifiedBySystemUserId = sysId;
                            context.InstantEFTTransactions.Add(BulkInstantEFTTransaction);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SmartBillEFTPayment(InstantEFTViewModel vm)
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

                    InstantEFTViewModel dovm = new InstantEFTViewModel()
                    {
                        Customer = cust,
                        CurrentBankAccounts = cxt.BankAccounts.Where(o => o.CustomerId == cust.Id && o.IsActive && !o.IsDeleted).ToList<BankAccount>(),
                        BankAccountTypes = cxt.BankAccountTypes.Where(o => o.IsActive == true && o.IsDeleted == false).ToList<BankAccountType>()
                    };

                    // vm.DebitOrder.AccountId = 6;
                    if (vm.InstantEFT.Id == 0)
                    {
                        //// Add new debit order.
                        //if (vm.InstantEFT.IsRecurring == true)
                        //{
                        //    //check if recurring debit order is a duplicate
                        //    var RecurringDuplicate = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == true && x.IsActive && !x.IsDeleted).FirstOrDefault();

                        //    if (RecurringDuplicate != null)
                        //    {
                        //        RecurringDuplicate.IsDeleted = true;

                        //        cxt.Entry(RecurringDuplicate).State = EntityState.Modified;
                        //        cxt.SaveChanges();
                        //    }
                        //    else
                        //    {

                        //    }

                        //}
                        //else if (vm.DebitOrder.IsRecurring == false)
                        //{

                        //    var dup = cxt.DebitOrders.Where(x => x.AccountId == vm.DebitOrder.AccountId && x.IsRecurring == false && x.IsActive && !x.IsDeleted && x.StatusId != cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSent).Id).FirstOrDefault();
                        //    if (dup != null)
                        //    {
                        //        dup.IsDeleted = true;

                        //        cxt.Entry(dup).State = EntityState.Modified;
                        //        cxt.SaveChanges();

                        //    }
                        //    else
                        //    {

                        //    }

                        //}
                        vm.InstantEFT.CustomerId = cust.Id;
                        vm.InstantEFT.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess).Id;
                        cxt.InstantEFTs.Add(vm.InstantEFT);
                        cxt.SaveChanges();
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Instant Eft successfully created.";
                        dovm.InstantEFT = cxt.InstantEFTs.SingleOrDefault(o => o.Id == vm.InstantEFT.Id && o.IsActive && !o.IsDeleted);
                    }
                    else
                    {
                        // Update debit order.
                        var order = cxt.InstantEFTs.SingleOrDefault(o => o.Id == vm.InstantEFT.Id && o.IsActive && !o.IsDeleted);
                        if (order != null && order.CustomerId != cust.Id)
                        {
                            ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Instant Eft already created by another user.";
                            return View(dovm);
                        }

                        if (ModelState.IsValid)
                        {
                            order.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderPending).Id;
                            order.Amount = vm.InstantEFT.Amount;
                            cxt.Entry(order).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }

                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Instant EFT successfully updated.";
                        dovm.InstantEFT = new InstantEFT();
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



        #region All Instant EFT PAyments GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult ViewIEFTPayments()
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

        #region Load All Instant EFT Payments GET
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public JsonResult LoadIEFTPayments(int pageSize, string startDate, string endDate, int pageNumber, string searchText)
        {
            var total = 0;
            var instantEfts = new List<InstantEFTTransaction>();
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
                    var ieftModel = context.InstantEFTTransactions.Include(o => o.Account).Include(o => o.InstantEFT)
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
                        if (searchText.ToUpper() == "BULK" || searchText.ToUpper() == "BULK PAYMENT")
                        {
                            searchText = "true";
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.IsBatch.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "SINGLE" || searchText.ToUpper() == "SINGLE PAYMENT")
                        {
                            searchText = "false";
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.IsBatch.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        //else if (searchText.ToUpper() == "RECURRING" || searchText.ToUpper() == "RECURRING PAYMENT" || searchText.ToUpper() == "RECURRING DEBIT ORDER")
                        //{
                        //    searchText = "true";
                        //    instantEfts = ieftModel.Where(s => (s.IsRecurring.ToString() ?? "").Contains(searchText)).ToList();
                        //}
                        //else if (searchText.ToUpper() == "ONCE-OFF" || searchText.ToUpper() == "ONCE-OFF PAYMENT" || searchText.ToUpper() == "ONCE-OFF DEBIT ORDER" || searchText.ToUpper() == "ONCEOFF" || searchText.ToUpper() == "ONCE OFF")
                        //{
                        //    searchText = "false";
                        //    instantEfts = ieftModel.Where(s => (s.IsRecurring.ToString() ?? "").Contains(searchText)).ToList();
                        //}
                        else if (searchText.ToUpper() == "ACTIVE" || searchText.ToUpper() == "ACTIVE EFT" || searchText.ToUpper() == "ACTIVE PAYMENT")
                        {
                            searchText = "true";
                            instantEfts = ieftModel.Where(s => (s.IsActive.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "CANCELLED" || searchText.ToUpper() == "CANCELLED EFT" || searchText.ToUpper() == "CANCELLED PAYMENT")
                        {
                            searchText = "false";
                            instantEfts = ieftModel.Where(s => (s.IsActive.ToString() ?? "").Contains(searchText)).ToList();
                        }
                        else if (searchText.ToUpper() == "PENDING")
                        {
                            int searchId = queryPending.Id;
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.Status.Id == searchId)).ToList();
                        }
                        else if (searchText.ToUpper() == "SENT" || searchText.ToUpper() == "PROCESSING")
                        {
                            int searchId = querySent.Id;
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.Status.Id == searchId)).ToList();
                        }
                        else if (searchText.ToUpper() == "SUCCESS")
                        {
                            int searchId = querySuccess.Id;
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.Status.Id == searchId)).ToList();
                        }
                        else if (searchText.ToUpper() == "FAILED")
                        {
                            int searchId = queryFailed.Id;
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.Status.Id == searchId)).ToList();
                        }
                        else if (isNumericAccount)
                        {
                            instantEfts = ieftModel.Where(s => (s.Account.AccountNumber ?? "").Contains(searchText) ||
                                                                (s.InstantEFT.Customer.IdentificationNumber ?? "").Contains(searchText) ||
                                                                (s.Amount.ToString() ?? "").Contains(searchText) ||
                                                                (s.InstantEFT.ReferenceNumber.ToString() ?? "").Contains(searchText) ||
                                                                (s.InstantEFT.Customer.CellPhoneNumber ?? "").Contains(searchText) ||
                                                                (s.InstantEFT.Customer.SystemUser.MobileNumber ?? "").Contains(searchText)).ToList();
                        }
                        else if (!isNumericAccount)
                        {
                            instantEfts = ieftModel.Where(s => (s.InstantEFT.Customer.FirstName ?? "").Contains(searchText) ||
                                       (s.InstantEFT.Customer.LastName ?? "").Contains(searchText) ||
                                       (s.InstantEFT.Status.Name ?? "").Contains(searchText) ||
                                       (s.InstantEFT.ReferenceAlpha ?? "").Contains(searchText) ||
                                       (s.InstantEFT.ReferenceNumber.ToString() ?? "").Contains(searchText) ||
                                       (s.CreatedDateTime.ToString() ?? "").Contains(searchText) ||
                                       (s.InstantEFT.Customer.EmailAddress ?? "").Contains(searchText) ||
                                       (s.InstantEFT.Customer.SystemUser.EmailAddress ?? "").Contains(searchText)).ToList();
                        }
                        else
                        {
                            instantEfts = ieftModel.Where(s => (s.Account.AccountNumber ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.FirstName ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.LastName ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.IdentificationNumber ?? "").Contains(searchText) ||
                                                                 (s.Amount.ToString() ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.ReferenceAlpha ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.ReferenceNumber.ToString() ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.IsBatch.ToString() ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Status.Name ?? "").Contains(searchText) ||
                                                                 (s.CreatedDateTime.ToString() ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.SystemUser.EmailAddress ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.SystemUser.MobileNumber ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.EmailAddress ?? "").Contains(searchText) ||
                                                                 (s.InstantEFT.Customer.CellPhoneNumber ?? "").Contains(searchText)).ToList();
                        }

                        instantEfts = ieftModel.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                        total = ieftModel.Count();
                    }
                    else
                    {
                        instantEfts = ieftModel.OrderBy(l => l.Id).Skip(skip).Take(pageSize).ToList();
                        total = ieftModel.Count();
                    }

                    foreach (var instantEFT in instantEfts)
                    {
                        //var doPay = UserManager.FindByName(debitOrder.Customer.SystemUser.UserName);//Id = debitOrder.Id,

                        //if (instantEFT.) { recurringText = "Recurring"; }
                        //else { recurringText = "Once-Off"; }
                        if (instantEFT.InstantEFT.IsBatch == true)
                        { batchText = "Bulk"; }
                        else { batchText = "Single"; }
                        if (instantEFT.IsActive)
                        { paymentText = "Active"; }
                        else { paymentText = "Cancelled"; }

                        var custSystem = context.SystemUsers.FirstOrDefault(s => s.Id == instantEFT.InstantEFT.Customer.SystemUserId);
                        if (!string.IsNullOrEmpty(custSystem.EmailAddress))
                        {
                            emailText = custSystem.EmailAddress;
                        }
                        if (!string.IsNullOrEmpty(instantEFT.InstantEFT.Customer.EmailAddress))
                        {
                            emailText = instantEFT.InstantEFT.Customer.EmailAddress;
                        }
                        else
                        {
                            emailText = "N/A";
                        }

                        if (!string.IsNullOrEmpty(custSystem.MobileNumber))
                        {
                            mobileNoText = custSystem.MobileNumber;
                        }
                        if (!string.IsNullOrEmpty(instantEFT.InstantEFT.Customer.CellPhoneNumber))
                        {
                            mobileNoText = instantEFT.InstantEFT.Customer.CellPhoneNumber;
                        }
                        else
                        {
                            mobileNoText = "N/A";
                        }



                        rows.Add(new
                        {

                            municipalAccount = instantEFT.Account.AccountNumber,
                            customerName = instantEFT.InstantEFT.Customer.FullName,
                            customerId = instantEFT.InstantEFT.Customer.IdentificationNumber,
                            amount = instantEFT.Amount.ToString("0.00"),
                            reference = instantEFT.InstantEFT.ReferenceAlpha,
                            referenceNum = instantEFT.InstantEFT.ReferenceNumber,
                            type = batchText,
                            payment = paymentText,
                            status = instantEFT.InstantEFT.Status.Name,
                            createdDate = String.Format("{0:yyyy/MM/dd}", instantEFT.CreatedDateTime),
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
    }
}
