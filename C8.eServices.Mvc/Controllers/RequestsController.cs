using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using Microsoft.AspNet.Identity;

namespace C8.eServices.Mvc.Controllers
{
    public class RequestsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        // GET: Requests
        public ActionResult Index()
        {
            var requests = db.Requests.Include(r => r.Comment).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.RecipientCustomer).Include(r => r.ReferenceType).Include(r => r.Status);
            return View(requests.ToList());
        }

        // GET: Requests/Details/5
        [EncryptedActionParameter]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var request = db.Requests.Include(la => la.Comment)
           .Include(la => la.Status)
           .Include(la => la.Customer)
           .SingleOrDefault(la => la.Id == id);
            if (request == null)
            {
                return HttpNotFound();
            }
            return View(request);
        }

        // GET: Requests/Create
        public ActionResult Create()
        {
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "ReferenceAlpha");
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "Id");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.RecipientCustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber");
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name");
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name");
            return View();
        }

        // POST: Requests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CustomerId,ReferenceTypeId,ReferenceNumeric,ReferenceAlpha,CommentId,RecipientEmail,RecipientCustomerId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Request request)
        {
            if (ModelState.IsValid)
            {
                db.Requests.Add(request);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CommentId = new SelectList(db.Comments, "Id", "ReferenceAlpha", request.CommentId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", request.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", request.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", request.ModifiedBySystemUserId);
            ViewBag.RecipientCustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", request.RecipientCustomerId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", request.ReferenceTypeId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", request.StatusId);
            return View(request);
        }

        // GET: Requests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Request request = db.Requests.Find(id);
            if (request == null)
            {
                return HttpNotFound();
            }
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "ReferenceAlpha", request.CommentId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", request.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", request.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", request.ModifiedBySystemUserId);
            ViewBag.RecipientCustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", request.RecipientCustomerId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", request.ReferenceTypeId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", request.StatusId);
            return View(request);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CustomerId,ReferenceTypeId,ReferenceNumeric,ReferenceAlpha,CommentId,RecipientEmail,RecipientCustomerId,StatusId,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Request request)
        {
            if (ModelState.IsValid)
            {
                db.Entry(request).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CommentId = new SelectList(db.Comments, "Id", "ReferenceAlpha", request.CommentId);
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", request.CreatedBySystemUserId);
            ViewBag.CustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", request.CustomerId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", request.ModifiedBySystemUserId);
            ViewBag.RecipientCustomerId = new SelectList(db.Customers, "Id", "IdentificationNumber", request.RecipientCustomerId);
            ViewBag.ReferenceTypeId = new SelectList(db.ReferenceTypes, "Id", "Name", request.ReferenceTypeId);
            ViewBag.StatusId = new SelectList(db.Status, "Id", "Name", request.StatusId);
            return View(request);
        }

        // GET: Requests/Delete/5
        [EncryptedActionParameter]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var request = db.Requests.Include(la => la.Comment)
           .Include(la => la.Status)
           .Include(la => la.Customer)
           .SingleOrDefault(la => la.Id == id);
            if (request == null)
            {
                return HttpNotFound();
            }
            return View(request);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Request request = db.Requests.Find(id);
            request.IsActive = false;
            request.IsDeleted = true;
            db.Entry(request).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index","Profile");
        }

        public ActionResult RequestNotifications()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var CurrUser = User.Identity.GetUserName();
                    var customerSys = context.SystemUsers.SingleOrDefault(c => c.UserName == CurrUser);
                    var customer = context.Customers.Include(c => c.CustomerType)
                             .SingleOrDefault(c => c.SystemUserId == customerSys.Id);
               
                    if (customer != null)
                    {
                        var query = context.Requests.Include(x => x.Comment).Include(x => x.Customer)
                        .Include(x => x.Status)
                        .Where(x => x.RecipientCustomerId == customer.Id && x.Customer.IsActive
                        && x.Status.Key == StatusKeys.LinkedAccountPending && x.IsActive && x.IsDeleted == false);
                        var ReqCounter = query.ToList().Count;
                        Session["ReqCount"] = ReqCounter;
                    }
                    else
                    {
                        Session["ReqCount"] = 0;
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Session["ReqCount"] = 0;
                    return null;
                }

                return null;
            }
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
