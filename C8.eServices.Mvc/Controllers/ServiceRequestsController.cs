using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Engines;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly eServicesDbContext _db;
        private readonly ServiceRequestEngine _engine;

        public ServiceRequestsController()
        {
            _db = new eServicesDbContext();
            _engine = new ServiceRequestEngine(_db);
        }

        /// <summary>
        /// GET: ServiceRequests - List all service requests (UC17E Step 4)
        /// Tenants see only their own requests; CSO sees all
        /// </summary>
        public ActionResult Index()
        {
            var identityManager = new IdentityManager();
            var systemUser = identityManager.CurrentUser(User);
            Customer customer = null;
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
            }

            IQueryable<ServiceRequest> query = _db.ServiceRequests
                .Where(sr => sr.IsActive && !sr.IsDeleted);

            // Tenants see only their own requests; CSO sees all
            if (!User.IsInRole("Client Services Officer") && customer != null)
            {
                query = query.Where(sr => sr.CreatedByCustomerId == customer.Id);
            }

            var requests = query.OrderByDescending(sr => sr.DateSubmitted).ToList();

            // Check SLA escalation for each request (BR34)
            foreach (var request in requests)
            {
                if (!request.EscalationTriggered && _engine.ShouldEscalate(request))
                {
                    request.EscalationTriggered = true;
                    request.ModifiedDateTime = DateTime.Now;
                    _engine.LogAuditTrail(request.Id, "SLA Escalation Triggered",
                        "50% of SLA resolution time has elapsed - escalation flagged (BR34)",
                        null);
                }
            }
            _db.SaveChanges();

            return View(requests);
        }

        /// <summary>
        /// GET: ServiceRequests/Create - Display form to capture new service request (UC17D Step 4-5)
        /// </summary>
        public ActionResult Create()
        {
            PopulateDropdowns();

            // Pre-populate tenant details if logged in as tenant
            var identityManager = new IdentityManager();
            var systemUser = identityManager.CurrentUser(User);
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                var customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                if (customer != null)
                {
                    ViewBag.ReportedByName = customer.FirstName;
                    ViewBag.ReportedBySurname = customer.LastName;
                    ViewBag.ContactNumber = customer.CellPhoneNumber ?? "";
                    ViewBag.EmailAddress = customer.EmailAddress ?? "";
                }
            }

            return View();
        }

        /// <summary>
        /// POST: ServiceRequests/Create - Submit new service request (UC17D Step 17-20)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceRequest model, HttpPostedFileBase[] documents)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PopulateDropdowns();
                    return View(model);
                }

                // Generate ticket reference number (EHC_SR_###_YYYY)
                model.RequestReferenceNumber = _engine.GenerateRequestReferenceNumber();

                // Set initial status to Open (BR36)
                var openStatus = _db.Status.FirstOrDefault(s => s.Key == ServiceRequestStatusKeys.Open);
                if (openStatus != null)
                {
                    model.StatusId = openStatus.Id;
                }

                // Set dates and tracking
                model.DateSubmitted = DateTime.Now;
                model.EscalationTriggered = false;
                model.IsActive = true;
                model.IsDeleted = false;
                model.CreatedDateTime = DateTime.Now;
                model.DepartmentId = 1;

                // Track the creator (only creator can edit/delete per UC17E Note 1)
                var identityManager = new IdentityManager();
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }
                model.CreatedByCustomerId = customer?.Id;

                // Calculate SLA deadlines (BR23, BR24, BR34)
                _engine.CalculateSlaDeadlines(model);

                // Save service request
                _db.ServiceRequests.Add(model);
                _db.SaveChanges();

                // Save uploaded documents (UC17D Step 13-16)
                if (documents != null && documents.Length > 0)
                {
                    SaveDocuments(model.Id, documents);
                }

                // Log audit trail
                _engine.LogAuditTrail(model.Id, "Service Request Created",
                    $"Request {model.RequestReferenceNumber} created by {model.ReportedByName} {model.ReportedBySurname}",
                    model.CreatedByCustomerId);

                // Send email notification with ticket reference (UC17D Step 20)
                _engine.SendCreationNotification(model);

                TempData["SuccessMessage"] = $"Service request {model.RequestReferenceNumber} created successfully.";
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the service request: " + ex.Message);
                PopulateDropdowns();
                return View(model);
            }
        }

        /// <summary>
        /// GET: ServiceRequests/Details - View service request details (UC17E Step 5-6)
        /// Displays: Request #, Creation Date, Category, Priority, Description, Status, History, Attachments
        /// </summary>
        [EncryptedActionParameter]
        public ActionResult Details(int id)
        {
            var serviceRequest = _db.ServiceRequests.Find(id);
            if (serviceRequest == null)
            {
                return HttpNotFound();
            }

            // Check if current user can view this request
            var identityManager = new IdentityManager();
            var systemUser = identityManager.CurrentUser(User);
            Customer customer = null;
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
            }

            // Tenants can only view their own requests
            if (!User.IsInRole("Client Services Officer") && customer != null
                && serviceRequest.CreatedByCustomerId != customer.Id)
            {
                return new HttpStatusCodeResult(403, "You do not have permission to view this request.");
            }

            ViewBag.CanEditOrDelete = _engine.CanEditOrDelete(id)
                && customer != null
                && _engine.IsCreator(id, customer.Id);
            ViewBag.SlaStatus = _engine.GetSlaStatus(serviceRequest);

            return View(serviceRequest);
        }

        /// <summary>
        /// GET: ServiceRequests/Edit - Edit an open service request (UC17E Step 7-8)
        /// Only creator can edit; only when status = Open (BR37)
        /// </summary>
        [EncryptedActionParameter]
        public ActionResult Edit(int id)
        {
            var serviceRequest = _db.ServiceRequests.Find(id);
            if (serviceRequest == null)
            {
                return HttpNotFound();
            }

            // Validate: only creator can edit (UC17E Note 1)
            var identityManager = new IdentityManager();
            var systemUser = identityManager.CurrentUser(User);
            Customer customer = null;
            if (systemUser != null)
            {
                var systemUserId = systemUser.Id;
                customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
            }

            if (customer == null || !_engine.IsCreator(id, customer.Id))
            {
                TempData["ErrorMessage"] = "Only the creator of the service request can edit it.";
                return RedirectToAction("Details", new { id = id });
            }

            // Validate: status must be Open (BR37)
            if (!_engine.CanEditOrDelete(id))
            {
                TempData["ErrorMessage"] = "This service request can no longer be edited. Only requests with Open status can be modified.";
                return RedirectToAction("Details", new { id = id });
            }

            PopulateDropdowns(serviceRequest);
            return View(serviceRequest);
        }

        /// <summary>
        /// POST: ServiceRequests/Edit - Save edited service request (UC17E Step 9-14)
        /// Editable fields: Category, Priority, Description
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EncryptedActionParameter]
        public ActionResult Edit(int id, ServiceRequest model)
        {
            try
            {
                var serviceRequest = _db.ServiceRequests.Find(id);
                if (serviceRequest == null)
                {
                    return HttpNotFound();
                }

                // Validate: only creator can edit (UC17E Note 1)
                var identityManager = new IdentityManager();
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                if (customer == null || !_engine.IsCreator(id, customer.Id))
                {
                    TempData["ErrorMessage"] = "Only the creator of the service request can edit it.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Validate: status must be Open (BR37)
                if (!_engine.CanEditOrDelete(id))
                {
                    TempData["ErrorMessage"] = "This service request can no longer be edited. Only requests with Open status can be modified.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Track changes for audit trail
                var changes = new List<string>();
                if (serviceRequest.ServiceRequestCategoryId != model.ServiceRequestCategoryId)
                {
                    changes.Add("Category changed");
                    serviceRequest.ServiceRequestCategoryId = model.ServiceRequestCategoryId;
                }
                if (serviceRequest.ServiceRequestPriorityId != model.ServiceRequestPriorityId)
                {
                    changes.Add("Priority changed");
                    serviceRequest.ServiceRequestPriorityId = model.ServiceRequestPriorityId;
                    // Recalculate SLA deadlines when priority changes
                    _engine.CalculateSlaDeadlines(serviceRequest);
                }
                if (serviceRequest.DetailedDescription != model.DetailedDescription)
                {
                    changes.Add("Description updated");
                    serviceRequest.DetailedDescription = model.DetailedDescription;
                }

                serviceRequest.ModifiedDateTime = DateTime.Now;
                _db.SaveChanges();

                // Log audit trail
                _engine.LogAuditTrail(id, "Service Request Edited",
                    $"Changes: {string.Join(", ", changes)}",
                    customer.Id);

                TempData["SuccessMessage"] = "Service request updated successfully.";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the service request: " + ex.Message);
                var serviceRequest = _db.ServiceRequests.Find(id);
                PopulateDropdowns(serviceRequest);
                return View(serviceRequest);
            }
        }

        /// <summary>
        /// POST: ServiceRequests/Delete - Delete (soft-delete) an open service request (UC17E Alt Flow 2)
        /// Only creator can delete; only when status = Open (BR37); requires reason
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EncryptedActionParameter]
        public ActionResult Delete(int id, string deletionReason)
        {
            try
            {
                var serviceRequest = _db.ServiceRequests.Find(id);
                if (serviceRequest == null)
                {
                    return HttpNotFound();
                }

                // Validate: only creator can delete (UC17E Note 1)
                var identityManager = new IdentityManager();
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                if (customer == null || !_engine.IsCreator(id, customer.Id))
                {
                    TempData["ErrorMessage"] = "Only the creator of the service request can delete it.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Validate: status must be Open (BR37)
                if (!_engine.CanEditOrDelete(id))
                {
                    TempData["ErrorMessage"] = "This service request can no longer be deleted. Only requests with Open status can be deleted.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Validate: reason is mandatory (UC17E Step 9)
                if (string.IsNullOrWhiteSpace(deletionReason))
                {
                    TempData["ErrorMessage"] = "A reason for deletion is required.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Set status to Deleted (BR36)
                var deletedStatus = _db.Status.FirstOrDefault(s => s.Key == ServiceRequestStatusKeys.Deleted);
                if (deletedStatus != null)
                {
                    serviceRequest.StatusId = deletedStatus.Id;
                }

                serviceRequest.DeletionReason = deletionReason;
                serviceRequest.IsDeleted = true;
                serviceRequest.ModifiedDateTime = DateTime.Now;
                _db.SaveChanges();

                // Log audit trail
                _engine.LogAuditTrail(id, "Service Request Deleted",
                    $"Request {serviceRequest.RequestReferenceNumber} deleted. Reason: {deletionReason}",
                    customer.Id);

                TempData["SuccessMessage"] = $"Service request {serviceRequest.RequestReferenceNumber} has been deleted.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the service request: " + ex.Message;
                return RedirectToAction("Details", new { id = id });
            }
        }

        /// <summary>
        /// POST: ServiceRequests/UpdateStatus - Update service request status (for CSO use)
        /// Supports transitions: Open → In Progress, In Progress → Resolved, Resolved → Closed
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client Services Officer")]
        public ActionResult UpdateStatus(int id, string newStatusKey)
        {
            try
            {
                var serviceRequest = _db.ServiceRequests.Find(id);
                if (serviceRequest == null)
                {
                    return HttpNotFound();
                }

                var identityManager = new IdentityManager();
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }

                _engine.UpdateStatus(id, newStatusKey);

                var statusName = _db.Status.FirstOrDefault(s => s.Key == newStatusKey)?.Name ?? newStatusKey;
                _engine.LogAuditTrail(id, "Status Updated",
                    $"Status changed to {statusName}",
                    customer?.Id);

                TempData["SuccessMessage"] = $"Status updated to {statusName}.";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the status: " + ex.Message;
                return RedirectToAction("Details", new { id = id });
            }
        }

        /// <summary>
        /// Helper to populate dropdown lists
        /// </summary>
        private void PopulateDropdowns(ServiceRequest existing = null)
        {
            ViewBag.Categories = new SelectList(
                _db.ServiceRequestCategories.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.DisplayOrder),
                "Id", "Name",
                existing?.ServiceRequestCategoryId);

            ViewBag.Priorities = new SelectList(
                _db.ServiceRequestPriorities.Where(p => p.IsActive && !p.IsDeleted).OrderBy(p => p.Level),
                "Id", "Name",
                existing?.ServiceRequestPriorityId);

            ViewBag.Complexes = new SelectList(
                _db.PreferredComplexAreas.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.Name),
                "Id", "Name",
                existing?.ComplexId);
        }

        /// <summary>
        /// Helper to save uploaded documents (UC17D Step 13-16)
        /// </summary>
        private void SaveDocuments(int serviceRequestId, HttpPostedFileBase[] documents)
        {
            var uploadDirectory = Server.MapPath("~/Uploads/ServiceRequests/Documents/");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            foreach (var file in documents)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // Validate file size (10MB limit)
                    if (file.ContentLength > 10 * 1024 * 1024)
                    {
                        continue;
                    }

                    // Validate file extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        continue;
                    }

                    var fileName = $"{serviceRequestId}_{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadDirectory, fileName);
                    file.SaveAs(filePath);

                    var document = new ServiceRequestDocument
                    {
                        ServiceRequestId = serviceRequestId,
                        FileName = file.FileName,
                        FilePath = filePath,
                        FileType = file.ContentType,
                        FileSize = file.ContentLength,
                        UploadedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDateTime = DateTime.Now,
                        DepartmentId = 1
                    };

                    _db.ServiceRequestDocuments.Add(document);
                }
            }

            _db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
