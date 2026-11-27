using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    /// <summary>
    /// DPRE UC05 FR4.2 / DPRE-Q46: an official captures an application on behalf of an applicant who applied on paper.
    /// ASSUMPTIONS (recommended answers, to confirm with the BA):
    ///  - the official is a Property Officer (or anyone doing Real Estate Property Manager work, or an administrator);
    ///  - the applicant must already have an active Real Estate profile; the official finds it by ID number, e-mail or surname
    ///    and does not create profiles;
    ///  - the signed paper application form must be uploaded on submission.
    /// While capturing, the official uses the normal applicant screens; the application, documents and e-mails belong to
    /// the applicant, and the history log records which official captured it.
    /// </summary>
    public partial class RealEstateController
    {
        private const string OnBehalfSessionKey = "RE_OnBehalfCustomerId";
        public const string SignedPaperFormKey = "file_SignedForm";
        public const string SignedPaperFormName = "Signed paper application form";

        /// <summary>The official capturing on behalf of an applicant, or null when the applicant is capturing.</summary>
        private SystemUser _official;

        private bool IsOnBehalf { get { return _official != null; } }

        private bool CanCaptureOnBehalf()
        {
            return User != null && User.Identity.IsAuthenticated &&
                   (RealEstateRoles.CanDoPropertyManagerWork(User) || User.IsInRole("Back Office System Administrator") ||
                    User.IsInRole("Super Administrators") || User.IsInRole("Administrators") || User.IsInRole("Support Admin"));
        }

        /// <summary>Called at the end of Initialise(): switches the current applicant to the one the official is capturing for.</summary>
        private void ApplyOnBehalf()
        {
            _official = null;
            var customerId = Session == null ? null : Session[OnBehalfSessionKey] as int?;
            if (customerId == null) return;
            if (!CanCaptureOnBehalf()) { Session.Remove(OnBehalfSessionKey); return; }

            var applicant = db.Customers.Include(c => c.Status).FirstOrDefault(c => c.Id == customerId.Value && c.IsActive && !c.IsDeleted);
            var applicantUser = applicant == null || applicant.SystemUserId == null ? null
                : db.SystemUsers.FirstOrDefault(u => u.Id == applicant.SystemUserId.Value && u.IsActive && !u.IsDeleted);
            if (applicant == null || applicantUser == null) { Session.Remove(OnBehalfSessionKey); return; }

            _official = _systemUser;
            _customer = applicant;
            _systemUser = applicantUser;
            ViewBag.OnBehalfApplicant = applicant.FullName;
            ViewBag.OnBehalfOfficial = _official == null ? "" : _official.FullName;
        }

        /// <summary>On submission by an official, the signed paper form is required (DPRE-Q46). A copy saved with an earlier draft counts.</summary>
        private void ValidateSignedPaperForm(bool isDraft, bool alreadyUploaded = false)
        {
            if (!IsOnBehalf || isDraft || alreadyUploaded) return;
            var f = Request.Files[SignedPaperFormKey];
            if (f == null || f.ContentLength == 0)
                ModelState.AddModelError(SignedPaperFormKey, "Upload the applicant's signed paper application form.");
        }

        /// <summary>Saves the signed paper form if one was posted; on submission also records which official captured it.</summary>
        private void SaveSignedPaperFormAndLog(int applicationId, int refTypeId, bool submitted)
        {
            if (!IsOnBehalf) return;
            var f = Request.Files[SignedPaperFormKey];
            if (f != null && f.ContentLength > 0)
                SaveAndLinkFile(f, SignedPaperFormName, applicationId, 18, refTypeId);
            if (submitted)
                MatchingHelper.AddHistoryLog(db, applicationId, _official.Id,
                    string.Format("Captured by {0} on behalf of the applicant from a signed paper form.", _official.FullName));
        }

        // GET: RealEstate/CaptureOnBehalf
        public ActionResult CaptureOnBehalf(string search)
        {
            Initialise();
            if (!CanCaptureOnBehalf()) return new HttpStatusCodeResult(403, "Only Property Officers can capture on behalf of an applicant.");

            ViewBag.Search = search;
            var results = new System.Collections.Generic.List<Customer>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var deptId = db.ApplicationEntities.Where(ae => ae.Key == ApplicationEntityKeys.RealEstateDevelopment).Select(ae => (int?)ae.Id).FirstOrDefault();
                var reUserIds = db.SystemUsers.Where(u => u.DepartmentId == deptId && u.IsActive && !u.IsDeleted && !u.isInternalUser).Select(u => u.Id);
                results = db.Customers.Include(c => c.Status)
                    .Where(c => c.IsActive && !c.IsDeleted && c.SystemUserId != null && reUserIds.Contains(c.SystemUserId.Value) &&
                                (c.IdentificationNumber == term || c.EmailAddress == term || c.LastName.StartsWith(term)))
                    .OrderBy(c => c.LastName).Take(25).ToList();
            }
            ViewBag.Results = results;
            ViewBag.ActiveStatusKey = StatusKeys.CustomerActive;
            return View();
        }

        // POST: RealEstate/StartOnBehalf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartOnBehalf(int customerId)
        {
            Initialise();
            if (!CanCaptureOnBehalf()) return new HttpStatusCodeResult(403, "Only Property Officers can capture on behalf of an applicant.");

            var applicant = db.Customers.Include(c => c.Status).FirstOrDefault(c => c.Id == customerId && c.IsActive && !c.IsDeleted);
            if (applicant == null || applicant.SystemUserId == null)
            {
                TempData["ErrorMessage"] = "That applicant profile was not found.";
                return RedirectToAction("CaptureOnBehalf");
            }
            if (applicant.Status == null || applicant.Status.Key != StatusKeys.CustomerActive)
            {
                TempData["ErrorMessage"] = applicant.FullName + "'s profile is not approved yet. Approve the profile first, then capture the application.";
                return RedirectToAction("CaptureOnBehalf");
            }

            Session[OnBehalfSessionKey] = applicant.Id;
            TempData["SuccessMessage"] = "You are now capturing for " + applicant.FullName + ". Choose the lease type from their paper form.";
            return RedirectToAction("LeaseApplications");
        }

        // POST: RealEstate/StopOnBehalf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StopOnBehalf()
        {
            if (Session != null) Session.Remove(OnBehalfSessionKey);
            return RedirectToAction("CaptureOnBehalf");
        }
    }
}
