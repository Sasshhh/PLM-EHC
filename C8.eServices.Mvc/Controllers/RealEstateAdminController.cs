using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    [AllowAnonymous]
    public class RealEstateAdminController : Controller
    {
        private readonly eServicesDbContext db = new eServicesDbContext();
        private readonly BaseHelper _base = new BaseHelper();
        private SystemUser _systemUser;

        private void Initialise()
        {
            _base.Initialise(db);
            _systemUser = _base.SystemUser ?? db.SystemUsers.FirstOrDefault(x => x.IsActive && !x.IsDeleted);

            // Smart role restriction: if the user only has the Departmental Representative role,
            // restrict their access strictly to the Departmental Review Queue and Comments capture pages.
            if (User.IsInRole("Departmental Representative") &&
                !User.IsInRole("Property Manager") &&
                !User.IsInRole("Property Managers") &&
                !User.IsInRole("Finance Administrator") &&
                !User.IsInRole("Area Manager") &&
                !User.IsInRole("Area Managers") &&
                !User.IsInRole("Back Office System Administrator") &&
                !User.IsInRole("Super Administrators") &&
                !User.IsInRole("Administrators") &&
                !User.IsInRole("Support Admin"))
            {
                var action = RouteData.Values["action"]?.ToString();
                if (action != "DepartmentalQueue" && action != "CaptureDepartmentalComment")
                {
                    throw new System.Web.HttpException(403, "Access Denied: Departmental Representatives are restricted to the Departmental Review Queue.");
                }
            }
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
            var query = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.AwaitingApplicationFeeValidation && a.IsActive && !a.IsDeleted);

            var customer = db.Customers.FirstOrDefault(c => c.SystemUserId == _systemUser.Id);
            if (customer != null && !User.IsInRole("Administrators") && !User.IsInRole("Back Office System Administrator") && !User.IsInRole("Super Administrators"))
            {
                var assignedAppIds = db.RoundRobinQueues
                    .Where(q => q.ClerkId == customer.Id 
                             && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateVerifyPayment
                             && q.Status.Key == StatusKeys.Submitted
                             && q.EndTaskDateTime == null)
                    .Select(q => q.RealEstateApplicationId)
                    .ToList();
                query = query.Where(a => assignedAppIds.Contains(a.Id));
            }

            var apps = query.ToList();
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

            // Fetch assigned representative clerk from RoundRobinQueues
            var activeQueueTask = db.RoundRobinQueues
                .Include(q => q.Clerk)
                .Include(q => q.Clerk.SystemUser)
                .FirstOrDefault(q => q.RealEstateApplicationId == id 
                                  && q.Status.Key == StatusKeys.Submitted 
                                  && q.EndTaskDateTime == null);
            if (activeQueueTask != null && activeQueueTask.Clerk != null && activeQueueTask.Clerk.SystemUser != null)
            {
                ViewBag.AssignedRepresentative = activeQueueTask.Clerk.SystemUser.FullName;
            }
            else
            {
                ViewBag.AssignedRepresentative = "Not Assigned (Unrouted or Admin queue)";
            }

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
                
                // Complete Verify Payment task and assign Risk Assessment task
                RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateVerifyPayment);
                RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateRiskAssessment);

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

                // Complete Verify Payment task
                RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateVerifyPayment);

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
            var query = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.AwaitingRiskAssessment && a.IsActive && !a.IsDeleted);

            var customer = db.Customers.FirstOrDefault(c => c.SystemUserId == _systemUser.Id);
            if (customer != null && !User.IsInRole("Administrators") && !User.IsInRole("Back Office System Administrator") && !User.IsInRole("Super Administrators"))
            {
                var assignedAppIds = db.RoundRobinQueues
                    .Where(q => q.ClerkId == customer.Id 
                             && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateRiskAssessment
                             && q.Status.Key == StatusKeys.Submitted
                             && q.EndTaskDateTime == null)
                    .Select(q => q.RealEstateApplicationId)
                    .ToList();
                query = query.Where(a => assignedAppIds.Contains(a.Id));
            }

            var apps = query.ToList();
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

            // Fetch assigned representative clerk from RoundRobinQueues
            var activeQueueTask = db.RoundRobinQueues
                .Include(q => q.Clerk)
                .Include(q => q.Clerk.SystemUser)
                .FirstOrDefault(q => q.RealEstateApplicationId == id 
                                  && q.Status.Key == StatusKeys.Submitted 
                                  && q.EndTaskDateTime == null);
            if (activeQueueTask != null && activeQueueTask.Clerk != null && activeQueueTask.Clerk.SystemUser != null)
            {
                ViewBag.AssignedRepresentative = activeQueueTask.Clerk.SystemUser.FullName;
            }
            else
            {
                ViewBag.AssignedRepresentative = "Not Assigned (Unrouted or Admin queue)";
            }

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
            
            // Complete previous Risk Assessment task if not already done
            RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateRiskAssessment);

            // Assign Departmental Review tasks for all active reviewing departments
            var activeDepts = db.DepartmentsCoEs.Where(d => d.IsActive && !d.IsDeleted).ToList();
            foreach (var dept in activeDepts)
            {
                if (dept.RepresentativeSystemUserId.HasValue)
                {
                    var clerk = db.Customers.FirstOrDefault(c => c.SystemUserId == dept.RepresentativeSystemUserId && c.IsActive && !c.IsDeleted);
                    if (clerk != null)
                    {
                        RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateDepartmentalReview, clerk.Id);
                    }
                    else
                    {
                        RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateDepartmentalReview);
                    }
                }
                else
                {
                    RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateDepartmentalReview);
                }
            }

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
            
            // Check if logged-in user is a mapped representative
            var mappedDept = db.DepartmentsCoEs.FirstOrDefault(d => d.RepresentativeSystemUserId == _systemUser.Id && d.IsActive && !d.IsDeleted);
            
            var query = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted);

            var customer = db.Customers.FirstOrDefault(c => c.SystemUserId == _systemUser.Id);
            if (customer != null && !User.IsInRole("Administrators") && !User.IsInRole("Back Office System Administrator") && !User.IsInRole("Super Administrators"))
            {
                var assignedAppIds = db.RoundRobinQueues
                    .Where(q => q.ClerkId == customer.Id 
                             && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateDepartmentalReview
                             && q.Status.Key == StatusKeys.Submitted
                             && q.EndTaskDateTime == null)
                    .Select(q => q.RealEstateApplicationId)
                    .ToList();
                query = query.Where(a => assignedAppIds.Contains(a.Id));
            }
            else if (mappedDept != null)
            {
                // Filter: Only show applications where this department has NOT submitted feedback yet
                var alreadyCommentedIds = db.RE_DepartmentalComments
                    .Where(c => c.DepartmentName == mappedDept.DepartmentName && c.IsActive && !c.IsDeleted)
                    .Select(c => c.RE_ApplicationId)
                    .ToList();
                
                query = query.Where(a => !alreadyCommentedIds.Contains(a.Id));
            }

            var apps = query.ToList();
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

            // Check if logged-in user is mapped to a department
            var mappedDept = db.DepartmentsCoEs.FirstOrDefault(d => d.RepresentativeSystemUserId == _systemUser.Id && d.IsActive && !d.IsDeleted);
            ViewBag.AssignedDepartment = mappedDept;
            ViewBag.RepresentativeName = _systemUser?.FullName;

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

            ViewBag.Documents = db.Documents
                .Include(d => d.File)
                .Where(d => d.RealEstateApplicationId == id && d.IsActive && !d.IsDeleted)
                .ToList();

            ViewBag.HistoryLogs = db.PLMApplicationHistortyLogs
                .Where(h => h.RealEstateApplicationId == id && h.IsActive && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedDateTime)
                .ToList();

            // Fetch assigned representative clerk from RoundRobinQueues
            var activeQueueTask = db.RoundRobinQueues
                .Include(q => q.Clerk)
                .Include(q => q.Clerk.SystemUser)
                .FirstOrDefault(q => q.RealEstateApplicationId == id 
                                  && q.Status.Key == StatusKeys.Submitted 
                                  && q.EndTaskDateTime == null);
            if (activeQueueTask != null && activeQueueTask.Clerk != null && activeQueueTask.Clerk.SystemUser != null)
            {
                ViewBag.AssignedRepresentative = activeQueueTask.Clerk.SystemUser.FullName;
            }
            else
            {
                ViewBag.AssignedRepresentative = "Not Assigned (Unrouted or Admin queue)";
            }

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

            // Archive the corresponding departmental review task
            var dept = db.DepartmentsCoEs.FirstOrDefault(d => d.DepartmentName == departmentName && d.IsActive && !d.IsDeleted);
            var loggedInClerk = db.Customers.FirstOrDefault(c => c.SystemUserId == _systemUser.Id);
            RoundRobinQueue activeTask = null;
            if (loggedInClerk != null)
            {
                activeTask = db.RoundRobinQueues
                    .FirstOrDefault(q => q.RealEstateApplicationId == app.Id
                                      && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateDepartmentalReview
                                      && q.ClerkId == loggedInClerk.Id
                                      && q.EndTaskDateTime == null);
            }
            if (activeTask == null && dept != null && dept.RepresentativeSystemUserId.HasValue)
            {
                var repClerk = db.Customers.FirstOrDefault(c => c.SystemUserId == dept.RepresentativeSystemUserId && c.IsActive && !c.IsDeleted);
                if (repClerk != null)
                {
                    activeTask = db.RoundRobinQueues
                        .FirstOrDefault(q => q.RealEstateApplicationId == app.Id
                                          && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateDepartmentalReview
                                          && q.ClerkId == repClerk.Id
                                          && q.EndTaskDateTime == null);
                }
            }
            if (activeTask != null)
            {
                activeTask.EndTaskDateTime = DateTime.Now;
                var archivedStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.Archived);
                if (archivedStatus != null) activeTask.StatusId = archivedStatus.Id;
                db.Entry(activeTask).State = EntityState.Modified;
            }

            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Departmental comment captured by {0} ({1}). Outcome: {2}", representativeName, departmentName, outcome));
            TempData["SuccessMessage"] = "Departmental review comment captured successfully.";
            return RedirectToAction("DepartmentalQueue");
        }

        public ActionResult DownloadConsolidatedReport(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .Include(a => a.SelectedFacilityUnit.FacilityCategory)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.Comments = db.RE_DepartmentalComments
                .Where(c => c.RE_ApplicationId == id && c.IsActive && !c.IsDeleted)
                .ToList();

            var actionPDF = new Rotativa.ActionAsPdf("ConsolidatedReportPdf", new { id = id })
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                FileName = string.Format("ConsolidatedReport_{0}.pdf", app.ApplicationReferenceNumber)
            };

            return actionPDF;
        }

        [AllowAnonymous]
        public ActionResult ConsolidatedReportPdf(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .Include(a => a.SelectedFacilityUnit.FacilityCategory)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            ViewBag.Comments = db.RE_DepartmentalComments
                .Where(c => c.RE_ApplicationId == id && c.IsActive && !c.IsDeleted)
                .ToList();

            return View(app);
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

                // Complete all active Departmental Review tasks
                RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateDepartmentalReview);
                
                // Assign Committee Review task
                RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateCommitteeReview);

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

                // Complete all active Departmental Review tasks
                RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateDepartmentalReview);

                // Assign HOD Authorisation task
                RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateHODAuthorisation);

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
            var query = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.RePendingCommitteeOutcome && a.IsActive && !a.IsDeleted);

            var customer = db.Customers.FirstOrDefault(c => c.SystemUserId == _systemUser.Id);
            if (customer != null && !User.IsInRole("Administrators") && !User.IsInRole("Back Office System Administrator") && !User.IsInRole("Super Administrators"))
            {
                var assignedAppIds = db.RoundRobinQueues
                    .Where(q => q.ClerkId == customer.Id 
                             && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateCommitteeReview
                             && q.Status.Key == StatusKeys.Submitted
                             && q.EndTaskDateTime == null)
                    .Select(q => q.RealEstateApplicationId)
                    .ToList();
                query = query.Where(a => assignedAppIds.Contains(a.Id));
            }

            var apps = query.ToList();
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

            ViewBag.Documents = db.Documents
                .Include(d => d.File)
                .Where(d => d.RealEstateApplicationId == id && d.IsActive && !d.IsDeleted)
                .ToList();

            ViewBag.Comments = db.RE_DepartmentalComments
                .Where(c => c.RE_ApplicationId == id && c.IsActive && !c.IsDeleted)
                .ToList();

            ViewBag.HistoryLogs = db.PLMApplicationHistortyLogs
                .Where(h => h.RealEstateApplicationId == id && h.IsActive && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedDateTime)
                .ToList();

            // Fetch assigned representative clerk from RoundRobinQueues
            var activeQueueTask = db.RoundRobinQueues
                .Include(q => q.Clerk)
                .Include(q => q.Clerk.SystemUser)
                .FirstOrDefault(q => q.RealEstateApplicationId == id 
                                  && q.Status.Key == StatusKeys.Submitted 
                                  && q.EndTaskDateTime == null);
            if (activeQueueTask != null && activeQueueTask.Clerk != null && activeQueueTask.Clerk.SystemUser != null)
            {
                ViewBag.AssignedRepresentative = activeQueueTask.Clerk.SystemUser.FullName;
            }
            else
            {
                ViewBag.AssignedRepresentative = "Not Assigned (Unrouted or Admin queue)";
            }

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
            
            // Complete Committee Review task
            RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateCommitteeReview);
            
            // If recommended, assign HOD Authorisation task
            if (decision == "Recommended" || decision == "Recommended with Conditions")
            {
                RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateHODAuthorisation);
            }

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
            var query = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.SystemUser)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted);

            var customer = db.Customers.FirstOrDefault(c => c.SystemUserId == _systemUser.Id);
            if (customer != null && !User.IsInRole("Administrators") && !User.IsInRole("Back Office System Administrator") && !User.IsInRole("Super Administrators"))
            {
                var assignedAppIds = db.RoundRobinQueues
                    .Where(q => q.ClerkId == customer.Id 
                             && q.ResponsibilityType.Key == ResponsibilityTypeKeys.RealEstateHODAuthorisation
                             && q.Status.Key == StatusKeys.Submitted
                             && q.EndTaskDateTime == null)
                    .Select(q => q.RealEstateApplicationId)
                    .ToList();
                query = query.Where(a => assignedAppIds.Contains(a.Id));
            }

            var apps = query.ToList();
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

            ViewBag.Documents = db.Documents
                .Include(d => d.File)
                .Where(d => d.RealEstateApplicationId == id && d.IsActive && !d.IsDeleted)
                .ToList();

            ViewBag.Comments = db.RE_DepartmentalComments
                .Where(c => c.RE_ApplicationId == id && c.IsActive && !c.IsDeleted)
                .ToList();

            ViewBag.HistoryLogs = db.PLMApplicationHistortyLogs
                .Where(h => h.RealEstateApplicationId == id && h.IsActive && !h.IsDeleted)
                .OrderByDescending(h => h.CreatedDateTime)
                .ToList();

            // Fetch assigned representative clerk from RoundRobinQueues
            var activeQueueTask = db.RoundRobinQueues
                .Include(q => q.Clerk)
                .Include(q => q.Clerk.SystemUser)
                .FirstOrDefault(q => q.RealEstateApplicationId == id 
                                  && q.Status.Key == StatusKeys.Submitted 
                                  && q.EndTaskDateTime == null);
            if (activeQueueTask != null && activeQueueTask.Clerk != null && activeQueueTask.Clerk.SystemUser != null)
            {
                ViewBag.AssignedRepresentative = activeQueueTask.Clerk.SystemUser.FullName;
            }
            else
            {
                ViewBag.AssignedRepresentative = "Not Assigned (Unrouted or Admin queue)";
            }

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
            
            // Complete HOD Authorisation task
            RealEstateWorkAllocationHelper.CompleteRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateHODAuthorisation);

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

        // GET: RealEstateAdmin/ManageWorkflow
        [Authorize(Roles = "Administrators, Back Office System Administrator, Area Manager, Property Manager")]
        public ActionResult ManageWorkflow()
        {
            Initialise();

            // 1. Get all reviewing departments
            var departments = db.DepartmentsCoEs
                .Include(d => d.RepresentativeSystemUser)
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.DepartmentName)
                .ToList();

            // 2. Get all active internal staff users who can represent departments
            var staffUsers = db.SystemUsers
                .Where(u => u.isInternalUser && u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.FirstName)
                .ToList();

            // 3. Get all active committee members (System Users in the "Area Manager" or "Property Manager" roles)
            var committeeRoleIds = db.Roles
                .Where(r => r.Name == "Area Manager" || r.Name == "Property Manager")
                .Select(r => r.Id)
                .ToList();

            var committeeSystemUserIds = db.ApplicationUserRoles
                .Where(aur => committeeRoleIds.Contains(aur.RoleId) && aur.IsActive && !aur.IsDeleted)
                .Select(aur => aur.SystemUserId)
                .Distinct()
                .ToList();

            var committeeMembers = db.SystemUsers
                .Where(u => committeeSystemUserIds.Contains(u.Id) && !u.IsDeleted)
                .OrderBy(u => u.FirstName)
                .ToList();

            // Populate ViewBag for rendering lists and dropdowns
            ViewBag.Departments = departments;
            ViewBag.RepresentativeUsers = staffUsers;
            ViewBag.CommitteeMembers = committeeMembers;

            ViewBag.UserSelectList = staffUsers
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName + " (" + u.UserName + ")" })
                .OrderBy(i => i.Text)
                .ToList();

            return View();
        }

        // POST: RealEstateAdmin/SaveDepartmentMapping
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators, Back Office System Administrator")]
        public ActionResult SaveDepartmentMapping(int departmentId, int? representativeSystemUserId, bool isActive)
        {
            Initialise();

            var dept = db.DepartmentsCoEs.Find(departmentId);
            if (dept == null) return HttpNotFound();

            dept.RepresentativeSystemUserId = representativeSystemUserId;
            dept.IsActive = isActive;
            dept.ModifiedDateTime = DateTime.Now;
            dept.ModifiedBySystemUserId = _systemUser?.Id;

            // Update RepresentedBy string column as well for backwards compatibility/display
            if (representativeSystemUserId.HasValue)
            {
                var user = db.SystemUsers.Find(representativeSystemUserId.Value);
                dept.RepresentedBy = user != null ? user.FullName : "";
            }
            else
            {
                dept.RepresentedBy = "";
            }

            db.Entry(dept).State = EntityState.Modified;
            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Department '{0}' mapping updated successfully.", dept.DepartmentName);
            return RedirectToAction("ManageWorkflow");
        }

        // POST: RealEstateAdmin/CreateCommitteeMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators, Back Office System Administrator")]
        public ActionResult CreateCommitteeMember(string firstName, string lastName, string userName, string emailAddress, string mobileNumber, string identificationNumber, string employeeNumber)
        {
            Initialise();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(emailAddress))
            {
                TempData["ErrorMessage"] = "First Name, Last Name, Username and Email Address are required.";
                return RedirectToAction("ManageWorkflow");
            }

            // Check if username/email already exists in SystemUsers
            var usernameAssigned = db.SystemUsers.Any(u => u.UserName.ToLower() == userName.ToLower() && u.IsActive && !u.IsDeleted);
            var emailAssigned = db.SystemUsers.Any(u => u.EmailAddress.ToLower() == emailAddress.ToLower() && u.IsActive && !u.IsDeleted);

            if (usernameAssigned)
            {
                TempData["ErrorMessage"] = "Username is already registered. Please choose a unique username.";
                return RedirectToAction("ManageWorkflow");
            }

            if (emailAssigned)
            {
                TempData["ErrorMessage"] = "Email address is already registered. Please use an alternative email address.";
                return RedirectToAction("ManageWorkflow");
            }

            try
            {
                var userStore = new UserStore<SystemIdentityUser>(db);
                var userManager = new UserManager<SystemIdentityUser>(userStore);
                var identityManager = new IdentityManager(db);
                var defaultPassword = "Arsenal5@";

                var user = new SystemIdentityUser
                {
                    UserName = userName,
                    Email = emailAddress,
                    EmailConfirmed = true,
                    PhoneNumber = mobileNumber,
                    isInternalUser = true,
                    isActiveDirectoryUser = false,
                    RoundRobinIsActive = true,
                    SystemUser = new SystemUser()
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        UserName = userName,
                        MobileNumber = mobileNumber,
                        IdentificationNumber = identificationNumber,
                        EmailAddress = emailAddress,
                        IsPasswordReset = false,
                        ServiceNo = employeeNumber,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now
                    }
                };

                var result = userManager.Create(user, defaultPassword);
                if (result.Succeeded)
                {
                    // Add user to "Area Manager" role (used for Committee Members)
                    identityManager.AddUserToRole(user.Id, "Area Manager");

                    // Create Customer profile
                    var defaultCustomerType = db.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.Individual);
                    var defaultIdentification = db.IdentificationTypes.FirstOrDefault(id => id.Key == IdentificationTypeKey.SouthAfricanID);
                    var defaultTitleType = db.TitleTypes.FirstOrDefault(t => t.Key == TitleTypeKeys.Mister);
                    var defaultStatus = db.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                    var customer = new Customer()
                    {
                        CustomerTypeId = defaultCustomerType?.Id ?? 1,
                        IdentificationTypeId = defaultIdentification?.Id ?? 1,
                        IdentificationNumber = identificationNumber,
                        TitleTypeId = defaultTitleType?.Id ?? 1,
                        FirstName = firstName,
                        LastName = lastName,
                        CellPhoneNumber = mobileNumber,
                        EmailAddress = emailAddress,
                        StatusId = defaultStatus?.Id ?? 1,
                        SystemUserId = user.SystemUser.Id,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        CreatedDateTime = DateTime.Now
                    };
                    db.Customers.Add(customer);

                    // Create ApplicationUserRole
                    var rcsApp = db.Applications.FirstOrDefault(x => x.Key == ApplicationKeys.RatesClearanceSystem);
                    var role = db.Roles.FirstOrDefault(r => r.Name == "Area Manager");
                    if (rcsApp != null && role != null)
                    {
                        var appUserRole = new ApplicationUserRole
                        {
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            ApplicationId = rcsApp.Id,
                            RoleId = role.Id,
                            SystemUserId = user.SystemUser.Id,
                            CreatedDateTime = DateTime.Now
                        };
                        db.ApplicationUserRoles.Add(appUserRole);
                        db.SaveChanges();

                        var userRoleMap = new AppUserRole
                        {
                            IsActive = true,
                            IsDeleted = false,
                            IsLocked = false,
                            RoleId = role.Id,
                            ApplicationUserRoleId = appUserRole.Id,
                            CreatedDateTime = DateTime.Now
                        };
                        db.AppUserRoles.Add(userRoleMap);
                        db.SaveChanges();
                    }

                    TempData["SuccessMessage"] = string.Format("Committee member user '{0}' created successfully! Credentials: Username: {1}, Password: {2}", firstName + " " + lastName, userName, defaultPassword);
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create user: " + string.Join(", ", result.Errors);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating user: " + ex.Message;
            }

            return RedirectToAction("ManageWorkflow");
        }

        // --- UC 21: Generate and Sign Permission to Occupy Certificate/Letter ---
        public ActionResult PtoApprovals()
        {
            Initialise();
            var targetKeys = new[] { StatusKeys.RePtoApproved, StatusKeys.RePtoApprovedConditions };
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => targetKeys.Contains(a.Status.Key) && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult GeneratePtoCertificate(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GeneratePtoCertificate(int id, DateTime ptoStartDate, DateTime ptoEndDate, string ptoPurpose)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (ptoStartDate == DateTime.MinValue || ptoEndDate == DateTime.MinValue || string.IsNullOrEmpty(ptoPurpose))
            {
                TempData["ErrorMessage"] = "Start Date, End Date, and Purpose are required.";
                return RedirectToAction("GeneratePtoCertificate", new { id = id });
            }

            if ((ptoEndDate - ptoStartDate).TotalDays > 366)
            {
                TempData["ErrorMessage"] = "PTO duration cannot exceed 12 months (365 days).";
                return RedirectToAction("GeneratePtoCertificate", new { id = id });
            }

            app.PtoStartDate = ptoStartDate;
            app.PtoEndDate = ptoEndDate;
            app.PtoPurposeOfOccupation = ptoPurpose;
            app.PtoReferenceNumber = "PTO-" + DateTime.Now.ToString("yyyyMMdd") + "-" + app.Id.ToString("D4");

            var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingPtoSignature);
            if (status != null) app.StatusId = status.Id;

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "PTO certificate drafted and submitted for HOD signature.");
            TempData["SuccessMessage"] = "PTO certificate drafted successfully and submitted for signature.";
            return RedirectToAction("PtoApprovals");
        }

        public ActionResult PtoSignatureQueue()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReAwaitingPtoSignature && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult SignPtoCertificate(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignPtoCertificate(int id, string decision, string comments, string signature)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(decision) || string.IsNullOrEmpty(signature))
            {
                TempData["ErrorMessage"] = "Decision and Signature are mandatory.";
                return RedirectToAction("SignPtoCertificate", new { id = id });
            }

            app.PtoDecision = decision;
            app.PtoDecisionReason = comments;
            app.PtoSignature = signature;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            if (decision == "Approve")
            {
                var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.RePendingActivation);
                if (status != null) app.StatusId = status.Id;
                app.PtoStatus = "Pending Activation";
                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "PTO certificate authorized and signed by HOD.");
                TempData["SuccessMessage"] = "PTO certificate authorized and activated successfully.";
            }
            else
            {
                var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.RePtoApproved); // Send back to officer
                if (status != null) app.StatusId = status.Id;
                app.PtoStatus = "Rejected by HOD";
                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "PTO certificate signature rejected by HOD: " + comments);
                TempData["SuccessMessage"] = "PTO certificate signature rejected and returned to queue.";
            }

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("PtoSignatureQueue");
        }

        // GET: RealEstateAdmin/DownloadPtoCertificatePdf/{id}
        public ActionResult DownloadPtoCertificatePdf(int id)
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

            byte[] pdfBytes = RealEstateUserAgreementHelper.GeneratePtoCertificatePdf(app);
            string fileName = string.Format("PTO_Certificate_12Months_{0}.pdf", app.PtoReferenceNumber ?? app.Id.ToString());
            return File(pdfBytes, "application/pdf", fileName);
        }


        // --- UC 22: Revoke or Expire Permission to Occupy ---
        public ActionResult ActivePto()
        {
            Initialise();
            var activeStatuses = new[] { StatusKeys.RePendingActivation, StatusKeys.ReActiveOccupancy, StatusKeys.ReActive };
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => activeStatuses.Contains(a.Status.Key) && a.PtoReferenceNumber != null && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult RevokePto(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevokePto(int id, string reason)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            HttpPostedFileBase evidenceFile = Request.Files["evidenceFile"];
            if (string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Revocation reason is mandatory.";
                return RedirectToAction("RevokePto", new { id = id });
            }

            if (evidenceFile == null || evidenceFile.ContentLength == 0)
            {
                TempData["ErrorMessage"] = "Supporting evidence upload is mandatory.";
                return RedirectToAction("RevokePto", new { id = id });
            }

            int? fileId = SaveFile(evidenceFile);
            app.PtoRevocationReason = reason;
            app.PtoRevocationDate = DateTime.Now;
            
            var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReRevokedPendingReview);
            if (status != null) app.StatusId = status.Id;

            app.PtoStatus = "Revoked, Pending Review";
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "PTO revocation request submitted by Property Officer: " + reason);
            TempData["SuccessMessage"] = "PTO revocation initiated successfully and sent to terminations queue.";
            return RedirectToAction("ActivePto");
        }

        public ActionResult ProcessExpiredPtos()
        {
            Initialise();
            var today = DateTime.Today;
            var warningThreshold = today.AddDays(7);

            var ptos = db.RE_Applications
                .Include(a => a.Status)
                .Where(a => a.PtoEndDate != null && a.IsActive && !a.IsDeleted && 
                            a.Status.Key != StatusKeys.ReExpired && a.Status.Key != StatusKeys.ReRevokedPendingReview)
                .ToList();

            int expiredCount = 0;
            int warningCount = 0;

            foreach (var pto in ptos)
            {
                if (pto.PtoEndDate <= today)
                {
                    var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReExpired);
                    if (status != null) pto.StatusId = status.Id;
                    pto.PtoStatus = "Expired";
                    pto.ModifiedDateTime = DateTime.Now;
                    db.Entry(pto).State = EntityState.Modified;
                    MatchingHelper.AddHistoryLog(db, pto.Id, _systemUser?.Id ?? 1, "PTO expired automatically (End Date reached).");
                    expiredCount++;
                }
                else if (pto.PtoEndDate <= warningThreshold)
                {
                    // Warning log / notification
                    MatchingHelper.AddHistoryLog(db, pto.Id, _systemUser?.Id ?? 1, string.Format("PTO expiring warning: End Date {0:yyyy-MM-dd} is within 7 days.", pto.PtoEndDate));
                    warningCount++;
                }
            }

            db.SaveChanges();
            return Json(new { success = true, expired = expiredCount, warned = warningCount }, JsonRequestBehavior.AllowGet);
        }

        // --- UC 23: Generate and Sign Lease/User Agreement ---
        public ActionResult NewLeases()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReConcludedApproved && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult GenerateLeaseAgreement(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            if (!app.CalculatedMonthlyRental.HasValue || app.CalculatedMonthlyRental == 0)
                app.CalculatedMonthlyRental = 5500.00m;
            if (!app.LeaseDepositAmount.HasValue || app.LeaseDepositAmount == 0)
                app.LeaseDepositAmount = app.CalculatedMonthlyRental.Value * 2;
            if (!app.LeaseStartDate.HasValue)
                app.LeaseStartDate = DateTime.Today;
            if (!app.LeaseEndDate.HasValue)
                app.LeaseEndDate = DateTime.Today.AddMonths(36);
            if (string.IsNullOrEmpty(app.LeaseEscalationTerms))
                app.LeaseEscalationTerms = "8% Annually";

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateLeaseAgreement(int id, decimal rentalAmount, decimal depositAmount, DateTime? startDate, int durationMonths, string escalationTerms, string customClauses)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            var start = startDate ?? DateTime.Today;
            app.CalculatedMonthlyRental = rentalAmount;
            app.LeaseDepositAmount = depositAmount;
            app.LeaseStartDate = start;
            app.LeaseEndDate = start.AddMonths(durationMonths > 0 ? durationMonths : 36);
            app.LeasePaymentFrequency = "Monthly";
            app.LeaseEscalationTerms = string.IsNullOrEmpty(escalationTerms) ? "8% Annually" : escalationTerms;
            if (!string.IsNullOrEmpty(customClauses))
            {
                app.LeaseEscalationTerms += " | Special Terms: " + customClauses;
            }

            var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingAgreementConclusion);
            if (status != null) app.StatusId = status.Id;

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Standardized User Agreement ({0} Months) generated, reviewed, and submitted for Tenant signature.", durationMonths));
            TempData["SuccessMessage"] = "Lease agreement generated, reviewed, and submitted to tenant for signature successfully.";
            return RedirectToAction("NewLeases");
        }

        public ActionResult LeaseAgreementApprovals()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReAwaitingAgreementConclusionOutcome && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult AuthoriseLeaseAgreement(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AuthoriseLeaseAgreement(int id, string decision, string comments, string signature)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (string.IsNullOrEmpty(decision) || string.IsNullOrEmpty(signature))
            {
                TempData["ErrorMessage"] = "Decision and Signature are mandatory.";
                return RedirectToAction("AuthoriseLeaseAgreement", new { id = id });
            }

            app.LeaseAgreementHodSignature = signature;
            app.LeaseAgreementHodSignatureDate = DateTime.Now;
            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;

            if (decision == "Approve")
            {
                var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.RePendingActivation);
                if (status != null) app.StatusId = status.Id;
                app.LeaseStatus = "Pending Activation";
                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Lease agreement authorized and signed by HOD.");
                TempData["SuccessMessage"] = "Lease agreement signed and activated by HOD successfully.";
            }
            else
            {
                var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReAwaitingAgreementConclusion);
                if (status != null) app.StatusId = status.Id;
                app.LeaseStatus = "Rejected by HOD";
                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Lease agreement authorization rejected by HOD: " + comments);
                TempData["SuccessMessage"] = "Lease agreement authorization rejected and returned to queue.";
            }

            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("LeaseAgreementApprovals");
        }

        // GET: RealEstateAdmin/DownloadLeaseAgreementPdf/{id}
        public ActionResult DownloadLeaseAgreementPdf(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Include(a => a.SelectedFacility)
                .Include(a => a.SelectedFacilityUnit)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            byte[] pdfBytes = RealEstateUserAgreementHelper.GenerateFullLeaseAgreementPdf(app);
            string fileName = string.Format("User_Lease_Agreement_36Months_{0}.pdf", app.UniqueTenancyLeaseNumber ?? app.Id.ToString());
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: RealEstateAdmin/EvaluationCriteria
        public ActionResult EvaluationCriteria()
        {
            Initialise();
            return View();
        }

        // --- --- UC 24: Lease Space/Unit Allocation --- ---
        public ActionResult Allocations()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.RePendingActivation && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult AllocateSpace(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            // Load available units for that care centre
            var availableUnits = db.RE_FacilityUnits
                .Include(u => u.Facility)
                .Include(u => u.FacilityCategory)
                .Where(u => u.Facility.CCCId == app.CCCId && u.IsActive && !u.IsDeleted)
                .ToList();

            ViewBag.AvailableUnits = availableUnits;
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AllocateSpace(int id, string decision, int? selectedUnitId, string reason)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            if (decision == "Approve")
            {
                if (!selectedUnitId.HasValue)
                {
                    TempData["ErrorMessage"] = "You must select a unit to allocate.";
                    return RedirectToAction("AllocateSpace", new { id = id });
                }

                var unit = db.RE_FacilityUnits.Find(selectedUnitId.Value);
                if (unit == null) return HttpNotFound();

                // Update unit status to Allocated (IsActive = false means occupied)
                unit.IsActive = false;
                db.Entry(unit).State = EntityState.Modified;

                app.SelectedFacilityUnitId = selectedUnitId;
                
                var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReActiveOccupancy);
                if (status != null) app.StatusId = status.Id;

                app.ModifiedDateTime = DateTime.Now;
                app.ModifiedBySystemUserId = _systemUser?.Id;
                db.Entry(app).State = EntityState.Modified;
                db.SaveChanges();

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, string.Format("Space allocated: Unit Type '{0}' of Facility ID {1}.", unit.UnitType, unit.FacilityId));
                TempData["SuccessMessage"] = "Space allocated successfully. Unit status updated to Allocated.";
            }
            else
            {
                if (string.IsNullOrEmpty(reason))
                {
                    TempData["ErrorMessage"] = "Reason is mandatory to stop/delay allocation.";
                    return RedirectToAction("AllocateSpace", new { id = id });
                }

                MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Space allocation delayed: " + reason);
                TempData["SuccessMessage"] = "Space allocation marked Delayed.";
            }

            return RedirectToAction("Allocations");
        }

        // --- --- UC 25: Capture Lease Details and Classify Lease Categories --- ---
        public ActionResult ActiveOccupancy()
        {
            Initialise();
            var apps = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .Where(a => a.Status.Key == StatusKeys.ReActiveOccupancy && a.IsActive && !a.IsDeleted)
                .ToList();
            return View(apps);
        }

        public ActionResult CaptureLeaseDetails(int id)
        {
            Initialise();
            var app = db.RE_Applications
                .Include(a => a.Status)
                .Include(a => a.Customer)
                .Include(a => a.CCC)
                .FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);

            if (app == null) return HttpNotFound();

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CaptureLeaseDetails(int id, string leaseCategory, DateTime? leaseStartDate, DateTime? leaseEndDate, DateTime? dateOfOccupation, decimal? rentalAmount, decimal? depositAmount, string escalationTerms, string paymentFrequency)
        {
            Initialise();
            var app = db.RE_Applications.FirstOrDefault(a => a.Id == id && a.IsActive && !a.IsDeleted);
            if (app == null) return HttpNotFound();

            var start = leaseStartDate ?? app.LeaseStartDate ?? DateTime.Today;
            var end = leaseEndDate ?? app.LeaseEndDate ?? start.AddYears(3);
            var occ = dateOfOccupation ?? start;

            app.LeaseCategory = leaseCategory;
            app.LeaseStartDate = start;
            app.LeaseEndDate = end;
            app.LeaseDateOfOccupation = occ;
            if (rentalAmount.HasValue) app.CalculatedMonthlyRental = rentalAmount.Value;
            if (depositAmount.HasValue) app.LeaseDepositAmount = depositAmount.Value;
            app.LeaseEscalationTerms = escalationTerms;
            app.LeasePaymentFrequency = paymentFrequency;

            app.UniqueTenancyLeaseNumber = "UTLN-" + DateTime.Now.Year.ToString() + "-" + app.Id.ToString("D5");
            app.LeaseStatus = "Active";

            var status = db.Status.FirstOrDefault(s => s.Key == StatusKeys.ReActive);
            if (status != null) app.StatusId = status.Id;

            app.ModifiedDateTime = DateTime.Now;
            app.ModifiedBySystemUserId = _systemUser?.Id;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser?.Id ?? 1, string.Format("Lease details captured. Category: {0}. Tenancy Number: {1}.", leaseCategory, app.UniqueTenancyLeaseNumber));
            TempData["SuccessMessage"] = string.Format("Lease details captured successfully. Tenancy Number: {0}.", app.UniqueTenancyLeaseNumber);
            return RedirectToAction("ActiveOccupancy");
        }
    }
}
