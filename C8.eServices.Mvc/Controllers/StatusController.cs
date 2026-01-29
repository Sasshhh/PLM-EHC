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
    public class StatusController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public StatusController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        // GET: /Status/ //
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var status =db.Status.Include(s => s.CreatedBySystemUser)
                                .Include(s => s.ModifiedBySystemUser)
                                .Include(s => s.StatusType);

            return View(status.ToList());
        }

        // GET: /Status/Details/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Status status = db.Status.Include(s =>s.StatusType).FirstOrDefault(s => s.Id == id);
            if (status == null)
            {
                return HttpNotFound();
            }
            return View(status);
        }

        // GET: /Status/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            TempData["Success"] = null;
            TempData["Info"] = null;
            TempData["Error"] = null;
            #region ViewBags

            ViewBag.StatusTypeId = new SelectList(db.StatusTypes.Where(s => s.IsActive && !s.IsDeleted), "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");

            #endregion

           
            return View();
        }

        // POST: /Status/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,StatusTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Status status)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //L.M - Creates type key based on the Status Name
                status.Key = "s_" + (status.Name.ToLower()).Replace(" ", "_");
               
                //Add status record
                var statusName = _context.Status.Where(s => s.Name == status.Name).Select(s => s.Name).FirstOrDefault();

                if (statusName == null)
                {
                    IdentityManager.CurrentUser(User);
                    status.IsActive = true;
                    status.IsDeleted = false;
                    status.IsLocked = false;
                    _context.Status.Add(status);
                    _context.SaveChanges();
                    TempData["Success"] = "Status Sucessfully Saved!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Status Exists.";
                }
            }

            ViewBag.StatusTypeId = new SelectList(_context.StatusTypes.Where(s => s.IsActive && !s.IsDeleted), "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", status.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", status.ModifiedBySystemUserId);
            return View(status);
        }

        // GET: /Status/Edit/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Status status = db.Status.Find(id);
            if (status == null)
            {
                return HttpNotFound();
            }

            #region Viewbags
            ViewBag.StatusTypeId = new SelectList(db.StatusTypes.Where(s => s.IsActive && !s.IsDeleted), "Id", "Name", status.StatusTypeId);
            #endregion

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", status.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", status.ModifiedBySystemUserId);
            return View(status);
        }

        // POST: /Status/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Key,Description,StatusTypeId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Status status)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(status).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StatusTypeId = new SelectList(_context.StatusTypes.Where(s => s.IsActive && !s.IsDeleted), "Id", "Name", status.StatusTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", status.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", status.ModifiedBySystemUserId);
            return View(status);
        }

        // GET: /Status/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Status status = db.Status.Include(s => s.StatusType).FirstOrDefault(s => s.Id == id);
            if (status == null)
            {
                return HttpNotFound();
            }
            return View(status);
        }

        // POST: /Status/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            Status status = _context.Status.Find(id);

            //Soft deletes the record on the database
            status.IsDeleted = true;
            status.IsActive = false;

            _context.Entry(status).State = EntityState.Modified;
           // db.Status.Remove(status);

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
