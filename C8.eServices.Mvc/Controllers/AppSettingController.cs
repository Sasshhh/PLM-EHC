using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    public class AppSettingController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        public AppSettingController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        // GET: /AppSetting/
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Index()
        {
            var appsettings = db.AppSettings.Include(a => a.CreatedBySystemUser).Include(a => a.ModifiedBySystemUser);
            return View(appsettings.ToList());
        }

        // GET: /AppSetting/Details/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppSetting appsetting = db.AppSettings.Find(id);
            if (appsetting == null)
            {
                return HttpNotFound();
            }
            return View(appsetting);
        }

        // GET: /AppSetting/Create
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create()
        {
            TempData["Success"] = null;
            TempData["Error"] = null;
            TempData["Info"] = null;
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName");
            return View();
        }

        // POST: /AppSetting/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Create([Bind(Include="Id,Value,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] AppSetting appsetting)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                // create key type
                appsetting.Key = "as_" + (appsetting.Name.ToLower().ToString()).Replace(" ", "_");
            
                //Add app setting record
                var appSettName = _context.AppSettings.Where(a => a.Name == appsetting.Name).Select(a => a.Name).FirstOrDefault();

                if (appSettName == null)
                {
                    IdentityManager.CurrentUser(User);
                    appsetting.IsActive = true;
                    appsetting.IsDeleted = false;
                    appsetting.IsLocked = false;
                    _context.AppSettings.Add(appsetting);
                    _context.SaveChanges();
                    TempData["Success"] = "AppSetting Saved Successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "AppSettings Error.";
                }
            }

            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", appsetting.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", appsetting.ModifiedBySystemUserId);
            return View(appsetting);
        }

        // GET: /AppSetting/Edit/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppSetting appsetting = db.AppSettings.Find(id);
            if (appsetting == null)
            {
                return HttpNotFound();
            }
            ViewBag.CreatedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", appsetting.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(db.SystemUsers, "Id", "FirstName", appsetting.ModifiedBySystemUserId);
            return View(appsetting);
        }

        // POST: /AppSetting/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Edit([Bind(Include="Id,Value,Name,Description,Key,IsActive,IsDeleted,IsLocked,CreatedBySystemUserId,CreatedDateTime,ModifiedBySystemUserId,ModifiedDateTime")] AppSetting appsetting)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);

            if (ModelState.IsValid)
            {
                _context.Entry(appsetting).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CreatedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", appsetting.CreatedBySystemUserId);
            ViewBag.ModifiedBySystemUserId = new SelectList(_context.SystemUsers, "Id", "FirstName", appsetting.ModifiedBySystemUserId);
            return View(appsetting);
        }

        // GET: /AppSetting/Delete/5
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppSetting appsetting = db.AppSettings.Find(id);
            if (appsetting == null)
            {
                return HttpNotFound();
            }
            return View(appsetting);
        }

        // POST: /AppSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteConfirmed(int id)
        {
            eServicesDbContext _context = new eServicesDbContext();
            _base.Initialise(_context);
            AppSetting appsetting = _context.AppSettings.Find(id);

            //soft deletes the record on the database
            appsetting.IsDeleted = true;
            appsetting.IsActive = false;

            //db.AppSettings.Remove(appsetting);

            _context.Entry(appsetting).State = EntityState.Modified;

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

        [Authorize( Roles = "Super Administrators" )]
        public AppSetting GetAppSetting( string key )
        {
            if ( !string.IsNullOrEmpty( key ) )
                return db.AppSettings.SingleOrDefault( o => o.Key == key );
            else
                return null;
        }

        #region GET Application Updates 
        [Authorize(Roles = "System Administrators")]
        public ActionResult ApplicationUpdates()
        {
            try
            {
                using (var context = new eServicesDbContext())
                {
                    var eServicesUpdates = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.UpdateNotification);
                    if (eServicesUpdates == null) throw new Exception("Invalid app setting");
                    return View(eServicesUpdates);
                }            
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region POST Application Updates
        [HttpPost]
        [Authorize(Roles = "System Administrators")]
        public ActionResult ApplicationUpdates(AppSetting appsetting)
        {
            try
            {
                using (var context = new eServicesDbContext())
                {
                    _base.Initialise(context);
                    var response = string.Empty;

                    if (ModelState.IsValid)
                    {
                        context.Entry(appsetting).State = EntityState.Modified;
                        context.SaveChanges();
                        TempData["Success"] = true;
                        response = "Message Updated";
                    }
                    else
                    {
                        TempData["Success"] = false;
                        response = "Please try again later";
                    }

                    ViewBag.Response = response;
                    return View(appsetting);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        [AllowAnonymous]
        public ActionResult ReturnIncentivePolicyTsAndCs()
        {
            try
            {
                var appSetting = db.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.IncentivePolicyTermsAndConditions && !a.IsDeleted && a.IsActive);
                if (appSetting != null)
                {
                    var value = appSetting.Value;
                    return Redirect(value);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
    }
}

