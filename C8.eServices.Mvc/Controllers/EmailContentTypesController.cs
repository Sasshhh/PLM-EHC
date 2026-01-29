using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime.Misc;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Controllers
{
    public class EmailContentTypesController : Controller
    {
        

        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public EmailContentTypesController()

        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        // GET: EmailContentTypes
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Index()
        {
            var emailContentTypes = db.EmailContentTypes.Include(e => e.CreatedBySystemUser).Include(e => e.ModifiedBySystemUser);
            return View(emailContentTypes.ToList());
        }

        // GET: EmailContentTypes/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailContentType emailContentType = db.EmailContentTypes.Find(id);
            if (emailContentType == null)
            {
                return HttpNotFound();
            }
            return View(emailContentType);
        }

        // GET: EmailContentTypes/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: EmailContentTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] EmailContentType emailContentType)
        {
            if (ModelState.IsValid)
            {
                db.EmailContentTypes.Add(emailContentType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", emailContentType.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", emailContentType.ModifiedBySystemUserId);
            return View(emailContentType);
        }

        // GET: EmailContentTypes/Edit/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailContentType emailContentType = db.EmailContentTypes.Find(id);
            if (emailContentType == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", emailContentType.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", emailContentType.ModifiedBySystemUserId);
            return View(emailContentType);
        }

        // POST: EmailContentTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] EmailContentType emailContentType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(emailContentType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", emailContentType.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", emailContentType.ModifiedBySystemUserId);
            return View(emailContentType);
        }

        // GET: EmailContentTypes/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmailContentType emailContentType = db.EmailContentTypes.Find(id);
            if (emailContentType == null)
            {
                return HttpNotFound();
            }
            return View(emailContentType);
        }

        // POST: EmailContentTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            EmailContentType emailContentType = db.EmailContentTypes.Find(id);
            db.EmailContentTypes.Remove(emailContentType);
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
