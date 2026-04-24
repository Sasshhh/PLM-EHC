using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Engines;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class ComplaintsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        private static Random random = new Random();
        private IdentityManager identityManager;
        private NotificationEngine notificationEngine;
        private ComplaintWorkflowEngine workflowEngine;
        private ComplaintSLAEngine slaEngine;

        public ComplaintsController()
        {
            identityManager = new IdentityManager(db);
            notificationEngine = new NotificationEngine(db);
            workflowEngine = new ComplaintWorkflowEngine(db);
            slaEngine = new ComplaintSLAEngine(db);
        }

        // GET: Complaints/Index
        [Authorize(Roles = "Client Services Officer,Customers")]
        public ActionResult Index()
        {
            var systemUser = identityManager.CurrentUser(User);
            Customer customer = null;
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
            }

            IQueryable<TenantComplaint> complaints;

            if (User.IsInRole("Client Services Officer"))
            {
                complaints = db.TenantComplaints
                    .Include(t => t.ComplainantComplex)
                    .Include(t => t.RespondentComplex)
                    .Include(t => t.ComplaintCategory)
                    .Include(t => t.ComplaintType)
                    .Include(t => t.Status)
                    .Where(t => t.IsActive && !t.IsDeleted)
                    .OrderByDescending(t => t.DateSubmitted);
            }
            else
            {
                // Get tenant's allocated units in memory (anonymous type - fine here)
                var tenantUnits = db.MatchedUnits
                    .Where(mu => mu.CustomerId == customer.Id && mu.ApplicationAllocatedPropertyId != null)
                    .Select(mu => mu.ApplicationAllocatedProperty)
                    .Where(aap => aap != null)
                    .Select(aap => new { ComplexId = aap.OfferedComplexId, UnitNumber = aap.SpaceUnitNumber, BlockNumber = aap.BuildingName })
                    .ToList();

                // Build respondent complaint IDs using primitive values per unit (EF6-compatible)
                var respondentComplaintIds = new List<int>();
                foreach (var unit in tenantUnits)
                {
                    var ids = db.TenantComplaints
                        .Where(t => t.IsActive && !t.IsDeleted &&
                                    t.RespondentComplexId == unit.ComplexId &&
                                    t.RespondentUnitNumber == unit.UnitNumber &&
                                    (t.RespondentBlockNumber == null ||
                                     t.RespondentBlockNumber == unit.BlockNumber ||
                                     unit.BlockNumber == null ||
                                     unit.BlockNumber == ""))
                        .Select(t => t.Id)
                        .ToList();
                    respondentComplaintIds.AddRange(ids);
                }
                respondentComplaintIds = respondentComplaintIds.Distinct().ToList();

                complaints = db.TenantComplaints
                    .Include(t => t.ComplainantComplex)
                    .Include(t => t.RespondentComplex)
                    .Include(t => t.ComplaintCategory)
                    .Include(t => t.ComplaintType)
                    .Include(t => t.Status)
                    .Where(t => t.IsActive && !t.IsDeleted &&
                               (t.SubmittedByCustomerId == customer.Id ||
                                respondentComplaintIds.Contains(t.Id)))
                    .OrderByDescending(t => t.DateSubmitted);
            }

            ViewBag.SLAStats = slaEngine.GetSLAStatistics();
            return View(complaints.ToList());
        }

        // GET: Complaints/Create
        [Authorize(Roles = "Client Services Officer,Customers")]
        public ActionResult Create()
        {
            ViewBag.ComplaintCategoryId = new SelectList(db.ComplaintCategories.Where(x => x.IsActive), "Id", "Name");
            ViewBag.ComplexList = new SelectList(db.PreferredComplexAreas.OrderBy(a => a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex") && a.IsActive), "Id", "Name");
            return View();
        }

        // POST: Complaints/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client Services Officer,Customers")]
        public ActionResult Create(TenantComplaint complaint, HttpPostedFileBase[] evidenceFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var systemUser = identityManager.CurrentUser(User);
                    Customer customer = null;
                    if (systemUser != null)
                    {
                        var systemUserId = systemUser.Id;
                        customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                    }

                    complaint.CaseReferenceNumber = GenerateCaseReferenceNumber();
                    complaint.SubmittedByCustomerId = customer?.Id;
                    complaint.SubmittedByUserId = systemUser?.Id;
                    complaint.DateSubmitted = DateTime.Now;

                    var submittedStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Submitted);
                    complaint.StatusId = submittedStatus?.Id ?? 1;

                    complaint.IsActive = true;
                    complaint.IsDeleted = false;
                    complaint.CreatedDateTime = DateTime.Now;
                    complaint.DepartmentId = customer?.DepartmentId ?? 1;
                    complaint.WarningLetterCount = 0;

                    db.TenantComplaints.Add(complaint);
                    db.SaveChanges();

                    if (evidenceFiles != null && evidenceFiles.Length > 0)
                    {
                        SaveEvidenceFiles(complaint.Id, evidenceFiles, customer?.Id);
                    }

                    notificationEngine.SendComplaintAcknowledgement(complaint);
                    workflowEngine.RoundRobinComplaints(complaint.Id, ResponsibilityTypeKeys.ComplaintInvestigation);
                    workflowEngine.LogAuditTrail(complaint.Id, "Complaint Submitted",
                        $"Complaint {complaint.CaseReferenceNumber} submitted", customer?.Id);

                    ViewBag.MessageTitle = "Success";
                    ViewBag.MessageBody = $"Complaint submitted successfully. Your Case Reference Number is: <strong>{complaint.CaseReferenceNumber}</strong>";

                    return RedirectToAction("Index");
                }

                ViewBag.ComplaintCategoryId = new SelectList(db.ComplaintCategories.Where(x => x.IsActive), "Id", "Name", complaint.ComplaintCategoryId);
                ViewBag.ComplexList = new SelectList(db.PreferredComplexAreas.OrderBy(a => a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex") && a.IsActive), "Id", "Name");
                return View(complaint);
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while submitting the complaint. Please try again.";
                ViewBag.ComplaintCategoryId = new SelectList(db.ComplaintCategories.Where(x => x.IsActive), "Id", "Name", complaint.ComplaintCategoryId);
                ViewBag.ComplexList = new SelectList(db.PreferredComplexAreas.OrderBy(a => a.Name).Where(a => !a.Key.Equals("ekurhuleni_complex") && a.IsActive), "Id", "Name");
                return View(complaint);
            }
        }

        // GET: Complaints/Details/5
        [DecryptParameter]
        [Authorize(Roles = "Client Services Officer,Customers")]
        public ActionResult Details(int id)
        {
            var complaint = db.TenantComplaints
                .Include(t => t.ComplainantComplex)
                .Include(t => t.RespondentComplex)
                .Include(t => t.ComplaintCategory)
                .Include(t => t.ComplaintType)
                .Include(t => t.Status)
                .Include(t => t.Evidence)
                .Include(t => t.Investigations)
                .Include(t => t.AuditLogs)
                .FirstOrDefault(t => t.Id == id);

            if (complaint == null)
            {
                return HttpNotFound();
            }

            var systemUser = identityManager.CurrentUser(User);
            Customer customer = null;
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
            }

            if (!User.IsInRole("Client Services Officer"))
            {
                var tenantUnits = db.MatchedUnits
                    .Where(mu => mu.CustomerId == customer.Id && mu.ApplicationAllocatedPropertyId != null)
                    .Select(mu => mu.ApplicationAllocatedProperty)
                    .Where(aap => aap != null)
                    .Select(aap => new { ComplexId = aap.OfferedComplexId, UnitNumber = aap.SpaceUnitNumber, BlockNumber = aap.BuildingName })
                    .ToList();

                bool isRespondent = false;
                foreach (var unit in tenantUnits)
                {
                    if (unit.ComplexId == complaint.RespondentComplexId &&
                        unit.UnitNumber == complaint.RespondentUnitNumber &&
                        (string.IsNullOrEmpty(unit.BlockNumber) || unit.BlockNumber == complaint.RespondentBlockNumber))
                    {
                        isRespondent = true;
                        break;
                    }
                }

                if (complaint.SubmittedByCustomerId != customer?.Id && !isRespondent)
                {
                    return new HttpUnauthorizedResult();
                }
            }

            ViewBag.SLAStatus = slaEngine.GetSLAStatus(complaint);
            ViewBag.DaysOpen = slaEngine.GetDaysOpen(complaint);
            return View(complaint);
        }

        // GET: Complaints/ScheduleAppointment/5
        [DecryptParameter]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult ScheduleAppointment(int id)
        {
            var complaint = db.TenantComplaints
                .Include(t => t.Investigations)
                .FirstOrDefault(t => t.Id == id);

            if (complaint == null)
            {
                return HttpNotFound();
            }

            var investigation = complaint.Investigations.FirstOrDefault() ?? new ComplaintInvestigation
            {
                TenantComplaintId = id
            };

            ViewBag.ComplaintCaseNumber = complaint.CaseReferenceNumber;
            ViewBag.Complaint = complaint;
            return View(investigation);
        }

        // POST: Complaints/ScheduleAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult ScheduleAppointment(ComplaintInvestigation investigation)
        {
            try
            {
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                var existingInvestigation = db.ComplaintInvestigations
                    .FirstOrDefault(i => i.TenantComplaintId == investigation.TenantComplaintId);

                if (existingInvestigation == null)
                {
                    investigation.ScheduledById = customer?.Id;
                    investigation.DateScheduled = DateTime.Now;
                    investigation.IsActive = true;
                    investigation.IsDeleted = false;
                    investigation.CreatedDateTime = DateTime.Now;
                    investigation.DepartmentId = customer?.DepartmentId ?? 1;

                    db.ComplaintInvestigations.Add(investigation);
                }
                else
                {
                    existingInvestigation.AppointmentDate = investigation.AppointmentDate;
                    existingInvestigation.AppointmentTime = investigation.AppointmentTime;
                    existingInvestigation.ScheduledById = customer?.Id;
                    existingInvestigation.DateScheduled = DateTime.Now;
                    existingInvestigation.ModifiedDateTime = DateTime.Now;
                }

                var complaint = db.TenantComplaints.Find(investigation.TenantComplaintId);
                var awaitingStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.AwaitingInvestigation);
                complaint.StatusId = awaitingStatus?.Id ?? complaint.StatusId;

                db.SaveChanges();

                notificationEngine.SendAppointmentNotification(complaint, existingInvestigation ?? investigation);
                workflowEngine.LogAuditTrail(complaint.Id, "Appointment Scheduled", 
                    $"Investigation appointment scheduled for {investigation.AppointmentDate?.ToString("dd MMM yyyy")}", customer?.Id);

                ViewBag.MessageTitle = "Success";
                ViewBag.MessageBody = "Investigation appointment scheduled successfully.";

                return RedirectToAction("Details", new { q = new AesCrypto().Encrypt("id=" + investigation.TenantComplaintId) });
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while scheduling the appointment.";
                return View(investigation);
            }
        }

        // GET: Complaints/ConfirmAppointment/5
        [DecryptParameter]
        [Authorize(Roles = "Customers")]
        public ActionResult ConfirmAppointment(int id)
        {
            var complaint = db.TenantComplaints
                .Include(t => t.Investigations)
                .FirstOrDefault(t => t.Id == id);

            if (complaint == null)
            {
                return HttpNotFound();
            }

            var investigation = complaint.Investigations.FirstOrDefault();
            if (investigation == null)
            {
                return HttpNotFound();
            }

            ViewBag.ComplaintCaseNumber = complaint.CaseReferenceNumber;
            return View(investigation);
        }

        // POST: Complaints/ConfirmAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customers")]
        public ActionResult ConfirmAppointment(int id, string action, DateTime? alternativeDate, TimeSpan? alternativeTime, string alternativeReason)
        {
            try
            {
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                var investigation = db.ComplaintInvestigations.Find(id);
                if (investigation == null)
                {
                    return HttpNotFound();
                }

                if (action == "accept")
                {
                    investigation.RespondentConfirmed = true;
                    investigation.DateConfirmed = DateTime.Now;
                    investigation.ModifiedDateTime = DateTime.Now;
                    investigation.AlternativeApproved = null;

                    db.SaveChanges();

                    notificationEngine.SendAppointmentConfirmationToCSO(investigation);
                    workflowEngine.LogAuditTrail(investigation.TenantComplaintId, "Appointment Confirmed", 
                        $"Respondent confirmed appointment for {investigation.AppointmentDate?.ToString("dd MMM yyyy")}", customer?.Id);

                    ViewBag.MessageTitle = "Success";
                    ViewBag.MessageBody = "Appointment confirmed successfully.";
                }
                else if (action == "propose_alternative")
                {
                    investigation.ProposedAlternativeDate = alternativeDate;
                    investigation.ProposedAlternativeTime = alternativeTime;
                    investigation.AlternativeDateReason = alternativeReason;
                    investigation.AlternativeApproved = null;
                    investigation.ModifiedDateTime = DateTime.Now;

                    db.SaveChanges();

                    notificationEngine.SendAppointmentConfirmationToCSO(investigation);
                    workflowEngine.LogAuditTrail(investigation.TenantComplaintId, "Alternative Date Proposed", 
                        $"Respondent proposed alternative date: {alternativeDate?.ToString("dd MMM yyyy")}", customer?.Id);

                    ViewBag.MessageTitle = "Success";
                    ViewBag.MessageBody = "Alternative appointment date proposed successfully. The Client Services Officer will review your request.";
                }

                return RedirectToAction("Details", new { q = new AesCrypto().Encrypt("id=" + investigation.TenantComplaintId) });
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while confirming the appointment.";
                return RedirectToAction("Details", new { q = new AesCrypto().Encrypt("id=" + id) });
            }
        }

        // POST: Complaints/ApproveAlternativeDate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult ApproveAlternativeDate(int id, bool approve)
        {
            try
            {
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                var investigation = db.ComplaintInvestigations.Find(id);
                if (investigation == null)
                {
                    return HttpNotFound();
                }

                investigation.AlternativeApproved = approve;

                if (approve && investigation.ProposedAlternativeDate.HasValue)
                {
                    investigation.AppointmentDate = investigation.ProposedAlternativeDate;
                    investigation.AppointmentTime = investigation.ProposedAlternativeTime;
                    investigation.RespondentConfirmed = true;
                    investigation.DateConfirmed = DateTime.Now;
                }

                investigation.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();

                workflowEngine.LogAuditTrail(investigation.TenantComplaintId, 
                    approve ? "Alternative Date Approved" : "Alternative Date Rejected",
                    $"CSO {(approve ? "approved" : "rejected")} alternative appointment date", customer?.Id);

                return Json(new { success = true, message = approve ? "Alternative date approved" : "Alternative date rejected" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }

        // GET: Complaints/CaptureOutcome/5
        [DecryptParameter]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult CaptureOutcome(int id)
        {
            var complaint = db.TenantComplaints
                .Include(t => t.Investigations)
                .Include(t => t.ComplaintType)
                .FirstOrDefault(t => t.Id == id);

            if (complaint == null)
            {
                return HttpNotFound();
            }

            var investigation = complaint.Investigations.FirstOrDefault();
            if (investigation == null)
            {
                return HttpNotFound();
            }

            ViewBag.ComplaintCaseNumber = complaint.CaseReferenceNumber;
            ViewBag.WarningLetterCount = complaint.WarningLetterCount;
            ViewBag.IsSubLetting = workflowEngine.IsSubLettingComplaint(complaint);
            ViewBag.OutcomeOptions = new SelectList(new[]
            {
                new { Value = ComplaintOutcomeKeys.Resolved, Text = "Resolved" },
                new { Value = ComplaintOutcomeKeys.Referral, Text = "Referral" },
                new { Value = ComplaintOutcomeKeys.Unresolved, Text = "Unresolved" }
            }, "Value", "Text");

            return View(investigation);
        }

        // POST: Complaints/CaptureOutcome
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult CaptureOutcome(ComplaintInvestigation investigation, HttpPostedFileBase[] outcomeDocuments, 
            string agencyName, string agencyAddress, string agencyContact, string agencyEmail, string agencyDescription,
            bool sendWarningLetter = false)
        {
            try
            {
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                var existingInvestigation = db.ComplaintInvestigations.Find(investigation.Id);
                if (existingInvestigation == null)
                {
                    return HttpNotFound();
                }

                existingInvestigation.Outcome = investigation.Outcome;
                existingInvestigation.OutcomeDetails = investigation.OutcomeDetails;
                existingInvestigation.OutcomeDate = DateTime.Now;
                existingInvestigation.InvestigatedById = customer?.Id;
                existingInvestigation.ModifiedDateTime = DateTime.Now;

                var complaint = db.TenantComplaints
                    .Include(c => c.ComplaintType)
                    .FirstOrDefault(c => c.Id == existingInvestigation.TenantComplaintId);

                if (investigation.Outcome == ComplaintOutcomeKeys.Referral)
                {
                    var referral = new ComplaintExternalReferral
                    {
                        AgencyName = agencyName,
                        Address = agencyAddress,
                        ContactNumber = agencyContact,
                        ContactEmail = agencyEmail,
                        BriefDescription = agencyDescription,
                        ReferredById = customer?.Id,
                        DateReferred = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDateTime = DateTime.Now,
                        DepartmentId = customer?.DepartmentId ?? 1
                    };

                    db.ComplaintExternalReferrals.Add(referral);
                    db.SaveChanges();

                    existingInvestigation.ExternalReferralId = referral.Id;
                }

                Status outcomeStatus = null;

                switch (investigation.Outcome)
                {
                    case ComplaintOutcomeKeys.Resolved:
                        outcomeStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Resolved);

                        if (sendWarningLetter && complaint != null)
                        {
                            complaint.WarningLetterCount++;
                            complaint.LastWarningDate = DateTime.Now;
                            notificationEngine.SendWarningLetter(complaint, complaint.WarningLetterCount);

                            if (complaint.WarningLetterCount >= 3)
                            {
                                workflowEngine.LogAuditTrail(complaint.Id, "Third Warning Sent", 
                                    "Third warning letter sent - escalation required", customer?.Id);
                            }
                        }

                        if (workflowEngine.IsSubLettingComplaint(complaint))
                        {
                            workflowEngine.TriggerLeaseTermination(complaint);
                            complaint.LeaseTerminationTriggered = true;
                            complaint.LeaseTerminationDate = DateTime.Now;

                            workflowEngine.LogAuditTrail(complaint.Id, "Lease Termination Triggered", 
                                "Sub-letting confirmed - lease termination process initiated", customer?.Id);
                        }
                        break;

                    case ComplaintOutcomeKeys.Referral:
                        outcomeStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Referred);
                        break;

                    case ComplaintOutcomeKeys.Unresolved:
                        outcomeStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Unresolved);

                        workflowEngine.TriggerLeaseTermination(complaint);
                        complaint.LeaseTerminationTriggered = true;
                        complaint.LeaseTerminationDate = DateTime.Now;

                        workflowEngine.LogAuditTrail(complaint.Id, "Lease Termination Triggered", 
                            "Complaint unresolved - lease termination process initiated", customer?.Id);
                        break;
                }

                if (outcomeStatus != null)
                {
                    complaint.StatusId = outcomeStatus.Id;
                }

                if (outcomeDocuments != null && outcomeDocuments.Length > 0)
                {
                    SaveInvestigationDocuments(existingInvestigation.Id, outcomeDocuments, customer?.Id);
                }

                db.SaveChanges();

                notificationEngine.SendOutcomeNotification(complaint, existingInvestigation);
                workflowEngine.LogAuditTrail(complaint.Id, "Outcome Captured", 
                    $"Investigation outcome: {investigation.Outcome}", customer?.Id);

                if (customer != null)
                {
                    workflowEngine.RoundRobinMarkFinished(customer.Id, ResponsibilityTypeKeys.ComplaintInvestigation);
                }

                ViewBag.MessageTitle = "Success";
                ViewBag.MessageBody = "Investigation outcome captured successfully.";

                return RedirectToAction("Details", new { q = new AesCrypto().Encrypt("id=" + existingInvestigation.TenantComplaintId) });
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while capturing the outcome.";
                return View(investigation);
            }
        }

        // GET: Get complaint types by category (AJAX)
        public JsonResult GetComplaintTypes(int categoryId)
        {
            var types = db.ComplaintTypes
                .Where(t => t.ComplaintCategoryId == categoryId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .Select(t => new
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToList();

            return Json(types, JsonRequestBehavior.AllowGet);
        }

        #region Helper Methods

        private string GenerateCaseReferenceNumber()
        {
            string caseRef;
            do
            {
                var year = DateTime.Now.Year;
                var randomNumber = random.Next(100000, 999999);
                caseRef = $"COMP{year}{randomNumber}";
            }
            while (db.TenantComplaints.Any(tc => tc.CaseReferenceNumber == caseRef));

            return caseRef;
        }

        private void SaveEvidenceFiles(int complaintId, HttpPostedFileBase[] files, int? uploadedById)
        {
            var uploadPath = Server.MapPath("~/UploadedFiles/ComplaintEvidence");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in files.Where(f => f != null && f.ContentLength > 0))
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".mp4", ".avi" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    continue;

                if (file.ContentLength > 10 * 1024 * 1024)
                    continue;

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);
                file.SaveAs(filePath);

                var evidence = new ComplaintEvidence
                {
                    TenantComplaintId = complaintId,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FilePath = filePath,
                    FileSize = file.ContentLength,
                    UploadedById = uploadedById,
                    UploadDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.Now,
                    DepartmentId = 1
                };

                db.ComplaintEvidences.Add(evidence);
            }

            db.SaveChanges();
        }

        private void SaveInvestigationDocuments(int investigationId, HttpPostedFileBase[] files, int? uploadedById)
        {
            var uploadPath = Server.MapPath("~/UploadedFiles/ComplaintInvestigation");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in files.Where(f => f != null && f.ContentLength > 0))
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    continue;

                if (file.ContentLength > 10 * 1024 * 1024)
                    continue;

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);
                file.SaveAs(filePath);

                var document = new ComplaintInvestigationDocument
                {
                    ComplaintInvestigationId = investigationId,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FilePath = filePath,
                    FileSize = file.ContentLength,
                    UploadedById = uploadedById,
                    UploadDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDateTime = DateTime.Now,
                    DepartmentId = 1
                };

                db.ComplaintInvestigationDocuments.Add(document);
            }

            db.SaveChanges();
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
