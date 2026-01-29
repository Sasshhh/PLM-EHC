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
    public class BankAccountTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: BankAccountType
        public ActionResult Index()
        {
            var bankAccountTypes = db.BankAccountTypes.Include(b => b.CreatedBySystemUser).Include(b => b.ModifiedBySystemUser);
            return View(bankAccountTypes.ToList());
        }

        // GET: BankAccountType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAccountType bankAccountType = db.BankAccountTypes.Find(id);
            if (bankAccountType == null)
            {
                return HttpNotFound();
            }
            return View(bankAccountType);
        }

        // GET: BankAccountType/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: BankAccountType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] BankAccountType bankAccountType)
        {
            if (ModelState.IsValid)
            {
                db.BankAccountTypes.Add(bankAccountType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", bankAccountType.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", bankAccountType.ModifiedBySystemUserId);
            return View(bankAccountType);
        }

        // GET: BankAccountType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAccountType bankAccountType = db.BankAccountTypes.Find(id);
            if (bankAccountType == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", bankAccountType.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", bankAccountType.ModifiedBySystemUserId);
            return View(bankAccountType);
        }

        // POST: BankAccountType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] BankAccountType bankAccountType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bankAccountType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", bankAccountType.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", bankAccountType.ModifiedBySystemUserId);
            return View(bankAccountType);
        }

        // GET: BankAccountType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BankAccountType bankAccountType = db.BankAccountTypes.Find(id);
            if (bankAccountType == null)
            {
                return HttpNotFound();
            }
            return View(bankAccountType);
        }

        // POST: BankAccountType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BankAccountType bankAccountType = db.BankAccountTypes.Find(id);
            db.BankAccountTypes.Remove(bankAccountType);
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
