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

namespace C8.eServices.Mvc.Controllers
{
    public class StatusTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        public StatusTypeController()
        {
            IdentityManager = new IdentityManager(db);
        }

        IdentityManager IdentityManager {get; set;}

        // GET: /StatusType/
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var statustypes = db.StatusTypes.Include(s => s.CreatedBySystemUser).Include(s => s.ModifiedBySystemUser);
            return View(statustypes.ToList());
        }

        // GET: /StatusType/Details/5
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatusType statustype = db.StatusTypes.Find(id);
            if (statustype == null)
            {
                return HttpNotFound();
            }
            return View(statustype);
        }

        // GET: /StatusType/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            TempData["Success"] = null;
            TempData["Info"] = null;
            TempData["Error"] = null;
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: /StatusType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] StatusType statustype)
        {
            if (ModelState.IsValid)
            {
                // L.M. - Creates type key
                statustype.Key = "st_" + (statustype.Name.ToLower().ToString()).Replace(" ", "_");
               
                //Add status type record
                var statusTypeName = db.StatusTypes.Where(s => s.Name == statustype.Name).Select(s => s.Name).FirstOrDefault();

                if (statusTypeName == null)
                {
                    statustype.IsActive = true;
                    statustype.IsDeleted = false;
                    statustype.IsLocked = false;
                    db.StatusTypes.Add(statustype);
                    db.SaveChanges();
                    TempData["Success"] = "Status Type Successfully Saved!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Status Type Error.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", statustype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", statustype.ModifiedBySystemUserId);
            return View(statustype);
        }

        // GET: /StatusType/Edit/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatusType statustype = db.StatusTypes.Find(id);
            if (statustype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", statustype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", statustype.ModifiedBySystemUserId);
            return View(statustype);
        }

        // POST: /StatusType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators"  + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] StatusType statustype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(statustype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", statustype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", statustype.ModifiedBySystemUserId);
            return View(statustype);
        }

        // GET: /StatusType/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StatusType statustype = db.StatusTypes.Find(id);
            if (statustype == null)
            {
                return HttpNotFound();
            }
            return View(statustype);
        }

        // POST: /StatusType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            StatusType statustype = db.StatusTypes.Find(id);

            // Soft deletes the record on the database
            statustype.IsDeleted = true;
            statustype.IsActive = false;
            db.Entry(statustype).State = EntityState.Modified;
            //db.StatusTypes.Remove(statustype);

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
