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
    public class TitleTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public TitleTypeController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }


        // GET: /TitleType/
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Index()
        {
            var titletypes = db.TitleTypes.Include(t => t.CreatedBySystemUser).Include(t => t.ModifiedBySystemUser);
            return View(titletypes.ToList());
        }

        // GET: /TitleType/Details/5
        [Authorize(Roles = "Administrators"+ "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TitleType titletype = db.TitleTypes.Find(id);
            if (titletype == null)
            {
                return HttpNotFound();
            }
            return View(titletype);
        }

        // GET: /TitleType/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Create()
        {
            TempData["Success"] = null;
            TempData["Info"] = null;
            TempData["Error"] = null;
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: /TitleType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] TitleType titletype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //Creates the type key based on the title type name
                titletype.Key = "tt_" + (titletype.Name.ToLower()).Replace(" ", "_");

                //Add title type record
                var titleName = _context.TitleTypes.Where(t => t.Name == titletype.Name).Select(t => t.Name).FirstOrDefault();

                if (titleName == null)
                {
                    IdentityManager.CurrentUser(User);
                    titletype.IsActive = true;
                    titletype.IsDeleted = false;
                    titletype.IsLocked = false;
                    _context.TitleTypes.Add(titletype);
                    _context.SaveChanges();
                    TempData["Success"] = "Title Type Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Title Type Exists.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", titletype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", titletype.ModifiedBySystemUserId);
            return View(titletype);
        }

        // GET: /TitleType/Edit/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TitleType titletype = db.TitleTypes.Find(id);
            if (titletype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", titletype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", titletype.ModifiedBySystemUserId);
            return View(titletype);
        }

        // POST: /TitleType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] TitleType titletype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(titletype).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", titletype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", titletype.ModifiedBySystemUserId);
            return View(titletype);
        }

        // GET: /TitleType/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TitleType titletype = db.TitleTypes.Find(id);
            if (titletype == null)
            {
                return HttpNotFound();
            }
            return View(titletype);
        }

        // POST: /TitleType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Back Office System Administrator")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            TitleType titletype = _context.TitleTypes.Find(id);
            
            //Soft deletes the record on the database
            titletype.IsDeleted = true;
            titletype.IsActive = false;
            _context.Entry(titletype).State = EntityState.Modified;

            //db.TitleTypes.Remove(titletype);
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
