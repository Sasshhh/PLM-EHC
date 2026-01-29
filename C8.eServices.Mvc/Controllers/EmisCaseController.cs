using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace C8.eServices.Mvc.Controllers
{
    public class EmisCaseController : Controller
    {
        public EmisCaseController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {

        }

        public EmisCaseController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
        }

        public EmisCaseController(eServicesDbContext db)
        {
            UserManager =
            new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        public IdentityManager IdentityManager { get; set; }
        public SystemUser SystemUser { get; set; }
        public Customer Customer { get; set; }
        public Entity Entity { get; set; }
        public Agent Agent { get; set; }
        public int CustomerId { get; set; }

        #region Report Init

        private void Initialise()
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    IdentityManager = new IdentityManager(context);

                    if (User != null && User.Identity.IsAuthenticated)
                    {
                        IdentityManager.CurrentUser(User);
                        SystemUser = IdentityManager.CurrentUser(User);
                    }

                    if (SystemUser != null)
                    {
                        Customer =
                            context.Customers.Where(o => o.SystemUserId == SystemUser.Id)
                                .Include(o => o.CustomerType)
                                .Include(o => o.Country)
                                .Include(o => o.IdentificationType)
                                .Include(o => o.TitleType)
                                .FirstOrDefault();

                        if (Customer != null)
                        {
                            Entity =
                                context.Entities.Where(o => o.CustomerId == Customer.Id)
                                    .Include(o => o.EntityType)
                                    .FirstOrDefault();
                            CustomerId = Customer.Id;
                        }

                    }

                    if (Customer != null)
                    {
                        Agent = context.Agents.FirstOrDefault(o => o.CustomerId == Customer.Id);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #endregion

        // GET: DebitOrder
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index()
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                var currentEmisCases = cxt.EmisCases.Include(o => o.Status).Where(o => o.CustomerId == Customer.Id).ToList();
                this.Customer.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", this.Customer.Id));

                EmisCaseViewModel vm = new EmisCaseViewModel()
                {
                    Customer = this.Customer
                };

                ViewBag.CurrentEmisCases = currentEmisCases;

                return View(vm);
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators" + "," + "Super Administrators" + "," + "Clerks" + "," + "Customers")]
        public ActionResult Index(EmisCaseViewModel vm)
        {
            using (var cxt = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    BaseHelper _base = new BaseHelper();
                    _base.Initialise(cxt);

                    // Setup supporting data.
                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id);
                    cust.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", cust.Id));

                    EmisCaseViewModel ecvm = new EmisCaseViewModel()
                    {
                        Customer = cust
                    };


                    if (vm.EmisCase.Id == 0)
                    {
                        // Add new EmisCase.
                        vm.EmisCase.CustomerId = cust.Id;
                        vm.EmisCase.CaseNumber = "CAS-" + DateTime.Now.ToString("yyyyMMddhhmmss") + "-W" + cust.Id.ToString();
                        vm.EmisCase.StatusId = 1;
                        cxt.EmisCases.Add(vm.EmisCase);
                        cxt.SaveChanges();
                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - Debit order successfully created.";
                        ecvm.EmisCase = cxt.EmisCases.SingleOrDefault(o => o.Id == vm.EmisCase.Id && o.IsActive && !o.IsDeleted);
                    }
                    else
                    {
                        // Update EmisCase.
                        var ec = cxt.EmisCases.SingleOrDefault(o => o.Id == vm.EmisCase.Id && o.IsActive && !o.IsDeleted);
                        
                        if (ModelState.IsValid)
                        {
                            ec.CustomerId = cust.Id;
                            ec.CaseNumber = vm.EmisCase.CaseNumber;
                            ec.Department = vm.EmisCase.Department;
                            ec.Division = vm.EmisCase.Division;
                            ec.Category = vm.EmisCase.Category;
                            ec.CallType = vm.EmisCase.CallType;
                            ec.Description = vm.EmisCase.Description;
                            ec.StreetNumber = vm.EmisCase.StreetNumber;
                            ec.StreetName = vm.EmisCase.StreetName;
                            ec.Township = vm.EmisCase.Township;
                            ec.TelephoneNumber = vm.EmisCase.TelephoneNumber;
                            ec.NearestCCC = vm.EmisCase.NearestCCC;
                            ec.ServiceLevelAgreement = vm.EmisCase.ServiceLevelAgreement;
                            ec.StatusId = 1;
                            cxt.Entry(ec).State = EntityState.Modified;
                            cxt.SaveChanges();
                        }

                        ViewBag.AccountsMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " - EMIS case successfully updated.";
                        ecvm.EmisCase = new EmisCase();
                    }

                    return View(ecvm);
                }
                catch (Exception x)
                {

                    throw;
                }
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadCase(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    EmisCase ec = cxt.EmisCases.Include(o => o.Status).FirstOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);

                    var json = Json(new { data = ec }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult LoadCases(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    List<EmisCase> list = cxt.EmisCases.Include(o => o.Status).Where(o => o.CustomerId == Id && o.IsActive && !o.IsDeleted).ToList();

                    foreach (var ec in list)
                    {
                        ec.Data = SecureActionLinkExtension.Encrypt(string.Format("Id={0}", ec.Id));
                    }

                    var json = Json(new { data = list }, JsonRequestBehavior.AllowGet);
                    json.MaxJsonLength = Int32.MaxValue;

                    return json;
                }

                return null;
            }
        }

        [EncryptedActionParameter]
        [Authorize(Roles = "Administrators" + "," + "Customers" + "," + "Super Administrators" + "," + "Clerks")]
        public ActionResult DeleteCase(int? Id)
        {
            using (var cxt = new eServicesDbContext())
            {
                Initialise();
                BaseHelper _base = new BaseHelper();
                _base.Initialise(cxt);

                if (Id != null && Id > 0)
                {
                    var ec = cxt.EmisCases.SingleOrDefault(o => o.Id == Id && o.IsActive && !o.IsDeleted);

                    if (ec != null)
                    {
                        ec.IsDeleted = true;
                        cxt.Entry(ec).State = EntityState.Modified;
                        cxt.SaveChanges();
                    }

                    var cust = cxt.Customers.FirstOrDefault(o => o.SystemUserId == SystemUser.Id && o.IsActive && !o.IsDeleted);

                    EmisCaseViewModel dovm = new EmisCaseViewModel()
                    {
                        Customer = cust
                    };

                    return RedirectToAction("Index", "EmisCase");
                }

                return null;
            }
        }
    }
}