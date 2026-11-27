using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    /// <summary>
    /// DPRE BR07 / DPRE-Q50: the Business Hub waiting list, worked by the Property Officer in date order.
    /// "Units free now" counts active units (a unit becomes inactive when it is allocated, see AllocateSpace).
    /// </summary>
    public partial class RealEstateAdminController
    {
        private bool CanManageWaitingList()
        {
            return RealEstateRoles.CanDoPropertyManagerWork(User) || User.IsInRole("Back Office System Administrator") ||
                   User.IsInRole("Super Administrators") || User.IsInRole("Administrators") || User.IsInRole("Support Admin");
        }

        // GET: RealEstateAdmin/WaitingList
        public ActionResult WaitingList(int? cccId)
        {
            Initialise();
            if (!CanManageWaitingList()) return new HttpStatusCodeResult(403, "The waiting list is limited to Property Officers and administrators.");

            var query = db.RE_WaitingList.Include(w => w.Customer).Include(w => w.CCC).Include(w => w.Facility)
                .Where(w => w.WaitingStatus != RE_WaitingListEntry.StatusRemoved && w.IsActive && !w.IsDeleted);
            if (cccId.HasValue) query = query.Where(w => w.CCCId == cccId.Value);
            var entries = query.OrderBy(w => w.CCCId).ThenBy(w => w.CreatedDateTime).ToList();

            // Free units per facility and per area, so the officer can see who can be offered a unit now.
            var freeUnits = db.RE_FacilityUnits.Include(u => u.Facility)
                .Where(u => u.IsActive && !u.IsDeleted && u.Facility.IsActive && !u.Facility.IsDeleted)
                .Select(u => new { u.FacilityId, u.Facility.CCCId })
                .ToList();
            ViewBag.FreeByFacility = freeUnits.GroupBy(u => u.FacilityId).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.FreeByCcc = freeUnits.GroupBy(u => u.CCCId).ToDictionary(g => g.Key, g => g.Count());
            ViewBag.CccFilter = new SelectList(db.CCCs.OrderBy(c => c.CCCName).ToList(), "Id", "CCCName", cccId);
            return View(entries);
        }

        // POST: RealEstateAdmin/OfferFromWaitingList
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OfferFromWaitingList(int id)
        {
            Initialise();
            if (!CanManageWaitingList()) return new HttpStatusCodeResult(403);
            var entry = db.RE_WaitingList.Include(w => w.Customer).Include(w => w.CCC).Include(w => w.Facility)
                .FirstOrDefault(w => w.Id == id && w.IsActive && !w.IsDeleted && w.WaitingStatus == RE_WaitingListEntry.StatusWaiting);
            if (entry == null) { TempData["ErrorMessage"] = "That waiting list entry is no longer waiting."; return RedirectToAction("WaitingList"); }

            entry.WaitingStatus = RE_WaitingListEntry.StatusOffered;
            entry.OfferedDateTime = DateTime.Now;
            entry.ModifiedBySystemUserId = _systemUser?.Id;
            entry.ModifiedDateTime = DateTime.Now;
            db.SaveChanges();

            try
            {
                var applicantUser = entry.Customer?.SystemUserId == null ? null : db.SystemUsers.Find(entry.Customer.SystemUserId.Value);
                if (applicantUser != null && !string.IsNullOrEmpty(applicantUser.EmailAddress))
                {
                    var where = entry.Facility != null ? entry.Facility.Name : (entry.CCC != null ? entry.CCC.CCCName : "your area");
                    var body = string.Format("Dear {0},<br/><br/>A Business Hub unit is now free at <strong>{1}</strong>. You were on the waiting list, so you can apply now from Lease Applications on the eServices portal.<br/><br/>Units are allocated through the normal application process.",
                        System.Web.HttpUtility.HtmlEncode(applicantUser.FullName), System.Web.HttpUtility.HtmlEncode(where));
                    new Email().GenerateEmail(applicantUser.EmailAddress, "Real Estate Development: a unit is available", body, entry.CustomerId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, applicantUser.FullName);
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }

            TempData["SuccessMessage"] = entry.Customer?.FullName + " has been told a unit is free and can apply now.";
            return RedirectToAction("WaitingList", new { cccId = entry.CCCId });
        }

        // POST: RealEstateAdmin/RemoveFromWaitingList
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromWaitingList(int id, string reason)
        {
            Initialise();
            if (!CanManageWaitingList()) return new HttpStatusCodeResult(403);
            if (string.IsNullOrWhiteSpace(reason)) { TempData["ErrorMessage"] = "Give a reason for removing someone from the waiting list."; return RedirectToAction("WaitingList"); }
            var entry = db.RE_WaitingList.FirstOrDefault(w => w.Id == id && w.IsActive && !w.IsDeleted);
            if (entry == null) return RedirectToAction("WaitingList");

            entry.WaitingStatus = RE_WaitingListEntry.StatusRemoved;
            entry.OutcomeReason = reason.Trim().Length > 500 ? reason.Trim().Substring(0, 500) : reason.Trim();
            entry.ModifiedBySystemUserId = _systemUser?.Id;
            entry.ModifiedDateTime = DateTime.Now;
            db.SaveChanges();
            TempData["SuccessMessage"] = "Removed from the waiting list.";
            return RedirectToAction("WaitingList", new { cccId = entry.CCCId });
        }
    }
}
