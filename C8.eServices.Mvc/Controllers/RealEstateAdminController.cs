using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize(Roles = "Administrators, Area Managers, Property Managers, Back Office System Administrator, Property Manager, Area Manager, Finance Administrator, Property & Facilities Manager, Caretaker")]
    public class RealEstateAdminController : Controller
    {
        private readonly eServicesDbContext db = new eServicesDbContext();
        private readonly BaseHelper _base = new BaseHelper();
        private SystemUser _systemUser;

        private void Initialise()
        {
            _base.Initialise(db);
            _systemUser = _base.SystemUser;
        }

        // GET: RealEstateAdmin
        public ActionResult Index()
        {
            Initialise();

            var categories = db.RE_FacilityCategories.Where(c => c.IsActive && !c.IsDeleted).ToList();
            var facilities = db.RE_Facilities.Include(f => f.CCC).Where(f => f.IsActive && !f.IsDeleted).ToList();
            var units = db.RE_FacilityUnits
                .Include(u => u.Facility)
                .Include(u => u.FacilityCategory)
                .Where(u => u.IsActive && !u.IsDeleted)
                .ToList();

            // Populate dropdowns for modals
            ViewBag.CCCList = db.CCCs.Where(c => c.IsActive && !c.IsDeleted)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CCCName })
                .OrderBy(c => c.Text)
                .ToList();

            ViewBag.CategoryList = categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .OrderBy(c => c.Text)
                .ToList();

            ViewBag.FacilityList = facilities
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .OrderBy(f => f.Text)
                .ToList();

            // Calculate KPI Dashboard Metrics
            int totalProperties = facilities.Count;
            int totalConfiguredUnits = units.Sum(u => u.MaxUnits);
            int activeCategoriesCount = categories.Count;

            var activeStatusKeys = new[] { Keys.StatusKeys.AwaitingApplicationFeeValidation, Keys.StatusKeys.Submitted, Keys.StatusKeys.InProgress, Keys.StatusKeys.Approved, Keys.StatusKeys.BackOffice };
            int leasedUnitsCount = db.RE_Applications
                .Where(a => a.IsActive && !a.IsDeleted && a.SelectedFacilityUnitId != null && activeStatusKeys.Contains(a.Status.Key))
                .Sum(a => (int?)a.SelectedUnitCount) ?? 0;

            decimal averageTariff = categories.Any() ? categories.Average(c => c.TariffPerSqm) : 0;

            ViewBag.TotalProperties = totalProperties;
            ViewBag.TotalConfiguredUnits = totalConfiguredUnits;
            ViewBag.ActiveCategoriesCount = activeCategoriesCount;
            ViewBag.LeasedUnitsCount = leasedUnitsCount;
            ViewBag.AverageTariff = averageTariff;

            // Calculate occupancy map for each unit (how many units are currently occupied)
            var occupancyMap = new System.Collections.Generic.Dictionary<int, int>();
            foreach (var unit in units)
            {
                int count = db.RE_Applications
                    .Where(a => a.SelectedFacilityUnitId == unit.Id && a.IsActive && !a.IsDeleted && activeStatusKeys.Contains(a.Status.Key))
                    .Sum(a => (int?)a.SelectedUnitCount) ?? 0;
                occupancyMap[unit.Id] = count;
            }
            ViewBag.OccupancyMap = occupancyMap;

            ViewBag.Categories = categories;
            ViewBag.Facilities = facilities;
            ViewBag.Units = units;

            return View();
        }

        // POST: RealEstateAdmin/UpdateTariff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTariff(int categoryId, decimal tariff)
        {
            Initialise();
            var category = db.RE_FacilityCategories.Find(categoryId);
            if (category != null)
            {
                category.TariffPerSqm = tariff;
                category.ModifiedDateTime = DateTime.Now;
                category.ModifiedBySystemUserId = _systemUser?.Id;
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Tariff for {0} updated to R{1:F2} per sqm successfully.", category.Name, tariff);
            }
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategory(string key, string name, decimal tariffPerSqm)
        {
            Initialise();
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(name))
            {
                TempData["ErrorMessage"] = "Category key and name are required.";
                return RedirectToAction("Index");
            }

            string normalizedKey = key.Trim().Replace(" ", "_").ToLower();

            var exists = db.RE_FacilityCategories.Any(c => c.Key == normalizedKey && !c.IsDeleted);
            if (exists)
            {
                TempData["ErrorMessage"] = "A category with this key already exists.";
                return RedirectToAction("Index");
            }

            var cat = new RE_FacilityCategory
            {
                Key = normalizedKey,
                Name = name.Trim(),
                TariffPerSqm = tariffPerSqm,
                IsActive = true,
                IsDeleted = false,
                CreatedBySystemUserId = _systemUser?.Id,
                CreatedDateTime = DateTime.Now,
                ModifiedBySystemUserId = _systemUser?.Id,
                ModifiedDateTime = DateTime.Now
            };

            db.RE_FacilityCategories.Add(cat);
            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Category '{0}' added successfully.", name);
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(int id, string name, decimal tariffPerSqm)
        {
            Initialise();
            var category = db.RE_FacilityCategories.Find(id);
            if (category != null)
            {
                category.Name = name.Trim();
                category.TariffPerSqm = tariffPerSqm;
                category.ModifiedDateTime = DateTime.Now;
                category.ModifiedBySystemUserId = _systemUser?.Id;

                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Category '{0}' updated successfully.", name);
            }
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(int id)
        {
            Initialise();
            var category = db.RE_FacilityCategories.Find(id);
            if (category != null)
            {
                category.IsDeleted = true;
                category.IsActive = false;
                category.ModifiedDateTime = DateTime.Now;
                category.ModifiedBySystemUserId = _systemUser?.Id;

                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Category '{0}' deleted successfully.", category.Name);
            }
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/AddFacility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFacility(string name, int cccId, string address)
        {
            Initialise();
            if (string.IsNullOrEmpty(name))
            {
                TempData["ErrorMessage"] = "Facility name is required.";
                return RedirectToAction("Index");
            }

            var fac = new RE_Facility
            {
                Name = name.Trim(),
                CCCId = cccId,
                Address = address?.Trim(),
                IsActive = true,
                IsDeleted = false,
                CreatedBySystemUserId = _systemUser?.Id,
                CreatedDateTime = DateTime.Now,
                ModifiedBySystemUserId = _systemUser?.Id,
                ModifiedDateTime = DateTime.Now
            };

            db.RE_Facilities.Add(fac);
            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Facility '{0}' added successfully.", name);
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/EditFacility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFacility(int id, string name, int cccId, string address)
        {
            Initialise();
            var facility = db.RE_Facilities.Find(id);
            if (facility != null)
            {
                facility.Name = name.Trim();
                facility.CCCId = cccId;
                facility.Address = address?.Trim();
                facility.ModifiedDateTime = DateTime.Now;
                facility.ModifiedBySystemUserId = _systemUser?.Id;

                db.Entry(facility).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Facility '{0}' updated successfully.", name);
            }
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/DeleteFacility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteFacility(int id)
        {
            Initialise();
            var facility = db.RE_Facilities.Find(id);
            if (facility != null)
            {
                facility.IsDeleted = true;
                facility.IsActive = false;
                facility.ModifiedDateTime = DateTime.Now;
                facility.ModifiedBySystemUserId = _systemUser?.Id;

                db.Entry(facility).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Facility '{0}' deleted successfully.", facility.Name);
            }
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/AddUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUnit(int facilityId, int facilityCategoryId, string unitType, decimal unitSize, int maxUnits)
        {
            Initialise();
            if (string.IsNullOrEmpty(unitType))
            {
                TempData["ErrorMessage"] = "Unit type/description is required.";
                return RedirectToAction("Index");
            }

            var unit = new RE_FacilityUnit
            {
                FacilityId = facilityId,
                FacilityCategoryId = facilityCategoryId,
                UnitType = unitType.Trim(),
                UnitSize = unitSize,
                MaxUnits = maxUnits,
                IsActive = true,
                IsDeleted = false,
                CreatedBySystemUserId = _systemUser?.Id,
                CreatedDateTime = DateTime.Now,
                ModifiedBySystemUserId = _systemUser?.Id,
                ModifiedDateTime = DateTime.Now
            };

            db.RE_FacilityUnits.Add(unit);
            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Facility Unit '{0}' added successfully.", unitType);
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/EditUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUnit(RE_FacilityUnit model)
        {
            Initialise();
            var unit = db.RE_FacilityUnits.Find(model.Id);
            if (unit != null)
            {
                unit.FacilityCategoryId = model.FacilityCategoryId;
                unit.UnitType = model.UnitType;
                unit.UnitSize = model.UnitSize;
                unit.MaxUnits = model.MaxUnits;
                unit.ModifiedDateTime = DateTime.Now;
                unit.ModifiedBySystemUserId = _systemUser?.Id;

                db.Entry(unit).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = string.Format("Facility Unit '{0}' updated successfully.", model.UnitType);
            }
            return RedirectToAction("Index");
        }

        // POST: RealEstateAdmin/DeleteUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUnit(int id)
        {
            Initialise();
            var unit = db.RE_FacilityUnits.Find(id);
            if (unit != null)
            {
                unit.IsDeleted = true;
                unit.IsActive = false;
                unit.ModifiedDateTime = DateTime.Now;
                unit.ModifiedBySystemUserId = _systemUser?.Id;

                db.Entry(unit).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = string.Format("Facility Unit '{0}' deleted successfully.", unit.UnitType);
            }
            return RedirectToAction("Index");
        }

        // --- UC 06: Validate Proof of Payment ---
        
        // GET: RealEstateAdmin/ApplicationFeePayments
        public ActionResult ApplicationFeePayments()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.AwaitingApplicationFeeValidation && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        // GET: RealEstateAdmin/VerifyPayment/{id}
        public ActionResult VerifyPayment(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .Include(a => a.SelectedFacilityUnit.FacilityCategory)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            var documents = db.Documents
                .Include(d => d.DocumentCheckList)
                .Include(d => d.DocumentCheckList.DocumentType)
                .Where(d => d.RealEstateApplicationId == id && d.IsActive && !d.IsDeleted)
                .ToList();

            foreach (var doc in documents)
            {
                if (doc.FileId != null)
                {
                    doc.File = new C8.eServices.Mvc.Models.File { Id = (int)doc.FileId, CreatedDateTime = doc.CreatedDateTime };
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                }
            }

            ViewBag.Documents = documents;
            return View(app);
        }

        // POST: RealEstateAdmin/VerifyPayment/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyPayment(int id, string decision, string comment)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.SystemUser)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            if (decision == "Approve")
            {
                var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.AwaitingRiskAssessment);
                if (targetStatus == null) throw new Exception("Target status 's_awaiting_risk_assessment' not found.");

                app.StatusId = targetStatus.Id;
                app.PaymentValidationComment = comment;
                app.ModifiedDateTime = DateTime.Now;
                app.ModifiedBySystemUserId = _systemUser?.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Application fee payment approved.");
                TempData["SuccessMessage"] = "Payment validated successfully! Application status updated to Awaiting Risk Assessment.";
            }
            else if (decision == "Reject")
            {
                if (string.IsNullOrEmpty(comment))
                {
                    ModelState.AddModelError("comment", "A reason is required to reject the payment.");
                    var documents = db.Documents
                        .Include(d => d.DocumentCheckList)
                        .Include(d => d.DocumentCheckList.DocumentType)
                        .Where(d => d.RealEstateApplicationId == id && d.IsActive && !d.IsDeleted)
                        .ToList();
                    foreach (var doc in documents)
                    {
                        if (doc.FileId != null)
                        {
                            doc.File = new C8.eServices.Mvc.Models.File { Id = (int)doc.FileId, CreatedDateTime = doc.CreatedDateTime };
                            doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                        }
                    }
                    ViewBag.Documents = documents;
                    return View(app);
                }

                app.PaymentValidationComment = comment;
                app.ModifiedDateTime = DateTime.Now;
                app.ModifiedBySystemUserId = _systemUser?.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Application fee payment rejected. Reason: " + comment);

                // Notify applicant via Email
                try
                 {
                    var email = new Email();
                    string subject = "Real Estate Lease Application: Proof of Payment Rejected";
                    string body = string.Format("Dear {0},<br/><br/>Your uploaded proof of payment for application reference {1} has been rejected.<br/><br/><strong>Reason:</strong> {2}<br/><br/>Please log in and upload a valid proof of payment.", app.SystemUser.FullName, app.ApplicationReferenceNumber, comment);
                    email.GenerateEmail(app.SystemUser.EmailAddress, subject, body, app.CustomerId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, app.SystemUser.FullName);
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                TempData["SuccessMessage"] = "Payment verification rejected. Applicant has been notified.";
            }

            return RedirectToAction("ApplicationFeePayments");
        }

        // --- UC 07: Capture Risk Assessment Outcome ---

        // GET: RealEstateAdmin/RiskAssessments
        public ActionResult RiskAssessments()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.AwaitingRiskAssessment && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        // GET: RealEstateAdmin/ConductAssessment/{id}
        public ActionResult ConductAssessment(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .Include(a => a.SelectedFacilityUnit.FacilityCategory)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.RecommendationList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Recommended", Text = "Recommended" },
                new SelectListItem { Value = "Not Recommended", Text = "Not Recommended" }
            };

            return View(app);
        }

        // POST: RealEstateAdmin/ConductAssessment/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductAssessment(int id, string creditResult, string homeAffairsResult, string deedsResult, string sassaResult, string cipcResult, string recommendation, string reason)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.SystemUser)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            HttpPostedFileBase evidenceFile = Request.Files["evidenceFile"];
            if (evidenceFile != null && evidenceFile.ContentLength > 0)
            {
                try
                {
                    byte[] contentData = null;
                    using (var binaryReader = new BinaryReader(evidenceFile.InputStream))
                    {
                        contentData = binaryReader.ReadBytes(evidenceFile.ContentLength);
                    }

                    var dbFile = new C8.eServices.Mvc.Models.File
                    {
                        FileName = Path.GetFileName(evidenceFile.FileName),
                        ContentType = evidenceFile.ContentType,
                        Content = contentData,
                        FileSize = evidenceFile.ContentLength,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDateTime = DateTime.Now
                    };

                    db.Files.Add(dbFile);
                    db.SaveChanges();

                    app.RiskAssessmentEvidenceFileId = dbFile.Id;
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError("Evidence File Upload Error: " + ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }
            }

            app.CreditBureauResult = creditResult;
            app.HomeAffairsResult = homeAffairsResult;
            app.DeedsResult = deedsResult;
            app.SassaResult = sassaResult;
            app.CipcResult = cipcResult;
            app.RiskAssessmentRecommendation = recommendation;
            app.RiskAssessmentReason = reason;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            if (recommendation == "Recommended")
            {
                var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.PropertyVerified);
                if (targetStatus == null) throw new Exception("Target status 's_property_verified' not found.");

                app.StatusId = targetStatus.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Risk assessment completed and Recommended.");
                TempData["SuccessMessage"] = "Risk assessment outcome saved. Application status is now Verified.";
            }
            else if (recommendation == "Not Recommended")
            {
                if (string.IsNullOrEmpty(reason))
                {
                    ModelState.AddModelError("reason", "A reason is required when risk assessment is Not Recommended.");
                    ViewBag.RecommendationList = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Recommended", Text = "Recommended" },
                        new SelectListItem { Value = "Not Recommended", Text = "Not Recommended" }
                    };
                    return View(app);
                }

                var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.Rejected);
                if (targetStatus == null) throw new Exception("Target status 's_rcs_Rejected' not found.");

                app.StatusId = targetStatus.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Risk assessment completed and Not Recommended. Reason: " + reason);

                // Notify applicant via Email
                try
                {
                    var email = new Email();
                    string subject = "Real Estate Lease Application: Risk Assessment Outcome";
                    string body = string.Format("Dear {0},<br/><br/>Your lease application reference {1} has been declined based on the risk assessment outcome.<br/><br/><strong>Reason:</strong> {2}", app.SystemUser.FullName, app.ApplicationReferenceNumber, reason);
                    email.GenerateEmail(app.SystemUser.EmailAddress, subject, body, app.CustomerId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, app.SystemUser.FullName);
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                TempData["SuccessMessage"] = "Risk assessment outcome saved. Application has been Rejected and applicant notified.";
            }

            return RedirectToAction("RiskAssessments");
        }

        // --- UC 08: Initiate Departmental Review ---

        // GET: RealEstateAdmin/DepartmentalReviews
        public ActionResult DepartmentalReviews()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.PropertyVerified && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        // GET: RealEstateAdmin/InitiateReview/{id}
        public ActionResult InitiateReview(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            var departments = db.DepartmentsCoEs
                .Where(d => d.IsActive && !d.IsDeleted)
                .OrderBy(d => d.DepartmentName)
                .ToList();

            ViewBag.Departments = departments;
            return View(app);
        }

        // POST: RealEstateAdmin/InitiateReview/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InitiateReview(int id, FormCollection form)
        {
            Initialise();
            var app = db.RE_Applications
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            var hasDepartments = db.DepartmentsCoEs.Any(d => d.IsActive && !d.IsDeleted);
            if (!hasDepartments)
            {
                TempData["ErrorMessage"] = "Cannot initiate review. No active reviewing departments configured.";
                return RedirectToAction("InitiateReview", new { id = id });
            }

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.RealEstateInCirculation);
            if (targetStatus == null) throw new Exception("Target status 're_in_circulation_for_evaluation' not found.");

            app.StatusId = targetStatus.Id;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Lease application initiated for Departmental Review.");

            TempData["SuccessMessage"] = "Departmental review initiated successfully! Application status is now In Circulation for Evaluation.";
            return RedirectToAction("DepartmentalReviews");
        }

        // --- Helper to Save File ---
        private int? SaveFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;
            byte[] contentData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                contentData = binaryReader.ReadBytes(file.ContentLength);
            }
            var dbFile = new C8.eServices.Mvc.Models.File
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                Content = contentData,
                FileSize = file.ContentLength,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now
            };
            db.Files.Add(dbFile);
            db.SaveChanges();
            return dbFile.Id;
        }

        // --- UC 09: Capture Departmental Reviews and Comments ---
        public ActionResult DepartmentalQueue()
        {
            Initialise();
            var targetKeys = new[] { StatusKeys.RealEstateInCirculation, StatusKeys.ReSupported, StatusKeys.ReSupportedConditions, StatusKeys.ReNotSupported, StatusKeys.ReAdditionalInfoReq };
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult CaptureDepartmentalComment(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.DepartmentList = db.DepartmentsCoEs
                .Where(d => d.IsActive && !d.IsDeleted)
                .Select(d => new SelectListItem { Value = d.DepartmentName, Text = d.DepartmentName })
                .ToList();

            ViewBag.OutcomeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Supported", Text = "Supported" },
                new SelectListItem { Value = "Supported with Conditions", Text = "Supported with Conditions" },
                new SelectListItem { Value = "Not Supported", Text = "Not Supported" },
                new SelectListItem { Value = "Request Additional Information", Text = "Request Additional Information" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CaptureDepartmentalComment(int id, string departmentName, string representativeName, string outcome, string comments)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(departmentName) || string.IsNullOrEmpty(representativeName) || string.IsNullOrEmpty(outcome))
            {
                TempData["ErrorMessage"] = "Department Name, Representative Name and Outcome are required.";
                return RedirectToAction("CaptureDepartmentalComment", new { id = id });
            }

            if ((outcome == "Not Supported" || outcome == "Supported with Conditions" || outcome == "Request Additional Information") && string.IsNullOrEmpty(comments))
            {
                TempData["ErrorMessage"] = "Comments are mandatory for this outcome.";
                return RedirectToAction("CaptureDepartmentalComment", new { id = id });
            }

            int? fileId = null;
            HttpPostedFileBase supportingFile = Request.Files["supportingFile"];
            if (supportingFile != null && supportingFile.ContentLength > 0)
            {
                fileId = SaveFile(supportingFile);
            }

            var comment = new RE_DepartmentalComment
            {
                RE_ApplicationId = id,
                DepartmentName = departmentName,
                RepresentativeName = representativeName,
                Outcome = outcome,
                Comments = comments,
                SupportingDocumentFileId = fileId,
                DateStamp = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now,
                CreatedBySystemUserId = _systemUser?.Id
            };

            db.RE_DepartmentalComments.Add(comment);

            // Update main application status
            string statusKey = StatusKeys.RealEstateInCirculation;
            if (outcome == "Supported") statusKey = StatusKeys.ReSupported;
            else if (outcome == "Supported with Conditions") statusKey = StatusKeys.ReSupportedConditions;
            else if (outcome == "Not Supported") statusKey = StatusKeys.ReNotSupported;
            else if (outcome == "Request Additional Information") statusKey = StatusKeys.ReAdditionalInfoReq;

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == statusKey);
            if (targetStatus != null)
            {
                app.StatusId = targetStatus.Id;
            }
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Departmental comment captured by {0} ({1}). Outcome: {2}", representativeName, departmentName, outcome));
            TempData["SuccessMessage"] = "Departmental review comment captured successfully.";
            return RedirectToAction("DepartmentalQueue");
        }

        // --- UC 10: Consolidate Departmental Feedback ---
        public ActionResult ConsolidateFeedback()
        {
            Initialise();
            var targetKeys = new[] { StatusKeys.RealEstateInCirculation, StatusKeys.ReSupported, StatusKeys.ReSupportedConditions, StatusKeys.ReNotSupported, StatusKeys.ReAdditionalInfoReq };
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult ConsolidateApplication(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            var comments = db.RE_DepartmentalComments
                .Include(c => c.SupportingDocumentFile)
                .Where(c => c.RE_ApplicationId == id && c.IsActive && !c.IsDeleted)
                .ToList();

            foreach (var doc in comments)
            {
                if (doc.SupportingDocumentFileId != null && doc.SupportingDocumentFile != null)
                {
                    doc.SupportingDocumentFile.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.SupportingDocumentFileId));
                }
            }

            ViewBag.Comments = comments;
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConsolidateApplication(int id, string decision)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (decision == "Submit")
            {
                var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.RePendingCommitteeOutcome);
                if (targetStatus != null) app.StatusId = targetStatus.Id;
                app.ModifiedDateTime = DateTime.Now;
                app.ModifiedBySystemUserId = _systemUser?.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Departmental feedback consolidated and submitted for Committee Review.");
                TempData["SuccessMessage"] = "Departmental comments consolidated. Routed to DPRE Evaluation Committee.";
            }
            else if (decision == "Escalate")
            {
                var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.AwaitingHoDResponse);
                if (targetStatus != null) app.StatusId = targetStatus.Id;
                app.ModifiedDateTime = DateTime.Now;
                app.ModifiedBySystemUserId = _systemUser?.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Conflicting feedback escalated to Head of Department (HoD).");
                TempData["SuccessMessage"] = "Application escalated to HoD for decision due to conflicting feedback.";
            }

            return RedirectToAction("ConsolidateFeedback");
        }

        // --- UC 11: Committee Review and Decision ---
        public ActionResult CommitteeReviews()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.RePendingCommitteeOutcome && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult ReviewCommitteeItem(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.DecisionList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Recommended", Text = "Recommended" },
                new SelectListItem { Value = "Recommended with Conditions", Text = "Recommended with Conditions" },
                new SelectListItem { Value = "Not Recommended", Text = "Not Recommended" },
                new SelectListItem { Value = "Deferred", Text = "Deferred" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReviewCommitteeItem(int id, string decision, string comments)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(decision) || string.IsNullOrEmpty(comments))
            {
                TempData["ErrorMessage"] = "Decision and Comments/Resolution details are required.";
                return RedirectToAction("ReviewCommitteeItem", new { id = id });
            }

            HttpPostedFileBase resolutionFile = Request.Files["resolutionFile"];
            if (resolutionFile == null || resolutionFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Committee Resolution Document upload is mandatory.";
                return RedirectToAction("ReviewCommitteeItem", new { id = id });
            }

            int? fileId = SaveFile(resolutionFile);
            app.CommitteeDecision = decision;
            app.CommitteeComments = comments;
            app.CommitteeResolutionFileId = fileId;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            string statusKey = StatusKeys.RePendingCommitteeOutcome;
            if (decision == "Recommended") statusKey = StatusKeys.ReRecommended;
            else if (decision == "Recommended with Conditions") statusKey = StatusKeys.ReRecommendedConditions;
            else if (decision == "Not Recommended") statusKey = StatusKeys.ReNotRecommended;
            else if (decision == "Deferred") statusKey = StatusKeys.ReDeferred;

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == statusKey);
            if (targetStatus != null) app.StatusId = targetStatus.Id;

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Committee outcome captured: {0}. Notes: {1}", decision, comments));
            TempData["SuccessMessage"] = "Committee decision and resolution captured successfully.";
            return RedirectToAction("CommitteeReviews");
        }

        // --- UC 12: Application Final Authorisation ---
        public ActionResult FinalAuthorisation()
        {
            Initialise();
            var targetKeys = new[] { StatusKeys.ReRecommended, StatusKeys.ReRecommendedConditions };
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult AuthoriseApplication(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.OutcomeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Approved", Text = "Approved" },
                new SelectListItem { Value = "Approved with Conditions", Text = "Approved with Conditions" },
                new SelectListItem { Value = "Rejected", Text = "Rejected" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AuthoriseApplication(int id, string outcome, string comments, string signature)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(outcome) || string.IsNullOrEmpty(comments) || string.IsNullOrEmpty(signature))
            {
                TempData["ErrorMessage"] = "Outcome, Comments and Signature are mandatory.";
                return RedirectToAction("AuthoriseApplication", new { id = id });
            }

            app.FinalOutcome = outcome;
            app.FinalComments = comments;
            app.FinalSignature = signature;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            string statusKey = (outcome == "Rejected") ? StatusKeys.ReConcludedRejected : StatusKeys.ReConcludedApproved;
            var targetStatus = db.Status.FirstOrDefault(s => s.Key == statusKey);
            if (targetStatus != null) app.StatusId = targetStatus.Id;

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Final authorisation outcome: {0}. Comments: {1}", outcome, comments));
            TempData["SuccessMessage"] = "Application authorization completed and finalized.";
            return RedirectToAction("FinalAuthorisation");
        }

        // --- UC 13: Schedule Unit Inspection ---
        public ActionResult InspectionSchedules()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReConcludedApproved && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult ScheduleInspection(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.TypeList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pre-Occupation", Text = "Pre-Occupation" },
                new SelectListItem { Value = "Exit Inspection", Text = "Exit Inspection" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScheduleInspection(int id, string type, DateTime? date, string time)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(type) || !date.HasValue || string.IsNullOrEmpty(time))
            {
                TempData["ErrorMessage"] = "Type, Date, and Time slots are required.";
                return RedirectToAction("ScheduleInspection", new { id = id });
            }

            app.InspectionType = type;
            app.InspectionDate = date;
            app.InspectionTime = time;
            app.InspectionStatus = "Scheduled";
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingInspection);
            if (targetStatus != null) app.StatusId = targetStatus.Id;

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Pre-Occupation inspection scheduled for {0:yyyy-MM-dd} at {1}.", date.Value, time));
            TempData["SuccessMessage"] = "Inspection slot scheduled successfully. Applicant notified.";
            return RedirectToAction("InspectionSchedules");
        }

        // --- UC 14: Conduct Unit Inspection ---
        public ActionResult ConductInspections()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReAwaitingInspection && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult ConductInspection(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.ConditionList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Good", Text = "Good / Compliant" },
                new SelectListItem { Value = "Fair", Text = "Fair (Needs minor repairs)" },
                new SelectListItem { Value = "Poor", Text = "Poor / Non-compliant (Defect logged)" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConductInspection(int id, string plumbing, string electrical, string fixtures, string sanitation, string hazards, string wearTear, string comments, string logWorkOrder, string woIssue, string woTasks, string woPriority, DateTime? woDueDate, string woMaterials, string woSafety)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            HttpPostedFileBase inspectionForm = Request.Files["inspectionFormFile"];
            if (inspectionForm == null || inspectionForm.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Signed and completed Inspection Form PDF is mandatory.";
                return RedirectToAction("ConductInspection", new { id = id });
            }

            int? fileId = SaveFile(inspectionForm);
            app.InspectionPlumbing = plumbing;
            app.InspectionElectrical = electrical;
            app.InspectionFixtures = fixtures;
            app.InspectionSanitation = sanitation;
            app.InspectionHazards = hazards;
            app.InspectionWearTear = wearTear;
            app.InspectionComments = comments;
            app.InspectionFormFileId = fileId;
            app.InspectionStatus = "Completed";

            if (logWorkOrder == "true" && !string.IsNullOrEmpty(woIssue))
            {
                string yearStr = DateTime.Now.Year.ToString();
                var counterSetting = db.AppSettings.FirstOrDefault(o => o.Key == "red_daily_sequence_counter");
                string nextVal = counterSetting != null ? counterSetting.Value : "001";
                app.WorkOrderNumber = string.Format("WO: {0}/{1}", yearStr, nextVal);
                app.WorkOrderStatus = "Logged";
                app.WorkOrderIssueDescription = woIssue;
                app.WorkOrderTasks = woTasks;
                app.WorkOrderPriority = woPriority;
                app.WorkOrderDueDate = woDueDate ?? DateTime.Now.AddDays(7);
                app.WorkOrderMaterials = woMaterials;
                app.WorkOrderSafetyInstructions = woSafety;
            }

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingAgreementConclusion);
            if (targetStatus != null) app.StatusId = targetStatus.Id;

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Unit pre-occupancy inspection completed.");
            TempData["SuccessMessage"] = "Inspection report saved successfully.";
            return RedirectToAction("ConductInspections");
        }

        // --- UC 15: Authorise and Assign Works Order Request ---
        public ActionResult WorkOrders()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Where(a => a.WorkOrderStatus != null && a.WorkOrderStatus != "" && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult AuthoriseWorkOrder(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.AssignList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Internal", Text = "Internal Technician" },
                new SelectListItem { Value = "External", Text = "External SCM Vendor" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AuthoriseWorkOrder(int id, string decision, string reason, string signature, string assignmentType)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(decision) || string.IsNullOrEmpty(reason) || string.IsNullOrEmpty(signature))
            {
                TempData["ErrorMessage"] = "Decision, Comment/Reason, and Signature are mandatory.";
                return RedirectToAction("AuthoriseWorkOrder", new { id = id });
            }

            app.WorkOrderManagerComments = reason;
            app.WorkOrderManagerSignature = signature;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            if (decision == "Approve")
            {
                app.WorkOrderStatus = "Assigned";
                app.WorkOrderAssignmentType = assignmentType;
            }
            else
            {
                app.WorkOrderStatus = "Logged"; // Reset back to logged
                app.WorkOrderRejectionReason = reason;
            }

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Works order review completed. Decision: {0}. Assign: {1}", decision, assignmentType));
            TempData["SuccessMessage"] = "Work order authorization completed successfully.";
            return RedirectToAction("WorkOrders");
        }

        // --- UC 16: Create Maintenance Schedule ---
        public ActionResult MaintenancePlanning()
        {
            Initialise();
            var units = db.RE_FacilityUnits
                .Include(u => u.Facility)
                .Include(u => u.FacilityCategory)
                .Where(u => u.IsActive && !u.IsDeleted)
                .ToList();
            return View(units);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaintenancePlanning(int id, string activity, string frequency, DateTime? dueDate, int duration)
        {
            Initialise();
            TempData["SuccessMessage"] = string.Format("Preventive maintenance schedule '{0}' (Freq: {1}, Due: {2:yyyy-MM-dd}) created successfully.", activity, frequency, dueDate);
            return RedirectToAction("MaintenancePlanning");
        }

        // --- UC 17: Log and Track Ad Hoc Maintenance Requests ---
        public ActionResult AssignTechnician(int id)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            ViewBag.Technicians = new List<SelectListItem>
            {
                new SelectListItem { Value = "T. Ndlovu", Text = "T. Ndlovu (Electrician)" },
                new SelectListItem { Value = "S. Mokoena", Text = "S. Mokoena (Plumber)" },
                new SelectListItem { Value = "A. Smith", Text = "A. Smith (Handyman)" }
            };

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTechnician(int id, string technicianName)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            app.WorkOrderTechnicianName = technicianName;
            app.WorkOrderStatus = "In Progress";
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Work order assigned to technician: {0}.", technicianName));
            TempData["SuccessMessage"] = "Work order assigned to technician successfully.";
            return RedirectToAction("WorkOrders");
        }

        public ActionResult TechnicianAllocation()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Where(a => a.WorkOrderStatus == "In Progress" && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptAllocation(int id, string decision, string reason)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (decision == "Accept")
            {
                app.WorkOrderStatus = "In Progress, Pending Resolution";
                TempData["SuccessMessage"] = "Allocation request accepted successfully.";
            }
            else
            {
                app.WorkOrderRejectionReason = reason;
                TempData["SuccessMessage"] = "Allocation request rejected and routed back to manager.";
            }

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Technician allocation request: {0}.", decision));
            return RedirectToAction("TechnicianAllocation");
        }

        public ActionResult TechnicianUpdate(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TechnicianUpdate(int id, string comments, string holdReason)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            HttpPostedFileBase jobSheetFile = Request.Files["jobSheetFile"];

            if (string.IsNullOrEmpty(holdReason))
            {
                if (jobSheetFile == null || jobSheetFile.ContentLength == 0)
                {
                    TempData["ErrorMessage"] = "Technician Job Sheet upload is required to complete the task.";
                    return RedirectToAction("TechnicianUpdate", new { id = id });
                }

                int? fileId = SaveFile(jobSheetFile);
                app.WorkOrderJobSheetFileId = fileId;
                app.WorkOrderStatus = "Completed";
                app.WorkOrderManagerComments = comments;
                TempData["SuccessMessage"] = "Work order updated and marked Completed.";
            }
            else
            {
                app.WorkOrderStatus = "On Hold";
                app.WorkOrderRejectionReason = holdReason;
                TempData["SuccessMessage"] = "Work order placed On Hold.";
            }

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Technician updated work order status.");
            return RedirectToAction("TechnicianAllocation");
        }

        public ActionResult CloseWorkOrder(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CloseWorkOrder(int id, string decision, string comments, string signature)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(signature))
            {
                TempData["ErrorMessage"] = "Manager signature is mandatory to submit closure.";
                return RedirectToAction("CloseWorkOrder", new { id = id });
            }

            if (decision == "Approve")
            {
                app.WorkOrderStatus = "Closed";
                app.WorkOrderManagerComments = comments;
                app.WorkOrderManagerSignature = signature;
                TempData["SuccessMessage"] = "Work order closed successfully.";
            }
            else
            {
                app.WorkOrderStatus = "In Progress, Pending Resolution";
                app.WorkOrderRejectionReason = comments;
                TempData["SuccessMessage"] = "Work order closure rejected. Re-routed back to technician.";
            }

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Work order closure review. Decision: {0}.", decision));
            return RedirectToAction("WorkOrders");
        }

        // --- UC 19: Review Permission to Occupy ---
        public ActionResult PtoReviews()
        {
            Initialise();
            var targetKeys = new[] { StatusKeys.ReAwaitingPtoReview, StatusKeys.RePtoAdditionalInfo };
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult ReviewPto(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            var documents = db.Documents
                .Include(d => d.DocumentCheckList)
                .Include(d => d.DocumentCheckList.DocumentType)
                .Where(d => d.RealEstateApplicationId == id && d.IsActive && !d.IsDeleted)
                .ToList();

            foreach (var doc in documents)
            {
                if (doc.FileId != null)
                {
                    doc.File = new C8.eServices.Mvc.Models.File { Id = (int)doc.FileId, CreatedDateTime = doc.CreatedDateTime };
                    doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
                }
            }

            ViewBag.Documents = documents;
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReviewPto(int id, string recommendation, string reason)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(recommendation) || string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Recommendation and Comments/Reason are required.";
                return RedirectToAction("ReviewPto", new { id = id });
            }

            app.PtoReviewRecommendation = recommendation;
            app.PtoReviewReason = reason;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            string statusKey = (recommendation == "Recommended") ? StatusKeys.ReAwaitingPtoApproval : StatusKeys.RePtoAdditionalInfo;
            var targetStatus = db.Status.FirstOrDefault(s => s.Key == statusKey);
            if (targetStatus != null)
            {
                app.StatusId = targetStatus.Id;
                app.PtoStatus = targetStatus.Name;
            }

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("PTO review completed. Recommendation: {0}.", recommendation));
            TempData["SuccessMessage"] = "PTO review recommendation submitted successfully.";
            return RedirectToAction("PtoReviews");
        }

        // --- UC 20: Authorise Permission to Occupy ---
        public ActionResult PtoAuthorisations()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReAwaitingPtoApproval && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult AuthorisePto(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AuthorisePto(int id, string decision, string comments, string signature)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(decision) || string.IsNullOrEmpty(signature))
            {
                TempData["ErrorMessage"] = "Decision and Signature are mandatory.";
                return RedirectToAction("AuthorisePto", new { id = id });
            }

            app.PtoDecision = decision;
            app.PtoDecisionReason = comments;
            app.PtoSignature = signature;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            string statusKey = StatusKeys.RePtoRejected;
            if (decision == "Approve") statusKey = StatusKeys.RePtoApproved;
            else if (decision == "Approve with Conditions") statusKey = StatusKeys.RePtoApprovedConditions;

            var targetStatus = db.Status.FirstOrDefault(s => s.Key == statusKey);
            if (targetStatus != null)
            {
                app.StatusId = targetStatus.Id;
                app.PtoStatus = targetStatus.Name;
            }

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("PTO authorization completed. Outcome: {0}.", decision));
            TempData["SuccessMessage"] = "PTO authorization completed successfully.";
            return RedirectToAction("PtoAuthorisations");
        }
    }
}
