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
    public class DepartmentsCoEsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: DepartmentsCoEs
        public ActionResult Index()
        {
            var departmentsCoEs = db.DepartmentsCoEs.Include(d => d.CreatedBySystemUser).Include(d => d.ModifiedBySystemUser);
            return View(departmentsCoEs.ToList());
        }

        // GET: DepartmentsCoEs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentsCoE departmentsCoE = db.DepartmentsCoEs.Find(id);
            if (departmentsCoE == null)
            {
                return HttpNotFound();
            }
            return View(departmentsCoE);
        }

        // GET: DepartmentsCoEs/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: DepartmentsCoEs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DepartmentName,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DepartmentsCoE departmentsCoE)
        {
            if (ModelState.IsValid)
            {
                db.DepartmentsCoEs.Add(departmentsCoE);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsCoE.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsCoE.ModifiedBySystemUserId);
            return View(departmentsCoE);
        }

        // GET: DepartmentsCoEs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentsCoE departmentsCoE = db.DepartmentsCoEs.Find(id);
            if (departmentsCoE == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsCoE.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsCoE.ModifiedBySystemUserId);
            return View(departmentsCoE);
        }

        // POST: DepartmentsCoEs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DepartmentName,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DepartmentsCoE departmentsCoE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(departmentsCoE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsCoE.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", departmentsCoE.ModifiedBySystemUserId);
            return View(departmentsCoE);
        }

        // GET: DepartmentsCoEs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentsCoE departmentsCoE = db.DepartmentsCoEs.Find(id);
            if (departmentsCoE == null)
            {
                return HttpNotFound();
            }
            return View(departmentsCoE);
        }

        // POST: DepartmentsCoEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DepartmentsCoE departmentsCoE = db.DepartmentsCoEs.Find(id);
            db.DepartmentsCoEs.Remove(departmentsCoE);
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
