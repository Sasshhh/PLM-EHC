using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
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
            "Property Manager", "Property Managers", "Back Office System Administrator",
            "Super Administrators", "Administrators", "Support Admin"
        };

        /// <summary>
        /// The controller is [AllowAnonymous], so lease pages check sign-in and role themselves.
        /// ASSUMPTION (DPRE-Q: actor "Property/LED Officer" has no role yet): Property Manager and admin roles can use these pages.
        /// </summary>
        private ActionResult EnsureLeaseAdmin()
        {
            if (!Request.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            if (!LeaseAdminRoles.Any(r => User.IsInRole(r)))
                return new HttpStatusCodeResult(403, "Access Denied: Lease Administration is limited to Property Managers and administrators.");
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
            ViewBag.Ccas = RE_LeaseLookups.Ccas;
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

            // CCA x status grid, same layout as the spreadsheet "LEASE DASHBOARD" pivot
            var ccas = RE_LeaseLookups.Ccas
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
            return View(lease);
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
            ValidateLease(model);

            if (!ModelState.IsValid)
            {
                PopulateLeaseLookups();
                return View(model);
            }

            var realEstateDeptId = db.ApplicationEntities.FirstOrDefault(ae => ae.Key == ApplicationEntityKeys.RealEstateDevelopment)?.Id;
            if (!string.IsNullOrWhiteSpace(model.Cca))
                model.CCCId = db.CCCs.Where(c => c.CCCName.ToUpper() == model.Cca.ToUpper()).Select(c => (int?)c.Id).FirstOrDefault();

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
    }
}
