using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Helpers.UnitEngine.Abstract;
using C8.eServices.Mvc.Helpers.UnitEngine.Concrete;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;

namespace C8.eServices.Mvc.Controllers
{
    public class RiskAssessmentOutcomesController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        private readonly eServicesDbContext dbContext;
        private readonly IUnitAllocation unitAllocationService;

        public RiskAssessmentOutcomesController()
        {
            dbContext = new eServicesDbContext();   
            unitAllocationService = new UnitAllocation(db);
        }

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

        }
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var rCSApplicationStatus = db.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).Include(r => r.Status).Where(x => x.StatusId == db.Status.FirstOrDefault(o => o.Key == StatusKeys.AwaitingHoDResponse).Id).ToList();
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

        public ActionResult Details(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskAssessmentOutcome riskAssessmentOutcome = db.RiskAssessmentOutcomes.Find(id);
            if (riskAssessmentOutcome == null)
            {
                return HttpNotFound();
            }
            return View(riskAssessmentOutcome);
        }

        [DecryptParameter]
        public ActionResult ConductAssessment(int rcsAppId)  
        {
            var context = new eServicesDbContext();
            try
            {
                Initialise();
                PropertyLeaseApplication PLA = context.PropertyLeaseApplications
                    .Include(x => x.HumanEHCOptions)
                    .Include(x => x.PreferredComplexArea)
                    .Include(x => x.PreferredComplexArea2)
                    .Include(x => x.Status)
                    .Include(x => x.PurchaserType)
                    .Include(x => x.Customer)
                    .Include(x => x.SystemUser)
                    .FirstOrDefault(x => x.Id == rcsAppId);
                ViewBag.PreferredComplexArea2 = context.PreferredComplexAreas.Find(PLA.PreferredComplexArea2Id).Name;
                Customer customer = context.Customers.FirstOrDefault(x => x.Id == PLA.CustomerId);
                RiskAssessmentOutcome rao = context.RiskAssessmentOutcomes.OrderByDescending(a => a.Id).FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);
                if (rao == null) rao = new RiskAssessmentOutcome();
                rao.FirstName = SystemUser.FirstName;
                rao.LastName = SystemUser.LastName;

                Models.Application application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                Models.ReferenceType referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");

                DocumentsViewModel dvm = new DocumentsViewModel();
                MatchingHelper.DocumentRiskAssessmentOutcome(dvm, context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsAppId, true);

                RatesRebateProperty ratesRebateProperty = new RatesRebateProperty();
                IncentivePolicyProperty incentivePolicyProperty = new IncentivePolicyProperty();

                CaptureViewModel cmv = new CaptureViewModel
                {
                    DocumentsViewModel = dvm,
                    PropertyLeaseApplication = PLA,
                    RiskAssessmentOutcome = rao,
                    vaIDno = PLA.Id.ToString()
                };

                MatchedUnits matchedUnits = context.MatchedUnits
                   .OrderByDescending(a => a.Id)
                   .Include(c => c.ApplicationAllocatedProperty)
                   .FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);

                if (matchedUnits != null) 
                    cmv.ApplicationAllocatedProperties = matchedUnits.ApplicationAllocatedProperty;

                IEnumerable<Int32> systemIdentityUsers = IdentityManager.FindUsersInRole("Letting Officer").Select(a => a.SystemUserId);
                IEnumerable<Customer> customerObjects = context.Customers.Where(a => systemIdentityUsers.Contains((Int32)a.SystemUserId) && (Int32)a.SystemUserId != SystemUser.Id).ToList();

                ViewBag.LettingOfficer = new SelectList(customerObjects, "Id", "FullName");

                ViewBag.DateStampConduct = DateTime.Now;
                ViewBag.ConductPrpertyId = PLA.Id;
                ViewBag.PreferredComplex = new SelectList(context.PreferredComplexAreas.OrderBy(a=>a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex")), "Id", "Name");
                ViewBag.humanEHCOptions = new SelectList(context.humanEHCOptions.OrderBy(a => a.Name), "Id", "Name");
                ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.ReallocateApplication || x.Key == RCSActionTypeKeys.RejectedITC || x.Key == RCSActionTypeKeys.RejectedDocuments || x.Key == RCSActionTypeKeys.RejectedUnaffordability).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.Outcome = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved  || x.Key == RCSActionTypeKeys.ReallocateApplication || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Id", "Name");
                ViewBag.date = DateTime.Now.Date;
                ViewBag.CreatedBySystemUserId = new SelectList(context.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(context.SystemUsers, "Id", "FirstName");
                return View(cmv);
            }
            catch (Exception Io)
            {

                throw;
            }
        }

        #region Revenue Manager Inbox


        public ActionResult InboxRM()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise(); // Sets up User context

                    // 1. Get the Key for 'Awaiting Revenue Manager' (ID 272)
                    // Ensure this string matches your DB Key for "Awaiting Revenue Manager Review"
                    var statusKey = "s_in_awaiting_revenue_managers_review";
                    var status = context.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyPropertyVerified);

                    if (status == null)
                    {
                        // Fallback / Log error if key is missing
                        return View(new List<PropertyLeaseApplication>());
                    }

                    // 2. Load Applications
                    var applications = context.PropertyLeaseApplications
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.HumanEHCOptions)     // Typology
                        .Include(r => r.PreferredComplexArea) // Complex
                        .Include(r => r.Status)
                        .Where(x => x.StatusId == status.Id && x.IsDeleted == false)
                        .OrderByDescending(x => x.CreatedDateTime)
                        .ToList();

                    // 3. ENCRYPT LINKS (The critical step you asked for)
                    foreach (var item in applications)
                    {
                        string rawString = string.Format("rcsAppId={0}", item.Id);
                        // Replace this line:
                        // item.Data = AesCrypto.Encrypt(rawString);

                        // With the following code to fix CS0120:
                        AesCrypto aesCrypto = new AesCrypto();
                        item.Data = aesCrypto.Encrypt(rawString);
                       
                    }



                    // 4. Handle Session Messages (Success/Fail notifications)
                    if (Session["ConductRiskAssessmentSession"] != null)
                    {
                        ViewBag.ConductRiskAssessmentSession = Session["ConductRiskAssessmentSession"].ToString();
                        Session["ConductRiskAssessmentSession"] = null;
                    }

                    return View(applications);
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        #endregion


        #region Revenue Manager Flow


        [DecryptParameter]
        public ActionResult RiskAssessmentRM(int rcsAppId)
        {
            var context = new eServicesDbContext();
            try
            {
                Initialise();

                // 1. Load Application
                PropertyLeaseApplication PLA = context.PropertyLeaseApplications
                    .Include(x => x.HumanEHCOptions)
                    .Include(x => x.PreferredComplexArea)
                    .Include(x => x.PreferredComplexArea2)
                    .Include(x => x.Status)
                    .Include(x => x.PurchaserType)
                    .Include(x => x.Customer)
                    .Include(x => x.SystemUser)
                    .FirstOrDefault(x => x.Id == rcsAppId);

                if (PLA == null) return HttpNotFound();

                // 2. Load Assessment Data
                RiskAssessmentOutcome rao = context.RiskAssessmentOutcomes
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);

                if (rao == null) rao = new RiskAssessmentOutcome();

                // --- 3. LOAD DOCUMENTS (READ-ONLY) ---
                // We fetch the configuration keys first
                var appKey = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var refType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                DocumentsViewModel dvm = new DocumentsViewModel();

                // Pass 'false' as the last parameter to indicate Read-Only / No Upload
                MatchingHelper.DocumentRiskAssessmentOutcome(dvm, context, PLA.CustomerId, PLA.CustomerId, (int)refType.Id, (int)appKey.Id, "", rcsAppId, false);

                // 4. Setup ViewModel
                CaptureViewModel cmv = new CaptureViewModel
                {
                    PropertyLeaseApplication = PLA,
                    RiskAssessmentOutcome = rao,
                    DocumentsViewModel = dvm, // <--- Attach documents here
                    vaIDno = PLA.Id.ToString()
                };

                // 5. ViewBags
                ViewBag.ConductPrpertyId = PLA.Id;
                ViewBag.ApprovalStatus = new SelectList(new[]
                {
            new { Value = "Supported", Text = "Supported" },
            new { Value = "Not Supported", Text = "Not Supported" },
        }, "Value", "Text");

                return View(cmv);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return RedirectToAction("InboxRM");
            }
            finally
            {
                context.Dispose();
            }
        }

        [DecryptParameter]
        public ActionResult RiskAssessmentRMOLD(int rcsAppId)
        {
            var context = new eServicesDbContext();
            try
            {
                Initialise();

                // 1. Load Application with ALL required relationships (Mirrors ConductAssessment)
                PropertyLeaseApplication PLA = context.PropertyLeaseApplications
                    .Include(x => x.HumanEHCOptions)        // Required for "Unit Typology" display
                    .Include(x => x.PreferredComplexArea)   // Required for "Complex" display
                    .Include(x => x.PreferredComplexArea2)  // Good practice to keep, prevents nulls if referenced
                    .Include(x => x.Status)                 // Required for Header Table
                    .Include(x => x.PurchaserType)          // CRITICAL: Required for "Application Type" column
                    .Include(x => x.Customer)               // Required for "Applicant Name"
                    .Include(x => x.SystemUser)
                    .FirstOrDefault(x => x.Id == rcsAppId);

                if (PLA == null) return HttpNotFound();

                // 2. Load the Existing Assessment (The CSO's work)
                RiskAssessmentOutcome rao = context.RiskAssessmentOutcomes
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);

                // If no assessment exists (which shouldn't happen in this flow), provide a dummy to prevent crash
                if (rao == null) rao = new RiskAssessmentOutcome();

                // 3. Prepare ViewModel
                CaptureViewModel cmv = new CaptureViewModel
                {
                    PropertyLeaseApplication = PLA,
                    RiskAssessmentOutcome = rao,
                    vaIDno = PLA.Id.ToString()
                };

                // 4. CRITICAL VIEW BAGS (Required for the View to function)

                // This allows the "View Application" button to generate the encrypted link
                ViewBag.ConductPrpertyId = PLA.Id;

                // Populate the Dropdown specifically for Revenue Manager actions
                // We do NOT use the full list from the DB, we strictly want Supported/Not Supported
                ViewBag.ApprovalStatus = new SelectList(new[]
                {
            new { Value = "Supported", Text = "Supported" },
            new { Value = "Not Supported", Text = "Not Supported" },
        }, "Value", "Text");

                return View(cmv);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return RedirectToAction("InboxRM");
            }
            finally
            {
                context.Dispose();
            }
        }


        #region CEO Flow

        // 1. CEO INBOX (Loads ID 55)
        public ActionResult InboxCEO()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();

                    // Load applications waiting for CEO (Status 55)
                    var status = context.Status.FirstOrDefault(x => x.Key == "s_incentive_policy_application_approved");

                    if (status == null) return View(new List<PropertyLeaseApplication>());

                    var applications = context.PropertyLeaseApplications
                        .Include(r => r.CreatedBySystemUser)
                        .Include(r => r.Customer)
                        .Include(r => r.HumanEHCOptions)
                        .Include(r => r.PreferredComplexArea)
                        .Include(r => r.Status)
                        .Where(x => x.StatusId == status.Id && x.IsDeleted == false)
                        .OrderByDescending(x => x.CreatedDateTime)
                        .ToList();

                    // Encrypt Links
                    var crypto = new C8.eServices.Mvc.Helpers.AesCrypto();
                    foreach (var item in applications)
                    {
                        string rawString = string.Format("rcsAppId={0}", item.Id);
                        item.Data = crypto.Encrypt(rawString);
                    }

                    if (Session["ConductRiskAssessmentSession"] != null)
                    {
                        ViewBag.ConductRiskAssessmentSession = Session["ConductRiskAssessmentSession"].ToString();
                        Session["ConductRiskAssessmentSession"] = null;
                    }

                    return View(applications);
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        // 2. CEO ASSESSMENT VIEW (GET)
        [DecryptParameter]
     
        public ActionResult RiskAssessmentCEO(int rcsAppId)
        {
            var context = new eServicesDbContext();
            try
            {
                Initialise();

                // 1. Load Application
                var PLA = context.PropertyLeaseApplications
                    .Include(x => x.HumanEHCOptions)
                    .Include(x => x.PreferredComplexArea)
                    .Include(x => x.Status)
                    .Include(x => x.PurchaserType)
                    .Include(x => x.Customer)
                    .Include(x => x.SystemUser)
                    .FirstOrDefault(x => x.Id == rcsAppId);

                if (PLA == null) return HttpNotFound();

                // 2. Load Assessment
                var rao = context.RiskAssessmentOutcomes
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);

                if (rao == null) rao = new RiskAssessmentOutcome();

                // 3. LOAD DOCUMENTS (Fixes Null Reference)
                var application = context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));

                DocumentsViewModel dvm = new DocumentsViewModel();
                // Passing 'false' for Read-Only mode
                MatchingHelper.DocumentRiskAssessmentOutcome(dvm, context, PLA.CustomerId, PLA.CustomerId, (int)referenceType.Id, (int)application.Id, "", rcsAppId, false);

                var cmv = new CaptureViewModel
                {
                    DocumentsViewModel = dvm, // Pass docs to View
                    PropertyLeaseApplication = PLA,
                    RiskAssessmentOutcome = rao,
                    vaIDno = PLA.Id.ToString()
                };

                ViewBag.ConductPrpertyId = PLA.Id;
                ViewBag.ApprovalStatus = new SelectList(new[]
                {
            new { Value = "Approved", Text = "Approved" },
            new { Value = "Rejected", Text = "Rejected" },
        }, "Value", "Text");

                return View(cmv);
            }
            catch (Exception ex)
            {
                return RedirectToAction("InboxCEO");
            }
            finally { context.Dispose(); }
        }
        // 3. CEO SUBMISSION (POST)

        // ==========================================================================
        // PASTE THIS INTO: RiskAssessmentOutcomesController.cs
        // ==========================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
        public ActionResult RiskAssessmentCEO(int rcsAppId, CaptureViewModel capture, string ApprovalStatusddl)
        {
            try
            {
                Initialise(); // standard init if you have it
                var context = new eServicesDbContext();

                // -----------------------------------------------------------
                // 1. SAVE THE CEO DECISION & SIGNATURE
                // -----------------------------------------------------------
                var rao = context.RiskAssessmentOutcomes.FirstOrDefault(r => r.PropertyLeaseApplicationId == rcsAppId);
                if (rao != null)
                {
                    rao.CEO_OfficialNumber = capture.RiskAssessmentOutcome.CEO_OfficialNumber;
                    rao.CEO_Outcome = ApprovalStatusddl;
                    rao.CEO_DateStamp = DateTime.Now;
                    rao.CEO_SystemUserId = SystemUser.Id; // Ensure SystemUser is valid in your context

                    // Save Signature
                    // (Ensure your View passes this inside capture.RiskAssessmentOutcome.SignatureBlob)
                    if (!string.IsNullOrEmpty(capture.RiskAssessmentOutcome.SignatureBlob))
                    {
                        rao.SignatureBlob = capture.RiskAssessmentOutcome.SignatureBlob;
                    }

                    // Handle Conditional Comment (Clean up if approved)
                    if (ApprovalStatusddl == "Approved")
                    {
                        rao.CEO_Reason = capture.RiskAssessmentOutcome.CEO_Reason;
                    }
                    else
                    {
                        rao.CEO_Reason = capture.RiskAssessmentOutcome.CEO_Reason;
                    }

                    context.SaveChanges();
                }

                // -----------------------------------------------------------
                // 2. HANDLE STATUS TRANSITIONS & WAITING LIST
                // -----------------------------------------------------------
                if (ApprovalStatusddl == "Approved")
                {
                    // A. GET APPLICATION DETAILS (For Preferences)
                    var app = context.PropertyLeaseApplications.Find(rcsAppId);

                    if (app != null)
                    {
                        // B. ADD TO WAITING LIST (Using Private Method below)
                        // We need the Complex and Typology to queue them correctly
                        int complexId = app.PreferredComplexAreaId ?? 0; // Ensure your App model has this
                        int typologyId = app.HumanEHCOptionsId ?? 0; // Ensure your App model has this (or TypologyId)

                        AddToPropertyWaitingList(context, rcsAppId, complexId, typologyId);

                        // C. UPDATE STATUS -> "Awaiting Unit"
                        // Change "s_added_to_waiting_list" to whatever Key matches your DB Status
                        SetApplicationStatus(context, rcsAppId, "s_added_to_waiting_list");

                        Session["ConductRiskAssessmentSession"] = "Application Approved & Added to Waiting List.";
                    }
                }
                else
                {
                    // REJECTED
                    SetApplicationStatus(context, rcsAppId, "s_credit_score_rejected");

                    // Notify User
                    var emailContent = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected);
                    if (emailContent != null)
                    {
                        EmailHelper.CustomerEmailNotification(context, rcsAppId, emailContent.Id);
                    }

                    Session["ConductRiskAssessmentSession"] = "Application Rejected by CEO.";
                }

                // -----------------------------------------------------------
                // 3. FINISH TASK (Workflow)
                // -----------------------------------------------------------
                //var ceoRole = context.ResponsibilityTypes.FirstOrDefault(x => x.Key == "CEO");
                //if (ceoRole != null)
                //{
                //    MatchingHelper.RoundRobinMarkJobAsFinished(context, rcsAppId, null, ceoRole.Id, Customer.Id);
                //}

                return RedirectToAction("InboxCEO");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine(ex.Message);
                return RedirectToAction("InboxCEO");
            }
        }

        // ==========================================================================
        // PRIVATE METHODS (Paste these below the POST method above)
        // ==========================================================================

        /// <summary>
        /// Adds the applicant to the PropertyLeaseWaitingList table if not already present.
        /// </summary>
        private void AddToPropertyWaitingList(eServicesDbContext context, int appId, int complexId, int typologyId)
        {
            // 1. Check for duplicates (Status is not 'Matched' or 'Allocated')
            var exists = context.PropertyLeaseWaitingLists
                .FirstOrDefault(w => w.PropertyLeaseApplicationId == appId
                                  && w.QueueStatus != "Matched");

            if (exists == null)
            {
                var entry = new PropertyLeaseWaitingList
                {
                    PropertyLeaseApplicationId = appId,
                    PreferredComplexId = complexId,
                    PreferredTypologyId = typologyId,
                    DateAdded = DateTime.Now, // This timestamp secures their place in line (FIFO)
                    QueueStatus = "Waiting",  // Initial Status
                    IsActive = true,
                    IsDeleted = false
                };

                context.PropertyLeaseWaitingLists.Add(entry);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Helper to safely update the Main Application Status
        /// </summary>
        private void SetApplicationStatus(eServicesDbContext context, int appId, string statusKey)
        {
            var status = context.Status.FirstOrDefault(s => s.Key == statusKey);
            var app = context.PropertyLeaseApplications.Find(appId);

            if (status != null && app != null)
            {
                app.StatusId = status.Id;
                context.SaveChanges();
            }
        }



        // ==========================================================================
        // WAITING LIST DASHBOARD (Updated with Correct Tables)
        // ==========================================================================
        [HttpGet]
        public ActionResult WaitingListIndex(int? complexId, int? typologyId)
        {
            var db = new eServicesDbContext();

            // 1. Base Query: Get everyone with status "Waiting"
            var query = db.PropertyLeaseWaitingLists
                .Include("PropertyLeaseApplication")
                .Include("PreferredComplexArea")
                .Include("HumanEHCOption") // Must match the property name above
                .Where(w => w.QueueStatus == "Waiting");
            // 2. Apply Filters (if user selected them)
            if (complexId.HasValue)
            {
                query = query.Where(w => w.PreferredComplexId == complexId.Value);
            }

            if (typologyId.HasValue)
            {
                query = query.Where(w => w.PreferredTypologyId == typologyId.Value);
            }

            // 3. SORTING: Critical FIFO Logic (Oldest DateAdded = First)
            var model = query.OrderBy(w => w.DateAdded).ToList();

            // ----------------------------------------------------------------------
            // 4. POPULATE DROPDOWNS (Updated with your specific queries)
            // ----------------------------------------------------------------------

            // Complex: Filter out "ekurhuleni_complex" and Order By Name
            var complexQuery = db.PreferredComplexAreas
                                 .Where(a => !a.Key.Equals("ekurhuleni_complex"))
                                 .OrderBy(a => a.Name);

            ViewBag.Complexes = new SelectList(complexQuery, "Id", "Name", complexId);

            // Typology: Order By Name
            var typologyQuery = db.humanEHCOptions
                                  .OrderBy(a => a.Name);

            ViewBag.Typologies = new SelectList(typologyQuery, "Id", "Name", typologyId);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
      
        public ActionResult RiskAssessmentCEOOOLDD(int rcsAppId, CaptureViewModel capture, string ApprovalStatusddl)
        {
            try
            {
                Initialise();
                var context = new eServicesDbContext();

                // A. Save CEO Decision
                var rao = context.RiskAssessmentOutcomes.FirstOrDefault(r => r.PropertyLeaseApplicationId == rcsAppId);
                if (rao != null)
                {
                    rao.CEO_OfficialNumber = capture.RiskAssessmentOutcome.CEO_OfficialNumber;
                    rao.CEO_Reason = capture.RiskAssessmentOutcome.CEO_Reason;
                    rao.CEO_Outcome = ApprovalStatusddl;

                    // --- NEW: Save the Signature ---
                    // Ensure your CaptureViewModel -> RiskAssessmentOutcome has the SignatureBlob property
                    rao.SignatureBlob = capture.RiskAssessmentOutcome.SignatureBlob;

                    rao.CEO_DateStamp = DateTime.Now; // Defaults to Current System Date
                    rao.CEO_SystemUserId = SystemUser.Id;
                    context.SaveChanges();
                }

                // B. Handle Status Transition
                if (ApprovalStatusddl == "Approved")
                {
                    // SUCCESS: Add to Waiting List Status (ID 138: Available Unit Matched / Awaiting Offer)
                    var successStatus = context.Status.FirstOrDefault(x => x.Key == "s_rcs_awaited"); // Verify Key

                    if (successStatus != null)
                        MatchingHelper.ChangeApplicationStatus(context, successStatus.Id, rcsAppId);

                    // TODO: Insert into actual Waiting List Table here if separate from Status

                    Session["ConductRiskAssessmentSession"] = "Application Approved & Added to Waiting List.";
                }
                else
                {
                    // REJECTED
                    var status = context.Status.FirstOrDefault(x => x.Key == "s_credit_score_rejected");
                    MatchingHelper.ChangeApplicationStatus(context, status.Id, rcsAppId);

                    // Notify
                    int emailId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected).Id;
                    EmailHelper.CustomerEmailNotification(context, rcsAppId, emailId);

                    Session["ConductRiskAssessmentSession"] = "Application Rejected by CEO.";
                }

                // Finish Task
                var ceoRole = context.ResponsibilityTypes.FirstOrDefault(x => x.Key == "CEO");
                MatchingHelper.RoundRobinMarkJobAsFinished(context, rcsAppId, null, ceoRole.Id, Customer.Id);

                return RedirectToAction("InboxCEO");
            }
            catch (Exception ex)
            {
                // Log error here if possible
                return RedirectToAction("InboxCEO");
            }
        }
        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
        public ActionResult RiskAssessmentRM(int rcsAppId, CaptureViewModel capture, string ApprovalStatusddl)
        {
            try
            {
                Initialise();
                var context = new eServicesDbContext();

                // 1. Load Existing Record
                var rao = context.RiskAssessmentOutcomes
                                 .FirstOrDefault(r => r.PropertyLeaseApplicationId == rcsAppId);

                if (rao != null)
                {
                    // 2. Save RM Specific Fields
                    rao.RM_OfficialNumber = capture.RiskAssessmentOutcome.RM_OfficialNumber;
                    rao.RM_Reason = capture.RiskAssessmentOutcome.RM_Reason;
                    rao.RM_Outcome = ApprovalStatusddl; // Supported / Not Supported
                    rao.RM_DateStamp = DateTime.Now;
                    rao.RM_SystemUserId = SystemUser.Id;

                    context.SaveChanges();
                }

                // 3. Handle Status Transition
                if (ApprovalStatusddl == "Supported")
                {
                    // Move to CEO
                    var status = context.Status.FirstOrDefault(x => x.Key == "s_incentive_policy_application_approved"); // Ensure Key matches DB
                    MatchingHelper.ChangeApplicationStatus(context, status.Id, rcsAppId);

                    // Assign to CEO Role (Round Robin)
                    //var ceoRole = context.ResponsibilityTypes.FirstOrDefault(x => x.Key == "CEO");
                    //PropertyLeaseApplicationController rr = new PropertyLeaseApplicationController();
                    //rr.BackOfficeNotification(rcsAppId, 0, ceoRole.Name);

                    Session["ConductRiskAssessmentSession"] = "Supported. Forwarded to CEO.";
                }
                else
                {
                    // Reject
                    var status = context.Status.FirstOrDefault(x => x.Key == StatusKeys.CreditScoreRejected);
                    MatchingHelper.ChangeApplicationStatus(context, status.Id, rcsAppId);

                    // Send Rejection Email
                    int emailId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected).Id;
                    EmailHelper.CustomerEmailNotification(context, rcsAppId, emailId);

                    Session["ConductRiskAssessmentSession"] = "Application Rejected by Revenue Manager.";
                }

                // Mark RM Task as Done
                //var responsibility = context.ResponsibilityTypes.FirstOrDefault(x => x.Key == "RevenueManager"); // Update Key if needed
                //MatchingHelper.RoundRobinMarkJobAsFinished(context, rcsAppId, null, responsibility.Id, Customer.Id);

                return RedirectToAction("InboxRM"); // Go back to Task List
            }
            catch (Exception ex)
            {
                // Log Error
                return RedirectToAction("Index");
            }
        }
        #endregion

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
        public ActionResult ConductAssessment(Int32 rcsAppId, Int32? LettingOfficer, CaptureViewModel capture, String ApprovalStatusddl)
        {

            try
            {
                Initialise();
                Int32 emailboodyId = 0;
                String ActivityTrackerMessage = String.Empty;
                PropertyLeaseApplication property = dbContext.PropertyLeaseApplications.Find(rcsAppId);
                ResponsibilityType ResponsibilityTypeId = dbContext.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RiskAssessment);
                //RiskAssessmentOutcome riskAssessmentOutcome = unitAllocationService.SaveRisk(capture, ApprovalStatusddl, rcsAppId, StatusKeys.Approved);
                RiskAssessmentOutcome rao = dbContext.RiskAssessmentOutcomes
                                             .OrderByDescending(r => r.Id)
                                             .FirstOrDefault(r => r.PropertyLeaseApplicationId == rcsAppId);
                if (rao == null)
                {
                    rao = new RiskAssessmentOutcome();
                    rao.PropertyLeaseApplicationId = rcsAppId;
                    rao.CreatedDateTime = DateTime.Now;
                    dbContext.RiskAssessmentOutcomes.Add(rao);
                }
                else
                {
                    dbContext.Entry(rao).State = EntityState.Modified;
                }

                rao.FirstName = SystemUser.FirstName;
                rao.LastName = SystemUser.LastName;
                rao.OfficialNumber = capture.RiskAssessmentOutcome.OfficialNumber;
                rao.Reason = capture.RiskAssessmentOutcome.Reason;
                rao.StatusId = (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyPropertyVerified)?.Id;
                rao.DateStamp = DateTime.Now;

                // Map "Approved" dropdown to "Recommended" text
                rao.Outcome = (ApprovalStatusddl == RCSActionTypeKeys.Approved) ? "Recommended" : "Not Recommended";

                // 3. SAVE TO DB
                dbContext.SaveChanges();

                switch (ApprovalStatusddl)
                {
                    case RCSActionTypeKeys.Approved:

                        property.PreferredComplexAreaId = capture.PropertyLeaseApplication.PreferredComplexAreaId;
                        property.HumanEHCOptionsId = capture.PropertyLeaseApplication.HumanEHCOptionsId;

                        // Save these changes to the PropertyLeaseApplication Table
                        dbContext.Entry(property).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.IncentivePolicyPropertyVerified)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitOfferToApplicant).Description.ToString();
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment approved for application reference ,{property.ApplicationReferenceNumber}");
                        break;
                    case RCSActionTypeKeys.Rejected:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.CreditScoreRejected)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}");
                        break;
                    case RCSActionTypeKeys.RejectedUnaffordability:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.RiskAssesmentRejectedUnaffordability)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        ActivityTrackerMessage = "Application rejected due to unaffordability, Reason: " + capture.RiskAssessmentOutcome.Reason;
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectDocuments).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}"+ " due to unaffordability,  reason: " + capture.RiskAssessmentOutcome.Reason);
                        break;
                    case RCSActionTypeKeys.RejectedITC:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.RiskAssesmentRejectedITCCheck)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        ActivityTrackerMessage = "Application rejected due to ITC Check, Reason: " + capture.RiskAssessmentOutcome.Reason;
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectITC).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}" + " due to ITC Check,  reason: " + capture.RiskAssessmentOutcome.Reason);
                        break;

                    case RCSActionTypeKeys.RejectedDocuments:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.ReuploadApplicationDocs)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        ActivityTrackerMessage = "Application rejected due to issue with uploaded documents, Reason: " + capture.RiskAssessmentOutcome.Reason;
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectUnaffordability).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}" + " due to documents,  reason: " + capture.RiskAssessmentOutcome.Reason);
                        break;


                    case RCSActionTypeKeys.ReallocateApplication:
                        PropertyLeaseApplicationController roundrobin = new PropertyLeaseApplicationController();
                        Status Status = dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ReallocatedForRiskAssessment).Description;

                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ReallocatedForAssessment).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);

                        Customer reallocateTo = dbContext.Customers.Find(LettingOfficer);
                        RoundRobinQueue roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.PropertyLeaseApplicationId = rcsAppId;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.StatusId = Status.Id;
                        roundRobinQueue.ClerkId = reallocateTo.Id;
                        unitAllocationService.Save(roundRobinQueue);
                        roundrobin.BackOfficeNotification(rcsAppId, reallocateTo.Id, ResponsibilityTypeId.Name);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment reallocated for application reference ,{property.ApplicationReferenceNumber}");
                        break;
                }


                MatchingHelper.RoundRobinMarkJobAsFinished(dbContext, rcsAppId, null, ResponsibilityTypeId.Id, Customer.Id);
                MatchingHelper.ActivityTrackerAudit(dbContext, rcsAppId, ActivityTrackerMessage, Customer.Id);
                return RedirectToAction("ApplicantRiskAssessment", "PropertyLeaseApplication");
            }
            catch(Exception IO)
            {
                return RedirectToAction("ApplicantRiskAssessment", "PropertyLeaseApplication");
            }
        }
        public ActionResult ConductAssessmentOLD(Int32 rcsAppId, Int32? LettingOfficer, CaptureViewModel capture, String ApprovalStatusddl)
        {

            try
            {
                Initialise();
                Int32 emailboodyId = 0;
                String ActivityTrackerMessage = String.Empty;
                PropertyLeaseApplication property = dbContext.PropertyLeaseApplications.Find(rcsAppId);
                ResponsibilityType ResponsibilityTypeId = dbContext.ResponsibilityTypes.FirstOrDefault(x => x.Key == ResponsibilityTypeKeys.RiskAssessment);
                RiskAssessmentOutcome riskAssessmentOutcome = unitAllocationService.SaveRisk(capture, ApprovalStatusddl, rcsAppId, StatusKeys.Approved);

                switch (ApprovalStatusddl)
                {
                    case RCSActionTypeKeys.Approved:


                        ApplicationAllocatedProperty collection = capture.ApplicationAllocatedProperties;
                        ApplicationAllocatedProperty collection1 = dbContext.ApplicationAllocatedProperty.FirstOrDefault(a => a.SolarReference == collection.SolarReference && a.SpaceUnitNumber == collection.SpaceUnitNumber);

                        if (collection1 != null)
                        {
                            eServicesDbContext _context = new eServicesDbContext();
                            ModelState.AddModelError("Error", "The unit is unavailable - allocated to another tenant");

                            PropertyLeaseApplication PLA = _context.PropertyLeaseApplications
                           .Include(x => x.HumanEHCOptions)
                           .Include(x => x.PreferredComplexArea)
                           .Include(x => x.PreferredComplexArea2)
                           .Include(x => x.Status)
                           .Include(x => x.PurchaserType)
                           .Include(x => x.Customer)
                           .Include(x => x.SystemUser)
                           .FirstOrDefault(x => x.Id == rcsAppId);

                            Customer customer = _context.Customers.FirstOrDefault(x => x.Id == PLA.CustomerId);
                            RiskAssessmentOutcome rao = _context.RiskAssessmentOutcomes.OrderByDescending(a => a.Id).FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);
                            if (rao == null) rao = new RiskAssessmentOutcome();
                            rao.FirstName = SystemUser.FirstName;
                            rao.LastName = SystemUser.LastName;

                            Models.Application application = _context.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                            Models.ReferenceType referenceType = _context.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                            if (referenceType == null) throw new Exception("Invalid reference type.");

                            DocumentsViewModel dvm = new DocumentsViewModel();
                            MatchingHelper.DocumentRiskAssessmentOutcome(dvm, _context, customer.Id, customer.Id, (int)referenceType.Id, (int)application.Id, "", rcsAppId, true);

                            RatesRebateProperty ratesRebateProperty = new RatesRebateProperty();
                            IncentivePolicyProperty incentivePolicyProperty = new IncentivePolicyProperty();

                            capture.DocumentsViewModel = dvm;
                            capture.PropertyLeaseApplication = PLA;
                            capture.RiskAssessmentOutcome = rao;
                            capture.vaIDno = PLA.Id.ToString();


                            MatchedUnits matchedUnits = _context.MatchedUnits
                               .OrderByDescending(a => a.Id)
                               .Include(c => c.ApplicationAllocatedProperty)
                               .FirstOrDefault(a => a.PropertyLeaseApplicationId == rcsAppId);

                            if (matchedUnits != null)
                                capture.ApplicationAllocatedProperties = matchedUnits.ApplicationAllocatedProperty;


                            IEnumerable<Int32> systemIdentityUsers = IdentityManager.FindUsersInRole("Letting Officer").Select(a => a.SystemUserId);
                            IEnumerable<Customer> customerObjects = _context.Customers.Where(a => systemIdentityUsers.Contains((Int32)a.SystemUserId) && (Int32)a.SystemUserId != SystemUser.Id).ToList();

                            ViewBag.LettingOfficer = new SelectList(customerObjects, "Id", "FullName");
                            ViewBag.PreferredComplexArea2 = _context.PreferredComplexAreas.Find(PLA.PreferredComplexArea2Id).Name;
                            ViewBag.DateStampConduct = DateTime.Now;
                            ViewBag.ConductPrpertyId = rcsAppId;
                            ViewBag.PreferredComplex = new SelectList(_context.PreferredComplexAreas.OrderBy(a => a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex")), "Id", "Name");
                            ViewBag.humanEHCOptions = new SelectList(_context.PreferredComplexAreas.OrderBy(a => a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex")), "Id", "Name");
                            ViewBag.ApprovalStatus = new SelectList(_context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReallocateApplication).OrderBy(x => x.Name), "Key", "Name");
                            ViewBag.Outcome = new SelectList(_context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected || x.Key == RCSActionTypeKeys.ReallocateApplication).OrderBy(x => x.Name), "Id", "Name");
                            ViewBag.date = DateTime.Now.Date;
                            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName");
                            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName");


                            return View(capture);
                        }
                        collection.AllocatedByUserId = SystemUser.Id;
                        collection.PropertyLeaseApplicationId = rcsAppId;
                        collection.IsTaken = true;
                        unitAllocationService.Save(collection);
                        MatchingHelper.SaveUnitHistory(dbContext, rcsAppId, "Unit Allocated", collection.Id, SystemUser.Id);
                        MatchedUnits matchedUnit = new MatchedUnits
                        {
                            ApplicationAllocatedPropertyId = collection.Id,
                            PropertyLeaseApplicationId = rcsAppId,
                            IsAccepted = false,
                            RejectedProperty = false,
                        };
                        unitAllocationService.Save(matchedUnit);

                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.awaited)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.UnitOfferToApplicant).Description.ToString();
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment approved for application reference ,{property.ApplicationReferenceNumber}");
                        break;
                    case RCSActionTypeKeys.Rejected:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.CreditScoreRejected)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentRejected).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}");
                        break;
                    case RCSActionTypeKeys.RejectedUnaffordability:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.RiskAssesmentRejectedUnaffordability)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        ActivityTrackerMessage = "Application rejected due to unaffordability, Reason: " + capture.RiskAssessmentOutcome.Reason;
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectDocuments).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}" + " due to unaffordability,  reason: " + capture.RiskAssessmentOutcome.Reason);
                        break;
                    case RCSActionTypeKeys.RejectedITC:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.RiskAssesmentRejectedITCCheck)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        ActivityTrackerMessage = "Application rejected due to ITC Check, Reason: " + capture.RiskAssessmentOutcome.Reason;
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectITC).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}" + " due to ITC Check,  reason: " + capture.RiskAssessmentOutcome.Reason);
                        break;

                    case RCSActionTypeKeys.RejectedDocuments:
                        MatchingHelper.ChangeApplicationStatus(dbContext, (Int32)dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.ReuploadApplicationDocs)?.Id, rcsAppId);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ApplicantUploadsDocuments).Description.ToString();
                        ActivityTrackerMessage = "Application rejected due to issue with uploaded documents, Reason: " + capture.RiskAssessmentOutcome.Reason;
                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.RiskAssessmentRejectUnaffordability).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment rejected for application reference ,{property.ApplicationReferenceNumber}" + " due to documents,  reason: " + capture.RiskAssessmentOutcome.Reason);
                        break;


                    case RCSActionTypeKeys.ReallocateApplication:
                        PropertyLeaseApplicationController roundrobin = new PropertyLeaseApplicationController();
                        Status Status = dbContext.Status.FirstOrDefault(x => x.Key == StatusKeys.Submitted);
                        ActivityTrackerMessage = dbContext.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.ReallocatedForRiskAssessment).Description;

                        //Send e-mail and SMS notification
                        emailboodyId = dbContext.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ReallocatedForAssessment).Id;
                        EmailHelper.CustomerEmailNotification(dbContext, rcsAppId, emailboodyId);

                        Customer reallocateTo = dbContext.Customers.Find(LettingOfficer);
                        RoundRobinQueue roundRobinQueue = new RoundRobinQueue();
                        roundRobinQueue.PropertyLeaseApplicationId = rcsAppId;
                        roundRobinQueue.ResponsibilityTypeId = ResponsibilityTypeId.Id;
                        roundRobinQueue.CurrentTaskDateTime = DateTime.Now;
                        roundRobinQueue.StatusId = Status.Id;
                        roundRobinQueue.ClerkId = reallocateTo.Id;
                        unitAllocationService.Save(roundRobinQueue);
                        roundrobin.BackOfficeNotification(rcsAppId, reallocateTo.Id, ResponsibilityTypeId.Name);
                        Session["ConductRiskAssessmentSession"] = string.Format($"Risk assessment reallocated for application reference ,{property.ApplicationReferenceNumber}");
                        break;
                }


                MatchingHelper.RoundRobinMarkJobAsFinished(dbContext, rcsAppId, null, ResponsibilityTypeId.Id, Customer.Id);
                MatchingHelper.ActivityTrackerAudit(dbContext, rcsAppId, ActivityTrackerMessage, Customer.Id);
                return RedirectToAction("ApplicantRiskAssessment", "PropertyLeaseApplication");
            }
            catch (Exception IO)
            {
                return RedirectToAction("ApplicantRiskAssessment", "PropertyLeaseApplication");
            }
        }
        // ==========================================================================
        // AUTO-ALLOCATION BATCH ENGINE (SINGLE TABLE OPTIMIZED)
        // ==========================================================================
        [HttpPost]
        // [Authorize(Roles = "Admin,CEO")] // Security: specific roles only
        public ActionResult RunAutoAllocation()
        {
            Initialise();
            var db = new eServicesDbContext();
            var log = new System.Text.StringBuilder();
            int matchCount = 0;

            try
            {
                // 1. GET ALL AVAILABLE UNITS
                // We verify IsTaken is false (Available) and IsActive is true (Valid)
                var availableUnits = db.ApplicationAllocatedProperty
                                       .Where(u => u.IsTaken == false && u.IsActive == true)
                                       .ToList();

                log.AppendLine($"Found {availableUnits.Count} available units.");

                foreach (var unit in availableUnits)
                {
                    // 2. CHECK DATA INTEGRITY
                    // We use your specific columns: OfferedComplexId and HumanEHCOptionId
                    if (unit.OfferedComplexId == null || unit.HumanEHCOptionId == null)
                    {
                        log.AppendLine($"Skipping Unit {unit.Id}: Missing Complex or Typology.");
                        continue;
                    }

                    // 3. FIND THE #1 WAITING APPLICANT (FIFO)
                    // We match the Unit's Complex/Typology to the Person's Preferences
                    var bestCandidate = db.PropertyLeaseWaitingLists
                                          .Where(w => w.PreferredComplexId == unit.OfferedComplexId
                                                   && w.PreferredTypologyId == unit.HumanEHCOptionId
                                                   && w.QueueStatus == "Waiting")
                                          .OrderBy(w => w.DateAdded) // Critical: Oldest Date = Priority #1
                                          .FirstOrDefault();

                    if (bestCandidate != null)
                    {
                        try
                        {
                            // 4. PERFORM ALLOCATION (The Match)

                            // A. Lock the Unit
                            unit.AllocatedByUserId = SystemUser.Id; // Current Admin User
                            unit.PropertyLeaseApplicationId = bestCandidate.PropertyLeaseApplicationId;
                            unit.IsTaken = true; // Mark as taken so next loop skips it

                            // B. Create the Offer Record (MatchedUnits)
                            var matchedUnit = new MatchedUnits
                            {
                                ApplicationAllocatedPropertyId = unit.Id,
                                PropertyLeaseApplicationId = bestCandidate.PropertyLeaseApplicationId,
                                IsAccepted = false,
                                RejectedProperty = false
                            };
                            db.MatchedUnits.Add(matchedUnit);

                            // C. Update Waiting List Status
                            bestCandidate.QueueStatus = "Offered";
                            bestCandidate.OfferedUnitId = unit.Id;

                            // D. Update Main Application Status -> "Awaited" (Waiting for customer)
                            var awaitedStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.awaited);
                            if (awaitedStatus != null)
                            {
                                MatchingHelper.ChangeApplicationStatus(db, awaitedStatus.Id, bestCandidate.PropertyLeaseApplicationId);
                            }

                            // E. History & Email
                            MatchingHelper.SaveUnitHistory(db, bestCandidate.PropertyLeaseApplicationId, "Unit Auto-Allocated", unit.Id, SystemUser.Id);

                            var emailContent = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved);
                            if (emailContent != null)
                            {
                                EmailHelper.CustomerEmailNotification(db, bestCandidate.PropertyLeaseApplicationId, emailContent.Id);
                            }

                            // Commit match immediately
                            db.SaveChanges();

                            matchCount++;
                            log.AppendLine($"MATCH: Unit {unit.Id} assigned to App {bestCandidate.PropertyLeaseApplicationId}");
                        }
                        catch (Exception ex)
                        {
                            log.AppendLine($"Error assigning Unit {unit.Id}: {ex.Message}");
                        }
                    }
                }

                TempData["Success"] = $"Auto-Allocation Complete. {matchCount} matches made.";
                TempData["AllocationLog"] = log.ToString(); // <--- Add this line to pass the full text
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Critical Error: " + ex.Message;
            }

            return RedirectToAction("WaitingListIndex");
        }
        public ActionResult ValidateUnit(String SpaceUnitNumber, String SolarReference)
        {
            ApplicationAllocatedProperty collection = dbContext.ApplicationAllocatedProperty.FirstOrDefault(a => a.SolarReference == SolarReference && a.SpaceUnitNumber == SpaceUnitNumber);
            if (collection == null)
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);

            MatchedUnits findItem = dbContext.MatchedUnits.OrderByDescending(a => a.Id).Include(c => c.PropertyLeaseApplication).FirstOrDefault(a => a.ApplicationAllocatedPropertyId == collection.Id);
            String Message = String.Format("The unit you're trying to allocate isn't available for allocation. It is currently allocated to application with ref {0}", findItem.PropertyLeaseApplication.ApplicationReferenceNumber);
            return Json(new { status = collection.IsTaken, data = Message }, JsonRequestBehavior.AllowGet);
        }
      

        public JsonResult ValidateWaitingListSorting()
        {
            var val = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value;
            bool result = Convert.ToBoolean(Convert.ToInt16(db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Risk Assessment OnLoad
        [DecryptParameter]
        public ActionResult RiskAssessmentTenant(int? rcsAppId)
        {
           
            ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Id", "Name");
            ViewBag.ApprovalStatus = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
            try
            {
                Initialise();
                var lease = db.LeaseDetails.Where(x => x.Id == rcsAppId).Include(x => x.SystemUser).Include(x => x.Status).Include(x => x.PurchaserType).FirstOrDefault();
                var customer = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var applicationProp = db.PropertyLeaseApplications.Include(x => x.Status).Include(x => x.PurchaserType).Include(x => x.Customer).Include(x => x.SystemUser).
                    Where(x => x.Id == lease.PropertyLeaseApplicationId).FirstOrDefault();
               
                var Customerid = Customer;
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                var systemusers = db.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                var userID = User.Identity.GetUserId();
                

                ViewBag.r = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                ViewBag.FirstName = applicationProp.FirstName;
                ViewBag.RfNo = applicationProp.ApplicationReferenceNumber;
                ViewBag.DateCreated = applicationProp.CreatedDateTime.Value;
                ViewBag.Sat = applicationProp.FirstName;
                RiskAssessmentOutcome rao = new RiskAssessmentOutcome();
                rao.FirstName = systemusers.LastName;
                rao.LastName = systemusers.FirstName;

                var application = db.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                var referenceType = db.ReferenceTypes.FirstOrDefault(a => a.Key.Equals(ReferenceTypeKeys.RCSUpload));
                if (referenceType == null) throw new Exception("Invalid reference type.");

                string baseFormat = "";
                var referenceId = customer.Id;
                DocumentsViewModel dvm = new DocumentsViewModel();
                bool IsUpload = true;
                MatchingHelper.DocumentTenantRiskAssessment(dvm, db, referenceId, customer.Id, (int)referenceType.Id, (int)application.Id, baseFormat, applicationProp.Id, IsUpload);

                var ratesRebateProperty = new RatesRebateProperty();
                var incentivePolicyProperty = new IncentivePolicyProperty();

                var cmv = new CaptureViewModel
                {
                    DocumentsViewModel = dvm,
                    LeaseDetails = lease,
                    PropertyLeaseApplication = applicationProp,
                    RiskAssessmentOutcome = rao,
                    vaIDno = applicationProp.Id.ToString()
                };

                ViewBag.DateStamp = DateTime.Now;
                ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
                return View(cmv);
            }
            catch
            {

            }
            return View();
        }
        #endregion

        #region Risk Assessment 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
        public ActionResult RiskAssessmentTenant(int? rcsAppId, CaptureViewModel capture, string ApprovalStatusddl)
        {

            using (var context = new eServicesDbContext())
            {
                RiskAssessmentOutcome rao = new RiskAssessmentOutcome();
                try
                {

                    Initialise();
                    int leaseId = (int)rcsAppId;
                    var Customerid = Customer;
                    var custmusers = context.Customers.FirstOrDefault(x => x.Id == Customer.Id);
                    var systemusers = context.SystemUsers.FirstOrDefault(x => x.Id == SystemUser.Id);
                    var userID = User.Identity.GetUserId();
                    ViewBag.ApprovalStatus = new SelectList(context.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
                    rao.LeaseDetailsId = capture.LeaseDetails.Id;
                    rao.FirstName = capture.RiskAssessmentOutcome.FirstName;
                    rao.LastName = capture.RiskAssessmentOutcome.LastName;
                    rao.Reason = capture.RiskAssessmentOutcome.Reason;
                    rao.Outcome = (ApprovalStatusddl == RCSActionTypeKeys.Approved) ? "Approved" : "Rejected";
                    rao.StatusId = context.RCSActionTypes.Where(x => x.Key == ApprovalStatusddl).FirstOrDefault().Id;
                    rao.LeaseDetailsId = leaseId;
                    rao.IsActive = true;
                    rao.IsDeleted = false;
                    rao.IsLocked = false;
                    context.RiskAssessmentOutcomes.Add(rao);
                    context.SaveChanges();

                    var lease = context.LeaseDetails.FirstOrDefault(x => x.Id == capture.LeaseDetails.Id);
                    PropertyLeaseActionComments comments = new PropertyLeaseActionComments
                    {
                        PropertyLeaseApplicationId = lease.PropertyLeaseApplicationId,
                        RejectReason = capture.RiskAssessmentOutcome.Reason,
                        RiskAssessmentTenant = true
                    };
                    context.propertyLeaseActionComments.Add(comments);
                    context.SaveChanges();

                    if (ApprovalStatusddl == RCSActionTypeKeys.Approved)
                    {
                        MatchingHelper.ChangeLeaseStatusII(context, context.Status.FirstOrDefault(x => x.Key == StatusKeys.ExtendedLeasingPeriod).Id, (int)leaseId);
                        //MatchingHelper.AddMonthsToDates(context, capture.LeaseDetails.Id);
                        int emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                        EmailHelper.CustomerEmailNotification(context, lease.PropertyLeaseApplicationId, emailboodyId);
                        var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantRiskApproveRenewal).Description.ToString();
                        MatchingHelper.ActivityTrackerAudit(context, capture.LeaseDetails.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);

                        lease.PeriodInMonths = lease.PeriodInMonths - lease.MonthsOffered;
                        lease.MonthsOffered = 0;
                        context.Entry(lease).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                    if (ApprovalStatusddl == RCSActionTypeKeys.Rejected)
                    {
                        MatchingHelper.ChangeLeaseStatusII(context, context.Status.FirstOrDefault(x => x.Key == StatusKeys.TerminateAtEndOfPeriod).Id, (int)leaseId);
                        //MatchingHelper.RemoveOfferedMonths(context, capture.LeaseDetails.Id);
                        int emailboodyId = context.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.ApplicationRiskAssessmentApproved).Id;
                        EmailHelper.CustomerEmailNotification(context, lease.PropertyLeaseApplicationId, emailboodyId);
                        var ActivityTrackerMessage = context.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.TenantRiskRejectRenewal).Description.ToString();
                        MatchingHelper.ActivityTrackerAudit(context, capture.LeaseDetails.PropertyLeaseApplicationId, ActivityTrackerMessage, custmusers.Id);
                    }

                    var ResponsibilityTypeId = db.ResponsibilityTypes.Where(x => x.Key == ResponsibilityTypeKeys.RenewalRiskAssessment).FirstOrDefault();
                    var activeDirectoryOn = Convert.ToInt16(db.AppSettings.Where(x => x.Key == AppSettingKeys.RevenueOfficer).FirstOrDefault().Value);
                    MatchingHelper.RoundRobinMarkJobAsFinished(db, (int)lease.PropertyLeaseApplicationId, null, ResponsibilityTypeId.Id, activeDirectoryOn);

                    return RedirectToAction("ApplicantRiskAssessment", "PropertyLeaseApplication");
                }
                catch (Exception Io)
                {
                    return RedirectToAction("ApplicantRiskAssessment", "PropertyLeaseApplication");
                }
            }
            
        }
        #endregion

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskAssessmentOutcome riskAssessmentOutcome = db.RiskAssessmentOutcomes.Find(id);
            if (riskAssessmentOutcome == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", riskAssessmentOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", riskAssessmentOutcome.ModifiedBySystemUserId);
            return View(riskAssessmentOutcome);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,LastName,FirstName,OfficialNumber,Outcome,Reason,DateStamp,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RiskAssessmentOutcome riskAssessmentOutcome)
        {
            if (ModelState.IsValid)
            {
                db.Entry(riskAssessmentOutcome).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", riskAssessmentOutcome.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", riskAssessmentOutcome.ModifiedBySystemUserId);
            return View(riskAssessmentOutcome);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RiskAssessmentOutcome riskAssessmentOutcome = db.RiskAssessmentOutcomes.Find(id);
            if (riskAssessmentOutcome == null)
            {
                return HttpNotFound();
            }
            return View(riskAssessmentOutcome);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RiskAssessmentOutcome riskAssessmentOutcome = db.RiskAssessmentOutcomes.Find(id);
            db.RiskAssessmentOutcomes.Remove(riskAssessmentOutcome);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
    }

}
