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
    public class EntityTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public EntityTypeController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }


        // GET: /EntityType/
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var entitytypes = db.EntityTypes.Include(e => e.CreatedBySystemUser).Include(e => e.ModifiedBySystemUser);
            return View(entitytypes.ToList());
        }

        // GET: /EntityType/Details/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EntityType entitytype = db.EntityTypes.Find(id);
            if (entitytype == null)
            {
                return HttpNotFound();
            }
            return View(entitytype);
        }

        // GET: /EntityType/Create
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

        // POST: /EntityType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] EntityType entitytype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //create key type
                entitytype.Key = "et_" + (entitytype.Name.ToLower().ToString()).Replace(" ", "_");
             
                //Add entity type record
                var EntityName = _context.EntityTypes.Where(e => e.Name == entitytype.Name).Select(e => e.Name).FirstOrDefault();

                if (EntityName == null)
                {
                    IdentityManager.CurrentSystemUser(User);
                    entitytype.IsActive = true;
                    entitytype.IsDeleted = false;
                    entitytype.IsLocked = false;
                    _context.EntityTypes.Add(entitytype);
                    _context.SaveChanges();
                    TempData["Success"] = "Entity Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Entity Type Exists.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", entitytype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", entitytype.ModifiedBySystemUserId);
            return View(entitytype);
        }

        // GET: /EntityType/Edit/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EntityType entitytype = db.EntityTypes.Find(id);
            if (entitytype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", entitytype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", entitytype.ModifiedBySystemUserId);
            return View(entitytype);
        }

        // POST: /EntityType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] EntityType entitytype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(entitytype).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", entitytype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", entitytype.ModifiedBySystemUserId);
            return View(entitytype);
        }

        // GET: /EntityType/Delete/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EntityType entitytype = db.EntityTypes.Find(id);
            if (entitytype == null)
            {
                return HttpNotFound();
            }
            return View(entitytype);
        }

        // POST: /EntityType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            EntityType entitytype = _context.EntityTypes.Find(id);

            //soft deletes record on the database
            entitytype.IsDeleted = true;
            entitytype.IsActive = false;

            //db.EntityTypes.Remove(entitytype);
            _context.Entry(entitytype).State = EntityState.Modified;
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
