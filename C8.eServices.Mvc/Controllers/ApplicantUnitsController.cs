using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;

namespace C8.eServices.Mvc.Controllers
{
    public class ApplicantUnitsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: ApplicantUnits
        public ActionResult Index()
        {
            var applicantUnits = db.ApplicantUnits.Include(a => a.CreatedBySystemUser).Include(a => a.ModifiedBySystemUser);
            return View(applicantUnits.ToList());
        }

        // GET: ApplicantUnits/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantUnit applicantUnit = db.ApplicantUnits.Find(id);
            if (applicantUnit == null)
            {
                return HttpNotFound();
            }
            return View(applicantUnit);
        }

        // GET: ApplicantUnits/Create
        public ActionResult ApplicantUnitScreen()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: ApplicantUnits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplicantUnitScreen([Bind(Include = "Id,LeaseID,MatchedID,DepositPaid,OutstandingDepopsitAmount,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ApplicantUnit applicantUnit)
        {
            if (ModelState.IsValid)
            {
                db.ApplicantUnits.Add(applicantUnit);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", applicantUnit.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", applicantUnit.ModifiedBySystemUserId);
            return View(applicantUnit);
        }

        // GET: ApplicantUnits/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantUnit applicantUnit = db.ApplicantUnits.Find(id);
            if (applicantUnit == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", applicantUnit.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", applicantUnit.ModifiedBySystemUserId);
            return View(applicantUnit);
        }

        // POST: ApplicantUnits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,LeaseID,MatchedID,DepositPaid,OutstandingDepopsitAmount,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ApplicantUnit applicantUnit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applicantUnit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", applicantUnit.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", applicantUnit.ModifiedBySystemUserId);
            return View(applicantUnit);
        }

        // GET: ApplicantUnits/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicantUnit applicantUnit = db.ApplicantUnits.Find(id);
            if (applicantUnit == null)
            {
                return HttpNotFound();
            }
            return View(applicantUnit);
        }

        // POST: ApplicantUnits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicantUnit applicantUnit = db.ApplicantUnits.Find(id);
            db.ApplicantUnits.Remove(applicantUnit);
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
