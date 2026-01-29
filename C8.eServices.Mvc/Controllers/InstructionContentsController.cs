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
    public class InstructionContentsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: InstructionContents
        public ActionResult Index()
        {
            var instructionContents = db.InstructionContents.Include(i => i.CreatedBySystemUser).Include(i => i.ModifiedBySystemUser);
            return View(instructionContents.ToList());
        }

        // GET: InstructionContents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InstructionContent instructionContent = db.InstructionContents.Find(id);
            if (instructionContent == null)
            {
                return HttpNotFound();
            }
            return View(instructionContent);
        }

        // GET: InstructionContents/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: InstructionContents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] InstructionContent instructionContent)
        {
            if (ModelState.IsValid)
            {
                db.InstructionContents.Add(instructionContent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instructionContent.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instructionContent.ModifiedBySystemUserId);
            return View(instructionContent);
        }

        // GET: InstructionContents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InstructionContent instructionContent = db.InstructionContents.Find(id);
            if (instructionContent == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instructionContent.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instructionContent.ModifiedBySystemUserId);
            return View(instructionContent);
        }

        // POST: InstructionContents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] InstructionContent instructionContent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(instructionContent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instructionContent.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", instructionContent.ModifiedBySystemUserId);
            return View(instructionContent);
        }

        // GET: InstructionContents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InstructionContent instructionContent = db.InstructionContents.Find(id);
            if (instructionContent == null)
            {
                return HttpNotFound();
            }
            return View(instructionContent);
        }

        // POST: InstructionContents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InstructionContent instructionContent = db.InstructionContents.Find(id);
            db.InstructionContents.Remove(instructionContent);
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
