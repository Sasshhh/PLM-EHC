using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;


namespace C8.eServices.Mvc.Controllers
{
    public class DepartmentalCommentsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public PropertyLeaseApplication PropertyLeaseApplication { get; set; }
        public ApplicationDepartment applicationDepartment { get; set; } 
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
        #region Index
        //[DecryptParameter]
        public ActionResult Index(/*int id*/)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var Customerid = Customer;
                    var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                    var userID = User.Identity.GetUserId();
                    var COEDepart = db.DepartmentsCoEs.Where(x => x.RepresentedBy == userID).FirstOrDefault().DepartmentName;
                    var rCSApplicationStatus = db.ApplicationDepart.Where(x => x.IsDeleted == false && x.DepartmentType==COEDepart).Include(r => r.Status).ToList();

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
        #endregion

        #region PLM Case & Details
        [DecryptParameter]
        public ActionResult PLMCases(int Id)
        {
            var departmentalComments = (from i in db.ApplicationDepart
                                        join o in db.PropertyLeaseApplications on i.PropertyApplicationRef equals o.ApplicationReferenceNumber
                                        select o);
            return View(departmentalComments.ToList());
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentalComments departmentalComments = db.DepartmentalComments.Find(id);
            if (departmentalComments == null)
            {
                return HttpNotFound();
            }
            return View(departmentalComments);
        }
        #endregion

