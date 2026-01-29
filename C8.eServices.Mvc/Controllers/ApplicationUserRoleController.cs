using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.DataAccessLayer;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Text;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using MoreLinq;
using C8.eServices.Mvc.ViewModels;
using System.Web.Security;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using static System.Net.Mime.MediaTypeNames;

namespace C8.eServices.Mvc.Controllers
{
    public class ApplicationUserRoleController : Controller
    {

        #region ApplicationUserRole Init
        private eServicesDbContext db = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();

        public ApplicationUserRoleController()
        {
            IdentityManager = new IdentityManager(db);
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            UserManager = new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(db));
        }

        public IdentityManager IdentityManager { get; set; }
        public RoleManager<IdentityRole> RoleManager { get; set; }
        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #endregion


        #region Init
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
        #endregion
        #region ApplicationUserRole Index
        //
        // GET: /ApplicationUserRole/
        [Authorize(Roles = "Super Administrators,Area Manager")]
        public ActionResult Index(int? id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var applicationUserRoles = context.ApplicationUserRoles.Include(a => a.SystemUser)
                                .Include(a => a.IdentityRole)
                                .Where(a => a.IsActive && a.IsDeleted == false);
                    string applicationName;

                    if (id != null)
                    {
                        var application = context.Applications.Find(id);
                        applicationUserRoles = applicationUserRoles.Where(a => a.ApplicationId == id);
                        applicationName = application.Name;
                    }
                    else { applicationName = "All Applications"; }

                    //if (TempData["DisplayMessage"] != null)
                    //{
                    //    ViewBag.Title = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.Rcs_assessment).FirstOrDefault().Description + TempData["ApplicationRefNo"].ToString();

                    //    ViewBag.Message = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.Rcs_assessment).FirstOrDefault().Description + TempData["ApplicationRefNo"].ToString();
                    //}
                    ViewBag.ApplicationId = id;
                    ViewBag.ApplicationName = applicationName;
                    return View(applicationUserRoles.ToList());
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion

        #region ApplicationUserRole Area manager
        //
        // GET: /ApplicationUserRole/
        [Authorize(Roles = "Area Manager")]
        public ActionResult UserIndex(int? id)
        {
            using (var context = new eServicesDbContext())
            {
                Initialise();
                try
                {

                    var CCCClerk = db.CCCs.Where(x => x.AreaManagerId == Customer.Id).FirstOrDefault();

                    var applicationUserRoles = context.ApplicationUserRoles.Include(a => a.SystemUser)
                                .Include(a => a.IdentityRole)
                                .Where(a => a.IsActive && a.IsDeleted == false);
                    string applicationName;


                    var application = context.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();




                    applicationUserRoles = applicationUserRoles.Include(a => a.SystemUser).Where(a => a.ApplicationId == application.Id && a.CCCId == CCCClerk.Id);
                    applicationName = application.Name;

                    foreach (var item in applicationUserRoles)
                    {
                        //var RolesArray =

                        var user = UserManager.FindByName(item.SystemUser.UserName);
                        var userId = user.Id;
                        // get user roles
                        List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                        string RolesList = "";
                        int counter = 0;
                        foreach (var Roles in rolesArray)
                        {
                            if (counter > 0)
                            {
                                RolesList += ", ";
                            }
                            counter++;
                            RolesList += Roles;
                        }
                        item.Data = RolesList;
                        //if (customerDocument.File != null)
                        //    customerDocument.File.Data = SecureActionLinkExtension.Encrypt(string.Format("fileId={0}", customerDocument.FileId));

                        //var docCheckList =
                        //     _context.DocumentCheckLists.Include(d => d.DocumentType)
                        //         .FirstOrDefault(c => c.Id == customerDocument.DocumentCheckListId && c.IsActive && !c.IsDeleted);
                        //customerDocument.DocumentLocation = string.Format("uploads/{0}/{1}", docCheckList.DocumentType.Name,
                        //    customerDocument.DocumentName);

                        //customerDocument.Data = HttpUtility.UrlEncode(SecureActionLinkExtension.Encrypt(string.Format("documentId={0}", customerDocument.Id)));
                    }
                    //if (TempData["DisplayMessage"] != null)
                    //{
                    //    ViewBag.Title = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.Rcs_assessment).FirstOrDefault().Description + TempData["ApplicationRefNo"].ToString();

                    //    ViewBag.Message = db.RCSTypes.Where(x => x.Key == RCSTypeKeys.Rcs_assessment).FirstOrDefault().Description + TempData["ApplicationRefNo"].ToString();
                    //}
                    ViewBag.ApplicationId = id;
                    ViewBag.ApplicationName = applicationName;
                    return View(applicationUserRoles.ToList());
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion
        #region ApplicationUserRole Admin
        [Authorize(Roles = "Back Office System Administrator,Support Admin")]
        public ActionResult AdminUserIndex(Int32? id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    Initialise();
                    String applicationName;
                    String CurrentUserDepartment = db.ApplicationEntities.Find(SystemUser.DepartmentId)?.Key;
                    IQueryable<ApplicationUserRole> applicationUserRoles = context.ApplicationUserRoles.Include(a => a.SystemUser)
                                .Include(a => a.IdentityRole)
                                .Where(a => a.IsActive && a.IsDeleted == false);

                    Models.Application application = context.Applications.Where(x => x.Key == ApplicationKeys.RatesClearanceSystem).FirstOrDefault();

                    applicationUserRoles = applicationUserRoles.Include(a => a.Department).Include(a => a.SystemUser.Department).Include(a => a.CCC).Where(a => a.ApplicationId == application.Id && a.CCCId != null);
                    applicationName = application.Name;

                    if (!String.IsNullOrEmpty(CurrentUserDepartment))
                        applicationUserRoles = applicationUserRoles.Where(x => x.SystemUser.Department.Key.Equals(CurrentUserDepartment) && !String.IsNullOrEmpty(x.SystemUser.Department.Key));

                    foreach (ApplicationUserRole item in applicationUserRoles)
                    {
                        SystemIdentityUser user = UserManager.FindByName(item.SystemUser.UserName);
                        String userId = user.Id;
                        List<String> rolesArray = UserManager.GetRoles(userId).ToList();
                        String RolesList = "";
                        Int32 counter = 0;
                        foreach (String Roles in rolesArray)
                        {
                            if (counter > 0) RolesList += ", ";
                            counter++;
                            RolesList += Roles;
                        }
                        item.Data = RolesList;
                    }
                    ViewBag.ApplicationId = id;
                    ViewBag.ApplicationName = applicationName;
                    return View(applicationUserRoles.ToList());
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion


        #region ApplicationUserRole Details
        //
        // GET: /ApplicationUserRole/Details/5
        [Authorize(Roles = "Super Administrators")]
        public ActionResult Details(int id)
        {
            return View();
        }

        #endregion


        #region ApplicationUserRole Create GET
        //
        // GET: /ApplicationUserRole/Create
        [Authorize(Roles = "Super Administrators")]
        public ActionResult Create(int? id)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {

                    if (id == null) throw new Exception("Invalid id");

                    var vm = new UserAdminViewModel();
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                                 && ar.ApplicationId == id).Select(cr => cr.RoleId);
                    var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Lease Management Senior Clerk" || r.Name == "Clerks" || r.Name == "Rates Clearance Clerk").ToList();
                    var users = context.Users.Where(u => u.Roles.Count == 0).ToList();
                    var userIds = users.Select(u => u.SystemUserId).ToList();
                    var unassignedSystemUsers = context.ApplicationUserRoles.Include(s => s.SystemUser)
                                                .Where(s => userIds.Contains(s.SystemUserId)).DistinctBy(s => s.SystemUserId).ToList();
                    vm.UserRole = unassignedSystemUsers;


                    vm.ID = 1;
                    vm.Name = "Test";

                    vm.Features = new SelectList(roles, "Name", "Name");
                    //vm.SelectedFeatures = vm.Features.Select(x => x.Text);
                    ViewBag.UserId = new SelectList(unassignedSystemUsers.DistinctBy(s => s.SystemUserId).Select(u => new
                    {
                        u.SystemUserId,
                        u.SystemUser.UserName
                    }), "SystemUserId", "UserName");
                    ViewBag.RoleId = new SelectList(roles, "Name", "Name");
                    ViewBag.ApplicationId = id;

                    return View(vm);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion

        #region ApplicationUserRole Create POST
        //
        // POST: /ApplicationUserRole/Creat
        [HttpPost]
        [Authorize(Roles = "Super Administrators")]
        public ActionResult Create(FormCollection collection, UserAdminViewModel vm, params string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    var userType = Request.Form["userType"];
                    var applicationId = Convert.ToInt32(Request.Form["applicationId"]);
                    var ipAddress = Request.Form["ipAddress"];
                    var role = Request.Form["RoleId"];
                    var randomPassword = GeneratePassword(10);
                    var email = new Email();
                    var identityManager = new IdentityManager();
                    var applicationUserRole = new ApplicationUserRole
                    {
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        ApplicationId = applicationId,
                        RoleId = RoleManager.FindByName(SelectedRoles[0]).Id
                    };

                    var defaultCustomer = context.CustomerTypes.FirstOrDefault(c => c.Key == CustomerTypeKeys.Individual);
                    var defaultIdentification = context.IdentificationTypes.FirstOrDefault(id => id.Key == IdentificationTypeKey.SouthAfricanID);
                    var defaultTitleType = context.TitleTypes.FirstOrDefault(t => t.Key == TitleTypeKeys.Mister);
                    var defaultStatus = context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);

                    if (defaultCustomer == null) throw new Exception("Invalid customer type");
                    if (defaultIdentification == null) throw new Exception("Invalid identification");
                    if (defaultTitleType == null) throw new Exception("Invalid title type");
                    if (defaultStatus == null) throw new Exception("Invalid status");

                    var customer = new Customer()
                    {
                        CustomerTypeId = defaultCustomer.Id,
                        IdentificationTypeId = defaultIdentification.Id,
                        CountryOfIssueTypeId = null,
                        IdentificationNumber = "1234567891011",
                        TitleTypeId = defaultTitleType.Id,
                        FirstName = Request.Form["firstName"],
                        LastName = Request.Form["lastName"],
                        Gender = null,
                        IsDeceased = false,
                        EmailAddress = Request.Form["emailAddress"],
                        PhysicalAddressCode = 0000,
                        PostalAddressCode = 0000,
                        StatusId = defaultStatus.Id,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false
                    };

                    if (userType == "Existing User")
                    {
                        var systemUserId = Convert.ToInt32(Request.Form["UserId"]);
                        var identityUser = context.Users.FirstOrDefault(i => i.SystemUserId == systemUserId);

                        if (identityUser == null) throw new Exception("Invalid identity user");

                        //identityManager.AddUserToRole(identityUser.Id, role);

                        foreach (var item in SelectedRoles)
                        {
                            identityManager.AddUserToRole(identityUser.Id, item.ToString());
                        }
                        applicationUserRole.SystemUserId = systemUserId;
                        customer.SystemUserId = systemUserId;

                        context.ApplicationUserRoles.Add(applicationUserRole);
                        context.Customers.Add(customer);

                        var applicationAccess = context.Applications.Find(applicationId);
                        const string emailSubject = "Property Lease Management System: User Application Role";
                        var emailBody = "<b>You have been successfully added to a new system.</b><br/><br/>" +
                                "<b>Application User Role Details:</b><br/>" +
                                "Application Access: " + applicationAccess.Name + "<br/>" +
                                "Role: " + role + "<br/><br/>";

                        email.GenerateEmail(identityUser.Email,
                            emailSubject, emailBody, identityUser.SystemUserId.ToString(), false,
                            AppSettingKeys.EservicesDefaultEmailTemplate, identityUser.SystemUser.FullName);

                        context.SaveChanges();

                        return RedirectToAction("Index", new { id = applicationId });
                    }

                    var firstName = Request.Form["firstName"];
                    var surname = Request.Form["lastName"];
                    var username = Request.Form["userName"];
                    var emailAddress = Request.Form["emailAddress"];

                    var usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() == username.ToLower()
                        && u.IsActive && u.IsDeleted == false);
                    var emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() == emailAddress.ToLower()
                        && u.IsActive && u.IsDeleted == false);

                    if (!usernameAssigned && emailAssigned)
                    {
                        TempData["Error"] = "Email address registered. Please use an alternative email address";
                    }
                    else if (usernameAssigned && !emailAssigned)
                    {
                        TempData["Error"] = "Username registered. Please choose a unique username.";
                    }
                    else if (usernameAssigned && emailAssigned)
                    {
                        TempData["Error"] = "Username and Email address registered. Please use an alternative email address and a unique username.";
                    }
                    else
                    {
                        // JK.20140724a - Passing values from the ViewModel to the Model.
                        var user = new SystemIdentityUser
                        {
                            UserName = username,
                            Email = emailAddress,
                            EmailConfirmed = true,
                            SystemUser = new SystemUser()
                            {
                                FirstName = firstName,
                                LastName = surname,
                                UserName = username,
                                EmailAddress = emailAddress,
                                IsActive = true,
                                IsDeleted = false,
                                IsLocked = false,
                                ModifiedDateTime = DateTime.Now
                            }
                        };

                        // JK.20140724a - Custom profile information.

                        // Send email to User with Username and Temp Password
                        identityManager.CreateUser(user, randomPassword);


                        foreach (var item in SelectedRoles)
                        {
                            identityManager.AddUserToRole(user.Id, item.ToString());
                        }

                        applicationUserRole.SystemUserId = user.SystemUserId;
                        customer.SystemUserId = user.SystemUserId;

                        context.ApplicationUserRoles.Add(applicationUserRole);
                        context.Customers.Add(customer);

                        var applicationAccess = context.Applications.Find(applicationId);
                        const string emailSubject = "Property Lease Management System: User Registration";
                        var emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                                        "<b>Login Details:</b><br/>" +
                                        "Username: " + user.UserName + "<br/>" +
                                        "Temporary Password: " + randomPassword + "<br/>" +
                                        "Application Access: " + applicationAccess.Name + "<br/>" +
                                        "Role: " + role + "<br/><br/>" +
                                        "<b> Please change temporary password on your first login.</b>";


                        email.GenerateEmail(user.Email, emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);

                        context.SaveChanges();

                        return RedirectToAction("Index", new { id = applicationId });
                    }

                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                    && ar.ApplicationId == applicationId).Select(cr => cr.RoleId);
                    var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Administrators" || r.Name == "Clerks").ToList();
                    var users = context.Users.Where(u => u.Roles.Count == 0).ToList();
                    var userIds = users.Select(u => u.SystemUserId).ToList();

                    var unassignedSystemUsers = context.ApplicationUserRoles.Include(s => s.SystemUser)
                                .Where(s => userIds.Contains(s.SystemUserId)).DistinctBy(s => s.SystemUserId).ToList();

                    ViewBag.UserId = new SelectList(unassignedSystemUsers.DistinctBy(s => s.SystemUserId).Select(u => new
                    {
                        u.SystemUserId,
                        u.SystemUser.UserName
                    }), "SystemUserId", "UserName");

                    ViewBag.RoleId = new SelectList(roles, "Name", "Name");
                    ViewBag.ApplicationId = applicationId;

                    return View(unassignedSystemUsers);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }
        #endregion


        #region ApplicationUserRole Edit GET
        //
        // GET: /ApplicationUserRole/Edit/5
        [Authorize(Roles = "Super Administrators, Area Manager,Back Office System Administrator,Support Admin")]
        [DecryptParameter]
        public ActionResult Edit(int? id, int? appId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (id == null) throw new Exception("Invalid id");

                    var applicationuserrole = context.ApplicationUserRoles.Find(id);
                    var systemUser = context.SystemUsers.Find(applicationuserrole.SystemUserId);
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                    && ar.ApplicationId == appId).Select(cr => cr.RoleId);
                    var user = UserManager.FindByName(systemUser.UserName);
                    var userId = user.Id;
                    // get user roles
                    List<string> rolesArray = UserManager.GetRoles(userId).ToList();

                    //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application").ToList();
                    var roles = context.Roles
                                        .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                        || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                        || r.Name == "Maintenance Supervisor" || r.Name == "Regional Manager" || r.Name == "Caretaker" || r.Name == "Senior Housing Specialist").ToList();

