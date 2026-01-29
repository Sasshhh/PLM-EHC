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
    public class SystemUserTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        // GET: /SystemUserType/
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var systemusertypes = db.SystemUserTypes.Include(s => s.CreatedBySystemUser).Include(s => s.ModifiedBySystemUser);
            return View(systemusertypes.ToList());
        }

        // GET: /SystemUserType/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SystemUserType systemusertype = db.SystemUserTypes.Find(id);
            if (systemusertype == null)
            {
                return HttpNotFound();
            }
            return View(systemusertype);
        }

        // GET: /SystemUserType/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: /SystemUserType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] SystemUserType systemusertype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //Creates type key
                systemusertype.Key = "typ_" + (systemusertype.Name.ToLower()).Replace(" ", "_");

                systemusertype.IsActive = true;
                systemusertype.IsDeleted = false;
                systemusertype.IsLocked = false;

                _context.SystemUserTypes.Add(systemusertype);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", systemusertype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", systemusertype.ModifiedBySystemUserId);

            return View(systemusertype);
        }

        // GET: /SystemUserType/Edit/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SystemUserType systemusertype = db.SystemUserTypes.Find(id);
            if (systemusertype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", systemusertype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", systemusertype.ModifiedBySystemUserId);
            return View(systemusertype);
        }

        // POST: /SystemUserType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] SystemUserType systemusertype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(systemusertype).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", systemusertype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", systemusertype.ModifiedBySystemUserId);
            return View(systemusertype);
        }

        // GET: /SystemUserType/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SystemUserType systemusertype = db.SystemUserTypes.Find(id);
            if (systemusertype == null)
            {
                return HttpNotFound();
            }
            return View(systemusertype);
        }

        // POST: /SystemUserType/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            SystemUserType systemusertype = _context.SystemUserTypes.Find(id);

            //Soft deletes the record
            systemusertype.IsDeleted = true;
            systemusertype.IsActive = false;
            _context.Entry(systemusertype).State = EntityState.Modified;
            //db.SystemUserTypes.Remove(systemusertype);

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
