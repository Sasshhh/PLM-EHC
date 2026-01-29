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
    public class DocumentsLeasesController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: DocumentsLeases
        public ActionResult Index()
        {
            var documentsLeases = db.DocumentsLeases.Include(d => d.CreatedBySystemUser).Include(d => d.Customer).Include(d => d.DocumentCheckList).Include(d => d.File).Include(d => d.leaseDetails).Include(d => d.LocationType).Include(d => d.ModifiedBySystemUser).Include(d => d.RCSApplicationStatus).Include(d => d.ReferenceType).Include(d => d.RefundApplication).Include(d => d.Status);
            return View(documentsLeases.ToList());
        }

        // GET: DocumentsLeases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentsLease documentsLease = db.DocumentsLeases.Find(id);
            if (documentsLease == null)
            {
                return HttpNotFound();
            }
            return View(documentsLease);
        }

        // GET: DocumentsLeases/Create
        public ActionResult Create()
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber");
            ViewBag.DocumentCheckListId = new SelectList(db.DocumentCheckLists, "Id", "Id");
            ViewBag.FileId = new SelectList(db.Files, "Id", "FileName");
            ViewBag.TenantLeaseId = new SelectList(db.LeaseDetails, "Id", "leaseApplicationRef");
            ViewBag.LocationTypeId = new SelectList(db.LocationTypes, "Id", "Name");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber");
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name");
            ViewBag.RefundApplicationId = new SelectList(db.RefundApplications, "Id", "ApplicationReferenceNumber");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            return View();
        }

        // POST: DocumentsLeases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerId,ReferenceTypeId,ReferenceId,LocationTypeId,DocumentLocation,DocumentName,StatusId,DocumentCheckListId,FileId,RCSApplicationStatusId,Comment,RefundApplicationId,TenantLeaseId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DocumentsLease documentsLease)
        {
            if (ModelState.IsValid)
            {
                db.DocumentsLeases.Add(documentsLease);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentsLease.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", documentsLease.CustomerId);
            ViewBag.DocumentCheckListId = new SelectList(db.DocumentCheckLists, "Id", "Id", documentsLease.DocumentCheckListId);
            ViewBag.FileId = new SelectList(db.Files, "Id", "FileName", documentsLease.FileId);
            ViewBag.TenantLeaseId = new SelectList(db.LeaseDetails, "Id", "leaseApplicationRef", documentsLease.TenantLeaseId);
            ViewBag.LocationTypeId = new SelectList(db.LocationTypes, "Id", "Name", documentsLease.LocationTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentsLease.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", documentsLease.RCSApplicationStatusId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", documentsLease.ReferenceTypeId);
            ViewBag.RefundApplicationId = new SelectList(db.RefundApplications, "Id", "ApplicationReferenceNumber", documentsLease.RefundApplicationId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", documentsLease.StatusId);
            return View(documentsLease);
        }

        // GET: DocumentsLeases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentsLease documentsLease = db.DocumentsLeases.Find(id);
            if (documentsLease == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentsLease.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", documentsLease.CustomerId);
            ViewBag.DocumentCheckListId = new SelectList(db.DocumentCheckLists, "Id", "Id", documentsLease.DocumentCheckListId);
            ViewBag.FileId = new SelectList(db.Files, "Id", "FileName", documentsLease.FileId);
            ViewBag.TenantLeaseId = new SelectList(db.LeaseDetails, "Id", "leaseApplicationRef", documentsLease.TenantLeaseId);
            ViewBag.LocationTypeId = new SelectList(db.LocationTypes, "Id", "Name", documentsLease.LocationTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentsLease.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", documentsLease.RCSApplicationStatusId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", documentsLease.ReferenceTypeId);
            ViewBag.RefundApplicationId = new SelectList(db.RefundApplications, "Id", "ApplicationReferenceNumber", documentsLease.RefundApplicationId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", documentsLease.StatusId);
            return View(documentsLease);
        }

        // POST: DocumentsLeases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerId,ReferenceTypeId,ReferenceId,LocationTypeId,DocumentLocation,DocumentName,StatusId,DocumentCheckListId,FileId,RCSApplicationStatusId,Comment,RefundApplicationId,TenantLeaseId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] DocumentsLease documentsLease)
        {
            if (ModelState.IsValid)
            {
                db.Entry(documentsLease).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentsLease.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", documentsLease.CustomerId);
            ViewBag.DocumentCheckListId = new SelectList(db.DocumentCheckLists, "Id", "Id", documentsLease.DocumentCheckListId);
            ViewBag.FileId = new SelectList(db.Files, "Id", "FileName", documentsLease.FileId);
            ViewBag.TenantLeaseId = new SelectList(db.LeaseDetails, "Id", "leaseApplicationRef", documentsLease.TenantLeaseId);
            ViewBag.LocationTypeId = new SelectList(db.LocationTypes, "Id", "Name", documentsLease.LocationTypeId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", documentsLease.ModifiedBySystemUserId);
            ViewBag.RCSApplicationStatusId = new SelectList(db.RCSApplicationStatus, "Id", "ApplicationReferenceNumber", documentsLease.RCSApplicationStatusId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", documentsLease.ReferenceTypeId);
            ViewBag.RefundApplicationId = new SelectList(db.RefundApplications, "Id", "ApplicationReferenceNumber", documentsLease.RefundApplicationId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", documentsLease.StatusId);
            return View(documentsLease);
        }

        // GET: DocumentsLeases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocumentsLease documentsLease = db.DocumentsLeases.Find(id);
            if (documentsLease == null)
            {
                return HttpNotFound();
            }
            return View(documentsLease);
        }

        // POST: DocumentsLeases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DocumentsLease documentsLease = db.DocumentsLeases.Find(id);
            db.DocumentsLeases.Remove(documentsLease);
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
