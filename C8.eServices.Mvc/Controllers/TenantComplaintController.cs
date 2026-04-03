using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class TenantComplaintController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        private static Random random = new Random();
        private IdentityManager identityManager;

        public TenantComplaintController()
        {
            identityManager = new IdentityManager(db);
        }

        // GET: TenantComplaint/Index
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
                // CSO sees all complaints
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
                // Customers see only their complaints
                complaints = db.TenantComplaints
                    .Include(t => t.ComplainantComplex)
                    .Include(t => t.RespondentComplex)
                    .Include(t => t.ComplaintCategory)
                    .Include(t => t.ComplaintType)
                    .Include(t => t.Status)
                    .Where(t => t.IsActive && !t.IsDeleted && t.SubmittedByCustomerId == customer.Id)
                    .OrderByDescending(t => t.DateSubmitted);
            }

            return View(complaints.ToList());
        }

        // GET: TenantComplaint/Create
        [Authorize(Roles = "Client Services Officer,Customers")]
        public ActionResult Create()
        {
            ViewBag.ComplaintCategoryId = new SelectList(db.ComplaintCategories.Where(x => x.IsActive), "Id", "Name");
            ViewBag.ComplexList = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive), "Id", "ComplexName");
            return View();
        }

        // POST: TenantComplaint/Create
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

                    // Generate Case Reference Number
                    complaint.CaseReferenceNumber = GenerateCaseReferenceNumber();

                    // Set submission details
                    complaint.SubmittedByCustomerId = customer?.Id;
                    complaint.SubmittedByUserId = systemUser?.Id;
                    complaint.DateSubmitted = DateTime.Now;

                    // Set initial status to Submitted
                    var submittedStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Submitted);
                    complaint.StatusId = submittedStatus?.Id ?? 1;

                    // Set base model fields
                    complaint.IsActive = true;
                    complaint.IsDeleted = false;
                    complaint.CreatedDateTime = DateTime.Now;
                    complaint.DepartmentId = customer?.DepartmentId ?? 1;

                    db.TenantComplaints.Add(complaint);
                    db.SaveChanges();

                    // Handle evidence file uploads
                    if (evidenceFiles != null && evidenceFiles.Length > 0)
                    {
                        SaveEvidenceFiles(complaint.Id, evidenceFiles, customer?.Id);
                    }

                    // Send acknowledgement notification to complainant
                    SendAcknowledgementNotification(complaint);

                    // Assign to Client Services Officer via Round Robin
                    AssignComplaintToCSO(complaint.Id);

                    ViewBag.MessageTitle = "Success";
                    ViewBag.MessageBody = $"Complaint submitted successfully. Your Case Reference Number is: <strong>{complaint.CaseReferenceNumber}</strong>";
                    ViewBag.Message = ViewBag.MessageBody;

                    return RedirectToAction("Index");
                }

                ViewBag.ComplaintCategoryId = new SelectList(db.ComplaintCategories.Where(x => x.IsActive), "Id", "Name", complaint.ComplaintCategoryId);
                ViewBag.ComplexList = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive), "Id", "ComplexName");
                return View(complaint);
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while submitting the complaint. Please try again.";
                ViewBag.ComplaintCategoryId = new SelectList(db.ComplaintCategories.Where(x => x.IsActive), "Id", "Name", complaint.ComplaintCategoryId);
                ViewBag.ComplexList = new SelectList(db.PreferredComplexAreas.Where(x => x.IsActive), "Id", "ComplexName");
                return View(complaint);
            }
        }

        // GET: TenantComplaint/Details/5
        [EncryptedActionParameter]
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
                .FirstOrDefault(t => t.Id == id);

            if (complaint == null)
            {
                return HttpNotFound();
            }

            // Check if user has permission to view this complaint
            var systemUser = identityManager.CurrentUser(User);
            Customer customer = null;
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
            }

            if (!User.IsInRole("Client Services Officer") && complaint.SubmittedByCustomerId != customer?.Id)
            {
                return new HttpUnauthorizedResult();
            }

            return View(complaint);
        }

        // GET: TenantComplaint/ScheduleAppointment/5
        [EncryptedActionParameter]
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
            return View(investigation);
        }

        // POST: TenantComplaint/ScheduleAppointment
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

                // Update complaint status
                var complaint = db.TenantComplaints.Find(investigation.TenantComplaintId);
                var awaitingStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.AwaitingInvestigation);
                complaint.StatusId = awaitingStatus?.Id ?? complaint.StatusId;

                db.SaveChanges();

                // Send notification to respondent
                SendAppointmentNotification(complaint, investigation);

                ViewBag.MessageTitle = "Success";
                ViewBag.MessageBody = "Investigation appointment scheduled successfully.";

                return RedirectToAction("Details", new { id = investigation.TenantComplaintId });
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while scheduling the appointment.";
                return View(investigation);
            }
        }

        // GET: TenantComplaint/ConfirmAppointment/5
        [EncryptedActionParameter]
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

        // POST: TenantComplaint/ConfirmAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customers")]
        public ActionResult ConfirmAppointment(int id, string action)
        {
            try
            {
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

                    db.SaveChanges();

                    // Send confirmation notification to CSO
                    SendConfirmationNotificationToCSO(investigation);

                    ViewBag.MessageTitle = "Success";
                    ViewBag.MessageBody = "Appointment confirmed successfully.";
                }

                return RedirectToAction("Details", new { id = investigation.TenantComplaintId });
            }
            catch (Exception ex)
            {
                ViewBag.MessageTitle = "Error";
                ViewBag.MessageBody = "An error occurred while confirming the appointment.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // GET: TenantComplaint/CaptureOutcome/5
        [EncryptedActionParameter]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult CaptureOutcome(int id)
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
            ViewBag.OutcomeOptions = new SelectList(new[]
            {
                new { Value = ComplaintOutcomeKeys.Resolved, Text = "Resolved" },
                new { Value = ComplaintOutcomeKeys.Referral, Text = "Referral" },
                new { Value = ComplaintOutcomeKeys.Unresolved, Text = "Unresolved" }
            }, "Value", "Text");

            return View(investigation);
        }

        // POST: TenantComplaint/CaptureOutcome
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult CaptureOutcome(ComplaintInvestigation investigation, HttpPostedFileBase[] outcomeDocuments, 
            string agencyName, string agencyAddress, string agencyContact, string agencyEmail, string agencyDescription)
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

                // Handle Referral outcome
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

                // Update complaint status based on outcome
                var complaint = db.TenantComplaints.Find(existingInvestigation.TenantComplaintId);
                Status outcomeStatus = null;

                switch (investigation.Outcome)
                {
                    case ComplaintOutcomeKeys.Resolved:
                        outcomeStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Resolved);
                        break;
                    case ComplaintOutcomeKeys.Referral:
                        outcomeStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Referred);
                        break;
                    case ComplaintOutcomeKeys.Unresolved:
                        outcomeStatus = db.Status.FirstOrDefault(s => s.Key == ComplaintStatusKeys.Unresolved);
                        break;
                }

                if (outcomeStatus != null)
                {
                    complaint.StatusId = outcomeStatus.Id;
                }

                // Handle document uploads
                if (outcomeDocuments != null && outcomeDocuments.Length > 0)
                {
                    SaveInvestigationDocuments(existingInvestigation.Id, outcomeDocuments, customer?.Id);
                }

                db.SaveChanges();

                // Send outcome notification to complainant
                SendOutcomeNotification(complaint, existingInvestigation);

                ViewBag.MessageTitle = "Success";
                ViewBag.MessageBody = "Investigation outcome captured successfully.";

                return RedirectToAction("Details", new { id = existingInvestigation.TenantComplaintId });
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
            var year = DateTime.Now.Year;
            var randomNumber = random.Next(100000, 999999);
            return $"COMP{year}{randomNumber}";
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

        private void AssignComplaintToCSO(int complaintId)
        {
            // Get Client Services Officers
            var csoRole = db.Roles.FirstOrDefault(r => r.Name == "Client Services Officer");
            if (csoRole == null) return;

            var csoUsers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == csoRole.Id)).ToList();
            if (!csoUsers.Any()) return;

            // Simple round-robin assignment (you can enhance this logic)
            var assignedUser = csoUsers.OrderBy(u => Guid.NewGuid()).FirstOrDefault();
            if (assignedUser == null) return;

            // Find SystemUser using assignedUser.SystemUserId from AspNetUsers table
            var systemUser = db.SystemUsers.FirstOrDefault(su => su.Id == assignedUser.SystemUserId);
            if (systemUser == null) return;

            var customer = db.Customers.FirstOrDefault(c => c.SystemUserId == systemUser.Id);
            if (customer == null) return;

            var complaint = db.TenantComplaints.Find(complaintId);
            complaint.AssignedToId = customer.Id;
            complaint.DateAssigned = DateTime.Now;

            db.SaveChanges();

            // Send notification to assigned CSO
            SendAssignmentNotificationToCSO(complaint, customer);
        }

        private void SendAcknowledgementNotification(TenantComplaint complaint)
        {
            // TODO: Implement email/SMS notification
            // Use existing EmailHelper or create notification
        }

        private void SendAppointmentNotification(TenantComplaint complaint, ComplaintInvestigation investigation)
        {
            // TODO: Implement email/SMS notification to respondent
        }

        private void SendConfirmationNotificationToCSO(ComplaintInvestigation investigation)
        {
            // TODO: Implement email/SMS notification to CSO
        }

        private void SendAssignmentNotificationToCSO(TenantComplaint complaint, Customer cso)
        {
            // TODO: Implement email/SMS notification
        }

        private void SendOutcomeNotification(TenantComplaint complaint, ComplaintInvestigation investigation)
        {
            // TODO: Implement email/SMS notification to complainant
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
