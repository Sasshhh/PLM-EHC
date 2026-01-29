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
using C8.eServices.Mvc.ViewModels;

namespace C8.eServices.Mvc.Controllers
{
    public class REACsController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }
        private void Initialise()
        {

            try
            {
                IdentityManager = new IdentityManager(db);

                if (User != null && User.Identity.IsAuthenticated)
                {
                    IdentityManager.CurrentUser(User);
                    SystemUser = IdentityManager.CurrentUser(User);
                }

                if (SystemUser != null)
                {
                    Customer =
                        db.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                            .Include(o => o.CustomerType)
                            .Include(o => o.Country)
                            .Include(o => o.IdentificationType)
                            .Include(o => o.TitleType)
                              .Include(o => o.Status)
                            .FirstOrDefault();

                    if (Customer != null)
                    {
                        Entity =
                            db.Entities.Where(o => o.CustomerId == Customer.Id)
                                .Include(o => o.EntityType)
                                .FirstOrDefault();
                        CustomerId = Customer.Id;
                    }

                }

                if (Customer != null)
                {
                    Agent = db.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                }
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                throw;
            }

        }

        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    var model = cxt.PropertyLeaseApplications.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.Customer).Include(r => r.ModifiedBySystemUser).Include(r => r.HumanEHCOptions).ToList();
                    var model2 = cxt.LeaseDetails.Where(x => x.IsDeleted == false).Include(r => r.CreatedBySystemUser).Include(r => r.PurchaserType).Include(r => r.ModifiedBySystemUser).Include(r => r.Status).ToList();

                    var referenceType = cxt.ReferenceTypes.Where(x => x.Key == ReferenceTypeKeys.RCSUpload).FirstOrDefault();
                    var application = cxt.Applications.FirstOrDefault(a => a.Key.Equals(ApplicationKeys.RatesClearanceSystem));
                    foreach (var item in model)
                    {
                        item.Data = SecureActionLinkExtension.Encrypt(string.Format("rcsAppId={0}", item.Id));
                    }
                    return View(model);
                }
                catch (Exception io)
                {
                    EventLogHelper.LogSystemError(io.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                }

                return RedirectToAction("Login", "Account");
            }
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REAC rEAC = db.REACs.Find(id);
            if (rEAC == null)
            {
                return HttpNotFound();
            }
            return View(rEAC);
        }


        [DecryptParameter]
        public ActionResult Create(int id)
        {
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.Outcome = new SelectList(db.RCSActionTypes.Where(x => x.Key == RCSActionTypeKeys.Approved || x.Key == RCSActionTypeKeys.Rejected).OrderBy(x => x.Name), "Key", "Name");
            ViewBag.date = DateTime.Now.Date;

            var PLA = db.PropertyLeaseApplications.FirstOrDefault(x => x.Id == id);

            var vm = new CaptureViewModel
            {
                PropertyLeaseApplication = PLA
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CaptureViewModel capture)
        {
            if (ModelState.IsValid)
            {
                PropertyLeaseApplication prop = new PropertyLeaseApplication();

                prop = capture.PropertyLeaseApplication;
                db.PropertyLeaseApplications.Add(prop);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REAC rEAC = db.REACs.Find(id);
            if (rEAC == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rEAC.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rEAC.ModifiedBySystemUserId);
            return View(rEAC);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PropertyLeaseID,Outcome,REACCommments,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] REAC rEAC)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rEAC).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rEAC.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", rEAC.ModifiedBySystemUserId);
            return View(rEAC);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REAC rEAC = db.REACs.Find(id);
            if (rEAC == null)
            {
                return HttpNotFound();
            }
            return View(rEAC);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            REAC rEAC = db.REACs.Find(id);
            db.REACs.Remove(rEAC);
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
