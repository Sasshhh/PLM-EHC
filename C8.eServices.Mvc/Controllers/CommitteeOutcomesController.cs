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
    public class CommitteeOutcomesController : Controller
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

        #region Indexs
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.AllDepartmentsApproved ).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.NotAllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.RecomendedByDFC).Id).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();

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
                    return View(rCSApplicationStatus);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }
        public ActionResult DFCList()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.AllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.NotAllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.RecomendedByDFC).Id).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    var leaseApplicationDfc = db.LeaseDetails.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.AllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.NotAllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.RecomendedByDFC).Id).Include(r => r.CreatedBySystemUser).Include(r => r.ModifiedBySystemUser).Include(r => r.PurchaserType).Include(r => r.Status).ToList();

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
                    foreach (var item in leaseApplicationDfc)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }

                    var vm = new DepartmentsApprovalViewModel();
                    vm.LeaseDetailsList = leaseApplicationDfc;
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
        #endregion

        #region REAC GET&POST
        [DecryptParameter]
        public ActionResult PLMOutcome(int? id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstName;
                ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Sat = applicationProp.FirstName;
                CommitteeOutcome hd = new CommitteeOutcome();
                hd.Surname = systemusers.LastName;
                hd.FirstName = systemusers.FirstName;


                var dvm = new DepartmentsApprovalViewModel
                {

                    PropertyLeaseApplication = applicationProp,
                    CommitteeOutcome = hd
                };
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.CommitteeDFC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.CommitteeREAC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Name== "REAC"), "Key", "Name");
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Description");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Description");
                ViewBag.date = DateTime.Now.Date;
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
        public ActionResult PLMOutcome(DepartmentsApprovalViewModel documents)
        {
            CommitteeOutcome com = new CommitteeOutcome();
            PropertyLeaseApplication property = new PropertyLeaseApplication();
            property= db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == documents.PropertyLeaseApplication.ApplicationReferenceNumber).FirstOrDefault();
            com = documents.CommitteeOutcome;
            try
             {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                var CustomerId = Customer;
                var custmusersI = context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var Systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userId = User.Identity.GetUserId();
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Name");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Name");
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.FirstName = documents.CommitteeOutcome.FirstName;
                com.Surname = documents.CommitteeOutcome.Surname;
                com.OfficialNumber = documents.CommitteeOutcome.OfficialNumber;
                com.Reason = documents.CommitteeOutcome.Reason;
                var i = db.recommendations.Where(x => x.Id == documents.CommitteeOutcome.RecommendationId).FirstOrDefault();
                com.RecommendationId = i.Id;
                com.Committee_Name = documents.CommitteeOutcome.Committee_Name;
                com.PropertyLeaseApplicationId = documents.PropertyLeaseApplication.Id;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                com.DateStamp = DateTime.Now;
                db.committeeOutcomes.Add(com);
                db.SaveChanges();

                

                var PropertyId = db.PropertyLeaseApplications.Where(x => x.Id == documents.PropertyLeaseApplication.Id).FirstOrDefault().Id;
                if (i.Name=="HOD_Approve")
                {
                    MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingHoDResponse)?.Id, PropertyId);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.REACRecomendHOD).Description.ToString() + ": " + custmusersI.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusersI.Id);
                }
                else if (i.Name == "HOD_Cannot_Approve")
                {

                }
                else if (i.Name == "Submit_to_council")
                {

                }
                else if (i.Name == "Follow_SCM_Process")
                {

                }

                return RedirectToAction("Index");
            }
            catch(Exception e) {
            
            }
            var Customerid = Customer;
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
            var userID = User.Identity.GetUserId();
        
            CommitteeOutcome hd = new CommitteeOutcome();
            var dvm = new DepartmentsApprovalViewModel
            {
                CommitteeOutcome = hd
            };

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.ModifiedBySystemUserId);
            ViewBag.CommitteeName = new SelectList(db.committeeNames.OrderBy(x => x.CommitteeN), "Id", "Name");
            ViewBag.Recommendations = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName), "Id", "Name");
            ViewBag.date = DateTime.Now.Date;
            return View(dvm);
        }
        #endregion



        #region DFC GET&POST
        [DecryptParameter]
        public ActionResult DFC(int? id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstName;
                ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Sat = applicationProp.FirstName;
                CommitteeOutcome hd = new CommitteeOutcome();
                hd.Surname = systemusers.LastName;
                hd.FirstName = systemusers.FirstName;


                var dvm = new DepartmentsApprovalViewModel
                {

                    PropertyLeaseApplication = applicationProp,
                    CommitteeOutcome = hd
                };
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.CommitteeDFC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Name== "DFC"), "Key", "Name");
                ViewBag.CommitteeNameREAC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Description");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Description");
                ViewBag.date = DateTime.Now.Date;
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
        public ActionResult DFC(DepartmentsApprovalViewModel documents)
        {
            CommitteeOutcome com = new CommitteeOutcome();
            PropertyLeaseApplication property = new PropertyLeaseApplication();
            property = db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == documents.PropertyLeaseApplication.ApplicationReferenceNumber).FirstOrDefault();
            com = documents.CommitteeOutcome;
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "dfc"), "Id", "Name");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "reac"), "Id", "Name");
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.FirstName = documents.CommitteeOutcome.FirstName;
                com.Surname = documents.CommitteeOutcome.Surname;
                com.OfficialNumber = documents.CommitteeOutcome.OfficialNumber;
                com.Reason = documents.CommitteeOutcome.Reason;
                var i = db.recommendations.Where(x => x.Id == documents.CommitteeOutcome.RecommendationId).FirstOrDefault();
                com.RecommendationId = i.Id;
                com.Committee_Name = documents.CommitteeOutcome.Committee_Name;
                com.PropertyLeaseApplicationId = documents.PropertyLeaseApplication.Id;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                com.DateStamp = DateTime.Now;
                db.committeeOutcomes.Add(com);
                db.SaveChanges();

                var PropertyId = db.PropertyLeaseApplications.Where(x => x.Id == documents.PropertyLeaseApplication.Id).FirstOrDefault().Id;
                if (i.Name == "Approve")
                {
                    MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.RecomendedByDFC)?.Id, PropertyId);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.DFCApprovedApplication).Description.ToString() + ": " + Customer.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, Customer.Id);
                }
                else if (i.Name == "Decline")
                {
                    MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.RejectedByDFC)?.Id, PropertyId);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.DFCRejectedApplication).Description.ToString() + ": " + Customer.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, Customer.Id);
                }


                return RedirectToAction("DFClist");
            }
            catch (Exception e)
            {

            }
            var Customerid = Customer;
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
            var userID = User.Identity.GetUserId();

            CommitteeOutcome hd = new CommitteeOutcome();
            var dvm = new DepartmentsApprovalViewModel
            {
                //PropertyLeaseApplication = applicationProp,
                CommitteeOutcome = hd
            };

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.ModifiedBySystemUserId);
            ViewBag.CommitteeName = new SelectList(db.committeeNames.OrderBy(x => x.CommitteeN), "Id", "Name");
            ViewBag.Recommendations = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName), "Id", "Name");
            ViewBag.date = DateTime.Now.Date;
            return View(dvm);
        }

        #endregion


        #region Tenant DFC GET&POST
        [DecryptParameter]
        public ActionResult TLADFC(int? id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.LeaseDetails.Include(x => x.SystemUser).Include(r=>r.CreatedBySystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstNames;
                ViewBag.RfNo = applicationProp.LeaseReferenceNo;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Sat = applicationProp.PurchaserType.Name;
                CommitteeOutcome hd = new CommitteeOutcome();
                hd.Surname = systemusers.LastName;
                hd.FirstName = systemusers.FirstName;


                var dvm = new DepartmentsApprovalViewModel
                {
                    LeaseDetails = applicationProp,
                    CommitteeOutcome = hd
                };

                Session["LeaseId"] = applicationProp.Id;

                ViewBag.LeaseId = applicationProp.Id;
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.CommitteeDFC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Name == "DFC"), "Key", "Name");
                ViewBag.CommitteeNameREAC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Description");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Description");
                ViewBag.date = DateTime.Now.Date;
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
        public ActionResult TLADFC(DepartmentsApprovalViewModel documents)
        {
            using(var _context = new eServicesDbContext())
            {
                CommitteeOutcome com = new CommitteeOutcome();
                LeaseDetails lease = new LeaseDetails();
                int Id = Convert.ToInt32(Session["LeaseId"].ToString());
                lease = _context.LeaseDetails.Where(x => x.Id == Id).FirstOrDefault();
                Session["LeaseId"] = "";
                com = documents.CommitteeOutcome;
                try
                {
                    Initialise();

                    

                    ViewBag.RecommendDFC = new SelectList(_context.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "dfc"), "Id", "Name");
                    ViewBag.RecommendREAC = new SelectList(_context.recommendations.OrderBy(x => x.RecommendatinName).Where(x => x.Key == "reac"), "Id", "Name");
                    ViewBag.ApprovalStatus = new SelectList(_context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                    com.FirstName = documents.CommitteeOutcome.FirstName;
                    com.Surname = documents.CommitteeOutcome.Surname;
                    com.OfficialNumber = documents.CommitteeOutcome.OfficialNumber;
                    com.Reason = documents.CommitteeOutcome.Reason;
                    var i = _context.recommendations.Where(x => x.Id == documents.CommitteeOutcome.RecommendationId).FirstOrDefault();
                    com.RecommendationId = i.Id;
                    com.Committee_Name = documents.CommitteeOutcome.Committee_Name;
                    com.LeaseDetailsId = lease.Id;
                    com.IsActive = true;
                    com.IsDeleted = false;
                    com.IsLocked = false;
                    com.DateStamp = DateTime.Now;
                    _context.committeeOutcomes.Add(com);
                    _context.SaveChanges();

                    if (i.Name == "Approve")
                    {
                        MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.RecomendedByDFC)?.Id, lease.Id);
                        var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.DFCApprovedApplication).Description.ToString() + ": " + Customer.FullName;
                        var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, Customer.Id);
                    }
                    else if (i.Name == "Decline")
                    {
                        MatchingHelper.ChangeLeaseStatusII(_context, (int)_context.Status.FirstOrDefault(x => x.Key == StatusKeys.RejectedByDFC)?.Id, lease.Id);
                        var ActivityTrackerMessage = _context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.DFCRejectedApplication).Description.ToString() + ": " + Customer.FullName;
                        var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, Customer.Id);
                    }


                    return RedirectToAction("DFClist");
                }
                catch (Exception e)
                {

                }
                var Customerid = Customer;
                var custmusers = _context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = _context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();

                CommitteeOutcome hd = new CommitteeOutcome();
                var dvm = new DepartmentsApprovalViewModel
                {
                    CommitteeOutcome = hd
                };

                ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.CreatedBySystemUserId);
                ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.ModifiedBySystemUserId);
                ViewBag.CommitteeName = new SelectList(_context.committeeNames.OrderBy(x => x.CommitteeN), "Id", "Name");
                ViewBag.Recommendations = new SelectList(_context.recommendations.OrderBy(x => x.RecommendatinName), "Id", "Name");
                ViewBag.date = DateTime.Now.Date;
                return View(dvm);
            }
            
        }

        #endregion



        public ActionResult REACList()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.AllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.NotAllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.RecomendedByDFC).Id).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).ToList();
                    var leaselist = db.LeaseDetails.Where(x => x.IsDeleted == false && x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.AllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.NotAllDepartmentsApproved).Id || x.StatusId == db.Status.FirstOrDefault(t => t.Key == StatusKeys.RecomendedByDFC).Id).Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).ToList();

                    foreach (var item in rCSApplicationStatus)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    foreach (var item in leaselist)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    var vm = new DepartmentsApprovalViewModel();
                    vm.PropertyLeaseApplicationList = rCSApplicationStatus;
                    vm.LeaseDetailsList = leaselist;

                    return View(vm);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
                return RedirectToAction("Login", "Account");
            }
        }


        #region Tenant REAC GET&POST
        [DecryptParameter]
        public ActionResult TLAOutcome(int? id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.LeaseDetails.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstNames;
                ViewBag.RfNo = applicationProp.LeaseReferenceNo;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Status = applicationProp.Status.Name;
                CommitteeOutcome hd = new CommitteeOutcome();
                hd.Surname = systemusers.LastName;
                hd.FirstName = systemusers.FirstName;
                Session["LeaseId"] = applicationProp.Id;

                var dvm = new DepartmentsApprovalViewModel
                {

                    LeaseDetails = applicationProp,
                    CommitteeOutcome = hd
                };



                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.CommitteeDFC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.CommitteeREAC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Name == "REAC"), "Key", "Name");
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Description");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Description");
                ViewBag.date = DateTime.Now.Date;
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
        public ActionResult TLAOutcome(DepartmentsApprovalViewModel documents)
        {
            CommitteeOutcome com = new CommitteeOutcome();

            com = documents.CommitteeOutcome;
            try
            {
                int leaseId = Convert.ToInt32(Session["LeaseId"].ToString());
                Session["LeaseId"] = "";

                LeaseDetails property = new LeaseDetails();
                property = db.LeaseDetails.Where(x => x.Id == leaseId).FirstOrDefault();
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                var CustomerId = Customer;
                var custmusersI = context.Customers.FirstOrDefault(x => x.Id == Customer.Id);

                var Systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userId = User.Identity.GetUserId();
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Name");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Name");
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.FirstName = documents.CommitteeOutcome.FirstName;
                com.Surname = documents.CommitteeOutcome.Surname;
                com.OfficialNumber = documents.CommitteeOutcome.OfficialNumber;
                com.Reason = documents.CommitteeOutcome.Reason;
                var i = db.recommendations.Where(x => x.Id == documents.CommitteeOutcome.RecommendationId).FirstOrDefault();
                com.RecommendationId = i.Id;
                com.Committee_Name = documents.CommitteeOutcome.Committee_Name;
                com.LeaseDetailsId = leaseId;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                com.DateStamp = DateTime.Now;
                db.committeeOutcomes.Add(com);
                db.SaveChanges();

                if (i.Name == "HOD_Approve")
                {
                    MatchingHelper.ChangeLeaseStatusII(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingHoDResponse)?.Id, leaseId);
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.REACRecomendHOD).Description.ToString() + ": " + custmusersI.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusersI.Id);
                }
                else if (i.Name == "HOD_Cannot_Approve")
                {

                }
                else if (i.Name == "Submit_to_council")
                {

                }
                else if (i.Name == "Follow_SCM_Process")
                {

                }

                return RedirectToAction("REACList");
            }
            catch (Exception e)
            {

            }
            var Customerid = Customer;
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
            var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
            var userID = User.Identity.GetUserId();

            CommitteeOutcome hd = new CommitteeOutcome();
            var dvm = new DepartmentsApprovalViewModel
            {
                CommitteeOutcome = hd
            };

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documents.CommitteeOutcome.ModifiedBySystemUserId);
            ViewBag.CommitteeName = new SelectList(db.committeeNames.OrderBy(x => x.CommitteeN), "Id", "Name");
            ViewBag.Recommendations = new SelectList(db.recommendations.OrderBy(x => x.RecommendatinName), "Id", "Name");
            ViewBag.date = DateTime.Now.Date;
            return View(dvm);
        }
        #endregion

        #region Tenant Termination REAC GET&POST
        [DecryptParameter]
        public ActionResult REACTermination(int? id)
        {
            try
            {
                Initialise();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var applicationProp = db.LeaseDetails.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == id).FirstOrDefault();
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstNames;
                ViewBag.RfNo = applicationProp.LeaseReferenceNo;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Status = applicationProp.Status.Name;
                CommitteeOutcome hd = new CommitteeOutcome();
                hd.Surname = systemusers.LastName;
                hd.FirstName = systemusers.FirstName;
                Session["LeaseId"] = applicationProp.Id;

                var dvm = new DepartmentsApprovalViewModel
                {

                    LeaseDetails = applicationProp,
                    CommitteeOutcome = hd
                };

                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.CommitteeDFC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true), "Key", "Name");
                ViewBag.CommitteeREAC = new SelectList(db.committeeNames.OrderBy(x => x.Name).Where(x => (bool)x.IsDeleted != true && x.Name == "REAC"), "Key", "Name");
                ViewBag.RecommendDFC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Description");
                ViewBag.RecommendREAC = new SelectList(db.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Description");
                ViewBag.date = DateTime.Now.Date;
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
        public ActionResult REACTermination(DepartmentsApprovalViewModel documents, int? id)
        {
            using (var _database=new eServicesDbContext())
            {
                CommitteeOutcome com = new CommitteeOutcome();
                com = documents.CommitteeOutcome;
                LeaseDetails property = new LeaseDetails();
                property = _database.LeaseDetails.Where(x => x.Id == id).FirstOrDefault();
                Initialise();
                var CustomerId = Customer;
                var custmusersI = _database.Customers.FirstOrDefault(x => x.Id == Customer.Id);

                var Systemusers = _database.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userId = User.Identity.GetUserId();
                ViewBag.RecommendDFC = new SelectList(_database.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "dfc"), "Id", "Name");
                ViewBag.RecommendREAC = new SelectList(_database.recommendations.OrderBy(x => x.Description).Where(x => x.Key == "reac"), "Id", "Name");
                ViewBag.ApprovalStatus = new SelectList(_database.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.FirstName = documents.CommitteeOutcome.FirstName;
                com.Surname = documents.CommitteeOutcome.Surname;
                com.OfficialNumber = documents.CommitteeOutcome.OfficialNumber;
                com.Reason = documents.CommitteeOutcome.Reason;
                var i = _database.recommendations.Where(x => x.Id == documents.CommitteeOutcome.RecommendationId).FirstOrDefault();
                com.RecommendationId = i.Id;
                com.Committee_Name = documents.CommitteeOutcome.Committee_Name;
                com.LeaseDetailsId = id;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                com.DateStamp = DateTime.Now;
                _database.committeeOutcomes.Add(com);
                _database.SaveChanges();

                if (i.Name == "HOD_Approve")
                {
                    MatchingHelper.ChangeLeaseStatusII(_database, (int)_database.Status.FirstOrDefault(x => x.Key == StatusKeys.AwaitingHoDResponse)?.Id, (int)id);
                    var ActivityTrackerMessage = _database.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.REACRecomendHOD).Description.ToString() + ": " + custmusersI.FullName;
                    var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusersI.Id);
                }
                else if (i.Name == "HOD_Cannot_Approve")
                {

                }
                else if (i.Name == "Submit_to_council")
                {

                }
                else if (i.Name == "Follow_SCM_Process")
                {

                }
                return RedirectToAction("REACList");
            }

        }
        #endregion

        #region 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommitteeOutcome committeeOutcome = db.committeeOutcomes.Find(id);
            if (committeeOutcome != null)
            {
                return HttpNotFound();
            }
            return View(committeeOutcome);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommitteeOutcome committeeOutcome = db.committeeOutcomes.Find(id);
            if (committeeOutcome == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", committeeOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", committeeOutcome.ModifiedBySystemUserId);
            return View(committeeOutcome);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Committee_Outcome,RepresentedBy,Surname,FirstName,OfficialNumber,Recommendations,Reason,SupportingDocuments,DateStamp,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] CommitteeOutcome committeeOutcome)
        {
            if (ModelState.IsValid)
            {
                db.Entry(committeeOutcome).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", committeeOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", committeeOutcome.ModifiedBySystemUserId);
            return View(committeeOutcome);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommitteeOutcome committeeOutcome = db.committeeOutcomes.Find(id);
            if (committeeOutcome == null)
            {
                return HttpNotFound();
            }
            return View(committeeOutcome);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CommitteeOutcome committeeOutcome = db.committeeOutcomes.Find(id);
            db.committeeOutcomes.Remove(committeeOutcome);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult check()
        {
            var list = new List<string>() { "Check 1", "Check 2", "Check 3" };
            ViewBag.list = list;
            return View();

        }
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
