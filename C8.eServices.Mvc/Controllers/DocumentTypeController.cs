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
    public class DocumentTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public DocumentTypeController()
        {
            IdentityManager = new IdentityManager(db);
        }

        IdentityManager IdentityManager { get; set; }

        // GET: /DocumentType/
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var documenttypes = db.DocumentTypes.Include(d => d.CreatedBySystemUser).Include(d => d.ModifiedBySystemUser);
            return View(documenttypes.ToList());
        }

        // GET: /DocumentType/Details/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentType documenttype = db.DocumentTypes.Find(id);
            if (documenttype == null)
            {
                return HttpNotFound();
            }
            return View(documenttype);
        }

        // GET: /DocumentType/Create
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            TempData["Success"] = null;
            TempData["Info"] = null;
            TempData["Error"] = null;
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: /DocumentType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DocumentType documenttype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //create key type
                documenttype.Key = "dt_" + (documenttype.Name.ToLower().ToString()).Replace(" ", "_");
                
                //Add document type record
                var documentName = _context.DocumentTypes.Where(d => d.Name == documenttype.Name).Select(d => d.Name).FirstOrDefault();

                if (documentName == null)
                {
                    IdentityManager.CurrentUser(User);
                    documenttype.IsActive = true;
                    documenttype.IsDeleted = false;
                    documenttype.IsLocked = false;
                    _context.DocumentTypes.Add(documenttype);
                    _context.SaveChanges();
                    TempData["Succcess"] = "Document Type Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Document Type Error.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documenttype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documenttype.ModifiedBySystemUserId);
            return View(documenttype);
        }

        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentType documenttype = db.DocumentTypes.Find(id);
            if (documenttype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documenttype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documenttype.ModifiedBySystemUserId);
            return View(documenttype);
        }

        // POST: /DocumentType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DocumentType documenttype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(documenttype).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documenttype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", documenttype.ModifiedBySystemUserId);
            return View(documenttype);
        }

        // GET: /DocumentType/Delete/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentType documenttype = db.DocumentTypes.Find(id);
            if (documenttype == null)
            {
                return HttpNotFound();
            }
            return View(documenttype);
        }

        // POST: /DocumentType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            DocumentType documenttype = _context.DocumentTypes.Find(id);
            
            //soft deletes record on the database
            documenttype.IsDeleted = true;
            documenttype.IsActive = false;

            //db.DocumentTypes.Remove(documenttype);
            _context.Entry(documenttype).State = EntityState.Modified;
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
