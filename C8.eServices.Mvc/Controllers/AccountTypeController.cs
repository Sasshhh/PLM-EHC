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
    public class AccountTypeController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public AccountTypeController()
        {
            IdentityManager = new IdentityManager(db);
            db.CurrentSystemUser = IdentityManager.CurrentUser( User );
        }

        public IdentityManager IdentityManager { get; set; }


        // GET: /AccountType/
        [Authorize(Roles = "Administrators" +  "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var accounttypes = db.AccountTypes.Include(a => a.CreatedBySystemUser).Include(a => a.ModifiedBySystemUser);
            return View(accounttypes.ToList());
        }

        // GET: /AccountType/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountType accounttype = db.AccountTypes.Find(id);
            if (accounttype == null)
            {
                return HttpNotFound();
            }
            return View(accounttype);
        }

        // GET: /AccountType/Create
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

        // POST: /AccountType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] AccountType accounttype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //Create type key
                accounttype.Key = "at_" + (accounttype.Name.ToLower().ToString()).Replace(" ", "_");

                //Add account type record
                var accountName = _context.AccountTypes.Where(a => a.Name == accounttype.Name).Select(a => a.Name).FirstOrDefault();

                if (accountName == null)
                {
                    IdentityManager.CurrentUser(User);
                    accounttype.IsActive = true;
                    accounttype.IsDeleted = false;
                    accounttype.IsLocked = false;
                    _context.AccountTypes.Add(accounttype);
                    _context.SaveChanges();
                    TempData["Success"] = "Account Type Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Account Type Error.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", accounttype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", accounttype.ModifiedBySystemUserId);
            return View(accounttype);
        }

        // GET: /AccountType/Edit/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountType accounttype = db.AccountTypes.Find(id);
            if (accounttype == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", accounttype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", accounttype.ModifiedBySystemUserId);
            return View(accounttype);
        }

        // POST: /AccountType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] AccountType accounttype)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(accounttype).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", accounttype.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", accounttype.ModifiedBySystemUserId);
            return View(accounttype);
        }

        // GET: /AccountType/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountType accounttype = db.AccountTypes.Find(id);
            if (accounttype == null)
            {
                return HttpNotFound();
            }
            return View(accounttype);
        }

        // POST: /AccountType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            AccountType accounttype = db.AccountTypes.Find(id);

            //soft deletes record on the database
            accounttype.IsDeleted = true;
            accounttype.IsActive = false;

            //db.AccountTypes.Remove(accounttype);

            _context.Entry(accounttype).State = EntityState.Modified;

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
