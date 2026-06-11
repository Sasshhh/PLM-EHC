using System;
using System.Linq;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize(Roles = "Back Office System Administrator,Super Administrators")]
    public class TestingToolsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetTestState()
        {
            var path = Server.MapPath("~/Tests/Playwright/test-state.json");
            if (System.IO.File.Exists(path))
            {
                var content = System.IO.File.ReadAllText(path);
                return Json(content, JsonRequestBehavior.AllowGet);
            }
            return Json("{}", JsonRequestBehavior.AllowGet);
        }

        // ─────────────────────────────────────────────
        // WAITING LIST BACKDATE (existing)
        // ─────────────────────────────────────────────

        [HttpGet]
        public ActionResult WaitingListBackdate()
        {
            using (var db = new eServicesDbContext())
            {
                var list = db.waitingListQues
                    .Include("PropertyLeaseApplication")
                    .Where(x => x.IsActive && !x.IsDeleted)
                    .OrderByDescending(x => x.QueueDate)
                    .ToList();
                return View(list);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BackdateApplication(int waitingListQueId)
        {
            using (var db = new eServicesDbContext())
            {
                var item = db.waitingListQues.Find(waitingListQueId);
                if (item != null)
                {
                    item.QueueDate = DateTime.Now.AddDays(-155);
                    item.IsReListed = false;
                    item.ModifiedDateTime = DateTime.Now;
                    db.SaveChanges();
                    TempData["Success"] = "Application backdated to 155 days ago. It is now eligible for the 5-month checker.";
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
                return RedirectToAction("WaitingListBackdate");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunReEntryChecker()
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    MatchingHelper.WaitingListNotificationAtOneYear(db);
                }
                TempData["Success"] = "5-Month Re-Entry Checker completed successfully."; 
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Checker error: " + ex.Message;
            }
            return RedirectToAction("WaitingListBackdate");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendReEntryReminder(int propertyLeaseApplicationId)
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    // Directly trigger re-entry for a single application
                    MatchingHelper.WaitingListReEntry(db, propertyLeaseApplicationId, 0);
                }
                TempData["Success"] = "Re-entry activated for application " + propertyLeaseApplicationId + ".";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            return RedirectToAction("WaitingListBackdate");
        }

        // ─────────────────────────────────────────────
        // LEASE BACKDATE (new — UC018 testing tool)
        // ─────────────────────────────────────────────

        [HttpGet]
        public ActionResult LeaseBackdate()
        {
            using (var db = new eServicesDbContext())
            {
                var activeStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ActiveLease);
                var activeStatusId = activeStatus != null ? activeStatus.Id : 0;

                var leases = db.LeaseDetails
                    .Include("PropertyLeaseApplication")
                    .Include("Status")
                    .Where(x => x.IsActive && !x.IsDeleted
                             && x.StatusId == activeStatusId
                             && x.EndDate != null
                             && x.Completed)
                    .OrderBy(x => x.EndDate)
                    .ToList();

                return View(leases);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BackdateLeaseExpiry(int leaseId)
        {
            using (var db = new eServicesDbContext())
            {
                var lease = db.LeaseDetails.Find(leaseId);
                if (lease == null)
                {
                    TempData["Error"] = "Lease record not found.";
                    return RedirectToAction("LeaseBackdate");
                }

                // Force ALL conditions required by RenewalNotificationAtEndOfTime:
                // x.IsActive                 -> already true (it's in the active lease list)
                // list.Contains(AppId)       -> must be in ApplicantUnits (can't force here)
                // x.PeriodInMonths >= 12     -> set to 24 if currently 0/null
                // x.Completed               -> must be true
                // x.RenewalNotice <= now     -> set to 91 days ago
                // x.IsNew                   -> must be true
                // !x.IsRenewed              -> set to false
                lease.EndDate = DateTime.Now;
                lease.RenewalNotice = DateTime.Now.AddDays(-91);
                lease.TerminationNotice = DateTime.Now.AddDays(-30);
                lease.IsRenewed = false;
                lease.IsNew = true;
                lease.Completed = true;
                if (lease.PeriodInMonths == null || lease.PeriodInMonths < 12)
                    lease.PeriodInMonths = 24;
                lease.ModifiedDateTime = DateTime.Now;

                db.SaveChanges();

                var appRef = lease.leaseApplicationRef ?? lease.Id.ToString();
                TempData["Success"] = $"Lease {appRef} backdated. EndDate = today, RenewalNotice = 91 days ago. Run the scanner to flag it for renewal.";
                return RedirectToAction("LeaseBackdate");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunRenewalScanner()
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    MatchingHelper.RenewalNotificationAtEndOfTime(db);
                }
                TempData["Success"] = "Lease Renewal Scanner completed. Eligible leases have been flagged with status 'Application Up For Renewal' and CSOs notified.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Scanner error: " + ex.Message;
            }
            return RedirectToAction("LeaseBackdate");
        }

        // ─────────────────────────────────────────────
        // BUILD RENEWAL HISTORY — seed demo data for UC018 testing
        // ─────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuildRenewalHistory(int leaseId)
        {
            using (var db = new eServicesDbContext())
            {
                try
                {
                    var lease = db.LeaseDetails
                        .Include("PropertyLeaseApplication")
                        .FirstOrDefault(x => x.Id == leaseId);

                    if (lease == null || lease.PropertyLeaseApplication == null)
                    {
                        TempData["Error"] = "Lease not found.";
                        return RedirectToAction("LeaseBackdate");
                    }

                    var app    = lease.PropertyLeaseApplication;
                    var appRef = app.ApplicationReferenceNumber;
                    var cust   = db.Customers.FirstOrDefault(c => c.Id == app.CustomerId);
                    int complexId = 24; // default

                    string fName   = lease.FirstNames ?? "Tenant";
                    string lName   = lease.LastName   ?? "Demo";
                    string email   = (cust != null && cust.EmailAddress != null) ? cust.EmailAddress : "demo@test.com";
                    string phone   = "0800000001";
                    int deptId     = 1;
                    int assignedTo = 190; // default letting officer

                    int srOpen       = db.Status.FirstOrDefault(s => s.Key == "sr_status_open")     != null ? db.Status.First(s => s.Key == "sr_status_open").Id     : 4347;
                    int srInProgress = db.Status.FirstOrDefault(s => s.Key == "sr_status_in_progress") != null ? db.Status.First(s => s.Key == "sr_status_in_progress").Id : 4348;
                    int srResolved   = db.Status.FirstOrDefault(s => s.Key == "sr_status_resolved") != null ? db.Status.First(s => s.Key == "sr_status_resolved").Id   : 4349;
                    int srClosed     = db.Status.FirstOrDefault(s => s.Key == "sr_status_closed")   != null ? db.Status.First(s => s.Key == "sr_status_closed").Id     : 4350;
                    int ptSubmitted  = db.Status.FirstOrDefault(s => s.Key == "payment_transgression_status_submitted")      != null ? db.Status.First(s => s.Key == "payment_transgression_status_submitted").Id      : 4343;
                    int ptLetterSent = db.Status.FirstOrDefault(s => s.Key == "payment_transgression_status_letter_sent")    != null ? db.Status.First(s => s.Key == "payment_transgression_status_letter_sent").Id    : 4345;
                    int ptClosed     = db.Status.FirstOrDefault(s => s.Key == "payment_transgression_status_closed")         != null ? db.Status.First(s => s.Key == "payment_transgression_status_closed").Id         : 4346;
                    int cmpSubmitted = db.Status.FirstOrDefault(s => s.Key == "complaint_status_submitted") != null ? db.Status.First(s => s.Key == "complaint_status_submitted").Id : 4336;
                    int cmpResolved  = db.Status.FirstOrDefault(s => s.Key == "complaint_status_resolved")  != null ? db.Status.First(s => s.Key == "complaint_status_resolved").Id  : 4339;

                    int inserted = 0;

                    // ── Service Requests ──────────────────────────
                    string[] srRefs = { "SR_HIST_" + leaseId + "_A", "SR_HIST_" + leaseId + "_B", "SR_HIST_" + leaseId + "_C", "SR_HIST_" + leaseId + "_D" };

                    if (!db.ServiceRequests.Any(s => s.RequestReferenceNumber == srRefs[0]))
                    {
                        db.ServiceRequests.Add(new Models.ServiceRequest {
                            RequestReferenceNumber = srRefs[0], ReportedByName = fName, ReportedBySurname = lName,
                            ContactNumber = phone, EmailAddress = email, ComplexId = complexId,
                            ServiceRequestCategoryId = 1, ServiceRequestPriorityId = 1,
                            DetailedDescription = "Severe water leak from bathroom ceiling. Dripping onto electrical fittings. Urgent.",
                            StatusId = srInProgress, DateSubmitted = DateTime.Now.AddDays(-30),
                            ResponseDeadline = DateTime.Now.AddDays(-29), ResolutionDeadline = DateTime.Now.AddDays(-28),
                            EscalationTriggered = false, CreatedByCustomerId = app.CustomerId, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddDays(-30),
                            ModifiedDateTime = DateTime.Now.AddDays(-29), IsLocked = false });
                        inserted++;
                    }
                    if (!db.ServiceRequests.Any(s => s.RequestReferenceNumber == srRefs[1]))
                    {
                        db.ServiceRequests.Add(new Models.ServiceRequest {
                            RequestReferenceNumber = srRefs[1], ReportedByName = fName, ReportedBySurname = lName,
                            ContactNumber = phone, EmailAddress = email, ComplexId = complexId,
                            ServiceRequestCategoryId = 1, ServiceRequestPriorityId = 2,
                            DetailedDescription = "Circuit breaker tripping in kitchen repeatedly. Power failure 3x this week.",
                            StatusId = srResolved, DateSubmitted = DateTime.Now.AddDays(-60),
                            ResponseDeadline = DateTime.Now.AddDays(-59), ResolutionDeadline = DateTime.Now.AddDays(-57),
                            EscalationTriggered = false, DateResolved = DateTime.Now.AddDays(-55),
                            CreatedByCustomerId = app.CustomerId, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddDays(-60),
                            ModifiedDateTime = DateTime.Now.AddDays(-55), IsLocked = false });
                        inserted++;
                    }
                    if (!db.ServiceRequests.Any(s => s.RequestReferenceNumber == srRefs[2]))
                    {
                        db.ServiceRequests.Add(new Models.ServiceRequest {
                            RequestReferenceNumber = srRefs[2], ReportedByName = fName, ReportedBySurname = lName,
                            ContactNumber = phone, EmailAddress = email, ComplexId = complexId,
                            ServiceRequestCategoryId = 2, ServiceRequestPriorityId = 3,
                            DetailedDescription = "Requesting handrail installation and ramp for elderly occupant with mobility issues.",
                            StatusId = srOpen, DateSubmitted = DateTime.Now.AddDays(-10),
                            ResponseDeadline = DateTime.Now.AddDays(-9), ResolutionDeadline = DateTime.Now.AddDays(-7),
                            EscalationTriggered = false, CreatedByCustomerId = app.CustomerId, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddDays(-10),
                            ModifiedDateTime = DateTime.Now.AddDays(-10), IsLocked = false });
                        inserted++;
                    }
                    if (!db.ServiceRequests.Any(s => s.RequestReferenceNumber == srRefs[3]))
                    {
                        db.ServiceRequests.Add(new Models.ServiceRequest {
                            RequestReferenceNumber = srRefs[3], ReportedByName = fName, ReportedBySurname = lName,
                            ContactNumber = phone, EmailAddress = email, ComplexId = complexId,
                            ServiceRequestCategoryId = 3, ServiceRequestPriorityId = 4,
                            DetailedDescription = "Disputing water billing for prior quarter. Meter reading appears incorrect.",
                            StatusId = srClosed, DateSubmitted = DateTime.Now.AddMonths(-6),
                            ResponseDeadline = DateTime.Now.AddMonths(-5), ResolutionDeadline = DateTime.Now.AddMonths(-5),
                            EscalationTriggered = false, DateResolved = DateTime.Now.AddMonths(-5), DateClosed = DateTime.Now.AddMonths(-5),
                            CreatedByCustomerId = app.CustomerId, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddMonths(-6),
                            ModifiedDateTime = DateTime.Now.AddMonths(-5), IsLocked = false });
                        inserted++;
                    }

                    // ── Payment Transgressions ─────────────────────
                    string[] ptRefs = { "PT_HIST_" + leaseId + "_A", "PT_HIST_" + leaseId + "_B", "PT_HIST_" + leaseId + "_C" };

                    if (!db.PaymentTransgressions.Any(p => p.CaseReferenceNumber == ptRefs[0]))
                    {
                        db.PaymentTransgressions.Add(new Models.PaymentTransgression {
                            CaseReferenceNumber = ptRefs[0], OfficialNumber = appRef, TenancyReferenceNumber = appRef,
                            TenantName = fName, TenantSurname = lName, TenantEmail = email, TenantCellphone = phone,
                            ComplexId = complexId, BlockNumber = "A", UnitNumber = "1", AccountNumber = "ACC-" + leaseId,
                            LastPaymentAmount = 3500m, LastPaymentDate = DateTime.Now.AddMonths(-5), TotalAmountDue = 450m,
                            PaymentTransgressionCategoryId = 1, PaymentTransgressionTypeId = 1, PaymentTransgressionSeverityId = 1,
                            DetailedDescription = "Late payment (10 days) in Month 1. First offence. Reminder letter issued.",
                            LetterType = "Payment Transgression Notice",
                            LetterGeneratedDate = DateTime.Now.AddMonths(-5), LetterSentDate = DateTime.Now.AddMonths(-5),
                            DateSubmitted = DateTime.Now.AddMonths(-6), StatusId = ptClosed,
                            AssignedToCustomerId = assignedTo, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddMonths(-6), ModifiedDateTime = DateTime.Now.AddMonths(-5), IsLocked = false });
                        inserted++;
                    }
                    if (!db.PaymentTransgressions.Any(p => p.CaseReferenceNumber == ptRefs[1]))
                    {
                        db.PaymentTransgressions.Add(new Models.PaymentTransgression {
                            CaseReferenceNumber = ptRefs[1], OfficialNumber = appRef, TenancyReferenceNumber = appRef,
                            TenantName = fName, TenantSurname = lName, TenantEmail = email, TenantCellphone = phone,
                            ComplexId = complexId, BlockNumber = "A", UnitNumber = "1", AccountNumber = "ACC-" + leaseId,
                            LastPaymentAmount = 0m, LastPaymentDate = null, TotalAmountDue = 2800m,
                            PaymentTransgressionCategoryId = 1, PaymentTransgressionTypeId = 2, PaymentTransgressionSeverityId = 2,
                            DetailedDescription = "2 months rental arrears (R2,800). Written warning issued. 14 days to settle or enter payment arrangement.",
                            LetterType = "Written Warning Letter",
                            LetterGeneratedDate = DateTime.Now.AddDays(-15), LetterSentDate = DateTime.Now.AddDays(-14),
                            DateSubmitted = DateTime.Now.AddMonths(-2), StatusId = ptLetterSent,
                            AssignedToCustomerId = assignedTo, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddMonths(-2), ModifiedDateTime = DateTime.Now.AddDays(-14), IsLocked = false });
                        inserted++;
                    }
                    if (!db.PaymentTransgressions.Any(p => p.CaseReferenceNumber == ptRefs[2]))
                    {
                        db.PaymentTransgressions.Add(new Models.PaymentTransgression {
                            CaseReferenceNumber = ptRefs[2], OfficialNumber = appRef, TenancyReferenceNumber = appRef,
                            TenantName = fName, TenantSurname = lName, TenantEmail = email, TenantCellphone = phone,
                            ComplexId = complexId, BlockNumber = "A", UnitNumber = "1", AccountNumber = "ACC-" + leaseId,
                            LastPaymentAmount = null, LastPaymentDate = null, TotalAmountDue = 6200m,
                            PaymentTransgressionCategoryId = 2, PaymentTransgressionTypeId = 3, PaymentTransgressionSeverityId = 3,
                            DetailedDescription = "Tenant in violation of lease - suspected subletting without authorisation. Matter referred to legal.",
                            LetterType = "Final Written Warning Letter",
                            DateSubmitted = DateTime.Now.AddDays(-3), StatusId = ptSubmitted,
                            AssignedToCustomerId = assignedTo, DepartmentId = deptId,
                            IsActive = true, IsDeleted = false, CreatedDateTime = DateTime.Now.AddDays(-3), ModifiedDateTime = DateTime.Now.AddDays(-3), IsLocked = false });
                        inserted++;
                    }

                    // ── Complaints ─────────────────────────────────
                    string[] cRefs = { "TC_HIST_" + leaseId + "_A", "TC_HIST_" + leaseId + "_B" };

                    if (!db.TenantComplaints.Any(c => c.CaseReferenceNumber == cRefs[0]))
                    {
                        db.TenantComplaints.Add(new Models.TenantComplaint {
                            CaseReferenceNumber = cRefs[0], OfficialNumber = appRef,
                            ComplainantComplexId = complexId, ComplainantFirstName = "Neighbour", ComplainantSurname = "Smith",
                            ComplainantEmail = "n.smith@demo.com", ComplainantCellphone = "0700000001",
                            ComplainantBlockNumber = "A", ComplainantUnitNumber = "2",
                            RespondentComplexId = complexId, RespondentBlockNumber = "A", RespondentUnitNumber = "1",
                            RespondentFirstName = fName, RespondentSurname = lName,
                            ComplaintCategoryId = 3, ComplaintTypeId = 3,
                            DetailedDescription = "Excessive noise from unit late night (11pm-2am). Music and social gatherings disturbing residents.",
                            StatusId = cmpResolved, IsActive = true, IsDeleted = false,
                            CreatedDateTime = DateTime.Now.AddMonths(-4), ModifiedDateTime = DateTime.Now.AddMonths(-3) });
                        inserted++;
                    }
                    if (!db.TenantComplaints.Any(c => c.CaseReferenceNumber == cRefs[1]))
                    {
                        db.TenantComplaints.Add(new Models.TenantComplaint {
                            CaseReferenceNumber = cRefs[1], OfficialNumber = appRef,
                            ComplainantComplexId = complexId, ComplainantFirstName = "Building", ComplainantSurname = "Management",
                            ComplainantEmail = "mgmt@complex.co.za", ComplainantCellphone = "0800000002",
                            ComplainantBlockNumber = "Admin", ComplainantUnitNumber = "Office",
                            RespondentComplexId = complexId, RespondentBlockNumber = "A", RespondentUnitNumber = "1",
                            RespondentFirstName = fName, RespondentSurname = lName,
                            ComplaintCategoryId = 2, ComplaintTypeId = 1,
                            DetailedDescription = "Undeclared occupant found in unit. Suspected subletting of a bedroom without management authorisation.",
                            StatusId = cmpSubmitted, IsActive = true, IsDeleted = false,
                            CreatedDateTime = DateTime.Now.AddDays(-20), ModifiedDateTime = DateTime.Now.AddDays(-20) });
                        inserted++;
                    }

                    db.SaveChanges();
                    TempData["Success"] = string.Format("Renewal history built for lease {0}: {1} record(s) inserted (service requests, transgressions, complaints). Reload the RecommendForRenewal page to see all data.", appRef, inserted);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Build history error: " + ex.Message;
                }
                return RedirectToAction("LeaseBackdate");
            }
        }

        // ─────────────────────────────────────────────
        // BR19 — LEASE SIGNING EXPIRY (30-day deadline)
        // ─────────────────────────────────────────────

        [HttpGet]
        public ActionResult LeaseSigningExpiry()
        {
            using (var db = new eServicesDbContext())
            {
                var awaitingStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.LeaseAgreementGenerated);
                int awaitingStatusId = awaitingStatus != null ? awaitingStatus.Id : 0;

                var agreements = db.propertyLeaseAgreementMasters
                    .Include("PropertyLeaseApplication")
                    .Include("PropertyLeaseApplication.Status")
                    .Where(a => a.IsActive && !a.IsDeleted
                             && !a.TenantSigned
                             && a.PropertyLeaseApplication != null
                             && a.PropertyLeaseApplication.StatusId == awaitingStatusId)
                    .OrderBy(a => a.CreatedDateTime)
                    .ToList();

                return View(agreements);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BackdateLeaseAgreement(int agreementId)
        {
            using (var db = new eServicesDbContext())
            {
                var agreement = db.propertyLeaseAgreementMasters.Find(agreementId);
                if (agreement == null)
                {
                    TempData["Error"] = "Agreement record not found.";
                    return RedirectToAction("LeaseSigningExpiry");
                }

                // Set CreatedDateTime to 31 days ago to trigger the 30-day checker
                agreement.CreatedDateTime = DateTime.Now.AddDays(-31);
                agreement.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();

                TempData["Success"] = string.Format("Agreement {0} backdated. CreatedDateTime = 31 days ago. Run the checker to enforce BR19.", agreementId);
                return RedirectToAction("LeaseSigningExpiry");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunLeaseSigningChecker()
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    MatchingHelper.CheckLeaseSigningExpiry(db);
                }
                TempData["Success"] = "BR19 Lease Signing Checker completed. Applications where agreements were unsigned for 30+ days have been disregarded.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Checker error: " + ex.Message;
            }
            return RedirectToAction("LeaseSigningExpiry");
        }

        // ─────────────────────────────────────────────
        // BR09 — UNIT OFFER EXPIRY (30-day deadline)
        // ─────────────────────────────────────────────

        [HttpGet]
        public ActionResult UnitOfferExpiry()
        {
            using (var db = new eServicesDbContext())
            {
                var matchedStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.awaited);
                int matchedStatusId = matchedStatus != null ? matchedStatus.Id : 0;

                var matchedUnits = db.MatchedUnits
                    .Include("PropertyLeaseApplication")
                    .Include("PropertyLeaseApplication.Status")
                    .Include("ApplicationAllocatedProperty")
                    .Where(m => !m.IsAccepted
                             && !m.RejectedProperty
                             && !m.IsDeleted
                             && m.PropertyLeaseApplication != null
                             && m.PropertyLeaseApplication.StatusId == matchedStatusId)
                    .OrderBy(m => m.CreatedDateTime)
                    .ToList();

                return View(matchedUnits);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BackdateUnitOffer(int matchedUnitId)
        {
            using (var db = new eServicesDbContext())
            {
                var match = db.MatchedUnits.Find(matchedUnitId);
                if (match == null)
                {
                    TempData["Error"] = "Matched unit record not found.";
                    return RedirectToAction("UnitOfferExpiry");
                }

                // Set CreatedDateTime to 31 days ago to trigger the 30-day checker
                match.CreatedDateTime = DateTime.Now.AddDays(-31);
                match.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();

                TempData["Success"] = string.Format("Matched unit {0} backdated. CreatedDateTime = 31 days ago. Run the checker to enforce BR09.", matchedUnitId);
                return RedirectToAction("UnitOfferExpiry");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RunUnitOfferChecker()
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    MatchingHelper.CheckUnitOfferExpiry(db);
                }
                TempData["Success"] = "BR09 Unit Offer Checker completed. Unit offers pending 30+ days have been withdrawn and applicants re-listed.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Checker error: " + ex.Message;
            }
            return RedirectToAction("UnitOfferExpiry");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult TestSash38Lease(int? id)
        {
            string activeConnStr = "Unknown";
            try
            {
                using (var db = new eServicesDbContext())
                {
                    activeConnStr = db.Database.Connection.ConnectionString;
                }
            }
            catch (Exception ex)
            {
                activeConnStr = "Error reading: " + ex.Message;
            }

            try
            {
                int appId = id ?? 14157;
                using (var db = new eServicesDbContext())
                {
                    var application = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == appId && !x.IsDeleted);
                    if (application == null)
                    {
                        return Content(string.Format("Application with ID {0} not found.", appId));
                    }

                    // 1. Ensure LeaseDetails exists
                    var lease = db.LeaseDetails.FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.IsNew && !x.IsDeleted);
                    if (lease == null)
                    {
                        lease = new Models.LeaseDetails
                        {
                            PropertyLeaseApplicationId = application.Id,
                            IsNew = true,
                            IsActive = true,
                            IsDeleted = false,
                            Completed = true,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddYears(1),
                            CreatedDateTime = DateTime.Now,
                            IsRenewed = false,
                            PeriodInMonths = 12
                        };
                        db.LeaseDetails.Add(lease);
                        db.SaveChanges();
                    }
                    else
                    {
                        lease.IsActive = true;
                        lease.Completed = true;
                        lease.IsNew = true;
                        db.Entry(lease).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    // 2. Ensure propertyLeaseAgreementMasters exists
                    var master = db.propertyLeaseAgreementMasters.FirstOrDefault(x => x.PropertyLeaseApplicationId == application.Id && x.LeaseDetailsId == lease.Id && x.IsActive && !x.IsDeleted);
                    if (master == null)
                    {
                        master = new Models.PropertyLeaseAgreementMaster
                        {
                            PropertyLeaseApplicationId = application.Id,
                            LeaseDetailsId = lease.Id,
                            IsActive = true,
                            IsDeleted = false,
                            ApplicantFullName = application.ApplicantFullName ?? "Mr S Moodley",
                            ApplicantIdentityNumber = application.IDNo ?? "9411091404087",
                            RepresentedBy = "EHC Housing Official",
                            MonthlyUnitRental = 1500.0,
                            InitialDepositPremises = 1500.0,
                            CreditCheckFee = 150.0,
                            UnitRentalAmountPM = 1500.0,
                            _water = 120.0,
                            ELEC = true,
                            Electricity = 300.0,
                            _refuse = 80.0,
                            _sewerage = 90.0,
                            CarportParkingBayNumber = "BAY-12",
                            STR = true,
                            StoreRooms = 50.0,
                            CommencementDay = "1st",
                            CommencementDate = DateTime.Now.ToString("MMMM yyyy"),
                            TenantSignDay = DateTime.Now.ToString("dd"),
                            TenantSignDate = DateTime.Now.ToString("MMMM yyyy"),
                            PropertyManagerSignatureDate = DateTime.Now,
                            ManagersSignDate = DateTime.Now.ToString("dd MMMM yyyy"),
                            LeaseAdministrationFee = 100.0,
                            KeyDeposit = 200m,
                            AccessCardDeposit = 100m,
                            HasDSTV = true,
                            DSTVActivationFee = 150m,
                            DSTVMonthlyLevy = 50m,
                            TenantBankName = "Standard Bank",
                            TenantAccountNumber = "123456789",
                            TenantAccountHolderName = "S Moodley",
                            TenantAccountType = "Savings",
                            TenantBranchCode = "051001",
                            OccupantONE = "Occupant One",
                            OccupantONEIdentityNo = "9501015012081",
                            Witness1Name = "Witness One",
                            TenantSignature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
                            PropertyManagersSignature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
                            RevenueManagersSignature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
                            TenantSigned = true,
                            PropertyManagerSigned = true,
                            RevenueManagerSigned = true,
                            CreatedDateTime = DateTime.Now
                        };
                        db.propertyLeaseAgreementMasters.Add(master);
                        db.SaveChanges();
                    }
                    else
                    {
                        master.TenantSignature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
                        master.PropertyManagersSignature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
                        master.RevenueManagersSignature = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
                        master.TenantSigned = true;
                        master.PropertyManagerSigned = true;
                        master.RevenueManagerSigned = true;
                        master.IsActive = true;
                        master.IsDeleted = false;
                        db.Entry(master).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    // 3. Set Application Status to Active Lease
                    var activeStatus = db.Status.FirstOrDefault(x => x.Key == StatusKeys.ActiveLease);
                    if (activeStatus != null)
                    {
                        application.StatusId = activeStatus.Id;
                        db.Entry(application).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                // 4. Instantiate Controller and invoke AutoUploadFinalLeaseAgreement via Reflection
                var controller = new PropertyLeaseApplicationController();
                controller.ControllerContext = this.ControllerContext;

                var method = typeof(PropertyLeaseApplicationController).GetMethod("AutoUploadFinalLeaseAgreement", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method == null)
                {
                    return Content("AutoUploadFinalLeaseAgreement method not found in PropertyLeaseApplicationController.");
                }

                method.Invoke(controller, new object[] { appId });

                // 5. Query the saved document to verify and show info
                using (var db2 = new eServicesDbContext())
                {
                    var docType = db2.DocumentTypes.FirstOrDefault(dt => dt.Key == DocumentTypeKeys.ApplicationLeaseAgreementEHC);
                    var doc = db2.Documents
                        .OrderByDescending(d => d.Id)
                        .FirstOrDefault(d => d.PropertyLeaseApplicationId == appId && d.DocumentCheckList.DocumentTypeId == docType.Id && d.IsActive && !d.IsDeleted);

                    if (doc != null)
                    {
                        return Content(string.Format("Success! Signed Lease PDF generated and saved successfully.<br/>" +
                            "Document ID: {0}<br/>" +
                            "Document Name: {1}<br/>" +
                            "File ID: {2}<br/>" +
                            "Date Created: {3}<br/><br/>" +
                            "You can now view the details page for application ID {4} to see the signed lease agreement.", 
                            doc.Id, doc.DocumentName, doc.FileId, doc.CreatedDateTime, appId));
                    }
                    else
                    {
                        return Content("Logic executed, but failed to retrieve the newly created Document record.");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(string.Format("Active DB Connection String: {0}<br/><br/>Error: {1}<br/>Inner: {2}<br/>Stack: {3}", activeConnStr, ex.Message, ex.InnerException?.Message, ex.StackTrace));
            }
        }
    }
}

