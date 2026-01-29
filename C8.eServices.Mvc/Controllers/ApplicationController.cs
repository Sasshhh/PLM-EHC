    using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    public class ApplicationController : Controller
    {
        #region Application Init
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public ApplicationController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        #endregion

        #region Application Index
        // GET: /Application/
        [Authorize(Roles = "Super Administrators" + "," + "Administrators")]
        public ActionResult Index()
        {
            if(User.IsInRole("Administrators"))
            {
                var applications = db.Applications.Include(a => a.CreatedBySystemUser).Include(a => a.ModifiedBySystemUser).Where(a => a.Key == ApplicationKeys.LinkedAccounts);
                return View(applications.ToList());
            }
            else
            {
                var applications = db.Applications.Include(a => a.CreatedBySystemUser).Include(a => a.ModifiedBySystemUser);
                return View(applications.ToList());
            }
            //Application App = db.Applications.FirstOrDefault(o => o.Key == ApplicationKeys.LinkedAccounts);
           
        }

        #endregion


        #region Application Details
        // GET: /Application/Details/5
        [Authorize(Roles = "Super Administrators")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var application = db.Applications.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }
            return View(application);
        }

        #endregion


        #region Application Create GET
        // GET: /Application/Create
        [Authorize(Roles =  "Super Administrators" )]
        public ActionResult Create()
        {
            TempData["Success"] = null;
            TempData["Info"] = null;
            TempData["Error"] = null;
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }
        #endregion

        #region Application Create POST
        // POST: /Application/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =  "Super Administrators" )]
        public ActionResult Create([Bind(Include="Id,ApplicationAbbreviation,IsOnline,Name,Description,Key,SeedDate,Seed,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Application application)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                //create key type
                application.Key = "a_" + (application.Name.ToLower()).Replace(" ", "_");
                
                //Add application record
                var applicationName = _context.Applications.Where(a => a.Name == application.Name).Select(a => a.Name).FirstOrDefault();

                if (applicationName == null)
                {
                    IdentityManager.CurrentUser(User);
                    application.SeedDate = DateTime.Now;
                    application.Seed = 1;
                    application.IsActive = true;
                    application.IsDeleted = false;
                    application.IsLocked = false;
                    _context.Applications.Add(application);
                    _context.SaveChanges();
                    TempData["Success"] = "Application Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Application Error.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", application.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", application.ModifiedBySystemUserId);
            return View(application);
        }

        #endregion


        #region Application Edit GET
        // GET: /Application/Edit/5
        [Authorize(Roles =  "Super Administrators")]
        public ActionResult Edit(int? id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Application application = _context.Applications.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", application.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", application.ModifiedBySystemUserId);
            return View(application);
        }
        #endregion

        #region Application Edit POST
        // POST: /Application/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Administrators")]
        public ActionResult Edit([Bind(Include="Id,ApplicationAbbreviation,IsOnline,Name,Description,Key,SeedDate,Seed,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] Application application)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(application).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", application.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", application.ModifiedBySystemUserId);
            return View(application);
        }
        #endregion


        #region Application Delete GET
        // GET: /Application/Delete/5
        [Authorize(Roles =  "Super Administrators")]
        public ActionResult Delete(int? id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Application application = _context.Applications.Find(id);
            if (application == null)
            {
                return HttpNotFound();
            }
            return View(application);
        }
        #endregion

        #region Application Delete POST
        // POST: /Application/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =  "Super Administrators")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            Application application = _context.Applications.Find(id);

            //soft deletes record on the database
            application.IsDeleted = true;
            application.IsActive = false;

            _context.Entry(application).State = EntityState.Modified;

            _context.Applications.Remove(application);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        #endregion


        #region Application Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
