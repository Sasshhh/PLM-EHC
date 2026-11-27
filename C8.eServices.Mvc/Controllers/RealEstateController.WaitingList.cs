using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    /// <summary>
    /// DPRE BR07 / DPRE-Q50 (built on the recommended answer, to confirm with the BA): when no Business Hub unit is free
    /// in the chosen area or facility, the applicant can join a waiting list instead of applying. A Property Officer works
    /// the list in date order from Lease Administration &gt; Waiting List and tells the applicant when a unit is free.
    /// </summary>
    public partial class RealEstateController
    {
        // POST: RealEstate/JoinWaitingList (called from the Business Hub form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JoinWaitingList(int cccId, int? facilityId, string purposeOfLease)
        {
            Initialise();
            if (_customer == null || !User.Identity.IsAuthenticated)
                return Json(new { ok = false, message = "Please log in first." });

            var ccc = db.CCCs.FirstOrDefault(c => c.Id == cccId);
            if (ccc == null) return Json(new { ok = false, message = "Select a Customer Care Area first." });
            if (facilityId.HasValue && !db.RE_Facilities.Any(f => f.Id == facilityId.Value && f.CCCId == cccId && f.IsActive && !f.IsDeleted))
                facilityId = null;

            bool already = db.RE_WaitingList.Any(w => w.CustomerId == _customer.Id && w.CCCId == cccId && w.FacilityId == facilityId &&
                                                      w.WaitingStatus == RE_WaitingListEntry.StatusWaiting && w.IsActive && !w.IsDeleted);
            if (already)
                return Json(new { ok = true, message = "You are already on the waiting list for this area. We will let you know when a unit is free." });

            var now = DateTime.Now;
            db.RE_WaitingList.Add(new RE_WaitingListEntry
            {
                CustomerId = _customer.Id,
                CCCId = cccId,
                FacilityId = facilityId,
                LeaseType = "Business Hub",
                PurposeOfLease = string.IsNullOrWhiteSpace(purposeOfLease) ? null : purposeOfLease.Trim(),
                WaitingStatus = RE_WaitingListEntry.StatusWaiting,
                IsActive = true,
                IsDeleted = false,
                CreatedBySystemUserId = IsOnBehalf ? _official.Id : _systemUser.Id,
                CreatedDateTime = now
            });
            db.SaveChanges();

            int position = db.RE_WaitingList.Count(w => w.CCCId == cccId && w.FacilityId == facilityId &&
                                                        w.WaitingStatus == RE_WaitingListEntry.StatusWaiting && w.IsActive && !w.IsDeleted && w.CreatedDateTime <= now);
            return Json(new { ok = true, message = string.Format("You are on the waiting list for {0}, number {1} in line. We will let you know when a unit is free.", ccc.CCCName, position) });
        }

        // POST: RealEstate/LeaveWaitingList
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveWaitingList(int id)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");
            var entry = db.RE_WaitingList.FirstOrDefault(w => w.Id == id && w.CustomerId == _customer.Id && w.IsActive && !w.IsDeleted);
            if (entry != null && entry.WaitingStatus != RE_WaitingListEntry.StatusRemoved)
            {
                entry.WaitingStatus = RE_WaitingListEntry.StatusRemoved;
                entry.OutcomeReason = "Left the waiting list";
                entry.ModifiedBySystemUserId = IsOnBehalf ? _official.Id : _systemUser.Id;
                entry.ModifiedDateTime = DateTime.Now;
                db.SaveChanges();
                TempData["SuccessMessage"] = "You have left the waiting list.";
            }
            return RedirectToAction("MyApplications");
        }

        private void LoadMyWaitingList()
        {
            ViewBag.WaitingList = _customer == null ? new System.Collections.Generic.List<RE_WaitingListEntry>()
                : db.RE_WaitingList.Include(w => w.CCC).Include(w => w.Facility)
                    .Where(w => w.CustomerId == _customer.Id && w.WaitingStatus != RE_WaitingListEntry.StatusRemoved && w.IsActive && !w.IsDeleted)
                    .OrderBy(w => w.CreatedDateTime).ToList();
        }
    }
}
