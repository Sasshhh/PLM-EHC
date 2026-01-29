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
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Controllers
{
    public class RefundApplicationsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        #region init
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RCSApplicationController"/> class.
        /// </summary>
        public RefundApplicationsController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RCSApplicationController"/> class.
        /// </summary>
        /// <param name="userManager">The userManager<see cref="UserManager{SystemIdentityUser}"/>.</param>
        public RefundApplicationsController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RCSApplicationController"/> class.
        /// </summary>
        /// <param name="db">The db<see cref="eServicesDbContext"/>.</param>
        public RefundApplicationsController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        /// <summary>
        /// Gets or sets the IdentityManager.
        /// </summary>
        public IdentityManager IdentityManager { get; set; }

        /// <summary>
        /// Gets or sets the SystemUser.
        /// </summary>
        public SystemUser SystemUser { get; set; }

        /// <summary>
        /// Gets or sets the Customer.
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the Entity.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the Agent.
        /// </summary>
        public Agent Agent { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// The Initialise.
        /// </summary>
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
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        [DecryptParameter]
        public ActionResult Details(int? refNo)
        {
            if (refNo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var vm = new ApplicationDetailsViewModel();
            var refund = db.RefundApplications.Where(x => x.Id == refNo && x.IsDeleted == false).ToList().FirstOrDefault();
            RCSApplicationStatus rcsapp = db.RCSApplicationStatus.Where(x => x.Id == refund.RCSApplicationStatusId && x.IsDeleted == false).ToList().FirstOrDefault();
            var TransferInformationID = rcsapp.TransferInformationId;
            var MunicipalAccountInformationID = rcsapp.MunicipalAccountInformationId;
            TransferInformation TransferInformation = db.TransferInformations.Include(x => x.TransferTypes).Where(x => x.Id == TransferInformationID).ToList().FirstOrDefault();
            SellerInformation sellerInformation = db.SellerInformations.Where(x => x.RCSApplicationStatusId == refNo).ToList().FirstOrDefault();
            MunicipalAccountInformation MunicipalAccountInformation = db.MunicipalAccountInformations.Where(x => x.Id == MunicipalAccountInformationID).ToList().FirstOrDefault();

            WalkInApplicantDetails walkInApplicantDetails = db.WalkInApplicantDetails.Include(x => x.ActOnBehalfType).Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList().FirstOrDefault();
            if (walkInApplicantDetails != null)
            {
                ViewBag.walkIn = "walkIn";
            }
            if (TransferInformation.TransferTypeName == "tt_SectionalTitle")
            {
                ViewBag.sectionaltitle = "ST";
            }
            ConveyancingAttorneyDetail conveyancingAttorneyDetail = db.ConveyancingAttorneyDetails.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList().FirstOrDefault();


            var CaseHistoryLog =
db.RCSApplicationHistoryLogs.Where(d => d.RefundApplicationId == refund.Id && d.IsActive == true && d.IsDeleted == false).Include(d => d.RCSApplicationStatus).Include(d => d.User).ToList();

            vm.ElectricityMeterList = db.ElectricityMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
            vm.WaterMeterList = db.WaterMeterInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();
            vm.PurchaserList = db.PurchaserInformations.Where(x => x.RCSApplicationStatusId == rcsapp.Id).ToList();

            vm.ConveyancingAttorneyDetail = conveyancingAttorneyDetail;
            vm.TransferInformation = TransferInformation;
            vm.SellerInformation = sellerInformation;
            vm.MunicipalAccountInformation = MunicipalAccountInformation;
            vm.WalkInApplicant = walkInApplicantDetails;
            vm.RCSApplicationHistoryLogs = CaseHistoryLog;



            return View(vm);
        }





        // GET: RefundApplications
        public ActionResult Index2()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var rCSApplicationStatus = db.RCSApplicationStatus.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.MunicipalAccountInformation).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).ToList();

                    if (TempData["Display"] != null)
                    {

                        if (TempData["ApplicationRefNo"] != null)
                        {

                            if (TempData["Display"].ToString() == "True")
                            {
                                ViewBag.Display = "True";
                                ViewBag.MessageTitle3 = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.Rcs_assessment).FirstOrDefault().Description + TempData["ApplicationRefNo"].ToString();
                            }
                            else
                            {
                                ViewBag.MessageTitle3 = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.SuccessMessage).FirstOrDefault().Name + TempData["ApplicationRefNo"].ToString();
                            }


                            //ViewBag.MessageBody3 = TempData["ApplicationRefNo"].ToString();

                        }
                        if (TempData["MessageBody"] != null)
                        {



                            ViewBag.MessageBody3 = TempData["MessageBody"].ToString();

                        }



                    }
                    else
                    {
                        if (TempData["ApplicationRefNo"] != null)
                        {
                            ViewBag.MessageBody = TempData["ApplicationRefNo"].ToString();
                            ViewBag.MessageTitle = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.SuccessMessage).FirstOrDefault().Name;
                        }
                        if (TempData["ReceiptValues"] != null)
                        {
                            ViewBag.Value = TempData["ReceiptValues"];
                        }
                    }

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    //int num = 5;
                    foreach (var item in rCSApplicationStatus)
                    {
                        //item.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = num })));
                        //item.Data= Convert.ToString(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id }));
                        ////item.Data= new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));
                        //item.DataList = new List<string>();
                        //item.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("referenceId={0}??customerId={1}??referenceTypeId={2}??applicationId={3}??agentId={4}??returnUrl={5}??rcsappId={6}??ratesRebateId={7}??incentivePolicyId={8}??errorList={8}", Customer.Id, Customer.Id, referenceType.Id, application.Id, application.Id, "sds", item.Id,1,1,""))));
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));

                        //return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));

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


        [DecryptParameter]
        public ActionResult Index3(int RcsApplicationId)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                 
                    var refundApplications = db.RefundApplications.Where(x => x.IsDeleted == false).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.RCSApplicationStatus).Include(r => r.Status).Where(x => x.Id == RcsApplicationId).FirstOrDefault();


                

         

                  
                    Session["ApplicationRefNo"] = refundApplications.ApplicationReferenceNumber;
                    Session["MessageBody"] = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RefundApplicationSuccessMessage).FirstOrDefault().Body + refundApplications.ApplicationReferenceNumber;


                    return RedirectToAction("Index");
                  
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult Index()
        {
            Initialise();

            var refundApplications = db.RefundApplications.Where(x=>x.IsDeleted==false).Include(r => r.Clerk).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.RCSApplicationStatus).Include(r => r.Status).Where(x => x.CustomerId == Customer.Id).ToList();
            if (Session["ApplicationRefNo"] != null)
            {

              
               
                    ViewBag.MessageTitle3 = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RCSSuccessMessage).FirstOrDefault().Body + Session["ApplicationRefNo"].ToString();
              


                Session["Display"] = null;
                Session["ApplicationRefNo"] = null;
            }
            if (Session["MessageBody"] != null)
            {



                ViewBag.MessageBody3 = Session["MessageBody"].ToString();
                Session["MessageBody"] = null;
            }
            foreach (var item in refundApplications)
            {
                //item.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = num })));
                //item.Data= Convert.ToString(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id }));
                ////item.Data= new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));
                //item.DataList = new List<string>();
                //item.DataList.Add(HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("referenceId={0}??customerId={1}??referenceTypeId={2}??applicationId={3}??agentId={4}??returnUrl={5}??rcsappId={6}??ratesRebateId={7}??incentivePolicyId={8}??errorList={8}", Customer.Id, Customer.Id, referenceType.Id, application.Id, application.Id, "sds", item.Id,1,1,""))));
                item.Data = SecureActionLinkExtension.Encrypt(string.Format("refundAppId={0}", item.Id));

                //return RedirectToAction("Index", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", rcsappId = item.Id })));

            }
            return View(refundApplications);
        }

        public ActionResult RefundRequest(int id)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    var refundApplication = db.RCSApplicationStatus.Where(x=>x.IsDeleted==false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.Status).Include(r => r.TransferInformation).Where(x => x.CustomerId == Customer.Id).Where(x => x.Id == id).FirstOrDefault();

                    var referenceType = db.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSRefund).FirstOrDefault();
                    var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    int num = 5;

                    return RedirectToAction("UploadRefundDocs", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = Customer.Id, customerId = Customer.Id, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, returnUrl = "sds", refundappId = id })));



                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }

        // GET: RefundApplications/Details/5
   

        // GET: RefundApplications/Create
        public ActionResult Create()
        {
            ViewBag.ClerkId = new SelectList(db.Customers, "Id", "IdentificationNumber");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            return View();
        }

        // POST: RefundApplications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ApplicationReferenceNumber,StatusId,CustomerId,RCSApplicationStatusId,ClerkId,RefundAmount,CollectionDate,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RefundApplication refundApplication)
        {
            if (ModelState.IsValid)
            {
                Initialise();
                var userID = Customer.Id;
                var Keys = db.Status;
                //var refundRatesNUm = (refundApplication.ApplicationReferenceNumber).Substring(0, 10);
                var RCSApplication = db.RCSApplicationStatus.Where(x=>x.IsDeleted==false).Include(x=>x.Status).Include(x=>x.TransferInformation).Where(x => x.CustomerId== userID && x.TransferInformation.RatesNumber == refundApplication.ApplicationReferenceNumber).OrderByDescending(x => x.Id).FirstOrDefault();
                //db.RefundApplications.Add(refundApplication);
                //db.SaveChanges();
                int PickUpStatus = Keys.Where(x => x.Key == StatusKeys.RCSApplicationCompleted).FirstOrDefault().Id;
                if (RCSApplication != null && RCSApplication.StatusId == PickUpStatus)
                {

                    var RefundApplicationExists = db.RefundApplications.Where(x => x.RCSApplicationStatusId == RCSApplication.Id && x.IsDeleted == false).FirstOrDefault();
                    //if there are no exisiting refund applications add one, else show error message
                    if(RefundApplicationExists ==null)
                    {

                        int limiter = 0;
                        //var AppSettings = db.AppSettings.ToList();
                        AppSetting query = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequence);
                        var SeqLimit = db.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.RCSRefundSequenceLimit);
                        var BatchCounter = query.Value;
                        limiter = Convert.ToInt16(SeqLimit.Value);
                        if (query.ModifiedDateTime.Value.Date == DateTime.Now.Date)
                        {
                            var lastRef = query.Value;
                            BatchCounter = lastRef.ToString();
                            int nextSeq = Convert.ToInt16(query.Value) + 1;
                            string nextVal = nextSeq.ToString().PadLeft(limiter, '0');
                            query.Value = nextVal;
                            db.Entry(query).State = EntityState.Modified;
                            db.SaveChanges();
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
                            db.Entry(query).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        //if (RCSApplication.TransferInformation != null)
                        //{
                            var refs = RCSApplication.TransferInformation.RatesNumber;
                            var RefNum = refs + "RCSREFUND" + DateTime.Now.ToString("ddMMyyyy") + BatchCounter;
                         
                        DepartmentsApprovalsController dac = new DepartmentsApprovalsController();
                        //var Ref = dac.RefNum();
                        RefundApplication rf = new RefundApplication
                        {
                            RCSApplicationStatusId = RCSApplication.Id,
                            StatusId = Keys.Where(x => x.Key == StatusKeys.RefundAwaitingUpload).FirstOrDefault().Id,
                            CustomerId = userID,
                            ApplicationReferenceNumber = RefNum,
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            CreatedDateTime = DateTime.Now,
                            ModifiedDateTime = DateTime.Now

                        };
                        db.RefundApplications.Add(rf);
                        db.SaveChanges();
                        var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSRefund));
                        var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                        var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundConevayncerCapturesNewApplication).Description.ToString();
                        DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                        var Result = DA.ActivityTrackerAuditRefunds(rf.Id, RCSApplication.Id, ActivityTrackerMessage, Customer.Id);

                      var getcustomer = db.Customers.Where(x => x.Id == userID).FirstOrDefault();
                        Email SendMail = new Email();
                        string attorneyemail = getcustomer.EmailAddress;
                        string attorneyname = getcustomer.FullName;
                        string emailbody = "Your RCS Refund Application has been captured succesfully your Reference Number is : " + rf.ApplicationReferenceNumber;
                        var ActivityTrackerEmail = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RefundEmailNewApplication).Description.ToString() + " " + emailbody;

                        SendMail.GenerateRefundEmail(rf.Id, RCSApplication.Id, Customer.Id, ActivityTrackerEmail, attorneyemail, "RCS- New Online Application Submission",
                                      emailbody, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, attorneyname);
                        return RedirectToAction("Refund", "Document", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { referenceId = RCSApplication.CustomerId, customerId = RCSApplication.CustomerId, referenceTypeId = referenceType.Id, applicationId = application.Id, agentId = application.Id, rcsappId = rf.Id })));

                        //var app = db.AppSettings.Where(x => x.Key == AppSettingKeys.RCSSequence).FirstOrDefault();
                        //app.Value = "0002";
                        //db.Entry(app).State = EntityState.Modified;
                        //   db.SaveChanges();



                    }
                    else
                    {
                        var TitleBody = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.DuplicateRefundApplication).FirstOrDefault();
                        ViewBag.MessageTitle = TitleBody.Title; //No Applications Found 
                        ViewBag.Message = TitleBody.Body; // Unable to find any applications linked to the entered municipal account number.
                        return View();
                       
                        //TempData["MessageBody"] = "Please upload a reciept of assessment fee payment for application with reference: " + rCSApplicationStatus.ApplicationReferenceNumber + ", This can be done by navigating to your side nav bar, clicking on RCS Applications and then Inbox.";

                    }
               
                }
                else
                {
                  

                    if (RCSApplication == null)
                    {
                        var TitleBody = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RCSApplicationNotFound).FirstOrDefault();
                        ViewBag.MessageTitle = TitleBody.Title;
                        ViewBag.Message = TitleBody.Body;
                        return View();
                    }
                    else if (RCSApplication.StatusId != PickUpStatus)
                    {
                        var TitleBody = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.RCSApplicationNotComplete).FirstOrDefault();
                        ViewBag.MessageTitle = TitleBody.Title; //RCS Application Not Completed  
                        ViewBag.Message = TitleBody.Body; // Please note a refund application can only be started once the RCS Application Linked to the municipal account number has been completed.
                        return View();
                    }


                }





                return RedirectToAction("Index");
            }

            ViewBag.ClerkId = new SelectList(db.Customers, "Id", "IdentificationNumber", refundApplication.ClerkId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", refundApplication.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", refundApplication.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", refundApplication.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", refundApplication.RCSApplicationStatusId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", refundApplication.StatusId);
            return View(refundApplication);
        }

        // GET: RefundApplications/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RefundApplication refundApplication = db.RefundApplications.Find(id);
            if (refundApplication == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClerkId = new SelectList(db.Customers, "Id", "IdentificationNumber", refundApplication.ClerkId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", refundApplication.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", refundApplication.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", refundApplication.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", refundApplication.RCSApplicationStatusId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", refundApplication.StatusId);
            return View(refundApplication);
        }

        // POST: RefundApplications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ApplicationReferenceNumber,StatusId,CustomerId,RCSApplicationStatusId,ClerkId,RefundAmount,CollectionDate,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RefundApplication refundApplication)
        {
            if (ModelState.IsValid)
            {
                db.Entry(refundApplication).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClerkId = new SelectList(db.Customers, "Id", "IdentificationNumber", refundApplication.ClerkId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", refundApplication.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", refundApplication.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", refundApplication.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", refundApplication.RCSApplicationStatusId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", refundApplication.StatusId);
            return View(refundApplication);
        }

        // GET: RefundApplications/Delete/5


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