        #region Comments GET
        [DecryptParameter]
        public ActionResult Comments(string Id)
        {
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                var proppplica = db.PropertyLeaseApplications.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.ApplicationReferenceNumber == Id).FirstOrDefault();
                ViewBag.ActionDate = db.ApplicationDepart.Where(x => x.PropertyLeaseApplicationId == proppplica.Id).FirstOrDefault().ActionDueDate.ToString();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var dep = db.DepartmentalComments.FirstOrDefault();
                DepartmentalComments initdep = new DepartmentalComments();
                initdep.LastName = systemusers.LastName;
                initdep.FirstName = systemusers.FirstName;
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));

               

                if (application == null) throw new Exception("Invalid application.");
                if (referenceType == null) throw new Exception("Invalid reference type.");


                //var PurchaseInfo = _context.PurchaserInformations.Where(x => x.RCSApplicationStatusId == RcsApplication.Id).Include(r => r.RCSApplicationStatus)/*.Include(r =>r.PurchaseType)*/.FirstOrDefault();
                //if (PurchaseInfo == null) throw new Exception("Invalid purchaser info.");


                //var systemUser = new IdentityManager().CurrentUser( User );
                //var customer = _context.Customers.SingleOrDefault( o => o.SystemUserId == systemUser.Id );

                // Always defaults ID document to application. 
                var documentCheckLists = new List<DocumentCheckList>();

                //var idDocumentType = _context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.IdDocument);

                //documentCheckLists.Add(_context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == idDocumentType.Id && dcl.ReferenceTypeId == referenceTypeId));
                //required
                var AuthorityToActAttorney = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AuthoritytoActAttorney);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == AuthorityToActAttorney.Id && dcl.ReferenceTypeId == referenceType.Id));


                //var ProofOfProperty = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.AssesmentRatesAccount);

                //documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == ProofOfProperty.Id && dcl.ReferenceTypeId == referenceTypeId));

                //required
                var MunicipalStatement = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.MunicipalStatement);

                var MunicipalCheckList = context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceType.Id);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == MunicipalStatement.Id && dcl.ReferenceTypeId == referenceType.Id));


                //required
                var BankConfirmationLetter = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.BankConfirmationLetter);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == BankConfirmationLetter.Id && dcl.ReferenceTypeId == referenceType.Id));

                //
                var DeedSearch = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DeedSearch);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == DeedSearch.Id && dcl.ReferenceTypeId == referenceType.Id));
                //required
                //this code will change
                var SellerID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.SellerIDDocument);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == SellerID.Id && dcl.ReferenceTypeId == referenceType.Id));
                //required
                var PurchaserID = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.PurchaserIDDocument);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == PurchaserID.Id && dcl.ReferenceTypeId == referenceType.Id));


                //OccupantsTypes

                var department1 = context.DocumentTypes.SingleOrDefault(dt => dt.Key == DocumentTypeKeys.DepartmentalComents);

                documentCheckLists.Add(context.DocumentCheckLists.Include(dcl => dcl.DocumentType).SingleOrDefault(dcl => dcl.DocumentTypeId == department1.Id && dcl.ReferenceTypeId == referenceType.Id));

                //var returnUrl2 = "Test"/*success.ToString(CultureInfo.InvariantCulture)*/;
                //return RedirectToAction("IndexTenants", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(
                //    new
                //    {
                //        referenceId = Customer.Id,
                //        customerId = Customer.Id,
                //        referenceTypeId = 12,
                //        applicationId = application.Id,
                //        agentId = application.Id,

                //        rcsappId = context.
                //    })));


                var dvmdocs = new DocumentsViewModel
                {
                  
                    // CustomerId = (agentId == null) ? customerId : referenceId,
                    CustomerId = proppplica.CustomerId,
                    ApplicationId = (int)application.Id,
                    Application = application,
                    PropertyLeaseApplicationId = proppplica.Id,
                    RcsApplicationId = proppplica.Id,
                    ReferenceTypeId = (int)referenceType.Id,
                    ReferenceType = referenceType,
                    ReferenceId = (int)referenceType.Id,
                    IsUploadView = true,
                    Documents = context.Documents.Include(o => o.File).Where(o => o.ReferenceId == referenceType.Id && o.ReferenceTypeId == referenceType.Id && o.PropertyLeaseApplicationId == proppplica.Id && o.IsActive && !o.IsDeleted).ToList()
                };


                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");

                var dvm = new DepartmentsApprovalViewModel
                {
                    DepartmentalComments = initdep,
                    PropertyLeaseApplication = proppplica,
                    Document = dvmdocs
                };


                ViewBag.DepartmentsCoEs = new SelectList(db.DepartmentsCoEs.Where(x => x.IsActive == true).OrderBy(x => x.DepartmentName), "Id", "DepartmentName");
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                return View(dvm);
            }
            catch
            {
            }
            return View();
        }
        #endregion

        #region comments POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Comments(DepartmentsApprovalViewModel dvm, string ApprovalStatusddl)
        {
            DepartmentalComments com = new DepartmentalComments();
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.FirstName = dvm.DepartmentalComments.FirstName;
                com.LastName = dvm.DepartmentalComments.LastName;
                com.OfficialNumber = dvm.DepartmentalComments.OfficialNumber;
                com.Outcome = dvm.DepartmentalComments.Outcome;

                com.Reason = dvm.DepartmentalComments.Reason;
                com.PropertyLeaseApplicationId = dvm.PropertyLeaseApplication.Id;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                com.DateStamp = DateTime.Now;
                db.DepartmentalComments.Add(com);
                db.SaveChanges();

                var Customerid = Customer;
                var custmusers = context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();

                var Pa = db.PropertyLeaseApplications.Where(x => x.Id == com.PropertyLeaseApplicationId).Include(x => x.SystemUser).FirstOrDefault();
                var depart = db.DepartmentsCoEs.Where(x => x.IsActive == true && x.RepresentedBy == userID).FirstOrDefault().DepartmentName;
                var AdModify = db.ApplicationDepart.Where(x => x.PropertyApplicationRef == Pa.ApplicationReferenceNumber && x.DepartmentType == depart).FirstOrDefault();
                //AdModify.IsActive = false;
                db.Entry(AdModify).State = EntityState.Modified;
                db.SaveChanges();
                if (com.Outcome== context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).FirstOrDefault().Key)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationDepartmentalApprove).Description.ToString() + ": " + custmusers.FullName;
                    MatchingHelper.ChangeDepartmentStatusAudit(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.DepartmentApproved)?.Id, AdModify.Id, ActivityTrackerMessage, AdModify.PropertyLeaseApplicationId, custmusers.Id);
                }
                else if (com.Outcome == context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Rejected).FirstOrDefault().Key)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationDepartmentalReject).Description.ToString() + ": " + custmusers.FullName;
                    MatchingHelper.ChangeDepartmentStatusAudit(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.DepartmentRejected)?.Id, AdModify.Id, ActivityTrackerMessage, AdModify.PropertyLeaseApplicationId, custmusers.Id);
                    
                }
                
                var Ad = db.ApplicationDepart.Where(x => x.PropertyApplicationRef == Pa.ApplicationReferenceNumber).ToList();
                var Ca = Ad.Where(x =>x.StatusId== context.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingDepartmentResponse).Id).ToList().Count();
                var Creject = Ad.Where(x => x.StatusId == context.Status.FirstOrDefault(o => o.Key == StatusKeys.DepartmentRejected).Id).ToList().Count();

                if (Ca == 0)
                {
                    var PropertyId = db.PropertyLeaseApplications.Where(x => x.ApplicationReferenceNumber == AdModify.PropertyApplicationRef).FirstOrDefault().Id;
                    if (Creject!=0)
                    {
                        MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.NotAllDepartmentsApproved)?.Id, PropertyId);
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotAllDepartmentsApproved).Description.ToString() + ": " + custmusers.FullName;
                        var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }
                    else
                    {
                        MatchingHelper.ChangeApplicationStatus(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AllDepartmentsApproved)?.Id, PropertyId);
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AllDepartmentsApproved).Description.ToString() + ": " + custmusers.FullName;
                        var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }
                }

                 

                return RedirectToAction("Index");
            }

            catch (Exception e)
            {

                throw;
            }

            var dvm2 = new DepartmentsApprovalViewModel

            {
                DepartmentalComments = com

            };
            return View(dvm2);
        }
        #endregion




        #region Comments GET
        [DecryptParameter]
        public ActionResult TLAComments(int? Id)
        {
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();
                var appdept = context.ApplicationDepart.Where(x => x.Id == Id).FirstOrDefault();
                var proppplica = db.LeaseDetails.Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).Where(x => x.Id == appdept.LeaseDetailsId).FirstOrDefault();
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                var dep = db.DepartmentalComments.FirstOrDefault();
                DepartmentalComments initdep = new DepartmentalComments();
                initdep.LastName = systemusers.LastName;
                initdep.FirstName = systemusers.FirstName;

                var dvm = new DepartmentsApprovalViewModel
                {
                    DepartmentalComments = initdep,
                    LeaseDetails = proppplica
                };

                Session["LeaseId"] = proppplica.Id;

                ViewBag.LeaseId = proppplica.Id;
                ViewBag.ActionDate = db.ApplicationDepart.Where(x => x.LeaseDetailsId == appdept.LeaseDetailsId).FirstOrDefault().ActionDueDate.ToString();
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.DepartmentsCoEs = new SelectList(db.DepartmentsCoEs.Where(x => x.IsActive == true).OrderBy(x => x.DepartmentName), "Id", "DepartmentName");
                ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                return View(dvm);
            }
            catch { }
            return View();

        }
        #endregion

        #region comments POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TLAComments(DepartmentsApprovalViewModel dvm, string ApprovalStatusddl)
        {
            DepartmentalComments com = new DepartmentalComments();
            try
            {
                eServicesDbContext context = new eServicesDbContext();
                Initialise();

                var LeaseId = Convert.ToInt32(Session["LeaseId"].ToString());
                Session["LeaseId"] = "";

                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                com.FirstName = dvm.DepartmentalComments.FirstName;
                com.LastName = dvm.DepartmentalComments.LastName;
                com.OfficialNumber = dvm.DepartmentalComments.OfficialNumber;
                com.Outcome = dvm.DepartmentalComments.Outcome;

                com.Reason = dvm.DepartmentalComments.Reason;
                com.LeaseDetailsId = LeaseId;
                com.IsActive = true;
                com.IsDeleted = false;
                com.IsLocked = false;
                com.DateStamp = DateTime.Now;
                db.DepartmentalComments.Add(com);
                db.SaveChanges();

                var Customerid = Customer;
                var custmusers = context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();

                var Pa = db.LeaseDetails.Where(x => x.Id == com.LeaseDetailsId).Include(x => x.SystemUser).FirstOrDefault();
                var depart = db.DepartmentsCoEs.Where(x => x.IsActive == true && x.RepresentedBy == userID).FirstOrDefault().DepartmentName;
                var AdModify = db.ApplicationDepart.Where(x => x.LeaseDetailsId == com.LeaseDetailsId && x.DepartmentType == depart).FirstOrDefault();
                db.Entry(AdModify).State = EntityState.Modified;
                db.SaveChanges();

                if (com.Outcome == context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved).FirstOrDefault().Key)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationDepartmentalApprove).Description.ToString() + ": " + custmusers.FullName;
                    MatchingHelper.TLAChangeDepartmentStatusAudit(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.DepartmentApproved)?.Id, AdModify.Id, ActivityTrackerMessage, AdModify.LeaseDetailsId, custmusers.Id);
                }
                else if (com.Outcome == context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Rejected).FirstOrDefault().Key)
                {
                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicationDepartmentalReject).Description.ToString() + ": " + custmusers.FullName;
                    MatchingHelper.TLAChangeDepartmentStatusAudit(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.DepartmentRejected)?.Id, AdModify.Id, ActivityTrackerMessage, AdModify.LeaseDetailsId, custmusers.Id);

                }

                var Ad = db.ApplicationDepart.Where(x => x.LeaseDetailsId == com.LeaseDetailsId).ToList();
                var Ca = Ad.Where(x => x.StatusId == context.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingDepartmentResponse).Id).ToList().Count();
                var Creject = Ad.Where(x => x.StatusId == context.Status.FirstOrDefault(o => o.Key == StatusKeys.DepartmentRejected).Id).ToList().Count();

                if (Ca == 0)
                {
                    if (Creject != 0)
                    {
                        MatchingHelper.ChangeLeaseStatusII(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.NotAllDepartmentsApproved)?.Id, LeaseId);
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.NotAllDepartmentsApproved).Description.ToString() + ": " + custmusers.FullName;
                        //var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }
                    else
                    {
                        MatchingHelper.ChangeLeaseStatusII(db, (int)db.Status.FirstOrDefault(x => x.Key == StatusKeys.AllDepartmentsApproved)?.Id, LeaseId);
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AllDepartmentsApproved).Description.ToString() + ": " + custmusers.FullName;
                        //var Result = ActivityTrackerAudit(com.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {

                throw;
            }

            var dvm2 = new DepartmentsApprovalViewModel

            {
                DepartmentalComments = com

            };
            return View(dvm2);
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
