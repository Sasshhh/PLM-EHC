using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    /// <summary>
    /// DPRE-Q39 (confirmed 26 Nov 2026): the application fee is paid AFTER the application is submitted.
    /// Submission no longer needs proof of payment. Without it the application waits in
    /// "Awaiting Application Fee Upload"; the applicant uploads proof from My Applications (UC06),
    /// which moves it to "Awaiting Application Fee Validation" and creates the Verify Payment task.
    /// A rejected payment returns the application to "Awaiting Application Fee Upload" so the applicant can upload again.
    /// </summary>
    public partial class RealEstateController
    {
        public const string ProofOfPaymentDocumentName = "Proof of Application Fee Payment";

        private int FeeStageStatusId(bool proofUploaded)
        {
            var key = proofUploaded ? StatusKeys.AwaitingApplicationFeeValidation : StatusKeys.AwaitingApplicationFeeUpload;
            var status = db.Status.FirstOrDefault(s => s.Key == key);
            if (status == null)
                throw new Exception("Status '" + key + "' is missing. Run DatabaseScripts/RE_ApplicationFee_AfterSubmission.sql.");
            return status.Id;
        }

        private bool HasProofOfPayment(int appId)
        {
            if (appId <= 0) return false;
            return db.Documents.Any(d => d.RealEstateApplicationId == appId && d.IsActive && !d.IsDeleted
                                         && d.DocumentName.StartsWith(ProofOfPaymentDocumentName));
        }

        private static string FeeNextStepText(bool proofUploaded)
        {
            return proofUploaded
                ? "Your proof of payment has been received and will now be verified."
                : "Next step: please pay the application fee and upload your proof of payment under My Applications. Your application will be processed once the payment is verified.";
        }

        // GET: RealEstate/UploadProofOfPayment/{id}
        public ActionResult UploadProofOfPayment(int id)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var app = LoadOwnAwaitingFeeApplication(id);
            if (app == null)
            {
                TempData["SuccessMessage"] = "That application is not waiting for a proof of payment.";
                return RedirectToAction("MyApplications");
            }
            return View(app);
        }

        // POST: RealEstate/UploadProofOfPayment/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadProofOfPayment(int id, HttpPostedFileBase file_Fee)
        {
            Initialise();
            if (_customer == null) return RedirectToAction("Login", "Account");

            var app = LoadOwnAwaitingFeeApplication(id);
            if (app == null)
            {
                TempData["SuccessMessage"] = "That application is not waiting for a proof of payment.";
                return RedirectToAction("MyApplications");
            }

            if (file_Fee == null || file_Fee.ContentLength == 0)
                ModelState.AddModelError("file_Fee", "Select your proof of payment.");
            else if (file_Fee.ContentLength > 10 * 1024 * 1024)
                ModelState.AddModelError("file_Fee", "The file is larger than 10 MB.");
            else
            {
                var ext = (Path.GetExtension(file_Fee.FileName) ?? "").ToLowerInvariant();
                if (!new[] { ".pdf", ".png", ".jpg", ".jpeg" }.Contains(ext))
                    ModelState.AddModelError("file_Fee", "Upload a PDF, PNG or JPG file.");
            }

            if (!ModelState.IsValid) return View(app);

            int refTypeId = db.ReferenceTypes.FirstOrDefault(r => r.Key == ReferenceTypeKeys.RealEstateApplication)?.Id ?? 16;
            SaveAndLinkFile(file_Fee, ProofOfPaymentDocumentName, app.Id, 18, refTypeId);

            app.StatusId = FeeStageStatusId(true);
            app.PaymentValidationComment = null;
            app.ModifiedBySystemUserId = _systemUser.Id;
            app.ModifiedDateTime = DateTime.Now;
            db.Entry(app).State = EntityState.Modified;
            db.SaveChanges();

            RealEstateWorkAllocationHelper.AssignRealEstateTask(db, app.Id, ResponsibilityTypeKeys.RealEstateVerifyPayment);
            MatchingHelper.AddHistoryLog(db, app.Id, _systemUser.Id, "Applicant uploaded proof of application fee payment.");

            TempData["SuccessMessage"] = "Proof of payment uploaded for " + app.ApplicationReferenceNumber + ". It will now be verified.";
            return RedirectToAction("MyApplications");
        }

        private RE_Application LoadOwnAwaitingFeeApplication(int id)
        {
            return db.RE_Applications
                .Include(a => a.Status)
                .FirstOrDefault(a => a.Id == id && a.CustomerId == _customer.Id && a.IsActive && !a.IsDeleted
                                     && a.Status.Key == StatusKeys.AwaitingApplicationFeeUpload);
        }
    }
}
