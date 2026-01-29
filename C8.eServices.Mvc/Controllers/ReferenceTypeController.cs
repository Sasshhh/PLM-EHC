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
    public class ReferenceTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public ReferenceTypeController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        // GET: /ReferenceType/
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var referencetypes = db.ReferenceTypes.Include(r => r.CreatedBySystemUser).Include(r => r.ModifiedBySystemUser);
            return View(referencetypes.ToList());
        }

        // GET: /ReferenceType/Details/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReferenceType referencetype = db.ReferenceTypes.Find(id);
            if (referencetype == null)
            {
                return HttpNotFound();
            }
            return View(referencetype);
        }

        // GET: /ReferenceType/Create
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

        // POST: /ReferenceType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ReferenceType referencetype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                // create key type
                referencetype.Key = "rt_" + (referencetype.Name.ToLower().ToString()).Replace(" ", "_");
                
                //Add reference type record
                var referenceName = _context.ReferenceTypes.Where(r => r.Name == referencetype.Name).Select(r => r.Name).FirstOrDefault();

                if (referenceName == null)
                {
                    IdentityManager.CurrentSystemUser(User);
                    referencetype.IsActive = true;
                    referencetype.IsDeleted = false;
                    referencetype.IsLocked = false;
                    _context.ReferenceTypes.Add(referencetype);
                    _context.SaveChanges();
                    TempData["Success"] = "Reference Type Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Reference Type Error.";
                }
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", referencetype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", referencetype.ModifiedBySystemUserId);
            return View(referencetype);
        }

        // GET: /ReferenceType/Edit/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReferenceType referencetype = db.ReferenceTypes.Find(id);
            if (referencetype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", referencetype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", referencetype.ModifiedBySystemUserId);
            return View(referencetype);
        }

        // POST: /ReferenceType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ReferenceType referencetype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(referencetype).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", referencetype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", referencetype.ModifiedBySystemUserId);
            return View(referencetype);
        }

        // GET: /ReferenceType/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReferenceType referencetype = db.ReferenceTypes.Find(id);
            if (referencetype == null)
            {
                return HttpNotFound();
            }
            return View(referencetype);
        }

        // POST: /ReferenceType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            ReferenceType referencetype = _context.ReferenceTypes.Find(id);

            //soft deletes the record on the database
            referencetype.IsDeleted = true;
            referencetype.IsActive = false;
            _context.Entry(referencetype).State = EntityState.Modified;
           // db.ReferenceTypes.Remove(referencetype);
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