                    var vm = new UserAdminViewModel();
                    vm.ID = 1;
                    vm.Name = "Test";

                    vm.Features = new SelectList(roles, "Name", "Name");
                    //vm.SelectedFeatures = vm.Features.Select(x => x.Text);
                    //List<string> SelectedRolesArray = new List<string>();
                    //SelectedRolesArray.Add("test");

                    //foreach (var item in vm.Features)
                    //{


                    //    if(rolesArray.Any(s => s.IndexOf(item.Text, StringComparison.CurrentCultureIgnoreCase) > -1))
                    //    {
                    //        SelectedRolesArray.Add(item.Text);


                    //    }
                    //}

                    //vm.SelectedFeatures = SelectedRolesArray;
                    var RolesList2 = roles.Select(x => new SelectListItem()
                    {
                        Selected = rolesArray.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });
                    vm.Features = RolesList2;

                    //var test = UserManager.GetRoles(systemUser)
                    //var rolesArray = Roles.GetRolesForUser(User.Identity.Name);

                    ViewBag.ApplicationId = appId;
                    ViewBag.SystemUser = systemUser;
                    ViewBag.RoleId = new SelectList(roles, "Name", "Name");

                    return View(vm);
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    return View("_Error");
                }
            }
        }
        #endregion

        #region ApplicationUserRole Edit POST
        //
        // POST: /ApplicationUserRole/Edit/5
        [HttpPost]
        [Authorize(Roles = "Super Administrators, Area Manager,Back Office System Administrator,Support Admin")]
        [DecryptParameter]
        public ActionResult Edit(int? id, FormCollection collection, params string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    var applicationId = Convert.ToInt32(Request.Form["applicationId"]);
                    var ipAddress = Request.Form["ipAddress"];
                    var email = new Email();
                    var applicationuserrole = context.ApplicationUserRoles.Include(a => a.SystemUser)
                                              .FirstOrDefault(a => a.Id == id);

                    if (applicationuserrole == null) throw new Exception("Invalid application user role");
                    var user = UserManager.FindByName(applicationuserrole.SystemUser.UserName);
                    if (user == null) throw new Exception("Invalid user");


                    var userId = user.Id;
                    List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                    var currentRole = "";

                    foreach (var item in rolesArray)
                    {
                        IdentityManager.UserManager.RemoveFromRole(user.Id, item);
                        currentRole = currentRole + item + ", ";
                    }
                    var newRole = "";
                    if (SelectedRoles != null)
                    {
                        foreach (var item in SelectedRoles)
                        {
                            IdentityManager.AddUserToRole(user.Id, item);
                            newRole = newRole + item + ", ";
                        }
                    }




                    var applicationAccess = context.Applications.Find(applicationId);
                    const string emailSubject = "Property Lease Management System: Application User Role Modified";
                    var emailBody = "<b>Your Application User Role Has Been Modified.</b><br/><br/>" +
                            "<b>Application User Role Details:</b><br/>" +
                            "Application Access: " + applicationAccess.Name + "<br/>" +
                            "Old Role: " + currentRole + "<br/>" +
                            "New Role: " + newRole + "<br/><br/>";

                    email.GenerateEmail(user.Email,
                        emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);

                    //IdentityManager.UserManager.RemoveFromRole(user.Id, currentRole);

                    if (SelectedRoles != null)
                    {
                        applicationuserrole.RoleId = RoleManager.FindByName(SelectedRoles[0]).Id;
                    }

                    applicationuserrole.IsActive = true;
                    applicationuserrole.IsDeleted = false;
                    applicationuserrole.IsLocked = false;

                    context.Entry(applicationuserrole).State = EntityState.Modified;
                    context.SaveChanges();
                    if (User.IsInRole("Back Office System Administrator") || User.IsInRole("Support Admin"))
                    {
                        return RedirectToAction("AdminUserIndex", new { id = applicationId });
                    }
                    else
                    {
                        return RedirectToAction("UserIndex", new { id = applicationId });
                    }

                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion

        #region OldEdit

        //
        // GET: /ApplicationUserRole/Edit/5
        [Authorize(Roles = "Super Administrators, Area Manager")]
        public ActionResult EditOld(int? id, int? appId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (id == null) throw new Exception("Invalid id");

                    var applicationuserrole = context.ApplicationUserRoles.Find(id);
                    var systemUser = context.SystemUsers.Find(applicationuserrole.SystemUserId);
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                    && ar.ApplicationId == appId).Select(cr => cr.RoleId);
                    var user = UserManager.FindByName(systemUser.UserName);
                    var userId = user.Id;
                    // get user roles
                    List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                    //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application").ToList();
                    var roles = context.Roles
                                            .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                            || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                            || r.Name == "Maintenance Supervisor" || r.Name == "Caretaker").ToList();
                    var vm = new UserAdminViewModel();
                    vm.ID = 1;
                    vm.Name = "Test";

                    vm.Features = new SelectList(roles, "Name", "Name");
                    //vm.SelectedFeatures = vm.Features.Select(x => x.Text);
                    //List<string> SelectedRolesArray = new List<string>();
                    //SelectedRolesArray.Add("test");

                    //foreach (var item in vm.Features)
                    //{


                    //    if(rolesArray.Any(s => s.IndexOf(item.Text, StringComparison.CurrentCultureIgnoreCase) > -1))
                    //    {
                    //        SelectedRolesArray.Add(item.Text);


                    //    }
                    //}

                    //vm.SelectedFeatures = SelectedRolesArray;
                    var RolesList2 = roles.Select(x => new SelectListItem()
                    {
                        Selected = rolesArray.Contains(x.Name),
                        Text = x.Name,
                        Value = x.Name
                    });
                    vm.Features = RolesList2;

                    //var test = UserManager.GetRoles(systemUser)
                    //var rolesArray = Roles.GetRolesForUser(User.Identity.Name);

                    ViewBag.ApplicationId = appId;
                    ViewBag.SystemUser = systemUser;
                    ViewBag.RoleId = new SelectList(roles, "Name", "Name");

                    return View(vm);
                }
                catch (Exception IO)
                {
                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    return View("_Error");
                }
            }
        }



        //
        // POST: /ApplicationUserRole/Edit/5
        [HttpPost]
        [Authorize(Roles = "Super Administrators, Area Manager")]
        public ActionResult EditOld(int? id, FormCollection collection, params string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);

                    var applicationId = Convert.ToInt32(Request.Form["applicationId"]);
                    var ipAddress = Request.Form["ipAddress"];
                    var email = new Email();
                    var applicationuserrole = context.ApplicationUserRoles.Include(a => a.SystemUser)
                                              .FirstOrDefault(a => a.Id == id);

                    if (applicationuserrole == null) throw new Exception("Invalid application user role");
                    var user = UserManager.FindByName(applicationuserrole.SystemUser.UserName);
                    if (user == null) throw new Exception("Invalid user");


                    var userId = user.Id;
                    List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                    var currentRole = "";

                    foreach (var item in rolesArray)
                    {
                        IdentityManager.UserManager.RemoveFromRole(user.Id, item);
                        currentRole = currentRole + item + ", ";
                    }
                    var newRole = "";
                    foreach (var item in SelectedRoles)
                    {
                        IdentityManager.AddUserToRole(user.Id, item);
                        newRole = newRole + item + ", ";
                    }



                    var applicationAccess = context.Applications.Find(applicationId);
                    const string emailSubject = "Property Lease Management System: Application User Role Modified";
                    var emailBody = "<b>Your Application User Role Has Been Modified.</b><br/><br/>" +
                            "<b>Application User Role Details:</b><br/>" +
                            "Application Access: " + applicationAccess.Name + "<br/>" +
                            "Old Role: " + currentRole + "<br/>" +
                            "New Role: " + newRole + "<br/><br/>";

                    email.GenerateEmail(user.Email,
                        emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);

                    //IdentityManager.UserManager.RemoveFromRole(user.Id, currentRole);


                    applicationuserrole.RoleId = RoleManager.FindByName(SelectedRoles[0]).Id;
                    applicationuserrole.IsActive = true;
                    applicationuserrole.IsDeleted = false;
                    applicationuserrole.IsLocked = false;

                    context.Entry(applicationuserrole).State = EntityState.Modified;
                    context.SaveChanges();

                    return RedirectToAction("Index", new { id = applicationId });
                }
                catch
                {
                    return View("_Error");
                }
            }
        }

        #endregion

        #region ApplicationUserRole Delete GET
        //
        // GET: /ApplicationUserRole/Delete/5
        [Authorize(Roles = "Super Administrators, Area Manager,Back Office System Administrator,Support Admin")]
        [DecryptParameter]
        public ActionResult Delete(int? id, int? appId)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    if (id == null) throw new Exception("Invalid id");
                    var applicationuserrole = context.ApplicationUserRoles.Include(l => l.SystemUser).Include(l => l.IdentityRole).Include(l => l.Application).Where(l => l.Id == id).FirstOrDefault();

                    if (applicationuserrole == null) throw new Exception("Invalid application user role");

                    var Message = TempData["RoundRobinRedistributionTitle"];
                    var Title = TempData["RoundRobinRedistribution"];
                    if (Message != null && Title != null)
                    {
                        ViewBag.MessageTitle = TempData["RoundRobinRedistributionTitle"].ToString();
                        ViewBag.Message = TempData["RoundRobinRedistribution"].ToString();
                    }

                    var user = UserManager.FindByName(applicationuserrole.SystemUser.UserName);
                    var userId = user.Id;
                    // get user roles
                    List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                    string RolesList = "";
                    int counter = 0;
                    foreach (var Roles in rolesArray)
                    {
                        if (counter > 0)
                        {
                            RolesList += ", ";
                        }
                        counter++;
                        RolesList += Roles;
                    }
                    applicationuserrole.Data = RolesList;




                    ViewBag.ApplicationId = appId;
                    return View(applicationuserrole);
                }
                catch
                {
                    return View("_Error");
                }
            }
        }
        #endregion

        #region ApplicationUserRole Delete POST
        //
        // POST: /ApplicationUserRole/Delete/5
        [HttpPost]
        [Authorize(Roles = "Super Administrators, Area Manager,Back Office System Administrator,Support Admin")]
        [DecryptParameter]
        public ActionResult Delete(int? id, FormCollection collection)
        {

            try
            {
                Initialise();

                var applicationId = Convert.ToInt32(Request.Form["applicationId"]);
                var email = new Email();

                if (id == null) throw new Exception("Invalid id");

                var applicationuserrole = db.ApplicationUserRoles.Find(id);
                var user = db.Users.First(u => u.SystemUserId == applicationuserrole.SystemUserId);
                //Start of reallocate cases 
                #region ReallocateCases




                var Keys = db.Status;
                int SubmittedId = Keys.Where(x => x.Key == StatusKeys.Submitted).FirstOrDefault().Id;

                var SysUserID = user.SystemUserId;
                var Clerk = db.Customers.Where(x => x.SystemUserId == SysUserID).FirstOrDefault();
                var rrqList = db.RoundRobinQueues.Include(x => x.Clerk).Include(x => x.ResponsibilityType).Include(x => x.Clerk.SystemUser).Include(x => x.RefundApplication).Include(x => x.RCSApplicationStatus).Where(x => x.ClerkId == Clerk.Id && x.StatusId == SubmittedId).ToList();
                AesCrypto AES = new AesCrypto();
                var q = AES.Encrypt("id=" + Convert.ToInt16(id) + "&appId=" + Convert.ToInt16(applicationId));
                foreach (var item in rrqList)
                {

                    DepartmentsApprovalsController DA = new DepartmentsApprovalsController();
                    CaptureController c = new CaptureController();
                    switch (item.ResponsibilityType.Key)
                    {
                        case (ResponsibilityTypeKeys.AcknowledgeRCSApplication):
                            {
                                var SystUserId = c.RoundRobinRedistribution(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);
                                //var SystUserId = c.RoundRobinCCC(true, false, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, false, false, 0);
                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;


                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);


                                }
                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                        case (ResponsibilityTypeKeys.SubmitFigures):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, true, false, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                        case (ResponsibilityTypeKeys.IssueCertificates):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, true, false, false, false, false, false, Convert.ToInt16(item.RCSApplicationStatusId), 0, item.Id, false, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                        case (ResponsibilityTypeKeys.Billing):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, false, true, false, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                        case (ResponsibilityTypeKeys.CreditControl):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, true, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                        case (ResponsibilityTypeKeys.SundryAccount):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, true, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                        case (ResponsibilityTypeKeys.Rates):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, true, false, false, false, 0, Convert.ToInt16(item.DepartmentApprovalId), item.Id, false, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAudit(Convert.ToInt16(item.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }

                        case (ResponsibilityTypeKeys.AcknowledgeRefundApplication):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, true, false);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }

                        case (ResponsibilityTypeKeys.IssueRefundCollection):
                            {
                                var SystUserId = c.RoundRobinRedistribution(false, false, false, false, false, false, false, false, Convert.ToInt16(item.RefundApplicationId), 0, item.Id, false, true);

                                if (SystUserId == 0)
                                {
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionUnsuccessful).Description.ToString();

                                    var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);
                                    var FailureMessage = db.RCSUserMessages.Where(x => x.Key == RCSUserMessageKeys.GenericRedistrubutionFailMessage).FirstOrDefault();

                                    TempData["RoundRobinRedistributionTitle"] = FailureMessage.Title;
                                    TempData["RoundRobinRedistribution"] = FailureMessage.Body + item.ResponsibilityType.Name;

                                    return RedirectToAction("Delete", new { q = q });
                                }
                                else
                                {
                                    var ClerkId = db.Customers.Where(x => x.SystemUserId == SystUserId).FirstOrDefault();
                                    var ActivityTrackerMessage = db.ActivityTrackerMessages.FirstOrDefault(x => x.Key == ActivityTrackerMessageKeys.RedistributionSuccessful).Description.ToString() + " " + ClerkId.FullName;

                                    var Result = DA.ActivityTrackerAuditRefunds(Convert.ToInt16(item.RefundApplicationId), Convert.ToInt16(item.RefundApplication.RCSApplicationStatusId), ActivityTrackerMessage, Customer.Id);

                                }

                                item.StatusId = Keys.Where(x => x.Key == StatusKeys.Archived).FirstOrDefault().Id;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                    }






                }





                #endregion

                //End of reallocate cases




                user.RoundRobinIsActive = false;
                user.isDeleted = true;
                var currentRole = RoleManager.FindById(applicationuserrole.RoleId).Name;
                var applicationAccess = db.Applications.Find(applicationId);

                var userId = user.Id;
                List<string> rolesArray = UserManager.GetRoles(userId).ToList();

                string RolesList = "";
                int counter = 0;


                foreach (var item in rolesArray)
                {
                    if (counter > 0)
                    {
                        RolesList += ", ";
                    }
                    counter++;
                    RolesList += item;
                    IdentityManager.UserManager.RemoveFromRole(user.Id, item);

                }
                const string emailSubject = "Property Lease Management System: Application User Role Revoked";
                var emailBody = "<b>Your access to " + applicationAccess.Name + " has been revoked .</b><br/><br/>" +
                        "<b>Application User Role Details:</b><br/>" +
                        "Application Access Revoked: " + applicationAccess.Name + "<br/>" +
                        "Role: " + RolesList + "<br/><br/>" +
                        "<b>Please contact the administrator for further details.</b>";

                email.GenerateEmail(user.Email,
                    emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate,
                    user.SystemUser.FullName);


                //IdentityManager.UserManager.RemoveFromRole(user.Id, currentRole);

                applicationuserrole.IsActive = false;
                applicationuserrole.IsDeleted = true;
                applicationuserrole.IsLocked = false;

                db.Entry(applicationuserrole).State = EntityState.Modified;
                db.SaveChanges();
                if (User.IsInRole("Back Office System Administrator") || User.IsInRole("Support Admin"))
                {
                    return RedirectToAction("AdminUserIndex", new { id = applicationId });
                }
                else
                {
                    return RedirectToAction("UserIndex", new { id = applicationId });
                }
            }
            catch
            {
                return View("_Error");
            }

        }
        #endregion

        #region Generate User Password
        public string GeneratePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = new StringBuilder();
            var rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        #endregion
    }
}
