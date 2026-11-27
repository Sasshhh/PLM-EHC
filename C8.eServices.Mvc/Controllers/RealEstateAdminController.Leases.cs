using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    /// <summary>
    /// DPRE UCXX - Lease Administration: Lease Dashboard (per CCA), Lease Register and Capture/Edit Lease Details.
    /// </summary>
    public partial class RealEstateAdminController
    {
        private const int LeasePageSize = 50;

        private static readonly string[] LeaseAdminRoles =
        {
            "Property Officer", "Property Manager", "Property Managers", "Back Office System Administrator",
            "Super Administrators", "Administrators", "Support Admin"
        };

        /// <summary>
        /// The controller is [AllowAnonymous], so lease pages check sign-in and role themselves.
        /// UCXX actor confirmed 26 Nov 2026 as "Property Officer" (role added by DatabaseScripts/RE_PropertyOfficer_Role.sql);
        /// Property Managers and admin roles keep access.
        /// </summary>
        private ActionResult EnsureLeaseAdmin()
        {
            if (!Request.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            if (!LeaseAdminRoles.Any(r => User.IsInRole(r)))
                return new HttpStatusCodeResult(403, "Access Denied: Lease Administration is limited to Property Officers, Property Managers and administrators.");
            return null;
        }

        private void PopulateLeaseLookups()
        {
            ViewBag.Classifications = RE_LeaseLookups.Classifications;
            ViewBag.LeaseTerms = RE_LeaseLookups.LeaseTerms;
            ViewBag.ApplicantTypes = RE_LeaseLookups.ApplicantTypes;
            ViewBag.ContactTypes = RE_LeaseLookups.ContactTypes;
            ViewBag.RecordStatuses = RE_LeaseLookups.RecordStatuses;
            ViewBag.PaymentFrequencies = RE_LeaseLookups.PaymentFrequencies;
            // DPRE-Q08 (26 Nov 2026): CCA comes from the current CCC list, not the old towns or spreadsheet groups.
            var cccs = db.CCCs.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.CCCName).ToList();
            ViewBag.Ccas = cccs.Select(c => c.CCCName).ToArray();
            ViewBag.CCCList = new SelectList(cccs, "Id", "CCCName");
            ViewBag.DashboardStatuses = RE_LeaseLookups.DashboardStatuses;
        }

        private IQueryable<RE_Lease> ActiveLeases()
        {
            return db.RE_Leases.Where(l => !l.IsDeleted);
        }

        // GET: RealEstateAdmin/LeaseDashboard  (UCXX Step 3: Lease Administration > Lease Dashboard)
        public ActionResult LeaseDashboard()
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();

            var today = DateTime.Today;
            var leases = ActiveLeases().ToList();
            var withStatus = leases.Select(l => new { Lease = l, Status = RE_LeaseLookups.DeriveDashboardStatus(l, today) }).ToList();

            // CCA x status grid, same layout as the spreadsheet "LEASE DASHBOARD" pivot, one row per current CCC (DPRE-Q08)
            var ccas = db.CCCs.Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.CCCName).Select(c => c.CCCName).ToList()
                .Union(leases.Select(l => l.Cca).Where(c => !string.IsNullOrWhiteSpace(c)), StringComparer.OrdinalIgnoreCase)
                .ToList();
            var grid = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
            foreach (var cca in ccas)
                grid[cca] = RE_LeaseLookups.DashboardStatuses.ToDictionary(s => s, s => 0);
            const string noCca = "(No CCA)";
            foreach (var x in withStatus)
            {
                var key = string.IsNullOrWhiteSpace(x.Lease.Cca) ? noCca : x.Lease.Cca;
                if (!grid.ContainsKey(key)) grid[key] = RE_LeaseLookups.DashboardStatuses.ToDictionary(s => s, s => 0);
                grid[key][x.Status]++;
            }

            ViewBag.Grid = grid;
            ViewBag.StatusTotals = RE_LeaseLookups.DashboardStatuses.ToDictionary(s => s, s => withStatus.Count(x => x.Status == s));
            ViewBag.ClassificationTotals = withStatus
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Lease.LeaseClassification) ? "(Not classified)" : x.Lease.LeaseClassification)
                .OrderByDescending(g => g.Count())
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.TotalLeases = leases.Count;
            ViewBag.ExpiringIn90Days = withStatus.Count(x => x.Status == RE_LeaseLookups.StatusActive
                && (x.Lease.LeaseTerminationDate ?? x.Lease.LeaseEndDate).HasValue
                && (x.Lease.LeaseTerminationDate ?? x.Lease.LeaseEndDate).Value <= today.AddDays(90));
            ViewBag.AsAt = today;
            ViewBag.Alerts = RE_LeaseLookups.UpcomingAlerts(leases, today, 90);
            PopulateLeaseLookups();
            return View();
        }

        // GET: RealEstateAdmin/LeaseRegister
        public ActionResult LeaseRegister(string cca, string status, string classification, string q, int page = 1)
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();

            var query = ActiveLeases();
            if (!string.IsNullOrWhiteSpace(cca))
                query = cca == "(No CCA)" ? query.Where(l => l.Cca == null || l.Cca == "") : query.Where(l => l.Cca == cca);
            if (!string.IsNullOrWhiteSpace(classification))
                query = classification == "(Not classified)"
                    ? query.Where(l => l.LeaseClassification == null || l.LeaseClassification == "")
                    : query.Where(l => l.LeaseClassification == classification);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(l => l.LeaseReferenceNumber.Contains(term) || l.TenantName.Contains(term)
                    || l.PropertyCode.Contains(term) || l.TenantCode.Contains(term) || l.PhysicalStreetAddress.Contains(term)
                    || l.ContractReferenceNumber.Contains(term));
            }

            // Status is derived from dates, so filter it in memory (register is ~700 rows).
            var today = DateTime.Today;
            var list = query.OrderBy(l => l.LeaseReferenceNumber).ToList();
            if (!string.IsNullOrWhiteSpace(status))
                list = list.Where(l => RE_LeaseLookups.DeriveDashboardStatus(l, today) == status).ToList();

            page = Math.Max(1, page);
            ViewBag.TotalCount = list.Count;
            ViewBag.Page = page;
            ViewBag.PageCount = Math.Max(1, (int)Math.Ceiling(list.Count / (double)LeasePageSize));
            ViewBag.FilterCca = cca;
            ViewBag.FilterStatus = status;
            ViewBag.FilterClassification = classification;
            ViewBag.FilterQ = q;
            PopulateLeaseLookups();
            return View(list.Skip((page - 1) * LeasePageSize).Take(LeasePageSize).ToList());
        }

        // GET: RealEstateAdmin/LeaseDetails/5
        public ActionResult LeaseDetails(int id)
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();

            var lease = ActiveLeases().FirstOrDefault(l => l.Id == id);
            if (lease == null) return HttpNotFound();
            ViewBag.History = db.RE_LeaseAudits.Where(a => a.Id == id).OrderByDescending(a => a.AuditId).Take(50).ToList();
            ViewBag.LeaseDocuments = LeaseDocuments(lease);
            ViewBag.LeaseDocumentTypes = LeaseDocumentTypes;
            return View(lease);
        }

        /// <summary>
        /// DPRE-Q21 (assumption): supporting documents can be attached to a lease record. Lease documents are linked by
        /// reference type rt_real_estate_lease; documents from the lease's application are listed with them (read-only).
        /// </summary>
        private static readonly string[] LeaseDocumentTypes =
        {
            "Signed Lease Agreement", "Memorandum of Agreement", "Municipal Item / Council Resolution", "Advert",
            "Addendum", "Correspondence", "Other"
        };

        private List<Document> LeaseDocuments(RE_Lease lease)
        {
            var leaseRefTypeId = db.ReferenceTypes.Where(r => r.Key == ReferenceTypeKeys.RealEstateLease).Select(r => (int?)r.Id).FirstOrDefault();
            var docs = db.Documents
                .Where(d => d.IsActive && !d.IsDeleted
                    && ((leaseRefTypeId != null && d.ReferenceTypeId == leaseRefTypeId && d.ReferenceId == lease.Id)
                        || (lease.RE_ApplicationId != null && d.RealEstateApplicationId == lease.RE_ApplicationId)))
                .OrderByDescending(d => d.CreatedDateTime)
                .ToList();
            foreach (var doc in docs.Where(d => d.FileId != null))
            {
                doc.File = new C8.eServices.Mvc.Models.File { Id = doc.FileId.Value, CreatedDateTime = doc.CreatedDateTime };
                doc.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", doc.FileId));
            }
            return docs;
        }

        // POST: RealEstateAdmin/UploadLeaseDocument/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadLeaseDocument(int id, string documentType, string description, System.Web.HttpPostedFileBase leaseFile)
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();

            var lease = ActiveLeases().FirstOrDefault(l => l.Id == id);
            if (lease == null) return HttpNotFound();

            var ext = (System.IO.Path.GetExtension(leaseFile?.FileName) ?? "").ToLowerInvariant();
            string error = null;
            if (leaseFile == null || leaseFile.ContentLength == 0) error = "Select a file to upload.";
            else if (leaseFile.ContentLength > 10 * 1024 * 1024) error = "The file is larger than 10 MB.";
            else if (!new[] { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx" }.Contains(ext)) error = "Upload a PDF, Word or image file.";
            else if (!LeaseDocumentTypes.Contains(documentType)) error = "Select the document type.";
            var refTypeId = db.ReferenceTypes.Where(r => r.Key == ReferenceTypeKeys.RealEstateLease).Select(r => (int?)r.Id).FirstOrDefault();
            if (error == null && refTypeId == null) error = "Lease documents are not set up yet. Run DatabaseScripts/RE_LeaseDocuments_Setup.sql.";
            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction("LeaseDetails", new { id });
            }

            var fileId = SaveFile(leaseFile);
            var name = documentType + (string.IsNullOrWhiteSpace(description) ? "" : ": " + description.Trim());
            db.Documents.Add(new Document
            {
                ReferenceTypeId = refTypeId.Value,
                ReferenceId = lease.Id,
                FileId = fileId,
                DocumentName = name.Length > 250 ? name.Substring(0, 250) : name,
                LocationTypeId = db.LocationTypes.FirstOrDefault(l => l.Key == LocationTypeKeys.Database)?.Id ?? 3,
                StatusId = db.Status.First(s => s.Key == StatusKeys.DocumentUploaded).Id,
                DocumentCheckListId = db.DocumentCheckLists.FirstOrDefault(d => d.DocumentTypeId == 18)?.Id ?? 1,
                IsActive = true,
                IsDeleted = false,
                CreatedBySystemUserId = _systemUser?.Id,
                CreatedDateTime = DateTime.Now
            });
            db.SaveChanges();
            TempData["SuccessMessage"] = "Document uploaded to lease " + lease.LeaseReferenceNumber + ".";
            return RedirectToAction("LeaseDetails", new { id });
        }

        // POST: RealEstateAdmin/RemoveLeaseDocument  (soft delete; only documents attached to the lease, not application documents)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveLeaseDocument(int id, int documentId)
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();

            var refTypeId = db.ReferenceTypes.Where(r => r.Key == ReferenceTypeKeys.RealEstateLease).Select(r => (int?)r.Id).FirstOrDefault();
            var doc = db.Documents.FirstOrDefault(d => d.Id == documentId && d.ReferenceId == id && d.ReferenceTypeId == refTypeId && !d.IsDeleted);
            if (doc != null)
            {
                doc.IsDeleted = true;
                doc.IsActive = false;
                doc.ModifiedBySystemUserId = _systemUser?.Id;
                doc.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Document removed.";
            }
            return RedirectToAction("LeaseDetails", new { id });
        }

        // GET: RealEstateAdmin/CaptureLease  (new)  or  /CaptureLease/5 (edit)
        public ActionResult CaptureLease(int? id)
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();
            PopulateLeaseLookups();

            if (id.HasValue)
            {
                var lease = ActiveLeases().FirstOrDefault(l => l.Id == id.Value);
                if (lease == null) return HttpNotFound();
                return View(lease);
            }
            return View(new RE_Lease { RecordStatus = RE_LeaseLookups.StatusActive, AgreementInPlace = true, ContactType = "Tenant" });
        }

        // POST: RealEstateAdmin/CaptureLease  (UCXX Steps 7-10: Submit, confirm, success, form ready for the next lease)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CaptureLease(RE_Lease model, string submitAction)
        {
            var denied = EnsureLeaseAdmin();
            if (denied != null) return denied;
            Initialise();

            model.LeaseReferenceNumber = model.LeaseReferenceNumber?.Trim().ToUpperInvariant();
            if (model.Id == 0 && string.IsNullOrEmpty(model.LeaseReferenceNumber))
            {
                model.LeaseReferenceNumber = NextLeaseReferenceFor(model.LeaseClassification, model.AgreementInPlace, model.IsMemorandumOfAgreement);
                ModelState.Remove("LeaseReferenceNumber");
            }
            ValidateLease(model);

            if (!ModelState.IsValid)
            {
                PopulateLeaseLookups();
                return View(model);
            }

            var realEstateDeptId = db.ApplicationEntities.FirstOrDefault(ae => ae.Key == ApplicationEntityKeys.RealEstateDevelopment)?.Id;
            // CCA is picked from the CCC list; Cca keeps the name for the dashboard and register.
            model.Cca = model.CCCId.HasValue ? db.CCCs.Where(c => c.Id == model.CCCId.Value).Select(c => c.CCCName).FirstOrDefault() : null;

            string savedRef;
            bool isNew = model.Id == 0;
            if (isNew)
            {
                model.Source = "Capture";
                model.DepartmentId = realEstateDeptId;
                model.CreatedBySystemUserId = _systemUser?.Id;
                model.CreatedDateTime = DateTime.Now;
                model.ModifiedBySystemUserId = _systemUser?.Id;
                model.ModifiedDateTime = DateTime.Now;
                db.RE_Leases.Add(model);
                savedRef = model.LeaseReferenceNumber;
            }
            else
            {
                var existing = ActiveLeases().FirstOrDefault(l => l.Id == model.Id);
                if (existing == null) return HttpNotFound();
                // Keep system fields; copy everything the form posts.
                var keep = new { existing.Source, existing.DepartmentId, existing.CreatedBySystemUserId, existing.CreatedDateTime, existing.RE_ApplicationId, existing.IsActive, existing.IsLocked };
                db.Entry(existing).CurrentValues.SetValues(model);
                existing.Source = keep.Source;
                existing.DepartmentId = keep.DepartmentId ?? realEstateDeptId;
                existing.CreatedBySystemUserId = keep.CreatedBySystemUserId;
                existing.CreatedDateTime = keep.CreatedDateTime;
                existing.RE_ApplicationId = keep.RE_ApplicationId;
                existing.IsActive = keep.IsActive;
                existing.IsLocked = keep.IsLocked;
                existing.IsDeleted = false;
                existing.ModifiedBySystemUserId = _systemUser?.Id;
                existing.ModifiedDateTime = DateTime.Now;
                savedRef = existing.LeaseReferenceNumber;
            }

            db.SaveChanges();

            TempData["SuccessMessage"] = string.Format("Lease record {0} has been confirmed and saved.", savedRef);
            // UCXX Step 10: success, then the capture screen is ready for another lease. Edits go back to the details page.
            if (!isNew && submitAction != "new")
                return RedirectToAction("LeaseDetails", new { id = model.Id });
            return RedirectToAction("CaptureLease", new { id = (int?)null });
        }

        private string NextLeaseReferenceFor(string classification, bool agreementInPlace, bool isMoa)
        {
            var series = RE_LeaseLookups.LeaseReferenceSeries(classification, agreementInPlace, isMoa);
            var existing = db.RE_Leases.Where(l => l.LeaseReferenceNumber.StartsWith(series)).Select(l => l.LeaseReferenceNumber).ToList();
            return RE_LeaseLookups.NextLeaseReference(series, existing);
        }

        /// <summary>
        /// ASSUMPTION (DPRE-Q02): UCXX also covers new leases coming out of the application workflow. When the lease details of an
        /// approved application are captured (RE_UC025), the lease register record is created or updated from the application.
        /// </summary>
        private void UpsertLeaseFromApplication(RE_Application app)
        {
            var lease = db.RE_Leases.FirstOrDefault(l => l.RE_ApplicationId == app.Id && !l.IsDeleted);
            bool isNew = lease == null;
            var site = db.RE_ApplicationSiteDetails.FirstOrDefault(s => s.RE_ApplicationId == app.Id);
            var classification = RE_LeaseLookups.FindLeaseApplicationType(app.LeaseType)?.Name ?? "Business Hub";
            var ccc = db.CCCs.Find(app.CCCId);

            if (isNew)
            {
                lease = new RE_Lease
                {
                    RE_ApplicationId = app.Id,
                    Source = "Application",
                    DepartmentId = app.DepartmentId,
                    RecordStatus = RE_LeaseLookups.StatusActive,
                    AgreementInPlace = true,
                    ContactType = "Tenant",
                    CreatedBySystemUserId = _systemUser?.Id,
                    CreatedDateTime = DateTime.Now,
                    LeaseReferenceNumber = NextLeaseReferenceFor(classification, true, false)
                };
                db.RE_Leases.Add(lease);
            }

            lease.ContractReferenceNumber = app.UniqueTenancyLeaseNumber;
            lease.LeaseClassification = classification;
            lease.LeaseTerm = "Lease Agreement";
            lease.PhysicalStreetAddress = app.PropertyAddress;
            lease.PhysicalBuildingNumberAndName = app.SelectedFacility != null ? app.SelectedFacility.Name : app.ErfFarmNumber;
            lease.CCCId = app.CCCId;
            lease.Cca = ccc != null ? ccc.CCCName : null;
            lease.LeaseDetailsAndPurpose = string.Format("{0}. Application {1}.", app.PurposeOfLease, app.ApplicationReferenceNumber);
            lease.LeasedAreaSqm = site != null ? (site.TotalExtentSqm ?? ((site.AreaGroundSqm ?? 0) + (site.AreaOnStructureSqm ?? 0))) : (decimal?)null;
            lease.LeaseStartDate = app.LeaseStartDate;
            lease.LeaseEndDate = app.LeaseEndDate;
            lease.EscalationDate = app.LeaseStartDate.HasValue ? app.LeaseStartDate.Value.AddYears(1) : (DateTime?)null;
            lease.EscalationTerms = app.LeaseEscalationTerms;
            lease.PaymentFrequency = app.LeasePaymentFrequency;
            lease.MonthlyRental = app.CalculatedMonthlyRental;
            // DPRE-Q11: escalation % comes from the facility category set up in the Administrator console,
            // unless the Property Officer has already captured one on the lease.
            if (lease.EscalationPercentage == null && app.SelectedFacilityUnitId.HasValue)
                lease.EscalationPercentage = db.RE_FacilityUnits.Where(u => u.Id == app.SelectedFacilityUnitId.Value)
                    .Select(u => u.FacilityCategory.EscalationPercentage).FirstOrDefault();
            lease.DepositHeld = app.LeaseDepositAmount;
            lease.TenantName = string.IsNullOrWhiteSpace(app.EntityName) ? "(Not captured)" : app.EntityName;
            lease.ApplicantType = app.ApplicantType;
            lease.RegistrationNumber = app.CompanyRegistrationNumber;
            lease.VatRegistrationNumber = app.VatRegistrationNumber;
            lease.DomiciliumStreetAddress = app.EntityRegisteredAddress;
            lease.DomiciliumPostalCode = app.EntityRegisteredPostalCode;
            lease.BillingContactName = app.AuthorizedRepresentativeName ?? app.EntityName;
            lease.BillingStreetAddress = site != null ? site.PostalAddress : app.EntityRegisteredAddress;
            lease.BillingMobile1 = app.EntityMobile;
            lease.BillingTelephone1 = app.EntityTelephone;
            lease.BillingFax1 = app.EntityFax;
            lease.BillingEmail1 = app.EntityEmail;
            lease.LeaseContactName = app.AuthorizedRepresentativeName ?? app.EntityName;
            lease.LeaseContactMobile1 = app.EntityMobile;
            lease.LeaseContactEmail1 = app.EntityEmail;
            lease.ModifiedBySystemUserId = _systemUser?.Id;
            lease.ModifiedDateTime = DateTime.Now;
            TruncateToMaxLength(lease);   // application fields are longer than some register columns
            db.SaveChanges();
        }

        private static void TruncateToMaxLength(object entity)
        {
            foreach (var p in entity.GetType().GetProperties().Where(x => x.PropertyType == typeof(string) && x.CanWrite))
            {
                var max = p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute), true)
                    .Cast<System.ComponentModel.DataAnnotations.MaxLengthAttribute>().FirstOrDefault();
                var v = p.GetValue(entity) as string;
                if (max != null && v != null && v.Length > max.Length) p.SetValue(entity, v.Substring(0, max.Length));
            }
        }

        private void ValidateLease(RE_Lease model)
        {
            if (string.IsNullOrWhiteSpace(model.LeaseReferenceNumber))
                ModelState.AddModelError("LeaseReferenceNumber", "Lease Reference Number is required.");
            else if (ActiveLeases().Any(l => l.LeaseReferenceNumber == model.LeaseReferenceNumber && l.Id != model.Id))
                ModelState.AddModelError("LeaseReferenceNumber", "Lease Reference Number " + model.LeaseReferenceNumber + " already exists. It must be unique.");

            if (string.IsNullOrWhiteSpace(model.TenantName))
                ModelState.AddModelError("TenantName", "Tenant Name is required.");
            if (string.IsNullOrWhiteSpace(model.LeaseClassification))
                ModelState.AddModelError("LeaseClassification", "Select a Lease Classification.");
            if (model.LeaseStartDate.HasValue && model.LeaseEndDate.HasValue && model.LeaseEndDate < model.LeaseStartDate)
                ModelState.AddModelError("LeaseEndDate", "Lease End Date cannot be before the Lease Start Date.");
            if (model.LeaseIndefinite && model.LeaseEndDate.HasValue)
                ModelState.AddModelError("LeaseEndDate", "An indefinite lease should not have an end date. Clear the end date or untick Lease Indefinite.");
            if (model.ApplicantType == "Other" && string.IsNullOrWhiteSpace(model.ApplicantTypeOther))
                ModelState.AddModelError("ApplicantTypeOther", "Please specify the applicant type.");
            if (model.ContactType == "Authorised Representative" && string.IsNullOrWhiteSpace(model.AuthorisedRepresentative))
                ModelState.AddModelError("AuthorisedRepresentative", "Enter the Authorised Representative's full name.");
            if (model.LeasedAreaSqm.HasValue && model.LeasedAreaSqm < 0)
                ModelState.AddModelError("LeasedAreaSqm", "Leased Area cannot be negative.");
            if (model.DepositHeld.HasValue && model.DepositHeld < 0)
                ModelState.AddModelError("DepositHeld", "Deposit Held cannot be negative.");
            if (model.TerminationNoticePeriodMonths.HasValue && (model.TerminationNoticePeriodMonths < 0 || model.TerminationNoticePeriodMonths > 120))
                ModelState.AddModelError("TerminationNoticePeriodMonths", "Notice period must be between 0 and 120 months.");
        }

        /// <summary>
        /// UC05 Scenarios 2-4: shows the Outdoor Advertising / Telecommunications / Leasehold answers on back-office screens.
        /// Renders nothing for Business Hub applications.
        /// </summary>
        [ChildActionOnly]
        public ActionResult SiteDetailsPanel(int applicationId)
        {
            var site = db.RE_ApplicationSiteDetails.FirstOrDefault(s => s.RE_ApplicationId == applicationId && !s.IsDeleted);
            if (site == null) return new EmptyResult();
            return PartialView("_RE_SiteDetailsPanel", site);
        }
    }
}
