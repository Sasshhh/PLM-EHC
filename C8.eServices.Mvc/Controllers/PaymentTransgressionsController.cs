using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Engines;
using C8.eServices.Mvc.Filters;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize(Roles = "Client Services Officer")]
    public class PaymentTransgressionsController : Controller
    {
        private readonly eServicesDbContext _db;
        private readonly PaymentTransgressionEngine _engine;
        private readonly PaymentTransgressionLetterEngine _letterEngine;

        public PaymentTransgressionsController()
        {
            _db = new eServicesDbContext();
            _engine = new PaymentTransgressionEngine(_db);
            _letterEngine = new PaymentTransgressionLetterEngine(_db);
        }

        /// <summary>
        /// GET: PaymentTransgressions - List all payment transgressions
        /// </summary>
        public ActionResult Index()
        {
            var transgressions = _db.PaymentTransgressions
                .Where(pt => pt.IsActive && !pt.IsDeleted)
                .OrderByDescending(pt => pt.DateSubmitted)
                .ToList();

            return View(transgressions);
        }

        /// <summary>
        /// GET: PaymentTransgressions/Create - Display form to create new payment transgression
        /// </summary>
        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(_db.PaymentTransgressionCategories.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.DisplayOrder), "Id", "Name");
            ViewBag.Severities = new SelectList(_db.PaymentTransgressionSeverities.Where(s => s.IsActive && !s.IsDeleted).OrderBy(s => s.Level), "Id", "Name");
            ViewBag.Complexes = new SelectList(_db.PreferredComplexAreas.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.Name), "Id", "Name");

            return View();
        }

        /// <summary>
        /// POST: PaymentTransgressions/Create - Submit new payment transgression
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PaymentTransgression model, HttpPostedFileBase[] uploadedFiles, string selectedLetterType)
        {
            try
            {
                // Remove server-generated fields from validation
                ModelState.Remove("CaseReferenceNumber");

                if (!ModelState.IsValid)
                {
                    var errorMessages = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Any())
                        {
                            errorMessages.Add(key + ": " + string.Join("; ", state.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message)));
                        }
                    }
                    
                    ModelState.AddModelError("", "VALIDATION ERROR: " + string.Join(" | ", errorMessages));
                    
                    ViewBag.Categories = new SelectList(_db.PaymentTransgressionCategories.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.DisplayOrder), "Id", "Name");
                    ViewBag.Severities = new SelectList(_db.PaymentTransgressionSeverities.Where(s => s.IsActive && !s.IsDeleted).OrderBy(s => s.Level), "Id", "Name");
                    ViewBag.Complexes = new SelectList(_db.PreferredComplexAreas.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.Name), "Id", "Name");
                    return View(model);
                }

                // Generate case reference number
                model.CaseReferenceNumber = _engine.GenerateCaseReferenceNumber();

                // Set initial status
                var submittedStatus = _db.Status.FirstOrDefault(s => s.Key == PaymentTransgressionStatusKeys.Submitted);
                if (submittedStatus != null)
                {
                    model.StatusId = submittedStatus.Id;
                }

                // Set dates and tracking
                model.DateSubmitted = DateTime.Now;
                model.LetterType = selectedLetterType;
                model.IsActive = true;
                model.IsDeleted = false;
                model.CreatedDateTime = DateTime.Now;
                model.DepartmentId = 1;

                // Get current CSO
                var identityManager = new IdentityManager();
                var systemUser = identityManager.CurrentUser(User);
                Customer customer = null;
                if (systemUser != null)
                {
                    var systemUserId = systemUser.Id;
                    customer = _db.Customers.FirstOrDefault(c => c.SystemUserId == systemUserId);
                }
                model.AssignedToCustomerId = customer?.Id;

                // Save payment transgression
                _db.PaymentTransgressions.Add(model);
                _db.SaveChanges();

                // Save uploaded documents
                if (uploadedFiles != null && uploadedFiles.Length > 0)
                {
                    SaveDocuments(model.Id, uploadedFiles);
                }

                // Log audit trail
                _engine.LogAuditTrail(model.Id, "Payment Transgression Created", 
                    $"Case {model.CaseReferenceNumber} created for tenant {model.TenantName} {model.TenantSurname}", 
                    model.AssignedToCustomerId ?? 0);

                // Redirect to generate letter
                TempData["SuccessMessage"] = $"Payment transgression case {model.CaseReferenceNumber} created successfully.";
                return RedirectToAction("GenerateLetter", new { q = new AesCrypto().Encrypt("id=" + model.Id) });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the payment transgression: " + ex.Message);
                ViewBag.Categories = new SelectList(_db.PaymentTransgressionCategories.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.DisplayOrder), "Id", "Name");
                ViewBag.Severities = new SelectList(_db.PaymentTransgressionSeverities.Where(s => s.IsActive && !s.IsDeleted).OrderBy(s => s.Level), "Id", "Name");
                ViewBag.Complexes = new SelectList(_db.PreferredComplexAreas.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.Name), "Id", "Name");
                return View(model);
            }
        }

        /// <summary>
        /// GET: PaymentTransgressions/GetTenantDetails - AJAX endpoint to auto-populate tenant details
        /// </summary>
        [HttpGet]
        public JsonResult GetTenantDetails(string officialNumber)
        {
            try
            {
                var tenantDetails = _engine.GetTenantDetailsByOfficialNumber(officialNumber);
                
                if (tenantDetails == null)
                {
                    return Json(new { success = false, message = "No tenant found with this Official Number" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        tenancyReferenceNumber = tenantDetails.ApplicationReferenceNumber,
                        tenantName = tenantDetails.FirstName,
                        tenantSurname = tenantDetails.LastName,
                        tenantEmail = tenantDetails.Email,
                        tenantCellphone = tenantDetails.PhoneNumber,
                        complexId = tenantDetails.ComplexId,
                        complexName = tenantDetails.ComplexName,
                        blockNumber = tenantDetails.BlockNumber,
                        unitNumber = tenantDetails.UnitNumber,
                        accountNumber = tenantDetails.CurrentAccountNumber,
                        lastPaymentAmount = tenantDetails.LastPaymentAmount,
                        lastPaymentDate = tenantDetails.LastPaymentDate?.ToString("yyyy-MM-dd"),
                        totalAmountDue = tenantDetails.TotalAmountDue
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving tenant details: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET: PaymentTransgressions/SearchTenants - AJAX endpoint to search active tenants
        /// </summary>
        [HttpGet]
        public JsonResult SearchTenants(string query)
        {
            try
            {
                var tenants = _engine.SearchTenants(query);
                return Json(new { success = true, data = tenants }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error searching tenants: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// GET: PaymentTransgressions/GetTypesByCategory - AJAX endpoint for cascading dropdown
        /// </summary>
        [HttpGet]
        public JsonResult GetTypesByCategory(int categoryId)
        {
            var types = _db.PaymentTransgressionTypes
                .Where(t => t.PaymentTransgressionCategoryId == categoryId && t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.DisplayOrder)
                .Select(t => new { t.Id, t.Name })
                .ToList();

            return Json(types, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// GET: PaymentTransgressions/Details - View payment transgression details
        /// </summary>
        [DecryptParameter]
        public ActionResult Details(int id)
        {
            var transgression = _db.PaymentTransgressions.Find(id);
            if (transgression == null)
            {
                return HttpNotFound();
            }

            return View(transgression);
        }

        /// <summary>
        /// GET: PaymentTransgressions/GenerateLetter - Display letter generation page
        /// </summary>
        [DecryptParameter]
        public ActionResult GenerateLetter(int id)
        {
            var transgression = _db.PaymentTransgressions.Find(id);
            if (transgression == null)
            {
                return HttpNotFound();
            }

            ViewBag.LetterTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Payment Transgression Notice", Value = PaymentTransgressionLetterTypes.PaymentTransgressionNotice },
                new SelectListItem { Text = "Written Warning Letter", Value = PaymentTransgressionLetterTypes.WrittenWarningLetter },
                new SelectListItem { Text = "Final Written Warning Letter", Value = PaymentTransgressionLetterTypes.FinalWrittenWarningLetter }
            };

            return View(transgression);
        }

        /// <summary>
        /// POST: PaymentTransgressions/GenerateLetter - Generate and send letter
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DecryptParameter]
        public ActionResult GenerateLetter(int id, string letterType)
        {
            try
            {
                var transgression = _db.PaymentTransgressions.Find(id);
                if (transgression == null)
                {
                    return HttpNotFound();
                }

                // Generate letter content
                var letterContent = _letterEngine.GenerateLetterContent(id, letterType);

                // Save letter as file
                var filePath = _letterEngine.SaveLetterAsFile(id, letterType, letterContent);

                // Update transgression
                transgression.LetterType = letterType;
                transgression.LetterGeneratedDate = DateTime.Now;
                transgression.LetterFilePath = filePath;

                var letterGeneratedStatus = _db.Status.FirstOrDefault(s => s.Key == PaymentTransgressionStatusKeys.LetterGenerated);
                if (letterGeneratedStatus != null)
                {
                    transgression.StatusId = letterGeneratedStatus.Id;
                }

                transgression.ModifiedDateTime = DateTime.Now;
                _db.SaveChanges();

                // Send notification to tenant
                _engine.SendTransgressionNotification(id, letterType);

                // Update status to Letter Sent
                var letterSentStatus = _db.Status.FirstOrDefault(s => s.Key == PaymentTransgressionStatusKeys.LetterSent);
                if (letterSentStatus != null)
                {
                    transgression.StatusId = letterSentStatus.Id;
                    transgression.LetterSentDate = DateTime.Now;
                }

                // TODO: Cesar Attachment Workflow
                // When letter templates are finalised, upgrade NotificationEngine to attach the generated HTML/PDF 
                // to the Cesar EmailAttachmentQueue so it gets physically emailed to the tenant.
                // Currently, only a plain text notification is sent.

                // Log audit trail
                _engine.LogAuditTrail(id, "Letter Generated and Sent", 
                    $"{letterType} generated and sent to tenant", 
                    transgression.AssignedToCustomerId ?? 0);

                // Check if Final Warning and trigger lease termination if necessary
                if (letterType == PaymentTransgressionLetterTypes.FinalWrittenWarningLetter)
                {
                    var count = _engine.GetTransgressionCountForTenant(transgression.OfficialNumber);
                    if (count >= 3)
                    {
                        _engine.TriggerLeaseTermination(id, "Final warning issued - 3+ payment transgressions recorded");
                    }
                }

                TempData["SuccessMessage"] = $"{letterType} generated and sent successfully to {transgression.TenantEmail}";
                return RedirectToAction("Details", new { q = new AesCrypto().Encrypt("id=" + id) });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while generating the letter: " + ex.Message;
                return RedirectToAction("Details", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + id) });
            }
        }

        /// <summary>
        /// GET: PaymentTransgressions/DownloadLetter
        /// Serves the generated letter file for viewing in the browser
        /// </summary>
        [HttpGet]
        [DecryptParameter]
        public ActionResult DownloadLetter(int id)
        {
            var transgression = _db.PaymentTransgressions.Find(id);
            if (transgression == null || string.IsNullOrEmpty(transgression.LetterFilePath))
            {
                return HttpNotFound("Letter not found.");
            }

            if (!System.IO.File.Exists(transgression.LetterFilePath))
            {
                return HttpNotFound("The physical letter file could not be found on the server.");
            }

            var mimeType = "text/html"; // Letters are currently generated as HTML
            return File(transgression.LetterFilePath, mimeType);
        }

        /// <summary>
        /// Helper method to save uploaded documents
        /// </summary>
        private void SaveDocuments(int paymentTransgressionId, HttpPostedFileBase[] documents)
        {
            var uploadDirectory = Server.MapPath("~/Uploads/PaymentTransgressions/Documents/");
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
                        continue; // Skip files larger than 10MB
                    }

                    // Validate file extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        continue; // Skip invalid file types
                    }

                    var fileName = $"{paymentTransgressionId}_{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadDirectory, fileName);
                    file.SaveAs(filePath);

                    var document = new PaymentTransgressionDocument
                    {
                        PaymentTransgressionId = paymentTransgressionId,
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

                    _db.PaymentTransgressionDocuments.Add(document);
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
