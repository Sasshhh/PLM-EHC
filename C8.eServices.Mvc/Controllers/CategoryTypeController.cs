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
    public class CategoryTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }

        public void Initialise()
        {
            _base.Initialise(db);
            SystemUser = _base.SystemUser;

        }
        // GET: /CategoryType/
        public ActionResult Index()
        {
           var categorytypes = db.CategoryTypes
               .Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser)
               .Where(c => c.IsActive);
            return View(categorytypes.ToList());
        }

        // GET: /CategoryType/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoryType categorytype = db.CategoryTypes.Find(id);
            if (categorytype == null)
            {
                return HttpNotFound();
            }
            return View(categorytype);
        }

        // GET: /CategoryType/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: /CategoryType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] CategoryType categorytype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //Create type key
                categorytype.Key = "Cat_" + (categorytype.Name.ToLower().ToString()).Replace(" ", "_");

                //Add category type record
                var CatName = _context.CategoryTypes.Where(l => l.Name == categorytype.Name).Select(l => l.Name).FirstOrDefault();

                if (CatName == null)
                {
                    categorytype.IsActive = true;
                    categorytype.IsDeleted = false;
                    categorytype.IsLocked = false;
                    _context.CategoryTypes.Add(categorytype);
                    _context.SaveChanges();
                    TempData["Success"] = "Log Type Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Category type Exists.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", categorytype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", categorytype.ModifiedBySystemUserId);
            return View(categorytype);
        }

        // GET: /CategoryType/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoryType categorytype = db.CategoryTypes.Find(id);
            if (categorytype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", categorytype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", categorytype.ModifiedBySystemUserId);
            return View(categorytype);
        }

        // POST: /CategoryType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] CategoryType categorytype)
        {
            if (ModelState.IsValid)
            {
                _base.Initialise(db);
                db.Entry(categorytype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", categorytype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", categorytype.ModifiedBySystemUserId);
            return View(categorytype);
        }

        // GET: /CategoryType/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CategoryType categorytype = db.CategoryTypes.Find(id);
            if (categorytype == null)
            {
                return HttpNotFound();
            }
            return View(categorytype);
        }

        // POST: /CategoryType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            CategoryType categorytype = _context.CategoryTypes.Find(id);
            _base.Initialise(_context);
            //soft delete record on the database
            categorytype.IsDeleted = true;
            categorytype.IsActive = false;

            _context.CategoryTypes.Remove(categorytype);
            _context.Entry(categorytype).State = EntityState.Modified;

            //// save changes
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
