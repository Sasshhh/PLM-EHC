using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;

namespace C8.eServices.Mvc.Controllers
{
    public class HoDsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }
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
                    Customer =
                        db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                            .Include(o => o.CustomerType)
                            .Include(o => o.Country)
                            .Include(o => o.IdentificationType)
                            .Include(o => o.TitleType)
                              .Include(o => o.Status)
                            .FirstOrDefault();

                    if (Customer != null)
                    {
                        Entity =
                            db.Entities.Where(o => o.CustomerId == Customer.Id)
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
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }

        }
        public bool ActivityTrackerAudit(int? PropertyId, string ActivityTrackerMessage, int CustomerID)
        {
            try
            {
                PLMApplicationHistortyLog newlog = new PLMApplicationHistortyLog();

                newlog.PropertyLeaseApplicationId = PropertyId;
                newlog.AuditAction = ActivityTrackerMessage;
                newlog.UserId = CustomerID;
                newlog.CreatedDateTime = DateTime.Now;
                newlog.IsActive = true;
                newlog.IsDeleted = false;
                newlog.IsLocked = false;

                db.PLMApplicationHistortyLogs.Add(newlog);
                db.SaveChanges();
                return true;
            }
            catch (Exception IO)
            {
                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return false;
            }

            //return "test";
        }
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).Where(x => x.StatusId== db.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingHoDResponse).Id).ToList();
                    var LeaseApplicatioons = db.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).Where(x => x.StatusId == db.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingHoDResponse).Id).ToList();

                    if (Session["Display"] != null)
                    {

                        if (Session["ApplicationRefNo"] != null)
                        {
                            Session["Display"] = "True";
                            var message = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.PLMSuccessMessage).FirstOrDefault();

                            if (Session["Display"].ToString() == "True")
                            {
                                ViewBag.Display = "True";
                                ViewBag.MessageTitle3 = message.Title + Session["ApplicationRefNo"];

                                ViewBag.MessageBody3 = Session["MessageBody"].ToString();
                                Session["MessageBody"] = null;
                            }
                            Session["Display"] = null;
                            Session["ApplicationRefNo"] = null;
                        }
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    foreach (var item in LeaseApplicatioons)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    var vm = new DepartmentsApprovalViewModel();
                    vm.LeaseDetailsList = LeaseApplicatioons;
                    vm.PropertyLeaseApplicationList = rCSApplicationStatus;

                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoD hoD = db.HoDs.Find(id);
            if (hoD == null)
            {
                return HttpNotFound();
            }
            return View(hoD);
        }




        #region PLM Application

        [DecryptParameter]
        public ActionResult PLMCase(int id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.PropertyLeaseApplications.Include(x=>x.SystemUser).Include(x => x.Status).Include(r=>r.Customer).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstName;
                ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Sat = applicationProp.FirstName;
                HoD hd = new HoD();
                hd.HoDFirstName = systemusers.LastName;
                hd.HoDLastName = systemusers.FirstName;

                

                var dvm = new DepartmentsApprovalViewModel
                {
                    
                    PropertyLeaseApplication = applicationProp,
                    HoD = hd
                };

                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                return View(dvm);
            }
            catch
            {  

            }
            return View();
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PLMCase(DepartmentsApprovalViewModel approvalViewModel)
        {

            HoD com = new HoD();
            
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.HoDFirstName = approvalViewModel.HoD.HoDFirstName;
                com.HoDLastName = approvalViewModel.HoD.HoDLastName;
                com.Outcome = approvalViewModel.HoD.Outcome;
                com.HoDCommments = approvalViewModel.HoD.HoDCommments;
                com.PropertyLeaseApplicationId = approvalViewModel.PropertyLeaseApplication.Id;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingLeaseAgreement)?.Id, approvalViewModel.PropertyLeaseApplication.Id);
                db.HoDs.Add(com);
                db.SaveChanges();
                if (com.Outcome== db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).FirstOrDefault().Key)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.HoDApprovedAplication).Description.ToString() + ": " + custmusers.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }
                else if(com.Outcome == db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Rejected).FirstOrDefault().Key)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.HoDRejectedApplication).Description.ToString() + ": " + custmusers.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                }

                return RedirectToAction("Index");
            }
            catch { 
            return RedirectToAction("Index");
            }
        }
        #endregion











        #region TLA Application

        [DecryptParameter]
        public ActionResult TLACase(int id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.LeaseDetails.Include(x=>x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstNames;
                ViewBag.RfNo = applicationProp.LeaseReferenceNo;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Status = applicationProp.Status.Name;
                ViewBag.Purchaser = applicationProp.PurchaserType.Name;
                HoD hd = new HoD();
                hd.HoDFirstName = systemusers.LastName;
                hd.HoDLastName = systemusers.FirstName;
                ViewBag.LeaseId = applicationProp.Id;

                Session["LeaseId"] = applicationProp.Id;

                var dvm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = applicationProp,
                    HoD = hd
                };

                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                return View(dvm);
            }
            catch
            {  

            }
            return View();
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TLACase(DepartmentsApprovalViewModel approvalViewModel)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    int LeaseId = Convert.ToInt32(Session["LeaseId"].ToString());

                    HoD com = new HoD();
                    var Customerid = Customer;
                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                    var userID = User.Identity.GetUserId();

                    ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                    com.HoDFirstName = approvalViewModel.HoD.HoDFirstName;
                    com.HoDLastName = approvalViewModel.HoD.HoDLastName;
                    com.Outcome = approvalViewModel.HoD.Outcome;
                    com.HoDCommments = approvalViewModel.HoD.HoDCommments;
                    com.LeaseDetailsId = LeaseId;
                    com.IsActive = true;
                    com.IsDeleted = false;
                    com.IsLocked = false;
                    
                    db.HoDs.Add(com);
                    db.SaveChanges();
                    if (com.Outcome == db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).FirstOrDefault().Key)
                    {
                        MatchingHelper.ChangeLeaseStatusII(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingRenewalTenantLease)?.Id, LeaseId);

                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.HoDApprovedAplication).Description.ToString() + ": " + custmusers.FullName;
                        var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }
                    else if (com.Outcome == db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Rejected).FirstOrDefault().Key)
                    {
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.HoDRejectedApplication).Description.ToString() + ": " + custmusers.FullName;
                        var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }

                    return RedirectToAction("Index");
                }
                catch
                {
                    return RedirectToAction("Index");
                }
            }
           
        }
        #endregion

















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
