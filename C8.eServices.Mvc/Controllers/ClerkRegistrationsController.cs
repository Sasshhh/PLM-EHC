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
    public class ClerkRegistrationsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: ClerkRegistrations
        public ActionResult Index()
        {
            var clerkRegistrations = db.ClerkRegistrations.Include(c => c.CCC).Include(c => c.CCCType).Include(c => c.CreatedBySystemUser).Include(c => c.ModifiedBySystemUser).Include(c => c.NotificationType).Include(c => c.Status);
            return View(clerkRegistrations.ToList());
        }

        // GET: ClerkRegistrations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
            if (clerkRegistration == null)
            {
                return HttpNotFound();
            }
            return View(clerkRegistration);
        }

        // GET: ClerkRegistrations/Create
        public ActionResult Create()
        {
            ViewBag.CCCId = new SelectList(db.CCCs, "Id", "CCCName");
            ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            return View();
        }

        // POST: ClerkRegistrations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,UserName,EmployeeNumber,RoleArray,EmailAddress,SystemUserTypeId,StatusId,IsPasswordReset,IsTemporaryPassword,IdentificationNumber,MobileNumber,Code,NotificationTypeId,CCCTypeId,CCCId,IsActiveDirectoryUser,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ClerkRegistration clerkRegistration)
        {
            if (ModelState.IsValid)
            {

                var clerkRegistration3 = new ClerkRegistration();
                clerkRegistration3.FirstName = "Siyanda";
                clerkRegistration3.LastName = "Ngxongo";
                clerkRegistration3.IdentificationNumber = "0001315412088";
                clerkRegistration3.UserName = "Noone4";
                clerkRegistration3.EmployeeNumber = "000131";
                //FirstName = model.FirstName,
                //LastName = model.LastName,
                //IdentificationNumber = model.IdentificationNumber,
                //UserName = model.UserName,
                //EmployeeNumber = model.EmployeeNumber,
                clerkRegistration3.RoleArray = "Clerks";
                clerkRegistration3.CCCTypeId = 1;
                clerkRegistration3.CCCId = 1;
                clerkRegistration3.IsActiveDirectoryUser = true;
                //IsActiveDirectoryUser= model.AdUser,
                clerkRegistration3.EmailAddress = "siyanda.ngxonga@xetgroup.com";
                clerkRegistration3.MobileNumber = "0846666435";
                //EmailAddress = model.EmailAddress,
                //MobileNumber = model.MobileNumber,
                clerkRegistration3.StatusId = 4;
                clerkRegistration3.IsActive = true;
                clerkRegistration3.IsDeleted = false;
                clerkRegistration3.IsLocked = false;
                clerkRegistration3.CreatedDateTime = DateTime.Now;
                clerkRegistration3.ModifiedDateTime = DateTime.Now;
                clerkRegistration3.IsPasswordReset = true;
                clerkRegistration3.NotificationTypeId = 1;

                db.ClerkRegistrations.Add(clerkRegistration3);
                db.SaveChanges();

                db.ClerkRegistrations.Add(clerkRegistration);
                db.SaveChanges();



                var clerkRegistration2 = new ClerkRegistration();
                clerkRegistration2.FirstName = "Siyanda";
                clerkRegistration2.LastName = "Ngxongo";
                clerkRegistration2.IdentificationNumber = "0001315412088";
                clerkRegistration2.UserName = "Noone";
                clerkRegistration2.EmployeeNumber = "000131";
                //FirstName = model.FirstName,
                //LastName = model.LastName,
                //IdentificationNumber = model.IdentificationNumber,
                //UserName = model.UserName,
                //EmployeeNumber = model.EmployeeNumber,
                clerkRegistration2.RoleArray = "Clerks";
                clerkRegistration2.CCCTypeId = clerkRegistration.CCCTypeId;
                clerkRegistration2.CCCId = clerkRegistration.CCCId;
                clerkRegistration2.IsActiveDirectoryUser = true;
                //IsActiveDirectoryUser= model.AdUser,
                clerkRegistration2.EmailAddress = "siyanda.ngxonga@xetgroup.com";
                clerkRegistration2.MobileNumber = "0846666435";
                //EmailAddress = model.EmailAddress,
                //MobileNumber = model.MobileNumber,
                clerkRegistration2.StatusId = clerkRegistration.StatusId;
                clerkRegistration2.IsActive = true;
                clerkRegistration2.IsDeleted = false;
                clerkRegistration2.IsLocked = false;
                clerkRegistration2.CreatedDateTime = DateTime.Now;
                clerkRegistration2.ModifiedDateTime = DateTime.Now;
                clerkRegistration2.IsPasswordReset = true;
                clerkRegistration2.NotificationTypeId = clerkRegistration.NotificationTypeId;
                db.ClerkRegistrations.Add(clerkRegistration2);
                db.SaveChanges();


                return RedirectToAction("Index");
            }

            ViewBag.CCCId = new SelectList(db.CCCs, "Id", "CCCName", clerkRegistration.CCCId);
            ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
            ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", clerkRegistration.StatusId);
            return View(clerkRegistration);
        }

        // GET: ClerkRegistrations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
            if (clerkRegistration == null)
            {
                return HttpNotFound();
            }
            ViewBag.CCCId = new SelectList(db.CCCs, "Id", "CCCName", clerkRegistration.CCCId);
            ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
            ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", clerkRegistration.StatusId);
            return View(clerkRegistration);
        }

        // POST: ClerkRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,UserName,EmployeeNumber,RoleArray,EmailAddress,SystemUserTypeId,StatusId,IsPasswordReset,IsTemporaryPassword,IdentificationNumber,MobileNumber,Code,NotificationTypeId,CCCTypeId,CCCId,IsActiveDirectoryUser,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] ClerkRegistration clerkRegistration)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clerkRegistration).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CCCId = new SelectList(db.CCCs, "Id", "CCCName", clerkRegistration.CCCId);
            ViewBag.CCCTypeId = new SelectList(db.CCCTypes, "Id", "Name", clerkRegistration.CCCTypeId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", clerkRegistration.ModifiedBySystemUserId);
            ViewBag.NotificationTypeId = new SelectList(db.NotificationTypes, "Id", "Name", clerkRegistration.NotificationTypeId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", clerkRegistration.StatusId);
            return View(clerkRegistration);
        }

        // GET: ClerkRegistrations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
            if (clerkRegistration == null)
            {
                return HttpNotFound();
            }
            return View(clerkRegistration);
        }

        // POST: ClerkRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ClerkRegistration clerkRegistration = db.ClerkRegistrations.Find(id);
            db.ClerkRegistrations.Remove(clerkRegistration);
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
