using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;

namespace C8.eServices.Mvc.Controllers
{
    public class DocumentCheckListController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        // GET: /DocumentCheckList/
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var documentchecklists = db.DocumentCheckLists.Include(d => d.Application).Include(d => d.CreatedBySystemUser).Include(d => d.DocumentType).Include(d => d.ModifiedBySystemUser).Include(d => d.ReferenceType);
            return View(documentchecklists.ToList());
        }

        // GET: /DocumentCheckList/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentCheckList documentchecklist = db.DocumentCheckLists.Find(id);
            if (documentchecklist == null)
            {
                return HttpNotFound();
            }
            return View(documentchecklist);
        }

        // GET: /DocumentCheckList/Create
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            ViewBag.ApplicationId = new SelectList(db.Applications, "Id", "ApplicationAbbreviation");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.DocumentTypeId = new SelectList(db.DocumentTypes, "Id", "Name");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name");
            return View();
        }

        // POST: /DocumentCheckList/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DocumentCheckList documentchecklist)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                {
                    _context.DocumentCheckLists.Add(documentchecklist);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
               
            }

            ViewBag.ApplicationId = new SelectList(_context.Applications, "Id", "ApplicationAbbreviation", documentchecklist.ApplicationId);
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documentchecklist.CreatedBySystemUserId);
            ViewBag.DocumentTypeId = new SelectList(_context.DocumentTypes, "Id", "Name", documentchecklist.DocumentTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documentchecklist.ModifiedBySystemUserId);
            ViewBag.ReferenceTypeId = new SelectList(_context.ReferenceTypes, "Id", "Name", documentchecklist.ReferenceTypeId);
            return View(documentchecklist);
        }

        // GET: /DocumentCheckList/Edit/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentCheckList documentchecklist = db.DocumentCheckLists.Find(id);
            if (documentchecklist == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApplicationId = new SelectList(db.Applications, "Id", "ApplicationAbbreviation", documentchecklist.ApplicationId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentchecklist.CreatedBySystemUserId);
            ViewBag.DocumentTypeId = new SelectList(db.DocumentTypes, "Id", "Name", documentchecklist.DocumentTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentchecklist.ModifiedBySystemUserId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", documentchecklist.ReferenceTypeId);
            return View(documentchecklist);
        }

        // POST: /DocumentCheckList/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,DocumentTypeId,ApplicationId,ReferenceTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DocumentCheckList documentchecklist)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(documentchecklist).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ApplicationId = new SelectList(_context.Applications, "Id", "ApplicationAbbreviation", documentchecklist.ApplicationId);
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documentchecklist.CreatedBySystemUserId);
            ViewBag.DocumentTypeId = new SelectList(_context.DocumentTypes, "Id", "Name", documentchecklist.DocumentTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documentchecklist.ModifiedBySystemUserId);
            ViewBag.ReferenceTypeId = new SelectList(_context.ReferenceTypes, "Id", "Name", documentchecklist.ReferenceTypeId);
            return View(documentchecklist);
        }

        // GET: /DocumentCheckList/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentCheckList documentchecklist = db.DocumentCheckLists.Find(id);
            if (documentchecklist == null)
            {
                return HttpNotFound();
            }
            return View(documentchecklist);
        }

        // POST: /DocumentCheckList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            DocumentCheckList documentchecklist = _context.DocumentCheckLists.Find(id);

            //soft deletes record on the database
            documentchecklist.IsDeleted = true;
            documentchecklist.IsActive = false;

            _context.Entry(documentchecklist).State = EntityState.Modified;

            _context.DocumentCheckLists.Remove(documentchecklist);
            _context.SaveChanges();
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
