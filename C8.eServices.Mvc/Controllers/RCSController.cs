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
    public class RCSController : Controller
    {
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        public RCSController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public RCSController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public RCSController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }
        private eServicesDbContext db = new eServicesDbContext();
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

        #region Create
        public ActionResult Create()
        {
            ViewBag.CountryOfIssueTypeId = new SelectList(db.Countries, "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.CustomerTypeId = new SelectList(db.CustomerTypes, "Id", "Name");
            ViewBag.IdentificationTypeId = new SelectList(db.IdentificationTypes, "Id", "Name");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            ViewBag.SystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name");
            return View();
        }

        // POST: RCS/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerTypeId,IdentificationTypeId,CountryOfIssueTypeId,IdentificationNumber,TitleTypeId,FirstName,LastName,Gender,IsDeceased,EmailAddress,WorkPhoneNumber,HomePhoneNumber,CellPhoneNumber,PhysicalAddress1,PhysicalAddress2,PhysicalAddress3,PhysicalAddress4,PhysicalAddress5,PhysicalAddressCode,PostalAddress1,PostalAddress2,PostalAddress3,PostalAddress4,PostalAddress5,PostalAddressCode,SystemUserId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Customer customer)
        {

            if (ModelState.IsValid)
            {
                using (var cxt = new eServicesDbContext())
                {
                    try
                    {
                        Initialise();
                        BaseHelper _base = new BaseHelper();
                        _base.Initialise(cxt);

                        var CodeForRCSSave = "";
                        if (CodeForRCSSave == "Ready")
                        {

                            TransferInformation ti = new TransferInformation();

                            cxt.TransferInformations.Add(ti);
                            cxt.SaveChanges();

                            var TransferInfoID = ti.Id;
                            RCSApplicationStatus appli = new RCSApplicationStatus();
                            appli.Customer = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id); ;
                            appli.StatusId = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.DebitOrderSuccess).Id;
                            appli.TransferInformationId = TransferInfoID;
                            appli.TransferInformation = ti;
                            cxt.RCSApplicationStatus.Add(appli);
                            cxt.SaveChanges();

                            PurchaserInformation purchInfo = new PurchaserInformation();
                            purchInfo.RCSApplicationStatusId = appli.Id;
                            purchInfo.RCSApplicationStatus = appli;

                            cxt.PurchaserInformations.Add(purchInfo);
                            cxt.SaveChanges();

                            //context.SaveChanges();
                            //bulkId = BulkInstantEFT.Id;
                            //foreach (var bdo in list)
                            //{
                            //    //create individual Line Items for Bulk Payment
                            //    InstantEFTTransaction BulkInstantEFTTransaction = new InstantEFTTransaction();
                            //    //BatDebitOrder.BankAccountId = BId;
                            //    BulkInstantEFTTransaction.InstantEFTId = bulkId;
                            //    BulkInstantEFTTransaction.AccountId = bdo.AccountId;
                            //    BulkInstantEFTTransaction.IsActive = true;
                            //    string arymT = ControllerDebitAmount[i];
                            //    arymT = arymT.Replace('.', ',');
                            //    decimal amtT = decimal.Parse(arymT);
                            //    BulkInstantEFTTransaction.Amount = amtT;
                            //    context.InstantEFTTransactions.Add(BulkInstantEFTTransaction);
                            //    context.SaveChanges();
                            //    i++;
                            //}

                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }

                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CountryOfIssueTypeId = new SelectList(db.Countries, "Id", "Name", customer.CountryOfIssueTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.CreatedBySystemUserId);
            ViewBag.CustomerTypeId = new SelectList(db.CustomerTypes, "Id", "Name", customer.CustomerTypeId);
            ViewBag.IdentificationTypeId = new SelectList(db.IdentificationTypes, "Id", "Name", customer.IdentificationTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.ModifiedBySystemUserId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", customer.StatusId);
            ViewBag.SystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", customer.SystemUserId);
            ViewBag.TitleTypeId = new SelectList(db.TitleTypes, "Id", "Name", customer.TitleTypeId);
            return View(customer);
        }


        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
#endregion