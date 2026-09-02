using System;
using System.Data.Entity;
using System.Data.Linq;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Helpers;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.Routing;
using System.DirectoryServices.AccountManagement;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Security.Claims;
using System.Threading;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        //private readonly eServicesDbContext _context = new eServicesDbContext();
        BaseHelper _base = new BaseHelper();
        private static int _loginId;
        public eServicesDbContext _context;

        public AccountController()
            : this(new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(new eServicesDbContext())))
        {
            //allow alphanumeric usernames  
            //UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        }

        public AccountController(UserManager<SystemIdentityUser> userManager)
        {
            UserManager = userManager;
            var provider = new DpapiDataProtectionProvider("eServices");
            UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
            UserManager.UserTokenProvider = new DataProtectorTokenProvider<SystemIdentityUser>(
                                            provider.Create("EmailConfirmation"))
            {
                TokenLifespan = TimeSpan.FromDays(14)
            };
        }
        //public AccountController(UserManager<SystemIdentityUser> userManager)
        //{
        //    UserManager = userManager;

        //    //S.M change the provider encryption to the IDataProtectionProvider in Startup_Auth
        //    var provider = Startup.DataProtectionProvider;
        //    UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
        //    UserManager.UserTokenProvider = new DataProtectorTokenProvider<SystemIdentityUser>(
        //                                    provider.Create("EmailConfirmation"))

        //    {
        //        TokenLifespan = TimeSpan.FromDays(14)
        //    };
        //}

        public AccountController(eServicesDbContext context)
        {
            eServicesDbContext _context = new eServicesDbContext();
            UserManager =
                new UserManager<SystemIdentityUser>(
                    new UserStore<SystemIdentityUser>(_context));
        }

        public UserManager<SystemIdentityUser> UserManager { get; private set; }

        #region Account Login GET
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //#EHCSpec Code
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("RCSHome", "Home");
            }
            //var attApi = C8.eServices.Mvc.ApiServices.RCSAttorneyApi.GetAttorneyDetails("10IDPT");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        #endregion
        #region Account Login POST
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                //incremental delay to prevent brute force attacks
                int incrementalDelay;
                var error = "";

                if (HttpContext.Application[Request.UserHostAddress] != null)
                {
                    // wait for delay if there is one
                    incrementalDelay = (int)HttpContext.Application[Request.UserHostAddress];
                    await Task.Delay(incrementalDelay * 1000);
                }

                if (ModelState.IsValid)
                {
                    var store = new UserStore<SystemIdentityUser>(context);
                    var UserManager = new UserManager<SystemIdentityUser>(store);
                    UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
                    // var user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                    var user = new SystemIdentityUser();
                    ADLogin adLogin = new ADLogin();
                    var usersr = context.SystemUsers.ToList();
                    var test = context.AppSettings.Where(x => x.Key == "active_directory_domain").FirstOrDefault();
                    var test33 = context.AppSettings.Where(x => x.Id == 1180).FirstOrDefault();

                    var activeDirectoryOn = context.AppSettings.Where(x => x.Key == AppSettingKeys.ActiveDirectoryActive).FirstOrDefault().Value;
                    var activeDirectoryDomain = context.AppSettings.Where(x => x.Key == AppSettingKeys.activeDirectoryDomain).FirstOrDefault().Value;

                    var Ad = false;
                    //var activeDirectoryOn = "True";
                    if (activeDirectoryOn == "True")
                    {
                        try
                        {
                            user = await UserManager.FindByNameAsync(model.UserName.Trim());

                            if (user != null)
                            {

                                if (user.isInternalUser == true && user.isActiveDirectoryUser == true)
                                {
                                    var Domain = new PrincipalContext(ContextType.Domain, activeDirectoryDomain);

                                    if (Domain != null)
                                    {
                                        //model.UserName = "Xetgroup";
                                        //model.Password = "/*@VPN2020!@#$*/";
                                        //model.UserName = "SCM_user";
                                        //model.Password = "@Eku2020$";

                                        Ad = adLogin.ValidateUser(model.UserName, model.Password);
                                        if (Ad)
                                        {
                                            //user = await UserManager.FindByNameAsync(model.UserName.Trim());
                                            //if (user == null)
                                            //{
                                            //    //if we decide to add users if they pass AD but fail local ,  Put code to insert user here  
                                            //    error = "Active Directory account exisits, local access to be granted. Contact System Admin";
                                            //    ModelState.AddModelError("", "Active Directory account exisits, local access to be granted. Contact System Admin.");
                                            //    ViewBag.Error = error;
                                            //}
                                            //else
                                            //{
                                            //Sets AD Internal User Session
                                            Session["IsInternalUser"] = true;
                                            //}
                                        }
                                        else
                                        {
                                            error = "Incorrect Active Directory username or password";
                                            ModelState.AddModelError("", "Invalid Active Directory username or password.");
                                            ViewBag.Error = error;
                                            return View(model);
                                        }
                                    }
                                }
                                else
                                {

                                    var Counter = user.AccessFailedCount;
                                    if (Counter > 20)
                                    {
                                        ViewBag.Error = "Your Account has been locked due to the wrong password entered multiple time, Please Use The Forgot Password To Unlock Your Account.";
                                        AuthenticationManager.SignOut();
                                        return View(model);
                                    }
                                    else
                                    {
                                        user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                                        if (user == null)
                                        {
                                            var user3 = await UserManager.FindByNameAsync(model.UserName.Trim());

                                            //user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim());
                                            user3.AccessFailedCount = user3.AccessFailedCount + 1;
                                            await UserManager.UpdateAsync(user3);
                                            context.SaveChanges();
                                        }


                                    }
                                }

                            }
                            else
                            {
                                user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                            }
                        }
                        catch (PrincipalServerDownException ex)
                        {
                            EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                            // show your error message
                            user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                        }
                    }

                    //Testing on Dev/QA
                    else
                    {
                        //var UserName = "ctadmin";
                        //var Password = "second1#";
                        user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                    }
                    //Uncomment below code to bypass login with user's password 
                    //var user = await UserManager.FindByNameAsync(model.UserName.Trim());

                    if (user != null)
                    {




                        //Checks if account is deactivated
                        if (user.SystemUser.IsLocked == true && user.SystemUser.IsDeleted == true)
                        {

                            ViewBag.Error = "Your Account Is Deactivated, Please Contact The System Administrator To Re-Activate Your Account.";
                            AuthenticationManager.SignOut();
                            return View(model);
                        }
                        AppSetting LockOut = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AccountLock);
                        //Checks if account is locked
                        if ((user.SystemUser.IsLocked == true && user.SystemUser.IsActive == true && user.SystemUser.IsDeleted == false) && (LockOut.IsActive == true))
                        {

                            ViewBag.Error = "Your Account has been locked due to suspicious activity, Please Contact The System Administrator To Unlock Your Account.";
                            AuthenticationManager.SignOut();
                            return View(model);
                        }


                        var applicationUser = context.ApplicationUserRoles.FirstOrDefault(a => a.SystemUserId == user.SystemUserId);

                        //if (applicationUser != null)
                        //{
                        //    var ipAddressRange = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.IpAddressRange);
                        //    if (ipAddressRange == null) throw new Exception("Invalid appsetting");

                        //    if (!HttpContext.Request.UserHostAddress.StartsWith(ipAddressRange.Value))
                        //    {
                        //        AuthenticationManager.SignOut();
                        //        ViewBag.Error = "Unauthorized external access. If you have any questions, please contact the website administrator.";
                        //        return View(model);
                        //    }
                        //}



                        // JK.20190623a - Using the user.emailconfirm to also validate the phone number, but email always takes preference.
                        if (user.EmailConfirmed == false && string.IsNullOrWhiteSpace(user.UnconfirmedEmail))
                        {
                            var guid = user.Id;
                            AuthenticationManager.SignOut();
                            ViewBag.EncodedUserId = guid;
                            ViewBag.EncodedSystemUserId = user.SystemUserId;
                            ViewBag.Data = user.SystemUserId;

                            if (user.SystemUser.EmailAddress != null)
                            {
                                ViewBag.Status = "Warning";
                                return View("_EmailActivated");
                            }
                            else
                                return RedirectToAction("ActivateMobile", "Account", new { id = guid });

                        }
                        if (user.SystemUser.IsLocked != null && (!user.SystemUser.IsPasswordReset && user.SystemUser.IsTemporaryPassword && (bool)user.SystemUser.IsLocked))
                        {
                            AuthenticationManager.SignOut();
                            error = "Temporary password expired. Please click forgot password link to reset password.";
                            ModelState.AddModelError("", "Temporary password expired. Please click forgot password link to reset password.");
                        }
                        else
                        {
                            var email = new Email();
                            var sms = new CesarSMS();

                            var statusIdsms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                            var systemUser = context.SystemUsers.FirstOrDefault(s => s.Id == user.SystemUserId);

                            //reset incremental delay on successful login
                            if (HttpContext.Application[Request.UserHostAddress] != null)
                            {
                                HttpContext.Application.Remove(Request.UserHostAddress);
                            }

                            //Uncomment below code once system is live
                            //user's email address must be verified before using eServices
                            var systemUserLogTime = new SystemUserLogTime
                            {
                                SystemUserId = user.SystemUserId,
                                LoginTime = DateTime.Now,
                                SessionId = HttpContext.Session.SessionID,
                                IPAddress = HttpContext.Request.UserHostAddress
                            };

                            //email login notification
                            if (systemUser != null)
                                email.GenerateEmail(systemUser.EmailAddress, "Property Lease Management System Login",
                                    "Ekurhuleni Municipality Property Lease Management System login notification on " + String.Format("{0:F}", DateTime.Now),
                                    systemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, systemUser.FullName);
                            if (systemUser != null && systemUser.EmailAddress == null && systemUser.MobileNumber != null)
                                sms.GenerateSMS(systemUser.MobileNumber,
                                   "Ekurhuleni Municipality Property Lease Management System login notification on " + String.Format("{0:F}", DateTime.Now),
                                   systemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdsms, systemUser.FullName);

                            // SmsHelper.Send(systemUser.MobileNumber, "Property Lease Management System login notification on " + String.Format("{0:F}", DateTime.Now));

                            context.SystemUserLogTimes.Add(systemUserLogTime);

                            // Audit Trail: Log successful login
                            AuditTrailHelper.LogLogin(user.SystemUserId, true);
                            try
                            {
                                var identityManager = new IdentityManager();
                                if (user.isInternalUser == false)
                                {
                                    if (!UserManager.IsInRole(user.Id, "Customers"))
                                    {
                                        identityManager.AddUserToRole(user.Id, "Customers");
                                    }
                                }
                            }
                            catch (Exception IO)
                            {
                                EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                                var err = IO.Message;
                            }



                            //if (!user.SystemUser.IsPasswordReset && user.SystemUser.IsTemporaryPassword)
                            //{
                            //    user.SystemUser.IsActive = true;
                            //    user.SystemUser.IsDeleted = false;
                            //    user.SystemUser.IsLocked = true;
                            //}
                            user.AccessFailedCount = 0;

                            await UserManager.UpdateAsync(user);
                            context.SaveChanges();

                            _loginId = systemUserLogTime.Id;
                            Session["Username"] = user.UserName;
                            Session["Password"] = model.Password.Trim();

                            var SystemUserNow = context.SystemUsers.Include(e => e.Department).FirstOrDefault(x => x.Id == user.SystemUser.Id);
                            Session["DepartmentEntity"] = SystemUserNow.Department?.Key;

                            //LF.20150316 - Force to change password on the first login.
                            if (user.SystemUser.IsPasswordReset == false)
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Manage", "Account");
                            }

                            if (UserManager.IsInRole(user.Id, "System Administrators"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Index", "Home");
                            }
                            if (UserManager.IsInRole(user.Id, "Internal Registration"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("InternalRegister", "Account");
                            }
                            if (UserManager.IsInRole(user.Id, "Area Manager"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Index", "AreaManager");
                            }
                            if (UserManager.IsInRole(user.Id, "Acknowledge RCS Application"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("PropertyLeaseApplications", "PropertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Submit Figures"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("AssessmentFiguresDashboard", "DepartmentsApprovals");
                            }
                            if (UserManager.IsInRole(user.Id, "Issue Certificate"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("RCCertificateDashboard", "DepartmentsApprovals");
                            }
                            if (UserManager.IsInRole(user.Id, "Billing"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Rates"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Credit Control"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("CreditDepartmentDashboard", "DepartmentsApprovals");
                            }
                            if (UserManager.IsInRole(user.Id, "Sundry Account"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Issue Refunds Collection"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Acknowledge Refund Application"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("RefundAcknowledgementDashboard", "DepartmentsApprovals");
                            }
                            if (UserManager.IsInRole(user.Id, "Rates Clearance Senior Clerk"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                //return RedirectToAction(secur)
                                return RedirectToAction("Index", "Customer", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { id = 0 })));
                                return RedirectToAction("Index", "DepartmentsApprovals");
                            }
                            if (UserManager.IsInRole(user.Id, "Rates Clearance Sectional Head"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                //return RedirectToAction(secur)
                                return RedirectToAction("AcknowledgementDashboard", "DepartmentsApprovals");
                                return RedirectToAction("Index", "DepartmentsApprovals");
                            }
                            if (UserManager.IsInRole(user.Id, "Support Admin"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                //return RedirectToAction(secur)
                                return RedirectToAction("ViewAllCases", "DepartmentsApprovals");
                                return RedirectToAction("Index", "DepartmentsApprovals");
                            }
                            if (user.UserName == "SashenMoodley")
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("CaptureWalkIn", "Capture");
                            }
                            if (UserManager.IsInRole(user.Id, "Community Development Officer"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("PropertyLeaseTenantTraining", "PropertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Housing Supervisor"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("PropertyLeaseInspections", "PropertyLeaseApplication");
                            }
                            if ((UserManager.IsInRole(user.Id, "Lease Official")) || (UserManager.IsInRole(user.Id, "Letting Officer")) || (UserManager.IsInRole(user.Id, "Client Services Officer")))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("UpdateLeaseDetails", "PropertyLeaseApplication");
                            }
                            if ((UserManager.IsInRole(user.Id, "Property Manager")) || (UserManager.IsInRole(user.Id, "Revenue Manager")))
                            {
                                await SignInAsync(user, model.RememberMe);
                                if (user.UserName.Equals("re_property_officer", StringComparison.OrdinalIgnoreCase))
                                {
                                    return RedirectToAction("Index", "RealEstateAdmin");
                                }
                                return RedirectToAction("PropertyLeaseAgreements", "PropertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Revenue Officer"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("PropertyLeaseDeposit", "PropertyLeaseApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Housing Liaison Officer"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Dashboard", "HumanSettlementApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Senior Housing Specialist"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Dashboard", "HumanSettlementApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Regional Manager"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Dashboard", "HumanSettlementApplication");
                            }
                            if (UserManager.IsInRole(user.Id, "Back Office System Administrator") || UserManager.IsInRole(user.Id, "Super Administrators"))
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("AdminInbox", "AreaManager");
                            }
                            else
                            {
                                await SignInAsync(user, model.RememberMe);
                                return RedirectToAction("Dashboard", "Profile");
                                //return RedirectToAction("Capture", "Capture");
                            }
                        }
                    }
                    else
                    {
                        //login failed
                        //increment the delay on failed attempt
                        if (HttpContext.Application[Request.UserHostAddress] == null)
                        {
                            incrementalDelay = 1;
                        }
                        else
                        {
                            incrementalDelay = (int)HttpContext.Application[Request.UserHostAddress] * 2;
                        }

                        HttpContext.Application[Request.UserHostAddress] = incrementalDelay;

                        error = "Invalid username/email or password";
                        ModelState.AddModelError("", "Invalid username/email or password.");

                        // Audit Trail: Log failed login attempt
                        try
                        {
                            using (var auditDb = new eServicesDbContext())
                            {
                                var failedUser = auditDb.SystemUsers.FirstOrDefault(u => u.UserName == model.UserName.Trim());
                                if (failedUser != null)
                                    AuditTrailHelper.LogLogin(failedUser.Id, false, "Invalid username or password");
                            }
                        }
                        catch { }
                    }
                }

                // If we got this far, something failed, redisplay form
                ViewBag.Error = error;
                return View(model);
            }
        }
        #endregion

        #region Activate Mobile GET

        [AllowAnonymous]
        public ActionResult ActivateMobile(string id)
        {
            using (var context = new eServicesDbContext())
            {
                ViewBag.Data = id;

                ActivateViewModel avm = new ActivateViewModel()
                {
                    Data = id
                };

                return View(avm);
            }
        }

        #endregion

        #region Activate Mobile POST

        // POST: /Account/ActivateCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateMobile(ActivateViewModel avm)
        {
            using (var context = new eServicesDbContext())
            {
                var identityUser = UserManager.FindById(avm.Data);
                if (identityUser == null) throw new Exception("Invalid identity user");

                var sysUser = context.SystemUsers.FirstOrDefault(o => o.Id == identityUser.SystemUserId);
                var emailConfirmationCode = UserManager.GenerateEmailConfirmationToken(identityUser.Id);

                if (identityUser.SystemUser.Code == avm.Code)
                {
                    sysUser.IsActive = true;
                    sysUser.IsDeleted = false;
                    sysUser.IsLocked = false;
                    sysUser.IsPasswordReset = true;
                    sysUser.IsTemporaryPassword = false;
                    context.Entry(sysUser).State = EntityState.Modified;

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception x)
                    {
                        EventLogHelper.LogSystemError(x.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                        ViewBag.Status = "Error";
                        return View();
                    }
                }
                else
                {

                    ViewBag.Status = "Warning";
                    ViewBag.EncodedSystemUserId = avm.Data;
                    ViewBag.Data = avm.Data;
                    return View();
                }

                if (!identityUser.EmailConfirmed)
                {
                    var result = await UserManager.ConfirmEmailAsync(identityUser.Id, emailConfirmationCode);

                    if (result.Succeeded)
                    {
                        await SignInAsync(identityUser, true);
                        return RedirectToAction("Index", "Profile");
                    }

                    // JK.20190621a - TODO: Switch from using the email confirmation to phone confirmation.
                    if (result.Succeeded && !string.IsNullOrEmpty(identityUser.UnconfirmedEmail))
                    {
                        UpdateCustomerEmail(identityUser.SystemUserId, identityUser.UnconfirmedEmail);
                    }
                }

                return RedirectToAction("Login");
}
        }
        #endregion

        private void PopulateRegistrationViewBags(eServicesDbContext context)
        {
            var departments = context.ApplicationEntities
                .Where(e => e.Key == ApplicationEntityKeys.EkurhuleniHousingCompany || e.Key == ApplicationEntityKeys.RealEstateDevelopment)
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name })
                .ToList();
            ViewBag.Departments = departments;

            var notificationTypes = context.NotificationTypes
                .Select(n => new SelectListItem { Value = n.Id.ToString(), Text = n.Name })
                .ToList();
            ViewBag.NotificationTypes = notificationTypes;
        }

        #region Account Register GET
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            using (var context = new eServicesDbContext())
            {
                PopulateRegistrationViewBags(context);
            }
            return View();
        }
        #endregion
        #region Account Register POST
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    PopulateRegistrationViewBags(context);

                    var captchaHelper = new CaptchaHelper();
                    var captchaResponse = Request.Params["g-recaptcha-response"];
                    var RecaptchaResult = captchaHelper.ValidateCaptcha(captchaResponse);
                    if (ModelState.IsValid /*&& !RecaptchaResult*/)
                    {
                        var Entity = context.ApplicationEntities;
                        var RegisterCheck = new Log
                        {
                            LogEntry = "Entered Valid Model If" + model.UserName,
                            LogTypeId = 9,
                            ReferenceId = 5,
                            ReferenceTypeId = 12,
                            DepartmentId = Entity.FirstOrDefault(r => r.Key == ApplicationEntityKeys.AllEntities)?.Id
                        };

                        context.Logs.Add(RegisterCheck);
                        context.SaveChanges();
                        var usernameAssigned = false;
                        var emailAssigned = false;
                        var mobileAssigned = false;

                        usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() == model.UserName.ToLower() && u.IsActive && u.IsDeleted == false);
                        if (model.EmailAddress != null)
                            emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower() && u.IsActive && u.IsDeleted == false);
                        if (model.MobileNumber != null)
                            mobileAssigned = context.SystemUsers.Any(u => u.MobileNumber == model.MobileNumber && u.IsActive && !u.IsDeleted);
                        var response = "";

                        if (model.EmailAddress == null && model.MobileNumber == null)
                        {
                            response = "The email address and mobile number are both invalid. Either one has to be valid.";
                        }

                        else if (usernameAssigned)
                        {
                            response = "The username entered is already taken.";
                        }
                        else if (emailAssigned)
                        {
                            response = "The email address entered is already in use.";
                        }
                        else if (mobileAssigned)
                        {
                            response = "The mobile number entered is already in use.";
                        }
                        else
                        {
                            // JK.20140724a - Passing values from the ViewModel to the model.
                            // JK.20190623a - Sort out proper authentication and vreification on mobile/ phone number (PhoneNumber = model.MobileNumber, PhoneNumberConfirmed = true).
                            //              - Currently using the customer code generated in the systemuser.code.

                            var user = new SystemIdentityUser { UserName = model.UserName, Email = model.EmailAddress, PhoneNumber = model.MobileNumber, PhoneNumberConfirmed = true };
                            var identityManager = new IdentityManager();
                            // JK.20190620a - Code used for mobile users.
                            var code = PasswordGenerator.GeneratePassword(true, true, true, false, false, 6);
                            
                            // JK.20140724a - Custom profile information.
                            user.SystemUser = new SystemUser
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IdentificationNumber = model.IdentificationNumber,
                                UserName = model.UserName,
                                Designation = model.Designation,
                                CompanyName = model.CompanyName,
                                EmailAddress = model.EmailAddress,
                                MobileNumber = model.MobileNumber,
                                IsActive = true,
                                IsDeleted = false,
                                IsLocked = false,
                                CreatedDateTime = DateTime.Now,
                                ModifiedDateTime = DateTime.Now,
                                IsPasswordReset = true,
                                Code = code,
                                NotificationTypeId = model.NotificationTypeId,
                                DepartmentId = model.DepartmentId
                            };

                            var result = await UserManager.CreateAsync(user, model.Password);
                            identityManager.AddUserToRole(user.Id, "Customers");

                            if (result.Succeeded)
                            {

                                Wso2Helper.Store(user.SystemUserId, model.Password);
                                if (model.EmailAddress != null && !model.EmailAddress.Trim().Equals(string.Empty))
                                {
                                    //Confirmation email
                                    var confirmationEmail = new Email();
                                    var emailConfirmationCode =
                                        await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                    emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
                                    const string subject = "Property Lease Management System: Email Confirmation";
                                    var body = string.Format("Thank you for your registration, please click on the below link to complete your registration: <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a>", Url.Action("ConfirmEmail", "Account", SecureActionLinkExtension.Encrypt(new { id = user.Id, token = emailConfirmationCode }), HttpContext.Request.Url.Scheme));
                                    confirmationEmail.GenerateEmail(user.SystemUser.EmailAddress, subject, body, user.SystemUserId.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);
                                }

                                if (model.MobileNumber != null && !model.MobileNumber.Trim().Equals(string.Empty))
                                {
                                    try
                                    {
                                        var smsConfirmation = new CesarSMS();
                                        var cxt = new eServicesDbContext();
                                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                        smsConfirmation.GenerateSMS(user.SystemUser.MobileNumber, string.Format("Welcome to the CoE Property Lease Management System, please confirm your account with the following code {0}.", code),
                                            user.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, user.SystemUser.FullName);
                                        //SmsHelper.Send(model.MobileNumber, string.Format("Welcome to Property Lease Management System, please confirm your account with the following code {0}.", code));
                                    }
                                    catch (Exception x)
                                    {
                                        EventLogHelper.LogSystemError(x.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                                    }

                                }

                                response = "Success";
                                ViewBag.EmailAddress = model.EmailAddress == null || model.EmailAddress.Trim() == string.Empty ? model.MobileNumber : model.EmailAddress;
                            }
                            else
                            {
                                AddErrors(result);
                            }

                            ViewBag.Response = response;
                            return View();
                        }

                        ModelState.AddModelError("", response);
                    }
                    else
                    {
                        var Recaptchacheck = new Log
                        {
                            LogEntry = RecaptchaResult + "  " + model.UserName,
                            LogTypeId = 9,
                            ReferenceId = 5,
                            ReferenceTypeId = 12
                        };

                        context.Logs.Add(Recaptchacheck);
                        context.SaveChanges();

                    }
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw ex;
                    //return View("_Error");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }
        #endregion


        #region Account Internal Register GET
        //
        // GET: /Account/Register
        [Authorize(Roles = "Internal Registration")]
        public ActionResult InternalRegister()
        {
            using (var context = new eServicesDbContext())
            {
                var CCCs = context.CCCTypes.OrderBy(x => x.Name).ToList();
                var vm = new InternalRegisterViewModel();
                vm.CCCTypes = new SelectList(CCCs, "Id", "Name");
                ViewBag.CCCTypes = new SelectList(CCCs, "Id", "Name");
                var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                              && ar.Application.Key == ApplicationKeys.RatesClearanceSystem).Select(cr => cr.RoleId);
                //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application" || r.Name == "Rates Clearance Sectional Head" || r.Name == "Support Admin" || r.Name == "Support Clerk").ToList();
                var roles = context.Roles
                                        .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                        || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                        || r.Name == "Maintenance Supervisor" || r.Name == "Caretaker").ToList(); vm.Features = new SelectList(roles, "Name", "Name");
                return View(vm);
            }

        }
        #endregion
        #region Account Internal Register POST
        //
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "Internal Registration")]
        [ValidateAntiForgeryToken]
        public ActionResult InternalRegister(InternalRegisterViewModel model, string[] SelectedRoles)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var currentRoleApplication = context.ApplicationRoles.Where(ar => ar.IsActive && ar.IsDeleted == false
                                       && ar.Application.Key == ApplicationKeys.RatesClearanceSystem).Select(cr => cr.RoleId);
                    //var roles = context.Roles.Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Issue Refunds Collection" || r.Name == "Acknowledge Refund Application" || r.Name == "Area Manager" || r.Name == "Internal Registration" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Issue Certificate" || r.Name == "Submit Figures" || r.Name == "Rates Clearance Senior Clerk" || r.Name == "Rates" || r.Name == "Billing" || r.Name == "Sundry Account" || r.Name == "Credit Control" || r.Name == "Acknowledge RCS Application"||r.Name== "Rates Clearance Sectional Head" || r.Name == "Support Admin" || r.Name == "Support Clerk").ToList();
                    var roles = context.Roles
                                            .Where(r => currentRoleApplication.Contains(r.Id) && r.Name == "Property Manager" || r.Name == "Housing Supervisor" || r.Name == "Finance Administrator" || r.Name == "Revenue Manager" || r.Name == "Community Development Officer"
                                            || r.Name == "Letting Officer" || r.Name == "Revenue Officer" || r.Name == "Back Office System Administrator" || r.Name == "Area Manager" || r.Name == "Senior Manager" || r.Name == "Senior specialist" || r.Name == "Housing liaison officer"
                                            || r.Name == "Maintenance Supervisor" || r.Name == "Caretaker").ToList();

                    model.Features = new SelectList(roles, "Name", "Name");
                    var CCCs = context.CCCTypes.OrderBy(x => x.Name).ToList();

                    ViewBag.CCCTypes = new SelectList(CCCs, "Id", "Name");

                    //return View(model);

                    _base.Initialise(context);
                    var captchaHelper = new CaptchaHelper();
                    var captchaResponse = Request.Params["g-recaptcha-response"];

                    if (ModelState.IsValid /*&& captchaHelper.ValidateCaptcha(captchaResponse)*/)
                    {
                        var usernameAssigned = false;
                        var emailAssigned = false;
                        var mobileAssigned = false;

                        usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() == model.UserName.ToLower() && u.IsActive && u.IsDeleted == false);
                        if (model.EmailAddress != null)
                            emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower() && u.IsActive && u.IsDeleted == false);
                        if (model.MobileNumber != null)
                            mobileAssigned = context.SystemUsers.Any(u => u.MobileNumber == model.MobileNumber && u.IsActive && !u.IsDeleted);
                        var response = "";
                        var Statuses = context.Status.Where(x => x.Key == StatusKeys.AccountPending).FirstOrDefault();
                        if (!emailAssigned)
                        {
                            emailAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.EmailAddress.ToLower() == model.EmailAddress.ToLower() && u.IsActive && u.IsDeleted == false && u.StatusId == Statuses.Id);
                        }
                        if (!mobileAssigned)
                        {
                            mobileAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.MobileNumber == model.MobileNumber && u.IsActive && u.IsDeleted == false && u.StatusId == Statuses.Id);
                        }
                        if (!usernameAssigned)
                        {
                            usernameAssigned = context.ClerkRegistrations.Include(x => x.Status).Any(u => u.UserName.ToLower() == model.UserName.ToLower() && u.IsActive && u.IsDeleted == false && u.StatusId == Statuses.Id);
                        }

                        if (model.EmailAddress == null && model.MobileNumber == null)
                        {
                            response = "The email address and mobile number are both invalid. Either one has to be valid.";
                        }

                        else if (usernameAssigned)
                        {
                            response = "The username entered is already taken.";
                        }
                        else if (emailAssigned)
                        {
                            response = "The email address entered is already in use.";
                        }
                        else if (mobileAssigned)
                        {
                            response = "The mobile number entered is already in use.";
                        }
                        else
                        {
                            // JK.20140724a - Passing values from the ViewModel to the model.
                            // JK.20190623a - Sort out proper authentication and vreification on mobile/ phone number (PhoneNumber = model.MobileNumber, PhoneNumberConfirmed = true).
                            //              - Currently using the customer code generated in the systemuser.code.
                            //var user = new SystemIdentityUser { UserName = model.UserName, Email = model.EmailAddress, PhoneNumber = model.MobileNumber, PhoneNumberConfirmed = true };
                            //var identityManager = new IdentityManager();
                            // JK.20190620a - Code used for mobile users.
                            //var code = PasswordGenerator.GeneratePassword(true, true, true, false, false, 6);
                            var NotificationTypeID = 0;
                            NotificationType SMSObj = context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.Sms);
                            NotificationType EmailObj = context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.Email);
                            NotificationType BothSMSEmail = context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.EmailSms);

                            if (model.EmailAddress != null && model.MobileNumber != null)
                            {
                                NotificationTypeID = BothSMSEmail.Id;
                            }
                            else if (model.EmailAddress != null)
                            {
                                NotificationTypeID = EmailObj.Id;
                            }
                            else
                            {
                                NotificationTypeID = SMSObj.Id;
                            }
                            var CCC = context.CCCs.Where(x => x.CCCTypeId == model.CCC).FirstOrDefault().Id;
                            //ClerkRegistration clerkRegistration = new ClerkRegistration();
                            var status = context.Status.ToList();
                            // JK.20140724a - Custom profile information.

                            var clerkRegistration = new ClerkRegistration()
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IdentificationNumber = model.IdentificationNumber,
                                UserName = model.UserName,
                                EmployeeNumber = model.EmployeeNumber,
                                RoleArray = "Clerks",
                                CCCTypeId = model.CCC,
                                CCCId = CCC,
                                IsActiveDirectoryUser = model.AdUser,
                                //IsActiveDirectoryUser = model.AdUser,
                                EmailAddress = model.EmailAddress,
                                MobileNumber = model.MobileNumber,
                                StatusId = status.Where(x => x.Key == StatusKeys.AccountPending).FirstOrDefault().Id,
                                IsActive = true,
                                IsDeleted = false,
                                IsLocked = false,
                                CreatedDateTime = DateTime.Now,
                                ModifiedDateTime = DateTime.Now,
                                IsPasswordReset = true,
                                NotificationTypeId = NotificationTypeID
                            };

                            context.ClerkRegistrations.Add(clerkRegistration);
                            context.SaveChanges();

                            foreach (var item in SelectedRoles)
                            {
                                var clerkRoles = new ClerkRole();
                                clerkRoles.RoleName = item;
                                clerkRoles.ClerkRegistrationId = clerkRegistration.Id;
                                context.ClerkRoles.Add(clerkRoles);
                                context.SaveChanges();
                            }
                            var result = true;
                            if (result == true)
                            {
                                if (model.EmailAddress != null && !model.EmailAddress.Trim().Equals(string.Empty))
                                {
                                    //Confirmation email
                                    var confirmationEmail = new Email();
                                    //var emailConfirmationCode =
                                    //    await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                                    //emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
                                    const string subject = "Property Lease Management System: Email Confirmation";
                                    var body = string.Format("Please note your internal registration application has been forwarded to the relevant CCC");
                                    confirmationEmail.GenerateEmail(clerkRegistration.EmailAddress, subject, body, "1", false, AppSettingKeys.EservicesDefaultEmailTemplate, clerkRegistration.FullName);
                                }

                                if (model.MobileNumber != null && !model.MobileNumber.Trim().Equals(string.Empty))
                                {
                                    try
                                    {
                                        var smsConfirmation = new CesarSMS();
                                        var cxt = new eServicesDbContext();
                                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                        smsConfirmation.GenerateSMS(clerkRegistration.MobileNumber, string.Format("Welcome to the CoE Property Lease Management System, Please note your internal registration application has been forwarded to the relevant CCC."),
   1.ToString(CultureInfo.InvariantCulture), statusIdSms, clerkRegistration.FullName);
                                        //SmsHelper.Send(model.MobileNumber, string.Format("Welcome to Property Lease Management System, please confirm your account with the following code {0}.", code));
                                    }
                                    catch (Exception x)
                                    {
                                        EventLogHelper.LogSystemError(x.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                                    }

                                }

                                response = "Success";
                                ViewBag.EmailAddress = model.EmailAddress == null || model.EmailAddress.Trim() == string.Empty ? model.MobileNumber : model.EmailAddress;
                            }
                            else
                            {
                                //AddErrors(result);
                            }
                            var vm = new InternalRegisterViewModel();
                            vm.Features = model.Features;
                            ViewBag.Response = response;
                            return View(vm);
                        }

                        ModelState.AddModelError("", response);
                    }

                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw ex;
                    //return View("_Error");
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }
        #endregion
        #region Area Manager Ajax POST
        public JsonResult AreaManager(int CCCTypeId)
        {
            var result = "";
            eServicesDbContext db = new eServicesDbContext();
            try
            {
                object Eilist = TempData["ep"];
                List<ElectricityMeterInformation> el = new List<ElectricityMeterInformation>();
                var CCC = db.CCCs.Where(x => x.CCCTypeId == CCCTypeId).Include(x => x.AreaManager).FirstOrDefault();
                if (CCC.AreaManagerId != null)
                {
                    result = CCC.AreaManager.FullName;
                }
                else
                {
                    result = "No Area Manager Mapped";
                }


            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region RegisterValidatation POST
        public JsonResult RegisterValidatation(string Username, string MobileNumber, string Email)
        {
            var result = "";
            eServicesDbContext db = new eServicesDbContext();
            try
            {
                RegistrationViewModel vm = new RegistrationViewModel();
                List<RoundRobinLog> errorList = new List<RoundRobinLog>();


                var usernameAssigned = false;
                var emailAssigned = false;
                var mobileAssigned = false;


                usernameAssigned = db.SystemUsers.Any(u => u.UserName.ToLower() == Username.ToLower() && u.IsActive && u.IsDeleted == false);

                emailAssigned = db.SystemUsers.Any(u => u.EmailAddress.ToLower() == Email.ToLower() && u.IsActive && u.IsDeleted == false);

                mobileAssigned = db.SystemUsers.Any(u => u.MobileNumber == MobileNumber && u.IsActive && !u.IsDeleted);

                var response = "";


                if (usernameAssigned)
                {
                    RoundRobinLog error = new RoundRobinLog();
                    error.LogEntry = "The username entered is already taken.";
                    errorList.Add(error);
                    //response = "The username entered is already taken.";
                }
                if (emailAssigned)
                {

                    RoundRobinLog error2 = new RoundRobinLog();
                    error2.LogEntry = "The email address entered is already in use.";
                    errorList.Add(error2);

                }
                if (mobileAssigned)
                {
                    RoundRobinLog error3 = new RoundRobinLog();
                    error3.LogEntry = "The mobile number entered is already in use.";
                    errorList.Add(error3);

                }


                if (usernameAssigned == false && emailAssigned == false && mobileAssigned == false)
                {
                    RoundRobinLog error4 = new RoundRobinLog();
                    error4.LogEntry = "Success";
                    errorList.Add(error4);
                }

                return Json(errorList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                var e = ex.Message;
                result = "Error";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Account Confirm Email POST
        [AllowAnonymous]
        [EncryptedActionParameter]
        public async Task<ActionResult> ConfirmEmail(string id, string token)
        {
            var user = UserManager.FindById(id);
            var status = "Error";
            token = HttpUtility.UrlDecode(token);

            if (user == null) { return RedirectToAction("Login"); }

            if (!user.EmailConfirmed)
            {
                var result = await UserManager.ConfirmEmailAsync(id, token);

                if (result.Succeeded)
                    status = "Activated";

                if (result.Succeeded && !string.IsNullOrEmpty(user.UnconfirmedEmail))
                {
                    UpdateCustomerEmail(user.SystemUserId, user.UnconfirmedEmail);
                }
            }
            else
            {
                return RedirectToAction("Login");
            }

            ViewBag.EncodedSystemUserId = user.SystemUserId;
            ViewBag.Status = status;
            return View("_EmailActivated");
        }
        #endregion


        #region Account Confirm Activation Email For Linked Emails
        [AllowAnonymous]
        [EncryptedActionParameter]
        public ActionResult ConfirmActivationEmail(int token)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                var linkedEmailId = token;
                var linkedEmail = context.LinkedEmails.Include(le => le.Status).FirstOrDefault(le => le.Id == linkedEmailId);
                var linkedEmails = context.LinkedEmails.Include(le => le.Status).Where(le => le.EmailAddress == linkedEmail.EmailAddress).
                                   ToList();
                var emailVerifiedStatus = context.Status.FirstOrDefault(s => s.Key.Equals(StatusKeys.EmailVerified));

                if (linkedEmail == null) throw new Exception("Invalid linked email");
                if (emailVerifiedStatus == null) throw new Exception("Invalid linked email status");

                foreach (var email in linkedEmails.Where(email => email.Status.Key != emailVerifiedStatus.Key))
                {
                    email.StatusId = emailVerifiedStatus.Id;

                }

                context.SaveChanges();
                ViewBag.Status = "Activated";
                return View("_EmailActivated");
            }
        }
        #endregion
        #region Account Resend User Activation Email
        [AllowAnonymous]
        [EncryptedActionParameter]
        public ActionResult ResendUserActivationEmail(int Id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                //Confirmation email
                var confirmationEmail = new Email();
                var user = context.SystemUsers.SingleOrDefault(u => u.Id == Id);
                if (user == null) throw new Exception("Invalid system user");
                var identityUser = UserManager.FindByName(user.UserName);
                if (identityUser == null) throw new Exception("Invalid identity user");

                var emailConfirmationCode = UserManager.GenerateEmailConfirmationToken(identityUser.Id);
                emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
                const string subject = "Ekurhuleni Property Lease Management System: Email Confirmation";
                var body = string.Format("Thank you for your registration, please click on the below link to complete your registration: <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a>", Url.Action("ConfirmEmail", "Account", SecureActionLinkExtension.Encrypt(new { id = identityUser.Id, token = emailConfirmationCode }), protocol: Request.Url.Scheme));
                confirmationEmail.GenerateEmail((!string.IsNullOrEmpty(identityUser.UnconfirmedEmail)) ? identityUser.UnconfirmedEmail : identityUser.Email, subject, body, user.Id.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.FullName);
                return View("_ResendConfirmationEmail");
            }
        }
        #endregion

        #region Account Resend User Activation Code
        [AllowAnonymous]
        [EncryptedActionParameter]
        public ActionResult ResendUserActivationCode(string Id)
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                //Confirmation code
                var identityUser = UserManager.FindById(Id);
                var user = context.SystemUsers.SingleOrDefault(u => u.Id == identityUser.SystemUserId);
                if (user == null) throw new Exception("Invalid system user");

                if (identityUser == null) throw new Exception("Invalid identity user");

                if (user.MobileNumber != null && !user.MobileNumber.Trim().Equals(string.Empty))
                {
                    var smsConfirmation = new CesarSMS();
                    var cxt = new eServicesDbContext();
                    var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                    smsConfirmation.GenerateSMS(user.MobileNumber, string.Format("Welcome to the CoE Property Lease Management System, please confirm your account with the following code {0}.", user.Code),
user.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, user.FullName);
                    //SmsHelper.Send(user.MobileNumber, string.Format("Welcome to Property Lease Management System, please confirm your account with the following code {0}.", user.Code));
                }

                return View("_ResendConfirmationEmail");
            }
        }
        #endregion

        #region Account Update Customer's Email
        public void UpdateCustomerEmail(int systemUserId, string email)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    var systemUser = context.SystemUsers.Find(systemUserId);
                    var customer = context.Customers.FirstOrDefault(c => c.SystemUserId == systemUser.Id);
                    var identityUser = UserManager.FindByName(systemUser.UserName);

                    //update username and email address on identity and system user if changed on manage profile
                    if (identityUser.Email == identityUser.UserName)
                    {
                        identityUser.UserName = email;
                        systemUser.UserName = email;
                    }

                    identityUser.Email = email;
                    identityUser.UnconfirmedEmail = "";
                    UserManager.Update(identityUser);

                    systemUser.EmailAddress = email;
                    context.Entry(systemUser).State = EntityState.Modified;

                    if (customer != null)
                    {
                        customer.EmailAddress = email;
                        context.Entry(customer).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        #region Account Disassociate
        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }
        #endregion


        #region Account Manage GET
        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            using (var context = new eServicesDbContext())
            {
                ManageUserViewModel model = new ManageUserViewModel();

                try
                {
                    _base.Initialise(context);
                    var systemUser = context.SystemUsers.FirstOrDefault(su => su.Id == _base.SystemUser.Id);
                    if (systemUser == null) throw new Exception("Invalid system user");

                    model.UserName = systemUser.UserName;
                    model.EmailAddress = systemUser.EmailAddress;
                    model.MobileNumber = systemUser.MobileNumber;
                    model.PrefMethodOfCommunication = Convert.ToInt32(systemUser.NotificationTypeId);

                    List<NotificationType> ntype = new List<NotificationType>();
                    ntype = context.NotificationTypes.ToList();
                    ViewBag.NotiType = ntype;

                    List<Notification> nt = new List<Notification>();
                    nt = context.Notifications.ToList();
                    ViewBag.Chk = nt;

                    List<NotificationSubscription> ns = new List<NotificationSubscription>();
                    ns = context.NotificationSubscriptions.Where(x => x.SystemUserId == systemUser.Id && x.IsDeleted != true).ToList();
                    ViewBag.NotificationSubscriptions = ns;

                    ViewBag.HasLocalPassword = HasPassword();
                    ViewBag.ReturnUrl = Url.Action("Manage");

                    ViewBag.StatusMessage =
                    message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                    : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                    : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                    : message == ManageMessageId.Error ? "An error has occurred."
                    : "";
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }

                return View(model);
            }
        }
        #endregion
        #region Account Manage POST
        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            var hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };

            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword.Trim(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        var cUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());

                        cUser.SystemUser.ModifiedBySystemUserId = cUser.SystemUser.Id;
                        cUser.SystemUser.ModifiedDateTime = DateTime.Today;
                        cUser.SystemUser.IsPasswordReset = true;

                        await UserManager.UpdateAsync(cUser);

                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }

                    AddErrors(result);
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }

                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion


        #region Account Modify Username
        [HttpPost]
        public ActionResult ModifyUserName(string userName)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    var systemUser = context.SystemUsers.FirstOrDefault(su => su.Id == _base.SystemUser.Id);
                    if (systemUser == null) throw new Exception("Invalid system user");

                    var identityUser = UserManager.FindByName(systemUser.UserName);
                    if (identityUser == null) throw new Exception("Invalid identity user");

                    var isUsernameValid = Regex.IsMatch(userName, @"^[a-zA-Z0-9]{6,20}$");
                    var usernameAssigned = context.SystemUsers.Any(u => u.UserName.ToLower() ==
                                            userName.ToLower() && u.IsActive && u.IsDeleted == false);

                    if (!isUsernameValid)
                    {
                        return Json(new { success = true, result = "Error! The UserName must be 6 to 20 characters long, without any special characters eg. !,@,#,$,%,&,*." });
                    }

                    if (!usernameAssigned)
                    {
                        systemUser.UserName = userName;
                        identityUser.UserName = userName;

                        UserManager.Update(identityUser);
                        context.Entry(systemUser).State = EntityState.Modified;
                        context.SaveChanges();
                        return Json(new { success = true, result = "Success! Username updated." });
                    }
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }

                return Json(new { success = true, result = "Error! The username entered is already taken." });
            }
        }
        #endregion

        #region Account Modify Email
        [HttpPost]
        public ActionResult ModifyEmail(string email)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var confirmationEmail = new Email();
                    _base.Initialise(context);

                    var systemUser = context.SystemUsers.FirstOrDefault(su => su.Id == _base.SystemUser.Id);
                    if (systemUser == null) throw new Exception("Invalid system user");
                    var identityUser = UserManager.FindByName(systemUser.UserName);
                    if (identityUser == null) throw new Exception("Invalid identity user");

                    var emailAssigned = context.SystemUsers.Any(u => u.EmailAddress.ToLower() ==
                                          email.ToLower() && u.IsActive && u.IsDeleted == false);
                    var isEmailValid = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

                    if (emailAssigned)
                    {
                        return Json(new { success = false, result = "Error! Email address exists, please enter another email address." });
                    }

                    if (!isEmailValid)
                    {
                        return Json(new { success = false, result = "Error! Invalid Email address." });
                    }

                    identityUser.UnconfirmedEmail = email;
                    identityUser.EmailConfirmed = false;
                    var result = UserManager.Update(identityUser);

                    if (result.Succeeded)
                    {
                        //AJC 2019/11/15 Update system user email and mobile number on update of customer.
                        systemUser.EmailAddress = email;
                        context.Entry(systemUser).State = EntityState.Modified;
                        context.SaveChanges();
                        //


                        var emailConfirmationCode = UserManager.GenerateEmailConfirmationToken(identityUser.Id);
                        emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);

                        const string subject = "Ekurhuleni Property Lease Management System: Email Confirmation";
                        string body = string.Format("You have requested to update your email address, please click on the link below to reactivate your eServices account : <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a>", Url.Action("ConfirmEmail", "Account", SecureActionLinkExtension.Encrypt(new { id = identityUser.Id, token = emailConfirmationCode }), protocol: System.Web.HttpContext.Current.Request.Url.Scheme));
                        confirmationEmail.GenerateEmail(email, subject, body, systemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, systemUser.FullName);

                        return Json(new { success = true, result = "Activation email sent to: " + email + ". Please activate new email address to complete update." });
                    }

                    return Json(new { success = false, result = "An error occurred. Please try again later" });
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        #region Account Modify Mobile
        [HttpPost]
        public ActionResult ModifyMobile(string mobile)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    var confirmationSms = new Email();
                    _base.Initialise(context);

                    var systemUser = context.SystemUsers.FirstOrDefault(su => su.Id == _base.SystemUser.Id);
                    if (systemUser == null) throw new Exception("Invalid system user");
                    var identityUser = UserManager.FindByName(systemUser.UserName);
                    if (identityUser == null) throw new Exception("Invalid identity user");

                    var mobileAssigned = context.SystemUsers.Any(u => u.MobileNumber.ToLower() ==
                                          mobile.ToLower() && u.IsActive && u.IsDeleted == false);
                    // var isEmailValid = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

                    if (mobileAssigned)
                    {
                        return Json(new { success = false, result = "Error! Mobile Number already in use, please enter another Mobile Number." });
                    }
                    var code = PasswordGenerator.GeneratePassword(true, true, true, false, false, 6);
                    //identityUser.UnconfirmedEmail = mobile;
                    identityUser.EmailConfirmed = true;
                    identityUser.PhoneNumber = mobile;
                    systemUser.MobileNumber = mobile;
                    //context.Entry(systemUser).State = EntityState.Modified;
                    //context.Entry(identityUser).State = EntityState.Modified;
                    context.SaveChanges();

                    var result = UserManager.Update(identityUser);
                    context.Entry(systemUser).State = EntityState.Modified;
                    code = ""; context.SaveChanges();
                    if (result.Succeeded)
                    {
                        //AJC 2019/11/15 Update system user email and mobile number on update of customer.
                        systemUser.MobileNumber = mobile;
                        context.Entry(systemUser).State = EntityState.Modified;
                        context.SaveChanges();
                        //

                        var smsConfirmation = new CesarSMS();
                        var cxt = new eServicesDbContext();
                        var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                        smsConfirmation.GenerateSMS(mobile, string.Format("Property Lease Management System. \\nChange of Mobile Number. \\n Your Property Lease Management System account mobile number has been updated {0}.", code),
systemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, systemUser.FullName);
                        // SmsHelper.Send(mobile, string.Format("Property Lease Management System. \\nChange of Mobile Number. \\n Your Property Lease Management System account mobile number has been updated {0}.", code));


                        return Json(new { success = true, result = "Notification sent to: " + mobile + ". Please note your mobile number has been updated on the Property Lease Management System Website." });
                    }

                    return Json(new { success = false, result = "An error occurred. Please try again later" });
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion

        #region Account Edit NotificationSubscriptions
        [HttpPost]
        public ActionResult NotificationSubscriptions(string[] NotificationKey, bool[] CheckedNotifications, int NotificationTypeID)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {


                    var subscription = new NotificationSubscription();
                    _base.Initialise(context);

                    var systemUser = context.SystemUsers.FirstOrDefault(su => su.Id == _base.SystemUser.Id);
                    if (systemUser == null) throw new Exception("Invalid system user");
                    var identityUser = UserManager.FindByName(systemUser.UserName);
                    if (identityUser == null) throw new Exception("Invalid identity user");
                    systemUser.NotificationTypeId = NotificationTypeID;
                    context.Entry(systemUser).State = EntityState.Modified;
                    context.SaveChanges();
                    if (NotificationKey != null && CheckedNotifications != null)
                    {
                        for (var i = 0; i < NotificationKey.Length; i++)
                        {
                            var NotificationKeyTemp = NotificationKey[i];
                            var notificationid = context.Notifications.Where(x => x.Key == NotificationKeyTemp).FirstOrDefault();
                            NotificationSubscription SubscriptionMade = context.NotificationSubscriptions.Where(u => u.SystemUserId ==
                                              systemUser.Id && u.NotificationId == notificationid.Id).FirstOrDefault();

                            if (SubscriptionMade != null && SubscriptionMade.IsDeleted == false)
                            {
                                if (CheckedNotifications[i] == true)
                                {
                                    SubscriptionMade.NotificationTypeId = NotificationTypeID;

                                }
                                else
                                {
                                    SubscriptionMade.IsActive = false;
                                    SubscriptionMade.IsDeleted = true;
                                }
                                context.SaveChanges();
                            }
                            else if (SubscriptionMade != null && SubscriptionMade.IsDeleted == true)
                            {

                                if (CheckedNotifications[i] == true)
                                {
                                    SubscriptionMade.NotificationTypeId = NotificationTypeID;
                                    SubscriptionMade.IsActive = true;
                                    SubscriptionMade.IsDeleted = false;
                                    context.SaveChanges();
                                }
                                else
                                {

                                }

                            }

                            else
                            {
                                //for existing customers who have created anew notification

                                NotificationSubscription ns = new NotificationSubscription();

                                ns.SystemUserId = systemUser.Id;
                                ns.NotificationId = notificationid.Id;
                                ns.NotificationTypeId = NotificationTypeID;
                                context.NotificationSubscriptions.Add(ns);
                                context.SaveChanges();
                            }
                        }

                        return Json(new { success = true, result = "Please note your notification subscriptions have been updated." });

                    }
                    return Json(new { success = true, result = "Please note your communicatoin method have been updated." });
                }
                catch (Exception ex)
                {
                    EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    throw;
                }
            }
        }
        #endregion





        #region Account Forgot Password GET
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.NetConnection = CheckForInternetConnection();
            TempData["Error"] = null;
            return View();
        }
        #endregion
        #region Account Forgot Password POST
        /// <summary>
        /// TG20150408 - Reset User Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            using (var context = new eServicesDbContext())
            {
                try
                {
                    _base.Initialise(context);
                    var captchaHelper = new CaptchaHelper();
                    var captchaResponse = Request.Params["g-recaptcha-response"];
                    var netConnection = CheckForInternetConnection();
                    //&& ((!netConnection) || captchaHelper.ValidateCaptcha(captchaResponse))
                    if (ModelState.IsValid)
                    {
                        //TG20150408a.
                        //This below code updates the Asp.NetUser tables with the users new password by getting the userid based on the users email address.
                        var store = new UserStore<SystemIdentityUser>(context);
                        var UserManager = new UserManager<SystemIdentityUser>(store);
                        UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };

                        SystemIdentityUser cUser = new SystemIdentityUser();
                        //var cUser = "";
                        if (model.Email != null)
                        {
                            cUser = await UserManager.FindByEmailAsync(model.Email);
                        }
                        else if (model.MobileNumber != null)
                        {
                            var id2 = context.Users.Where(x => x.PhoneNumber == model.MobileNumber).FirstOrDefault();

                            if (id2 == null)
                            {

                                TempData["Error"] = "Please enter a registered email address or phone number.";
                                return View(model);

                            }
                            else
                            {
                                cUser = UserManager.FindById(id2.Id);
                            }


                            //cUser = await UserManager.FindByEmailAsync(model.Email);
                        }

                        //if (model.MobileNumber != null)
                        //{
                        //    cUser = await UserManager.findby(model.MobileNumber);
                        //}
                        //cUser = await UserManager.FindByEmailAsync(model.Email);

                        if (cUser != null)
                        {
                            //String userId = User.Identity.GetUserId();
                            String newPassword = GeneratePassword();
                            String hashedNewPassword = UserManager.PasswordHasher.HashPassword(newPassword);
                            var user = cUser.SystemUser;
                            Wso2Helper.Store(user.Id, newPassword);
                            user.IsPasswordReset = false;
                            user.IsTemporaryPassword = true;
                            user.IsLocked = false;
                            user.IsActive = true;
                            user.IsDeleted = false;
                            cUser.AccessFailedCount = 0;

                            await store.SetPasswordHashAsync(cUser, hashedNewPassword);
                            await UserManager.UpdateAsync(cUser);

                            context.Entry(user).State = EntityState.Modified;
                            context.SaveChanges();

                            // Audit Trail: Log password reset (self-service)
                            AuditTrailHelper.LogPasswordReset(user.Id, user.Id, "SelfReset");

                            var resetPasswordEmail = new Email();



                            try
                            {
                                if (user.EmailAddress != null && !user.EmailAddress.Trim().Equals(string.Empty) && model.Email != null)
                                {
                                    const string subject = "Ekurhuleni Property Lease Management System: Reset Password";
                                    var body = "Your password has been reset. Please change temporary password on login. <br/>" +
                                        "Username: " + user.UserName + "<br/>" +
                                        "Temporary Password: " + newPassword;
                                    resetPasswordEmail.GenerateEmail(user.EmailAddress, subject, body, user.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.FullName);

                                }

                                if (user.MobileNumber != null && !user.MobileNumber.Trim().Equals(string.Empty))
                                {
                                    var bodysms = "Your Ekurhuleni Property Lease Management System password has been reset. Please change temporary password on login.\\n" +
                                        "Username: " + user.UserName + "\\n" +
                                        "Temporary Password: " + newPassword;
                                    var smsConfirmation = new CesarSMS();
                                    var cxt = new eServicesDbContext();
                                    var statusIdSms = cxt.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                    smsConfirmation.GenerateSMS(user.MobileNumber, bodysms.ToString(),
            user.Id.ToString(CultureInfo.InvariantCulture), statusIdSms, user.FullName);
                                    //SmsHelper.Send(user.MobileNumber, bodysms.ToString());

                                }
                                else if (model.Email == null)
                                {
                                    TempData["Error"] = "Please enter a registered email address or phone number.";
                                    return View(model);
                                }
                                else if (model.MobileNumber == null)
                                {
                                    TempData["Error"] = "Please enter a registered email address or phone number.";
                                    return View(model);
                                }
                                else
                                {
                                    TempData["Error"] = "Please enter a registered email address or phone number.";
                                    return View(model);
                                }


                            }
                            catch (Exception smserr)
                            {
                                EventLogHelper.LogSystemError(smserr.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                            }




                            return RedirectToAction("ForgotPasswordConfirmation", "Account");
                        }
                        if (model.Email == null)
                        {
                            TempData["Error"] = "Please enter a registered email address.";
                        }
                        else if (model.MobileNumber == null)
                        {
                            TempData["Error"] = "Please enter a phone number.";
                        }
                        else
                        {
                            TempData["Error"] = "Please enter a registered email address or phone number.";
                        }

                    }
                    return View(model);
                }
                catch (Exception e)
                {
                    EventLogHelper.LogSystemError(e.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                    return View("_Error");
                }
            }
        }
        #endregion
        #region Forgot Password Confirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            ViewBag.Link = TempData["ViewBagLink"];
            return View();
        }
        #endregion


        #region Account LogOff POST
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult LogOff()
        {
            using (var context = new eServicesDbContext())
            {
                _base.Initialise(context);

                if (_loginId != 0)
                {
                    var systemUserLogTime = context.SystemUserLogTimes.Find(_loginId);
                    systemUserLogTime.LogoutTime = DateTime.Now;

                    context.Entry(systemUserLogTime).State = EntityState.Modified;
                    context.SaveChanges();
                }

                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.ExternalBearer);

                // Audit Trail: Log logout
                try { AuditTrailHelper.LogLogout(_base.SystemUser != null ? _base.SystemUser.Id : 0); } catch { }
                Session.Abandon();
                Session.Clear();
                AuthenticationManager.SignOut();
                Response.Cookies.Clear();
                return RedirectToAction("Login", "Account");
            }
        }
        #endregion


        #region Account Generate Random Password
        /// <summary>
        /// Generates the password for each user.
        /// </summary>
        /// <returns></returns>
        public string GeneratePassword()
        {
            int codeCount = 8;
            string allChar = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z";
            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            int temp = -1;

            Random rand = new Random();
            for (int i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(36);
                if (temp != -1 && temp == t)
                {
                    return GeneratePassword();
                }
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;
        }
        #endregion


        #region Account Internet Connectivity
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        //#region Bulk Resend Registration Confirmation Emails 
        //[HttpGet]
        //[Authorize(Roles = "Super Administrators")]
        //public async Task<ActionResult> ResendConfirmationEmail()
        //{
        //    try
        //    {
        //        var emailList = new List<string>();

        //        foreach (var email in emailList)
        //        {
        //            var user = await UserManager.FindByEmailAsync(email);

        //            if (user != null)
        //            {
        //                //Confirmation email
        //                var confirmationEmail = new Email();
        //                var emailConfirmationCode =
        //                    await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
        //                emailConfirmationCode = HttpUtility.UrlEncode(emailConfirmationCode);
        //                const string subject = "eServices: Email Confirmation";
        //                var body = string.Format("Thank you for your registration, please click on the below link to complete your registration: <br/><a href=\"{0}\" title=\"User Email Confirm\">{0}</a>", Url.Action("ConfirmEmail", "Account", SecureActionLinkExtension.Encrypt(new { id = user.Id, token = emailConfirmationCode }), HttpContext.Request.Url.Scheme));
        //                confirmationEmail.GenerateEmail(user.Email, subject, body, user.SystemUserId.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);
        //            }
        //        }
        //        return RedirectToAction("Index", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //#endregion

        #region Identity Access Mnagement

        private ApplicationSignInManager _signInManager;
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult("OpenIdConnect", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        //[HttpPost]
        public async Task<ActionResult> ExternalLoginCallback(string code, string session_state = "")
        {
            Session["Workaround"] = 0;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            var returnUrl = "";
            var TestUrl = "~/Home/Index";
            //var code = Request.QueryString["code"].ToString();

            //LoadTokenDetails(code);
            var list = new List<string>();
            var claims = AuthenticationManager.User.Claims.ToList();



            foreach (var c in claims)
            {
                list.Add(string.Format("{0} - {1}", c.Type, c.Value));
            }
            Session["claims"] = list;

            var accest = list[4];
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

            if (accest != null)
            {
                return RedirectToAction("Dashboard", "Profile");
            }

            else
            {
                return RedirectToAction("Login");
            }


            if (claims == null)
            {
                return RedirectToAction("Index", "Home");

            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(TestUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        public string GetUserName()
        {
            var claims = AuthenticationManager.User.Claims.ToList();
            foreach (var claim in claims)
            {
                if (claim.Type == "name")
                {
                    var username = string.Empty;
                    if (Session["IAMUsername"] != null) username = Session["IAMUsername"].ToString() ?? null;
                    if (claim.Value != username && !String.IsNullOrEmpty(username)) return username;
                    return claim.Value;
                }
            }
            return "";
        }


        [AllowAnonymous]
        public ActionResult IAMExternalLoginCallback(string code, string session_state = "")
        {
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(session_state)) return RedirectToAction("Login");

            var model = new LoginViewModel();
            var core = new eServicesDbContext();
            var User = AuthenticationManager.User;
            var Name = User.Identities.FirstOrDefault().Name ?? null;
            if (string.IsNullOrEmpty(Name))
            {
                var claims = AuthenticationManager.User.Claims.ToList();
                var type = "email";
                var types = new List<string>();
                foreach (var claim in claims)
                {
                    types.Add(claim.Type);
                    if (claim.Type == type)
                    {
                        Name = claim.Value;
                    }
                }
                if (string.IsNullOrEmpty(Name)) Name = claims.FirstOrDefault(d => d.Type == "sub").Value;
            }
            var aUser = new SystemIdentityUser();
            var usern = core.SystemUsers.FirstOrDefault(x => x.EmailAddress == Name);
            if(usern == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Name = usern.UserName;
            bool GetUserIdentityOrRegister = SecurityHelper.IsAuthorized(User, ApplicationKeys.IncentivePolicy, User.Identities.FirstOrDefault().Name ?? Name, AuthenticationManager.User.Claims.ToList());

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string username = identity.Claims.Where(c => c.Type == ClaimTypes.GivenName)
                    .Select(c => c.Value).SingleOrDefault() ?? Name;
            core.CurrentUserName = username;
            aUser = UserManager.FindByName(username.Trim()) ?? null;
            core.CurrentSystemUser = aUser?.SystemUser ?? null;
            TempData["Claims"] = AuthenticationManager.User.Claims.ToList();
            if (GetUserIdentityOrRegister && aUser.SystemUser != null)
            {
                model.UserName = username;
                model.Password = "Arsenal5@";
                model.RememberMe = false;
            }

            return RedirectToAction("IAMLogin", "Account", new { q = new AesCrypto().Encrypt("username=" + model.UserName ?? username + "&password" + model.Password + "&returnUrl=" + username + "123456789abcdefghijk!@#$%^&**()@") });
        }

        [DecryptParameter]
        [AllowAnonymous]
        public async Task<ActionResult> IAMLogin(string username, string password, string returnUrl)
        {
            try
            {
                returnUrl = "";
                var model = new LoginViewModel
                {
                    UserName = username,
                    Password = password ?? "Arsenal5@",
                    RememberMe = true
                };

                using (var context = new eServicesDbContext())
                {
                    context.CurrentUserName = model.UserName;
                    _base.Initialise(context);

                    //incremental delay to prevent brute force attacks
                    int incrementalDelay;
                    var error = "";

                    if (HttpContext.Application[Request.UserHostAddress] != null)
                    {
                        // wait for delay if there is one
                        incrementalDelay = (int)HttpContext.Application[Request.UserHostAddress];
                        await Task.Delay(incrementalDelay * 1000);
                    }

                    if (ModelState.IsValid)
                    {

                        var store = new UserStore<SystemIdentityUser>(context);
                        var UserManager = new UserManager<SystemIdentityUser>(store);
                        UserManager.UserValidator = new UserValidator<SystemIdentityUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };
                        // var user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                        var user = new SystemIdentityUser();
                        ADLogin adLogin = new ADLogin();
                        var test = context.AppSettings.Where(x => x.Key == "active_directory_domain").FirstOrDefault();
                        var test33 = context.AppSettings.Where(x => x.Id == 1180).FirstOrDefault();

                        var activeDirectoryOn = context.AppSettings.Where(x => x.Key == AppSettingKeys.ActiveDirectoryActive).FirstOrDefault().Value;
                        var activeDirectoryDomain = context.AppSettings.Where(x => x.Key == AppSettingKeys.activeDirectoryDomain).FirstOrDefault().Value;

                        var Ad = false;
                        //var activeDirectoryOn = "True";
                        if (activeDirectoryOn == "True")
                        {
                            try
                            {
                                user = await UserManager.FindByNameAsync(model.UserName.Trim());
                                if (!user.isActiveDirectoryUser && user != null)
                                {
                                    user.isActiveDirectoryUser = true;
                                    user.SystemUser.isActiveDirectoryUser = true;
                                    user.SystemUser.IsIAMRegistered = true;
                                    user.isInternalUser = true;
                                    await UserManager.UpdateAsync(user);
                                    context.SaveChanges();
                                }
                                if (user != null)
                                {

                                    if (user.isInternalUser == true && user.isActiveDirectoryUser == true)
                                    {
                                        var Domain = new PrincipalContext(ContextType.Domain, activeDirectoryDomain);

                                        if (Domain != null)
                                        {
                                            Ad = adLogin.ValidateUser(model.UserName, model.Password);
                                            if (Ad || _base.SystemUser.IsIAMRegistered || user.isActiveDirectoryUser)
                                            {
                                                Session["IsInternalUser"] = true;
                                            }
                                            else
                                            {
                                                error = "Incorrect Active Directory username or password";
                                                ModelState.AddModelError("", "Invalid Active Directory username or password.");
                                                ViewBag.Error = error;
                                                return View("Login", model);
                                            }
                                        }
                                    }
                                    else
                                    {

                                        var Counter = user.AccessFailedCount;
                                        if (Counter > 20)
                                        {
                                            ViewBag.Error = "Your Account has been locked due to the wrong password entered multiple time, Please Use The Forgot Password To Unlock Your Account.";
                                            AuthenticationManager.SignOut();
                                            return View("Login", model);
                                        }
                                        else
                                        {
                                            user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                                            if (user == null)
                                            {
                                                var user3 = await UserManager.FindByNameAsync(model.UserName.Trim());
                                                user = user3;
                                                //user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim());
                                                user3.AccessFailedCount = user3.AccessFailedCount + 1;
                                                await UserManager.UpdateAsync(user3);
                                                context.SaveChanges();
                                            }


                                        }
                                    }

                                }
                                else
                                {
                                    user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                                }
                            }
                            catch (PrincipalServerDownException ex)
                            {
                                EventLogHelper.LogSystemError(ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                                // show your error message
                                user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                            }
                        }

                        //Testing on Dev/QA
                        else
                        {
                            //var UserName = "ctadmin";
                            //var Password = "second1#";
                            user = await UserManager.FindByUserNameOrEmailAsync(model.UserName.Trim(), model.Password.Trim());
                        }
                        //Uncomment below code to bypass login with user's password 
                        //var user = await UserManager.FindByNameAsync(model.UserName.Trim());

                        if (user != null)
                        {




                            //Checks if account is deactivated
                            if (user.SystemUser.IsLocked == true && user.SystemUser.IsDeleted == true)
                            {

                                ViewBag.Error = "Your Account Is Deactivated, Please Contact The System Administrator To Re-Activate Your Account.";
                                AuthenticationManager.SignOut();
                                return View("Login", model);
                            }
                            AppSetting LockOut = context.AppSettings.FirstOrDefault(o => o.Key == AppSettingKeys.AccountLock);
                            //Checks if account is locked
                            if ((user.SystemUser.IsLocked == true && user.SystemUser.IsActive == true && user.SystemUser.IsDeleted == false) && (LockOut.IsActive == true))
                            {

                                ViewBag.Error = "Your Account has been locked due to suspicious activity, Please Contact The System Administrator To Unlock Your Account.";
                                AuthenticationManager.SignOut();
                                return View("Login", model);
                            }


                            var applicationUser = context.ApplicationUserRoles.FirstOrDefault(a => a.SystemUserId == user.SystemUserId);

                            //if (applicationUser != null)
                            //{
                            //    var ipAddressRange = context.AppSettings.FirstOrDefault(a => a.Key == AppSettingKeys.IpAddressRange);
                            //    if (ipAddressRange == null) throw new Exception("Invalid appsetting");

                            //    if (!HttpContext.Request.UserHostAddress.StartsWith(ipAddressRange.Value))
                            //    {
                            //        AuthenticationManager.SignOut();
                            //        ViewBag.Error = "Unauthorized external access. If you have any questions, please contact the website administrator.";
                            //        return View(model);
                            //    }
                            //}



                            // JK.20190623a - Using the user.emailconfirm to also validate the phone number, but email always takes preference.
                            if (user.EmailConfirmed == false && string.IsNullOrWhiteSpace(user.UnconfirmedEmail))
                            {
                                var guid = user.Id;
                                AuthenticationManager.SignOut();
                                ViewBag.EncodedUserId = guid;
                                ViewBag.EncodedSystemUserId = user.SystemUserId;
                                ViewBag.Data = user.SystemUserId;

                                if (user.SystemUser.EmailAddress != null)
                                {
                                    ViewBag.Status = "Warning";
                                    return View("_EmailActivated");
                                }
                                else
                                    return RedirectToAction("ActivateMobile", "Account", new { id = guid });

                            }
                            if (user.SystemUser.IsLocked != null && (!user.SystemUser.IsPasswordReset && user.SystemUser.IsTemporaryPassword && (bool)user.SystemUser.IsLocked))
                            {
                                AuthenticationManager.SignOut();
                                error = "Temporary password expired. Please click forgot password link to reset password.";
                                ModelState.AddModelError("", "Temporary password expired. Please click forgot password link to reset password.");
                            }
                            else
                            {
                                var email = new Email();
                                var sms = new CesarSMS();

                                var statusIdsms = context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                                var systemUser = context.SystemUsers.FirstOrDefault(s => s.Id == user.SystemUserId);

                                //reset incremental delay on successful login
                                if (HttpContext.Application[Request.UserHostAddress] != null)
                                {
                                    HttpContext.Application.Remove(Request.UserHostAddress);
                                }

                                //Uncomment below code once system is live
                                //user's email address must be verified before using eServices
                                var systemUserLogTime = new SystemUserLogTime
                                {
                                    SystemUserId = user.SystemUserId,
                                    LoginTime = DateTime.Now,
                                    SessionId = HttpContext.Session.SessionID,
                                    IPAddress = HttpContext.Request.UserHostAddress
                                };

                                //email login notification
                                if (systemUser != null)
                                    email.GenerateEmail(systemUser.EmailAddress, "Property Lease Management System Login",
                                        "Ekurhuleni Municipality Property Lease Management System login notification on " + String.Format("{0:F}", DateTime.Now),
                                        systemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, systemUser.FullName);
                                if (systemUser != null && systemUser.EmailAddress == null && systemUser.MobileNumber != null)
                                    sms.GenerateSMS(systemUser.MobileNumber,
                                       "Ekurhuleni Municipality Property Lease Management System login notification on " + String.Format("{0:F}", DateTime.Now),
                                       systemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdsms, systemUser.FullName);

                                // SmsHelper.Send(systemUser.MobileNumber, "Property Lease Management System login notification on " + String.Format("{0:F}", DateTime.Now));

                                context.SystemUserLogTimes.Add(systemUserLogTime);
                                try
                                {
                                    var identityManager = new IdentityManager();
                                    if (user.isInternalUser == false)
                                    {
                                        if (!UserManager.IsInRole(user.Id, "Customers"))
                                        {
                                            identityManager.AddUserToRole(user.Id, "Customers");
                                        }
                                    }
                                }
                                catch (Exception IO)
                                {
                                    EventLogHelper.LogSystemError(IO.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                                    var err = IO.Message;
                                }



                                if (!user.SystemUser.IsPasswordReset && user.SystemUser.IsTemporaryPassword)
                                {
                                    user.SystemUser.IsActive = true;
                                    user.SystemUser.IsDeleted = false;
                                    user.SystemUser.IsLocked = true;
                                }
                                user.AccessFailedCount = 0;

                                await UserManager.UpdateAsync(user);
                                context.SaveChanges();

                                _loginId = systemUserLogTime.Id;
                                Session["Username"] = user.UserName;
                                Session["Password"] = model.Password.Trim();

                                //LF.20150316 - Force to change password on the first login.
                                if (user.SystemUser.IsPasswordReset == false)
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Manage", "Account");
                                }

                                if (UserManager.IsInRole(user.Id, "System Administrators"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Index", "Home");
                                }
                                if (UserManager.IsInRole(user.Id, "Internal Registration"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("InternalRegister", "Account");
                                }
                                if (UserManager.IsInRole(user.Id, "Area Manager"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Index", "AreaManager");
                                }
                                if (UserManager.IsInRole(user.Id, "Acknowledge RCS Application"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("PropertyLeaseApplications", "PropertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Submit Figures"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("AssessmentFiguresDashboard", "DepartmentsApprovals");
                                }
                                if (UserManager.IsInRole(user.Id, "Issue Certificate"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("RCCertificateDashboard", "DepartmentsApprovals");
                                }
                                if (UserManager.IsInRole(user.Id, "Billing"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Rates"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Credit Control"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("CreditDepartmentDashboard", "DepartmentsApprovals");
                                }
                                if (UserManager.IsInRole(user.Id, "Sundry Account"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Issue Refunds Collection"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("DepartmentalInput", "propertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Acknowledge Refund Application"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("RefundAcknowledgementDashboard", "DepartmentsApprovals");
                                }
                                if (UserManager.IsInRole(user.Id, "Rates Clearance Senior Clerk"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    //return RedirectToAction(secur)
                                    return RedirectToAction("Index", "Customer", new RouteValueDictionary(SecureActionLinkExtension.Encrypt(new { id = 0 })));
                                    return RedirectToAction("Index", "DepartmentsApprovals");
                                }
                                if (UserManager.IsInRole(user.Id, "Rates Clearance Sectional Head"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    //return RedirectToAction(secur)
                                    return RedirectToAction("AcknowledgementDashboard", "DepartmentsApprovals");
                                    return RedirectToAction("Index", "DepartmentsApprovals");
                                }
                                if (UserManager.IsInRole(user.Id, "Support Admin"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    //return RedirectToAction(secur)
                                    return RedirectToAction("ViewAllCases", "DepartmentsApprovals");
                                    return RedirectToAction("Index", "DepartmentsApprovals");
                                }
                                if (user.UserName == "SashenMoodley")
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("CaptureWalkIn", "Capture");
                                }
                                if (UserManager.IsInRole(user.Id, "Community Development Officer"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("PropertyLeaseTenantTraining", "PropertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Housing Supervisor"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("PropertyLeaseInspections", "PropertyLeaseApplication");
                                }
                                if ((UserManager.IsInRole(user.Id, "Lease Official")) || (UserManager.IsInRole(user.Id, "Letting Officer")) || (UserManager.IsInRole(user.Id, "Client Services Officer")))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("UpdateLeaseDetails", "PropertyLeaseApplication");
                                }
                                if ((UserManager.IsInRole(user.Id, "Property Manager")) || (UserManager.IsInRole(user.Id, "Revenue Manager")))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    if (user.UserName.Equals("re_property_officer", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return RedirectToAction("Index", "RealEstateAdmin");
                                    }
                                    return RedirectToAction("PropertyLeaseAgreements", "PropertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Revenue Officer"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("PropertyLeaseDeposit", "PropertyLeaseApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Housing Liaison Officer"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Dashboard", "HumanSettlementApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Senior Housing Specialist"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Dashboard", "HumanSettlementApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Regional Manager"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Dashboard", "HumanSettlementApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Caretaker"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("Dashboard", "HumanSettlementApplication");
                                }
                                if (UserManager.IsInRole(user.Id, "Back Office System Administrator"))
                                {
                                    await SignInAsync(user, model.RememberMe);
                                    return RedirectToAction("AdminInbox", "AreaManager");
                                }
                                else
                                {
                                    var user2 = user;
                                    //if (user.isInternalUser) return RedirectToAction("Manage", "Account");
                                    await SignInAsync(user2, model.RememberMe);
                                    return RedirectToAction("Dashboard", "Profile");
                                    //return RedirectToAction("Capture", "Capture");
                                }
                            }
                        }
                        else
                        {
                            //login failed
                            //increment the delay on failed attempt
                            if (HttpContext.Application[Request.UserHostAddress] == null)
                            {
                                incrementalDelay = 1;
                            }
                            else
                            {
                                incrementalDelay = (int)HttpContext.Application[Request.UserHostAddress] * 2;
                            }

                            HttpContext.Application[Request.UserHostAddress] = incrementalDelay;

                            error = "Invalid username/email or password";
                            ModelState.AddModelError("", "Invalid username/email or password.");
                        }
                    }

                    // If we got this far, something failed, redisplay form
                    ViewBag.Error = error;
                    return View("Login", model);
                }
            }
            catch (Exception ex)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.ExternalBearer);
                Session.Abandon();
                Session.Clear();
                AuthenticationManager.SignOut();
                Response.Cookies.Clear();
                return View("Error");
            }
        }



        private void LoadTokenDetails(String code)
        {
            string authorityUrl = System.Configuration.ConfigurationManager.AppSettings["Authority"];
            string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
            string grantType = System.Configuration.ConfigurationManager.AppSettings["GrantType"];
            string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];

            NameValueCollection param = new NameValueCollection();
            param.Add("client_id", clientId);
            param.Add("client_secret", clientSecret);
            param.Add("grant_type", grantType);
            param.Add("redirect_uri", redirectUri);
            param.Add("code", code);

            string serverUrl = string.Format("{0}/", authorityUrl);
            WebClient client = new WebClient();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            //Call the token endpoint and acess the token details
            var result = client.UploadValues(serverUrl + "oauth2/token", param);
            var response = System.Text.Encoding.Default.GetString(result);
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic data = js.Deserialize<dynamic>(response);
            System.Diagnostics.Debug.Print(response);

            string accessToken = data["access_token"];
            string refreshToken = data["refresh_token"];
            string scope = data["scope"];
            string idToken = "";
            if (data["id_token"] != null && data["id_token"] != "")
            {
                string tokenStr = data["id_token"] + "";
                string[] tokenArr = tokenStr.Split('.');
                string header = "HEADER:ALGORITHM & TOKEN TYPE >> " + "\r\n" + Base64Decode(tokenArr[0]);
                string payload = "PAYLOAD:DATA >> " + "\r\n" + Base64Decode(tokenArr[1]);
                idToken = header + "\r\n" + "\r\n" + payload;
            }
            string tokenType = data["token_type"];
            int expiresIn = data["expires_in"];

            // Show token details
            System.Diagnostics.Debug.Print("AccessToken: " + accessToken);
            System.Diagnostics.Debug.Print("RefreshToken: " + refreshToken);
            System.Diagnostics.Debug.Print("Scope: " + scope);
            System.Diagnostics.Debug.Print("IdToken: " + idToken);
            System.Diagnostics.Debug.Print("TokenType: " + tokenType);
            System.Diagnostics.Debug.Print("ExpiresIn: " + expiresIn.ToString());
        }

        //base64 decodes the given encoded string (data :encoded string)
        public string Base64Decode(string base64EncodedString)
        {
            try
            {
                //The length of a base64 encoded string is always a multiple of 4. 
                //If it is not a multiple of 4, then = characters are appended until it is
                int mod4 = base64EncodedString.Length % 4;
                if (mod4 > 0)
                {
                    base64EncodedString += new string('=', 4 - mod4);
                }
                var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedString);
                return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (Exception e)
            {
                return ("Error in base64Decode - " + e.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new SystemIdentityUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        public IAuthenticationManager AuthenticationManager2
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(SystemIdentityUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        public ActionResult LogOut()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        public String GetUserEnvironment()
        {
            var request = HttpContext.Request;
            var browser = request.Browser;
            var platform = GetUserPlatform();
            return string.Format("{0} {1} / {2}", browser.Browser, browser.Version, platform);
        }

        public String GetUserPlatform()
        {
            var request = HttpContext.Request;
            var ua = request.UserAgent;

            if (ua.Contains("Android"))
                return string.Format("Android {0}", GetMobileVersion(ua, "Android"));

            if (ua.Contains("iPad"))
                return string.Format("iPad OS {0}", GetMobileVersion(ua, "OS"));

            if (ua.Contains("iPhone"))
                return string.Format("iPhone OS {0}", GetMobileVersion(ua, "OS"));

            if (ua.Contains("Linux") && ua.Contains("KFAPWI"))
                return "Kindle Fire";

            if (ua.Contains("RIM Tablet") || (ua.Contains("BB") && ua.Contains("Mobile")))
                return "Black Berry";

            if (ua.Contains("Windows Phone"))
                return string.Format("Windows Phone {0}", GetMobileVersion(ua, "Windows Phone"));

            if (ua.Contains("Mac OS"))
                return "Mac OS";

            if (ua.Contains("Windows NT 5.1") || ua.Contains("Windows NT 5.2"))
                return "Windows XP";

            if (ua.Contains("Windows NT 6.0"))
                return "Windows Vista";

            if (ua.Contains("Windows NT 6.1"))
                return "Windows 7";

            if (ua.Contains("Windows NT 6.2"))
                return "Windows 8";

            if (ua.Contains("Windows NT 6.3"))
                return "Windows 8.1";

            if (ua.Contains("Windows NT 10"))
                return "Windows 10";

            //fallback to basic platform:
            return request.Browser.Platform + (ua.Contains("Mobile") ? " Mobile " : "");
        }

        public String GetMobileVersion(string userAgent, string device)
        {
            var temp = userAgent.Substring(userAgent.IndexOf(device, StringComparison.Ordinal) + device.Length).TrimStart();
            var version = string.Empty;

            foreach (var character in temp)
            {
                var validCharacter = false;
                int test;

                if (Int32.TryParse(character.ToString(CultureInfo.InvariantCulture), out test))
                {
                    version += character;
                    validCharacter = true;
                }

                if (character == '.' || character == '_')
                {
                    version += '.';
                    validCharacter = true;
                }

                if (validCharacter == false)
                    break;
            }

            return version;
        }
    }
}