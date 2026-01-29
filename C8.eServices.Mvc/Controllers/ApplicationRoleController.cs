using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Controllers
{
    public class ApplicationRoleController : Controller
    {
        #region ApplicationRole Init
        private eServicesDbContext db = new eServicesDbContext();

        public ApplicationRoleController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        #endregion

        #region ApplicationRole Index
        //
        // GET: /ApplicationRole/
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Index(int? id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var applicationRoles = context.ApplicationRoles.Include(ar => ar.IdentityRole)
                           .Where(ar => ar.IsActive && ar.IsDeleted == false);
                    string applicationName;

                    if (id != null)
                    {
                        applicationRoles = applicationRoles.Where(ar => ar.ApplicationId == id);
                        var application = context.Applications.Find(id);
                        applicationName = application.Name;
                    }
                    else { applicationName = "All Applications"; }

                    ViewBag.ApplicationId = id;
                    ViewBag.ApplicationName = applicationName;
                    return View(applicationRoles.ToList());
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region ApplicationRole Details
        //
        // GET: /ApplicationRole/Details/5
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Details( int id )
        {
            return View();
        }

        //
        #endregion


        #region ApplicationRole Create GET
        // GET: /ApplicationRole/Create
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Create( int? id )
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var application = context.Applications.FirstOrDefault(r => r.Key == ApplicationKeys.RatesClearanceSystem);
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                                  && ar.ApplicationId == id).Select(cr => cr.RoleId);
                    var newRoles = context.Roles.Where(r => !currentRoleApplication.Contains(r.Id)).ToList();

                    ViewBag.RoleId = new SelectList(newRoles, "Id", "Name");
                    ViewBag.ApplicationId = application.Id;
                    return View();
                }
                catch (Exception)
                {
                    return View("_Error");
                }
            }
        }

        //
        #endregion

        #region ApplicationRole Create POST
        // POST: /ApplicationRole/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Create( string roleId, int applicationId )
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var applicationrole = new ApplicationRole
                    {
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        ApplicationId = applicationId,
                        RoleId = roleId
                    };

                    context.ApplicationRoles.Add(applicationrole);
                    context.SaveChanges();
                    return RedirectToAction("Index", new { id = applicationId });
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        //
        #endregion


        #region ApplicationRole Edit GET
        // GET: /ApplicationRole/Edit/5
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Edit( int id )
        {
            return View();
        }

        //
        #endregion

        #region ApplicationRole Edit POST
        // POST: /ApplicationRole/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Edit( int id, FormCollection collection )
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View("_Error");
            }
        }

        //
        #endregion


        #region ApplicationRole Delete GET
        // GET: /ApplicationRole/Delete/5
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Delete( int id )
        {
            return View();
        }

        //
        #endregion

        #region ApplicationRole Delete POST
        // POST: /ApplicationRole/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize( Roles = "Super Administrators" )]
        public ActionResult Delete( int id, FormCollection collection )
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View("_Error");
            }
        }
        #endregion

    }
}
