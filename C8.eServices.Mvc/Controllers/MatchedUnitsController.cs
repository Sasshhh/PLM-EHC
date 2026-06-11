using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;

namespace C8.eServices.Mvc.Controllers
{
    public class MatchedUnitsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();


        public ActionResult Index()
        {
            var matchedUnits = db.MatchedUnits.Include(m => m.CreatedBySystemUser).Include(m => m.ModifiedBySystemUser);
            return View(matchedUnits.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MatchedUnits matchedUnits = db.MatchedUnits.Find(id);
            if (matchedUnits == null)
            {
                return HttpNotFound();
            }
            return View(matchedUnits);
        }

        [DecryptParameter]
        public ActionResult UnitDetails(int? RcsApplicationId)
        {

            var matchedUnit = db.MatchedUnits.OrderByDescending(a => a.CreatedDateTime).Include(r => r.PropertyLeaseApplication).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplicationId && !x.IsDeleted && !x.RejectedProperty);
            var appstatus = db.PropertyLeaseApplications.Include(r => r.Status).FirstOrDefault(x => x.Id == RcsApplicationId);
            
            if (appstatus == null || appstatus.Status.Key == StatusKeys.AwaitingUnitOffers || appstatus.Status.Key == StatusKeys.ApplicationDiscardedNoUnitAvailable || appstatus.Status.Key == StatusKeys.Approved) 
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = RcsApplicationId, appId = "" });

            if (matchedUnit == null)
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = RcsApplicationId, appId = "" });

            var units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.UnitsId && !x.IsDeleted);
            var appallunit = db.ApplicationAllocatedProperty.Find(matchedUnit.ApplicationAllocatedPropertyId);
            
            if (appallunit == null)
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = RcsApplicationId, appId = "" });

            UnitViewModel model = new UnitViewModel
            {
                MatchedUnitId = (int)matchedUnit.Id,
                UnitDescription = appallunit.SpaceUnitNumber + " - " + db.humanEHCOptions.FirstOrDefault(x => x.Id == appallunit.HumanEHCOptionId)?.Name ?? "No Description Available",
                AllocatedUnit = appallunit
            };
            return View(model);
        }
        [HttpPost]
        [DecryptParameter]
        public ActionResult UnitDetails(int RcsApplicationId)
        {
            var matchedUnit = db.MatchedUnits.Include(r=>r.PropertyLeaseApplication).OrderByDescending(x => x.Id).FirstOrDefault(x => x.PropertyLeaseApplicationId == RcsApplicationId && !x.IsAccepted && !x.IsDeleted);
            var p = db.PropertyLeaseApplications.Include(r=>r.Status).FirstOrDefault(x => x.Id == RcsApplicationId);
            
            if (p == null || p.Status.Key == StatusKeys.AwaitingDepositPaid) 
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = RcsApplicationId, appId = "" });

            if (matchedUnit == null)
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = RcsApplicationId, appId = "" });

            var units = db.Units.FirstOrDefault(x => x.Id == matchedUnit.UnitsId && !x.IsDeleted);
            var appallunit = db.ApplicationAllocatedProperty.Find(matchedUnit.ApplicationAllocatedPropertyId);

            if (appallunit == null)
                return RedirectToAction("_Error", "PropertyLeaseApplication", new { Id = RcsApplicationId, appId = "" });

            UnitViewModel model = new UnitViewModel
            {
                MatchedUnitId = (int)matchedUnit.Id,
                UnitInformation = units,
                UnitDescription = appallunit.SpaceUnitNumber + " - " + db.humanEHCOptions.FirstOrDefault(x => x.Id == appallunit.HumanEHCOptionId)?.Name ?? "No Description Available",
                AllocatedUnit = appallunit
            };
            return View(model);
        }

        public JsonResult AcceptMatchedUnit(int? id)
        {
            var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.Id == id) ?? null;
            var unitInformation = db.ApplicationAllocatedProperty.Find(matchedUnit.ApplicationAllocatedPropertyId);
            var result = MatchingHelper.AcceptMatchedUnit(db, unitInformation, (int)matchedUnit.PropertyLeaseApplicationId, 1, (int)id);

            if (result)
            {
                var rcsApps = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == matchedUnit.PropertyLeaseApplicationId);
                var custmusers = db.Customers.FirstOrDefault(x => x.Id == rcsApps.CustomerId);

                // Activity Tracker
                var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.AcceptMatchedUnit).Description.ToString();
                MatchingHelper.ActivityTrackerAudit(db, rcsApps.Id, ActivityTrackerMessage, custmusers.Id);

                // Send unit accepted email
                int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.Accetproperty).Id;
                EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);

                // Send deposit payment email with banking details
                int depositEmailId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.UnitAcceptedDepositPaymentDetails)?.Id ?? 0;
                if (depositEmailId > 0)
                {
                    EmailHelper.CustomerEmailNotification(db, rcsApps.Id, depositEmailId);
                }
            }

            var output = result == true ? "Success" : "Failure";
            return Json(output, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult RejectMatchedUnit(int? id, string reason)
        {
            var matchedUnit = db.MatchedUnits.FirstOrDefault(x => x.Id == id) ?? null;
            var unitInformation = db.ApplicationAllocatedProperty.FirstOrDefault(x => x.Id == matchedUnit.ApplicationAllocatedPropertyId);
            MatchingHelper.MarkMatchedUnitAsRejected(db, matchedUnit.Id);
            var result = MatchingHelper.RejectMatchedUnit(db, unitInformation, (int)matchedUnit.PropertyLeaseApplicationId, (int)id);
            var output = result == true ? "Success" : "Failure";
            var rcsApps = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == matchedUnit.PropertyLeaseApplicationId);
            var custmusers = db.Customers.FirstOrDefault(x => x.Id == rcsApps.CustomerId);
            
            // Record Reject Action in Activity Tracker
            var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RejectMatchedUnit).Description.ToString();
            MatchingHelper.ActivityTrackerAudit(db,rcsApps.Id, ActivityTrackerMessage, custmusers.Id);
            
            // Send email
            int emailboodyId = db.EmailContentTypes.FirstOrDefault(x => x.Key == EmailContentKeys.Rejectproperty).Id;
            EmailHelper.CustomerEmailNotification(db, rcsApps.Id, emailboodyId);

            // Log rejection reason in application history
            string logMessage = string.IsNullOrWhiteSpace(reason) ? "Unit Offer Rejected." : $"Unit Offer Rejected. Reason: {reason}";
            MatchingHelper.AddHistoryLog(db, rcsApps.Id, custmusers.Id, logMessage);

            return Json(output, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LeaseForm()
        {
            string txt1 = "I here by ";
            string txt2 = " SIGN and AGREE with the lease conditions or regulations";

            string person = User.Identity.Name;
            return View();
        }

        public JsonResult ValidateWaitingListSorting()
        {
            var val = db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value;
            bool result = Convert.ToBoolean(Convert.ToInt16(db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.WaitingListSorting).Value));
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult LeaseForm([Bind(Include = "Id,SignedBy,DateSigned,Signiturre,ApplicationRef")] LeaseForm lease)
        //{
        //    string txt1 = "I here by ";
        //    string txt2 = " SIGN and AGREE with the lease conditions or regulations";

        //    string person = User.Identity.Name;
        //    if (ModelState.IsValid)
        //    {
        //        lease.DateSigned = DateTime.Now;
        //        lease.SignedBy = User.Identity.Name;
        //        lease.ApplicationRef = TempData["ReferenceNo"].ToString();
        //        db.LeaseForm.Add(lease);
        //        db.SaveChanges();
        //        TempData["ReferenceNo"] = "";
        //        return RedirectToAction("Index", " MatchedUnits");
        //    }
            
        //    return View(lease);
        //}

        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    MatchedUnits matchedUnits = db.MatchedUnits.Find(id);
        //    if (matchedUnits == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var record = from i in db.MatchedUnits
        //                 join b in db.waitingListQues on i.LeaseID equals b.LeaseID
        //                 join u in db.Units on i.unitID equals u.Id
        //                 join o in db.OccupationTypes on u.OccupationID equals o.Id
        //                 join a in db.RCSApplicationStatus on b.LeaseID equals a.Id
                         
        //                 select new
        //                 {
        //                     _ApplicationRef = a.ApplicationReferenceNumber,
        //                     _ApplicationDate = a.CreatedDateTime,
        //                     _ApplicationOwnerName = a.CreatedBySystemUser.FirstName,
        //                     _ApplicationOwnerSurname = a.CreatedBySystemUser.LastName,
        //                     _UnitType = o.Propertytype,
        //                     _UnitPrice = u.PropertyPrice,
        //                     _UnitDeposit = u.PropertyDeposit,
        //                     _UnitDateAdded = u.CreatedDateTime,
        //                     _UnitAddress = u.Address,
        //                     _UnitMatchDate = i.CreatedDateTime,

        //                 };
        //    foreach(var item in record)
        //    {
        //        ViewBag._ApplicationRef = item._ApplicationRef;
        //        ViewBag._ApplicationDate = item._ApplicationDate;
        //        ViewBag._ApplicationOwnerName = item._ApplicationOwnerName;
        //        ViewBag._ApplicationOwnerSurname = item._ApplicationOwnerSurname;
        //        ViewBag._UnitType = item._UnitType;
        //        ViewBag._UnitPrice = item._UnitPrice;
        //        ViewBag._UnitDeposit = item._UnitDeposit;
        //        ViewBag._UnitAddress = item._UnitAddress;
        //        ViewBag._UnitDateAdded = item._UnitDateAdded;
        //        ViewBag._UnitMatchDate = item._UnitMatchDate;
        //        TempData["ReferenceNo"] = item._ApplicationRef;
        //    }
            
            
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", matchedUnits.CreatedBySystemUserId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", matchedUnits.ModifiedBySystemUserId);
        //    return View(matchedUnits);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,LeaseID,LeaseRefrence,unitID,IsAcceptde,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] MatchedUnits matchedUnits)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(matchedUnits).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", matchedUnits.CreatedBySystemUserId);
        //    ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", matchedUnits.ModifiedBySystemUserId);
        //    return View(matchedUnits);
        //}

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MatchedUnits matchedUnits = db.MatchedUnits.Find(id);
            if (matchedUnits == null)
            {
                return HttpNotFound();
            }
            return View(matchedUnits);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MatchedUnits matchedUnits = db.MatchedUnits.Find(id);
            db.MatchedUnits.Remove(matchedUnits);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
