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
    public class RCSUserMessagesController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        public RCSUserMessagesController()

        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }
        // GET: RCSUserMessages
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Index()
        {
            var rCSUserMessages = db.RCSUserMessages.Include(r => r.CreatedBySystemUser).Include(r => r.ModifiedBySystemUser);
            return View(rCSUserMessages.ToList());
        }

        // GET: RCSUserMessages/Details/5

        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCSUserMessage rCSUserMessage = db.RCSUserMessages.Find(id);
            if (rCSUserMessage == null)
            {
                return HttpNotFound();
            }
            return View(rCSUserMessage);
        }

        // GET: RCSUserMessages/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: RCSUserMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Body,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RCSUserMessage rCSUserMessage)
        {
            if (ModelState.IsValid)
            {
                db.RCSUserMessages.Add(rCSUserMessage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSUserMessage.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSUserMessage.ModifiedBySystemUserId);
            return View(rCSUserMessage);
        }

        // GET: RCSUserMessages/Edit/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCSUserMessage rCSUserMessage = db.RCSUserMessages.Find(id);
            if (rCSUserMessage == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSUserMessage.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSUserMessage.ModifiedBySystemUserId);
            return View(rCSUserMessage);
        }

        // POST: RCSUserMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Body,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] RCSUserMessage rCSUserMessage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rCSUserMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSUserMessage.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rCSUserMessage.ModifiedBySystemUserId);
            return View(rCSUserMessage);
        }

        // GET: RCSUserMessages/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RCSUserMessage rCSUserMessage = db.RCSUserMessages.Find(id);
            if (rCSUserMessage == null)
            {
                return HttpNotFound();
            }
            return View(rCSUserMessage);
        }

        // POST: RCSUserMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RCSUserMessage rCSUserMessage = db.RCSUserMessages.Find(id);
            db.RCSUserMessages.Remove(rCSUserMessage);
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
