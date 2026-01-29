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
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Controllers
{
    public class SystemRoleController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();
        public SystemRoleController()
        {
            IdentityManager = new IdentityManager(db);
        }

        public IdentityManager IdentityManager { get; set; }

        //
        // GET: /SystemRole/
        [Authorize( Roles = "Administrators" + "," + "Super Administrators" )]
        public ActionResult Index()
        {
            try
            {
                var roles = db.Roles.ToList();
                return View(roles);
            }
            catch (Exception)
            {
                return View("_Error");
            }
        }

        // GET: /Roles/Create
        [Authorize( Roles = "Administrators" + "," + "Super Administrators" )]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
        [Authorize( Roles = "Administrators" + "," + "Super Administrators" )]
        public ActionResult Create(string roleName)
        {
            try
            {
                IdentityRole role = new IdentityRole(roleName);
                db.Roles.Add(role);

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("_Error");
            }
        }
	}
}