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
    public class FlagsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: Flags
        public ActionResult Index()
        {
            var flags = db.Flags.Include(f => f.CreatedBySystemUser).Include(f => f.ModifiedBySystemUser).Include(f => f.ResponsibilityType);
            return View(flags.ToList());
        }

        // GET: Flags/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flag flag = db.Flags.Find(id);
            if (flag == null)
            {
                return HttpNotFound();
            }
            return View(flag);
        }

        // GET: Flags/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ResponsibilityTypeId = new SelectList(db.ResponsibilityTypes, "Id", "Name");
            return View();
        }

        // POST: Flags/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Key,TurnAroundDays,OnTargetColor,RunningLateColor,OverdueColor,ResponsibilityTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Flag flag)
        {
            if (ModelState.IsValid)
            {
                db.Flags.Add(flag);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", flag.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", flag.ModifiedBySystemUserId);
            ViewBag.ResponsibilityTypeId = new SelectList(db.ResponsibilityTypes, "Id", "Name", flag.ResponsibilityTypeId);
            return View(flag);
        }

        // GET: Flags/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flag flag = db.Flags.Find(id);
            if (flag == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", flag.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", flag.ModifiedBySystemUserId);
            ViewBag.ResponsibilityTypeId = new SelectList(db.ResponsibilityTypes, "Id", "Name", flag.ResponsibilityTypeId);
            return View(flag);
        }

        // POST: Flags/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Key,TurnAroundDays,OnTargetColor,RunningLateColor,OverdueColor,ResponsibilityTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Flag flag)
        {
            if (ModelState.IsValid)
            {
                db.Entry(flag).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", flag.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", flag.ModifiedBySystemUserId);
            ViewBag.ResponsibilityTypeId = new SelectList(db.ResponsibilityTypes, "Id", "Name", flag.ResponsibilityTypeId);
            return View(flag);
        }

        // GET: Flags/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flag flag = db.Flags.Find(id);
            if (flag == null)
            {
                return HttpNotFound();
            }
            return View(flag);
        }

        // POST: Flags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Flag flag = db.Flags.Find(id);
            db.Flags.Remove(flag);
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
