using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using System.Text;

namespace C8.eServices.Mvc.DataAccessLayer
{
    public class IdentityManager
    {
        private eServicesDbContext _context;
        public AccountController _account;
        public List<Claim> Claims { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityManager"/> class.
        /// </summary>
        public IdentityManager()
        {
            _context = new eServicesDbContext();
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
            UserManager = new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(_context));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityManager"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public IdentityManager(eServicesDbContext context)
        {
            _context = context;
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_context));
            UserManager = new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(_context));
        }

        /// <summary>
        /// Gets or sets the user manager.
        /// </summary>
        /// <value>
        /// The user manager.
        /// </value>
        public UserManager<SystemIdentityUser> UserManager { get; set; }
        public RoleManager<IdentityRole> RoleManager { get; set; }

        /// <summary>
        /// Roles the exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RoleExists(string name)
        {
            var rm = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(_context));
            return rm.RoleExists(name);
        }

        public bool UserExists(string name)
        {
            var um = new UserManager<SystemIdentityUser>(new UserStore<SystemIdentityUser>(_context));

            return um.FindByName(name) != null;
        }

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool CreateRole(string name)
        {
            try
            {
                var rm = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(_context));
                var idResult = rm.Create(new IdentityRole(name));

                return idResult.Succeeded;
            }
            catch (Exception x)
            {

                throw x;
            }
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool CreateUser(SystemIdentityUser user, string password)
        {
            var idResult = UserManager.Create(user, password);

            return idResult.Succeeded;
        }

        public IEnumerable<SystemIdentityUser> FindUsersInRole(String role)
        {
            if (!RoleExists(role)) return Enumerable.Empty<SystemIdentityUser>();
            role = _context.Roles.FirstOrDefault(a => a.Name == role).Id;
            var users = UserManager.Users.Where(o => o.Roles.Any(s => s.RoleId == role)).ToList();
            return users;
        }

        /// <summary>
        /// Adds the user to role.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public bool AddUserToRole(string userId, string roleName)
        {
            var idResult = UserManager.AddToRole(userId, roleName);

            return idResult.Succeeded;
        }
        public bool RemoveUserInRole(string userId, string roleName)
        {
            var idResult = UserManager.RemoveFromRole(userId, roleName);

            return idResult.Succeeded;
        }
        public SystemUser CurrentUser(List<Claim> claims, string username)
        {
            //SystemIdentityUser aUser = UserManager.FindById(systemUserGuid);
            SystemUser aUser = ReturnUser(username, claims);
            _context.CurrentSystemUser = aUser;
            return aUser;
        }


        //Paste code in IdentityManager 
        public SystemUser ReturnUser(string UserName, List<Claim> Claims)
        {
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string username = identity.Claims.Where(c => c.Type == ClaimTypes.GivenName)
                    .Select(c => c.Value).SingleOrDefault() ?? UserName;

            string user = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value ?? UserName;

            if (UserName == null)
            {
                return null;
            }
            SystemUser systemUser = _context.SystemUsers.FirstOrDefault(s => s.UserName == username) ?? null;

            

            // systemUser = _context.SystemUsers.FirstOrDefault(s => s.UserName == "solartest03") ?? null;

            if (systemUser == null)
            {
                string emailAddress = String.Empty;
                string phone = String.Empty;
                string firstName = String.Empty;
                string lastName = String.Empty;
                string department = String.Empty;
                string ccc = String.Empty;
                var roles = new List<string>();
                foreach (var c in Claims)
                {
                    switch (c.Type)
                    {
                        case IamKeys.Sub:
                            break;
                        case IamKeys.Name:
                            break;
                        case IamKeys.Upn:
                            break;
                        case IamKeys.CCC:
                            ccc = c.Value;
                            break;
                        case IamKeys.Department:
                            department = c.Value;
                            break;
                        case IamKeys.Roles:
                            var r = c.Value;
                            var rls = r.Split(',').ToList();
                            foreach (var d in rls)
                            {
                                roles.Add(d.Split('/').LastOrDefault());
                            }
                            roles.Remove("everyone");
                            break;
                        case IamKeys.PhoneNumber:
                            phone = c.Value;
                            break;
                        case IamKeys.GivenName:
                            firstName = c.Value;
                            break;
                        case IamKeys.FamilyName:
                            lastName = c.Value;
                            break;
                        case IamKeys.EmailAddress:
                            emailAddress = c.Value;
                            break;
                    }
                }

                //var cust = CreateSystemUserAsync(username, emailAddress, phone, roles);
                var boUser = CreateBOSystemUserAsync(username, emailAddress, phone, firstName, lastName, department, ccc, roles);

                systemUser = _context.SystemUsers.Where(s => s.UserName == username)
                .FirstOrDefault();
                return systemUser;
            }
            else
            {
                var identityManager = new IdentityManager();
                var roles = identity.Claims.Where(c => c.Type == ClaimTypes.Role)
                       .Select(c => c.Value).ToList();
                roles = roles.Where(c => c.Contains(IamKeys.Caretaker) || c.Contains(IamKeys.BOAdministrator) || c.Contains(IamKeys.MaintananceSupervisor)
                || c.Contains(IamKeys.SeniorHousingSpecialist) || c.Contains(IamKeys.RegionalManager) || c.Contains(IamKeys.HousingLiaisonOfficer) || c.Contains(IamKeys.SuperUser) || c.Contains(IamKeys.LettingOfficer) || c.Contains(IamKeys.HousingSupervisor) || c.Contains(IamKeys.ClientServicesOfficer) || c.Contains(IamKeys.PropertyManager) || c.Contains(IamKeys.RevenueOfficer) || c.Contains(IamKeys.RevenueManager) || c.Contains(IamKeys.CommunityDevelopmentOfficer)).ToList();

                //if(systemUser.Department.Key == )
                //    if (departmentKey == ApplicationEntityKeys.EkurhuleniHousingCompany)



                        var userId = UserManager.FindByName(systemUser.UserName).Id;
                List<string> rolesArray = UserManager.GetRoles(userId).ToList();
                var roles2 = rolesArray;
                var roles3 = new List<string>();
                var rolesAdded = new List<string>();
                var rolesRemoved = new List<string>();

                bool IdentityAccessRoles = false;
                foreach (var role in roles)
                {
                    switch (role)
                    {
                        case IamKeys.LettingOfficer:
                        case IamKeys.HousingSupervisor:
                        case IamKeys.ClientServicesOfficer:
                            if (!rolesArray.Contains("Client Services Officer"))
                            {
                                identityManager.AddUserToRole(userId, "Client Services Officer");
                                rolesAdded.Add("Client Services Officer");
                            }
                            if (rolesArray.Contains("Client Services Officer")) roles2.Remove(role);
                            roles3.Add("Client Services Officer");
                            break;
                        case IamKeys.PropertyManager:
                            if (!rolesArray.Contains("Property Manager"))
                            {
                                identityManager.AddUserToRole(userId, "Property Manager");
                                rolesAdded.Add("Property Manager");
                            }
                            if (rolesArray.Contains("Property Manager")) roles2.Remove(role);
                            roles3.Add("Property Manager");
                            break;
                        case IamKeys.RevenueOfficer:
                            if (!rolesArray.Contains("Revenue Officer"))
                            {
                                identityManager.AddUserToRole(userId, "Revenue Officer");
                                rolesAdded.Add("Revenue Officer");
                            }
                            if (rolesArray.Contains("Revenue Officer")) roles2.Remove(role);
                            roles3.Add("Revenue Officer");
                            break;
                        case IamKeys.RevenueManager:
                            if (!rolesArray.Contains("Revenue Manager"))
                            {
                                identityManager.AddUserToRole(userId, "Revenue Manager");
                                rolesAdded.Add("Revenue Manager");
                            }
                            if (rolesArray.Contains("Revenue Manager")) roles2.Remove(role);
                            roles3.Add("Revenue Manager");
                            break;
                        case IamKeys.CommunityDevelopmentOfficer:
                            if (!rolesArray.Contains("Community Development Officer"))
                            {
                                identityManager.AddUserToRole(userId, "Community Development Officer");
                                rolesAdded.Add("Community Development Officer");
                            }
                            if (rolesArray.Contains("Community Development Officer")) roles2.Remove(role);
                            roles3.Add("Community Development Officer");
                            break;
                        case IamKeys.Caretaker:
                            if (!rolesArray.Contains("Caretaker"))
                            {
                                identityManager.AddUserToRole(userId, "Caretaker");
                                rolesAdded.Add("Caretaker");
                            }
                            if (rolesArray.Contains("Caretaker")) roles2.Remove("Caretaker");
                            roles3.Add("Caretaker");
                            break;
                        case IamKeys.BOAdministrator:
                            if (!rolesArray.Contains("Back Office System Administrator"))
                            {
                                identityManager.AddUserToRole(userId, "Back Office System Administrator");
                                rolesAdded.Add("Back Office System Administrator");
                            }
                            if (rolesArray.Contains("Back Office System Administrator")) roles2.Remove(role);
                            roles3.Add("Back Office System Administrator");
                            break;
                        case IamKeys.MaintananceSupervisor:
                            if (!rolesArray.Contains("Maintenance Supervisor"))
                            {
                                identityManager.AddUserToRole(userId, "Maintenance Supervisor");
                                rolesAdded.Add("Maintenance Supervisor");
                            }
                            if (rolesArray.Contains("Maintenance Supervisor")) roles2.Remove(role);
                            roles3.Add("Maintenance Supervisor");
                            break;
                        case IamKeys.SeniorHousingSpecialist:
                            if (!rolesArray.Contains("Senior Housing Specialist"))
                            {
                                identityManager.AddUserToRole(userId, "Senior Housing Specialist");
                                rolesAdded.Add("Senior Housing Specialist");
                            }
                            if (rolesArray.Contains("Senior Housing Specialist")) roles2.Remove(role);
                            roles3.Add("Senior Housing Specialist");
                            break;
                        case IamKeys.RegionalManager:
                            if (!rolesArray.Contains("Regional Manager"))
                            {
                                identityManager.AddUserToRole(userId, "Regional Manager");
                                rolesAdded.Add("Regional Manager");
                            }
                            if (rolesArray.Contains("Regional Manager")) roles2.Remove(role);
                            roles3.Add("Regional Manager");
                            break;
                        case IamKeys.HousingLiaisonOfficer:
                            if (!rolesArray.Contains("Housing Liaison Officer"))
                            {
                                identityManager.AddUserToRole(userId, "Housing Liaison Officer");
                                rolesAdded.Add("Housing Liaison Officer");
                            }
                            if (rolesArray.Contains("Housing Liaison Officer")) roles2.Remove(role);
                            roles3.Add("Housing Liaison Officer");
                            break;
                        case IamKeys.SuperUser:
                            if (!rolesArray.Contains("Back Office System Administrator"))
                            {
                                identityManager.AddUserToRole(userId, "Back Office System Administrator");
                                rolesAdded.Add("Back Office System Administrator");
                            }
                            if (rolesArray.Contains("Back Office System Administrator")) roles2.Remove(role);
                            roles3.Add("Back Office System Administrator");
                            break;
                    }
                    IdentityAccessRoles = true;
                }
                if (systemUser.IsIAMRegistered && systemUser.isInternalUser) identityManager.RemoveUserInRole(userId, "Customers");
                if (IdentityAccessRoles && rolesArray.Count > 0)
                {
                    foreach (var role in rolesArray)
                    {
                        if (!roles3.Contains(role))
                        {
                            identityManager.RemoveUserInRole(userId, role);
                            rolesRemoved.Add(role);
                        }
                    }
                }

                var email = new Email();
                var sms = new CesarSMS();
                var statusIdsms = _context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                if (rolesAdded.Count > 0)
                {
                    var aRoles = string.Empty;
                    foreach (var role in rolesAdded)
                    {
                        if (string.IsNullOrEmpty(aRoles))
                        {
                            aRoles = role;
                        }
                        else
                        {
                            aRoles = string.Format("{0},{1}", aRoles, role);
                        }
                    }
                    email.GenerateEmail(systemUser.EmailAddress, "Property Lease Management Profile",
                                    "Please note your Property Lease Management Profile has been assigned to the following roles: " + aRoles,
                                    systemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, systemUser.FullName);
                    if (systemUser.EmailAddress == null && systemUser.MobileNumber != null)
                        sms.GenerateSMS(systemUser.MobileNumber,
                                       "Please note your Property Lease Management Profile has been assigned to the following roles: " + aRoles,
                                       systemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdsms, systemUser.FullName);
                }
                if (rolesRemoved.Count > 0)
                {
                    var rRoles = string.Empty;
                    foreach (var role in rolesRemoved)
                    {
                        if (string.IsNullOrEmpty(rRoles))
                        {
                            rRoles = role;
                        }
                        else
                        {
                            rRoles = string.Format("{0},{1}", rRoles, role);
                        }
                    }
                    email.GenerateEmail(systemUser.EmailAddress, "Property Lease Management Profile",
                                    "Please note your Property Lease Management Profile has been removed from the following roles: " + rRoles,
                                    systemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, systemUser.FullName);
                    if (systemUser.EmailAddress == null && systemUser.MobileNumber != null)
                        sms.GenerateSMS(systemUser.MobileNumber,
                                      "Please note your Property Lease Management Profile has been removed from the following roles: " + rRoles,
                                       systemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdsms, systemUser.FullName);
                }

                _context.CurrentSystemUser = systemUser;
                return systemUser;
            }
        }
        private SystemUser CreateSystemUserAsync(string username, string email, string phone, List<string> roles)
        {
            var user = new SystemIdentityUser { UserName = username, Email = email, PhoneNumber = phone, PhoneNumberConfirmed = true, EmailConfirmed = true };
            try
            {
                var identityManager = new IdentityManager();
                Account a = new Account();
                var code = PasswordGenerator.GeneratePassword(true, true, true, false, false, 6);
                var NotificationTypeID = 0;

                NotificationType SMSObj = _context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.Sms);
                NotificationType EmailObj = _context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.Email);
                NotificationType BothSMSEmail = _context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.EmailSms);


                NotificationTypeID = BothSMSEmail.Id;

                user.SystemUser = new SystemUser
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    IdentificationNumber = "9612195534089",
                    UserName = username,
                    EmailAddress = email,
                    MobileNumber = phone,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    IsPasswordReset = true,
                    IsIAMRegistered = true,
                    Code = code,
                    NotificationTypeId = NotificationTypeID
                };

                var result = UserManager.Create(user, "Arsenal5@");

                if (result.Succeeded)
                {
                    if (roles.Count == 0)
                    {
                        identityManager.AddUserToRole(user.Id, "Customers");
                    }
                    else
                    {
                        foreach (var role in roles)
                        {
                            switch (role)
                            {
                                case "PLMHLO":
                                    identityManager.AddUserToRole(user.Id, "Housing Liaison Officer");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    var profilePendingApproval = _context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);
                    var individualConveyer = _context.CustomerTypes.FirstOrDefault(s => s.Key == CustomerTypeKeys.Individual);
                    var IdentificationType = _context.IdentificationTypes.FirstOrDefault(s => s.Key == IdentificationTypeKey.SouthAfricanID);
                    var TitleType = _context.TitleTypes.FirstOrDefault(s => s.Key == TitleTypeKeys.Doctor);
                    var customer = new Customer
                    {
                        FirstName = "FirstName",
                        LastName = "LastName",
                        IdentificationNumber = "9612195534089",
                        EmailAddress = email,
                        CellPhoneNumber = phone,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        SystemUserId = user.SystemUser.Id,
                        StatusId = profilePendingApproval.Id,
                        CustomerTypeId = individualConveyer.Id,
                        IdentificationTypeId = IdentificationType.Id,
                        TitleTypeId = TitleType.Id

                    };
                    _context.Customers.Add(customer);
                    _context.SaveChanges();

                    var agent = new Agent
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        IdentificationNumber = customer.IdentificationNumber,
                        EmailAddress = customer.EmailAddress,
                        CellPhoneNumber = customer.CellPhoneNumber,
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        CustomerId = customer.Id,
                        StatusId = profilePendingApproval.Id,
                        CustomerTypeId = individualConveyer.Id,
                        IdentificationTypeId = IdentificationType.Id,
                        TitleTypeId = TitleType.Id
                    };
                    _context.Agents.Add(agent);
                    _context.SaveChanges();
                }
                else
                {
                    return user?.SystemUser;
                }

                return user?.SystemUser ?? new SystemUser();
            }
            catch (Exception Error)
            {
                return user?.SystemUser;
                //throw;
            }
            return user?.SystemUser;


            //SystemUser createSystemUser = new SystemUser();
            //createSystemUser.EmailAddress = email;
            //createSystemUser.MobileNumber = phone;
            //createSystemUser.UserName = username;

            //_context.SystemUsers.Add(createSystemUser);
            //_context.SaveChanges();
            //_context.CurrentSystemUser = createSystemUser;
            //return createSystemUser;
        }
        private SystemUser CreateBOSystemUserAsync(string username, string email, string phone, string firstName, string lastName, string DepartmentName, string CCCName, List<string> Roles)
        {
            var user = new SystemIdentityUser { UserName = username, Email = email, PhoneNumber = phone, PhoneNumberConfirmed = true, isActiveDirectoryUser = true };
            var identityManager = new IdentityManager();
            Account a = new Account();
            var code = PasswordGenerator.GeneratePassword(true, true, true, false, false, 6);
            var NotificationTypeID = 0;

            NotificationType SMSObj = _context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.Sms);
            NotificationType EmailObj = _context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.Email);
            NotificationType BothSMSEmail = _context.NotificationTypes.FirstOrDefault(o => o.Key == NotificationTypeKeys.EmailSms);
            ApplicationEntity Department = _context.ApplicationEntities.FirstOrDefault(o => o.Name == DepartmentName);
            CCCType CCC = _context.CCCTypes.FirstOrDefault(o => o.Name == CCCName);

            NotificationTypeID = BothSMSEmail.Id;

            user = new SystemIdentityUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phone,
                isInternalUser = true,
                isActiveDirectoryUser = true,
                RoundRobinIsActive = true,
                CCCId = CCC?.Id ?? 9,
                ServiceNo = "6626326",
                SystemUser = new SystemUser()
                {
                    FirstName = firstName ?? username,
                    LastName = lastName ?? username,
                    UserName = username,
                    MobileNumber = phone,
                    IdentificationNumber = "9612195534089",
                    EmailAddress = email,
                    IsPasswordReset = true,
                    CCCId = CCC?.Id ?? 9,
                    ServiceNo = "6626326",
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false,
                    IsIAMRegistered = true,
                    ModifiedDateTime = DateTime.Now,
                    isInternalUser = true,
                    isActiveDirectoryUser = true,
                    DepartmentId = Department?.Id
                }
            };

            var randomPassword = GeneratePassword(10);
            var result = UserManager.Create(user, randomPassword) ?? new IdentityResult();

            if (result.Succeeded)
            {

                var applicationUserRole = new ApplicationUserRole
                {
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false,
                    CCCId = CCC?.Id ?? 9,
                    SystemUserId = user.SystemUserId,
                    ApplicationId = _context.Applications.Where(x => x.Key == ApplicationKeys.PropertyLeaseManagementSysytem).FirstOrDefault().Id
                };


                _context.ApplicationUserRoles.Add(applicationUserRole);
                _context.SaveChanges();

                var profilePendingApproval = _context.Status.FirstOrDefault(s => s.Key == StatusKeys.CustomerActive);
                var individualConveyer = _context.CustomerTypes.FirstOrDefault(s => s.Key == CustomerTypeKeys.ManagingAgent);
                var IdentificationType = _context.IdentificationTypes.FirstOrDefault(s => s.Key == IdentificationTypeKey.SouthAfricanID);
                var TitleType = _context.TitleTypes.FirstOrDefault(s => s.Key == TitleTypeKeys.Doctor);
                var EmailOrSmsMessage = string.Empty;
                var EmailOrSmsMessage2 = string.Empty;
                var customer = new Customer
                {
                    FirstName = user.SystemUser.FirstName,
                    LastName = user.SystemUser.LastName,
                    IdentificationNumber = "9612195534089",
                    EmailAddress = email,
                    CellPhoneNumber = phone,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    SystemUserId = user.SystemUser.Id,
                    StatusId = profilePendingApproval.Id,
                    CustomerTypeId = individualConveyer.Id,
                    IdentificationTypeId = IdentificationType.Id,
                    TitleTypeId = TitleType.Id,
                    DepartmentId = Department?.Id
                };
                _context.Customers.Add(customer);
                _context.SaveChanges();

                var agent = new Agent
                {
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    IdentificationNumber = customer.IdentificationNumber,
                    EmailAddress = customer.EmailAddress,
                    CellPhoneNumber = customer.CellPhoneNumber,
                    IsActive = true,
                    IsDeleted = false,
                    IsLocked = false,
                    CreatedDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,
                    CustomerId = customer.Id,
                    StatusId = profilePendingApproval.Id,
                    CustomerTypeId = individualConveyer.Id,
                    IdentificationTypeId = IdentificationType.Id,
                    TitleTypeId = TitleType.Id,
                    DepartmentId = Department?.Id
                };
                _context.Agents.Add(agent);
                _context.SaveChanges();


                if (Roles.Contains(IamKeys.Caretaker) || Roles.Contains(IamKeys.HousingLiaisonOfficer) || Roles.Contains(IamKeys.SeniorHousingSpecialist) ||
                    Roles.Contains(IamKeys.RegionalManager) || Roles.Contains(IamKeys.MaintananceSupervisor) || Roles.Contains(IamKeys.BOAdministrator) || Roles.Contains(IamKeys.SuperUser))
                {
                    var rolenames = string.Empty;
                    foreach (var role in Roles)
                    {

                        switch (role)
                        {
                            case IamKeys.Caretaker:
                                identityManager.AddUserToRole(user.Id, "Caretaker");
                                break;
                            case IamKeys.BOAdministrator:
                                identityManager.AddUserToRole(user.Id, "Back Office System Administrator");
                                break;
                            case IamKeys.MaintananceSupervisor:
                                identityManager.AddUserToRole(user.Id, "Maintenance Supervisor");
                                break;
                            case IamKeys.SeniorHousingSpecialist:
                                identityManager.AddUserToRole(user.Id, "Senior Housing Specialist");
                                break;
                            case IamKeys.RegionalManager:
                                identityManager.AddUserToRole(user.Id, "Regional Manager");
                                break;
                            case IamKeys.HousingLiaisonOfficer:
                                identityManager.AddUserToRole(user.Id, "Housing Liaison Officer");
                                break;
                            case IamKeys.SuperUser:
                                identityManager.AddUserToRole(user.Id, "Super Administrators");
                                break;
                        }
                    }

                    EmailOrSmsMessage = "Please note the following Active " +
                                        "Directory User has been registered " +
                                        "in the system successfully";
                }
                else
                {
                    EmailOrSmsMessage = "Please note the following Active Directory User has been registered in " +
                                        "the system with no roles assiged to it, see the registration information " +
                                        "to assign relavant roles in active directory";
                }
                //notify existing BO's of this user
                var registerdUser = string.Format("<br/><br/><b>Active Directory Registration</b><br/>Username: {0}<br/>Full Name: {1}<br/>Email: {2}<br/>",
                                        user.UserName, user.SystemUser.FullName, user.SystemUser.EmailAddress);
                var email2 = new Email();
                var sms = new CesarSMS();
                var cc = new PropertyLeaseApplicationController();
                var statusIdsms = _context.Status.FirstOrDefault(o => o.Key == StatusKeys.SMSPending).Id;
                var roleId = _context.Roles.Where(x => x.Name == "Back Office System Administrator").FirstOrDefault().Id;
                var BackOfficeAdmins = cc.GetUsersInRole(roleId).ToList();
                BackOfficeAdmins = BackOfficeAdmins.Where(d => d.Id != user.Id).ToList();
                foreach (var backOfficeAdmin in BackOfficeAdmins)
                {
                    email2.GenerateEmail(backOfficeAdmin.SystemUser.EmailAddress, "PLM: Active Directory User Registraion",
                                EmailOrSmsMessage + registerdUser,
                               backOfficeAdmin.SystemUser.Id.ToString(CultureInfo.InvariantCulture), false, AppSettingKeys.EservicesDefaultEmailTemplate, backOfficeAdmin.SystemUser.FullName);
                    if (backOfficeAdmin.SystemUser.EmailAddress == null && backOfficeAdmin.SystemUser.MobileNumber != null)
                        sms.GenerateSMS(backOfficeAdmin.SystemUser.MobileNumber,
                                         EmailOrSmsMessage + registerdUser,
                                       backOfficeAdmin.SystemUser.Id.ToString(CultureInfo.InvariantCulture), statusIdsms, backOfficeAdmin.SystemUser.FullName);
                }

                //Email registering user
                var SelectedRoles = UserManager.GetRoles(user.Id).ToList();
                string rolesArray = "";
                foreach (var item in SelectedRoles)
                {
                    if (rolesArray == "") rolesArray = rolesArray + item.ToString();
                    else rolesArray = rolesArray + ", " + item.ToString();
                    var appUserRole = new AppUserRole
                    {
                        IsActive = true,
                        IsDeleted = false,
                        IsLocked = false,
                        RoleId = RoleManager.FindByName(item).Id,
                        ApplicationUserRoleId = applicationUserRole.Id
                    };
                    _context.AppUserRoles.Add(appUserRole);
                    _context.SaveChanges();
                }

                var applicationAccess = _context.Applications.Where(x => x.Key == ApplicationKeys.PropertyLeaseManagementSysytem).FirstOrDefault();
                const string emailSubject = "Property Lease Management System: User Registration";
                string emailBody = "";
                if (user.isActiveDirectoryUser == false)
                {
                    emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                  "<b>Login Details:</b><br/>" +
                  "Username: " + user.UserName + "<br/>" +
                  "Temporary Password: " + randomPassword + "<br/>" +
                  "Application Access: " + applicationAccess.Name + "<br/>" +
                  "Role(s): " + rolesArray ?? "No roles assigened" + "<br/><br/>" +
                  "<b> Please change temporary password on your first login.</b>";

                }
                else
                {
                    emailBody = "<b>You have been successfully added onto Property Lease Management System.</b><br/><br/>" +
                  "<b>Login Details:</b><br/>" +
                  "Active Directory Username: " + user.UserName + "<br/>" +
                  "Password: " + "Please use your own Active Directory password" + "<br/>" +
                  "Application Access: " + applicationAccess.Name + "<br/>" +
                  "Role(s): " + rolesArray ?? "No roles assigned" + "<br/><br/>";

                }
                email2.GenerateEmail(user.Email, emailSubject, emailBody, user.SystemUserId.ToString(), false, AppSettingKeys.EservicesDefaultEmailTemplate, user.SystemUser.FullName);
            }
            else
            {
                return user?.SystemUser;
            }

            return user?.SystemUser ?? new SystemUser();
        }
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
        /// <summary>
        /// Clears the user roles.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public void ClearUserRoles(string userId)
        {
            var user = UserManager.FindById(userId);
            var currentRoles = new List<IdentityUserRole>();
            currentRoles.AddRange(user.Roles);

            foreach (var role in currentRoles)
            {
                //um.RemoveFromRole( userId, role.Role.Name );
            }
        }

        /// <summary>
        /// Finds the system user.
        /// </summary>
        /// <param name="systemUserGuid">The application user unique identifier.</param>
        /// <returns></returns>
        public SystemIdentityUser CurrentSystemUser(string systemUserGuid)
        {
            SystemIdentityUser aUser = UserManager.FindById(systemUserGuid);
            _context.CurrentSystemUser = aUser.SystemUser;
            //var systemUserLogTime = new SystemUserLogTime
            //{
            //    SystemUserId = aUser.SystemUserId,
            //    LoginTime = DateTime.Now,
            //    SessionId = HttpContext.Session.SessionID,
            //    IPAddress = HttpContext.Request.UserHostAddress
            //};
            //_context.SystemUserLogTimes.Add(systemUserLogTime);
            //_context.SaveChanges();
            return aUser;
        }

        /// <summary>
        /// Finds the system user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public SystemIdentityUser CurrentSystemUser(System.Security.Principal.IPrincipal user)
        {
            if (user != null)
            {
                SystemIdentityUser aUser = UserManager.FindById(user.Identity.GetUserId());
                _context.CurrentSystemUser = aUser.SystemUser;
                return aUser;
            }

            return null;
        }

        /// <summary>
        /// Finds the user.
        /// </summary>
        /// <param name="systemUserGuid">The application user unique identifier.</param>
        /// <returns></returns>
        //public SystemUser CurrentUser(string systemUserGuid)
        //{
        //    SystemIdentityUser aUser = UserManager.FindById(systemUserGuid);
        //    _context.CurrentSystemUser = aUser.SystemUser;
        //    return aUser.SystemUser;
        //}

        /// <summary>
        /// Finds the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public SystemUser CurrentUser(System.Security.Principal.IPrincipal user)
        {
            if (user != null)
            {

                SystemIdentityUser aUser = UserManager.FindById(user.Identity.GetUserId());
                if (aUser != null)
                {
                    _context.CurrentSystemUser = aUser.SystemUser;
                    return aUser.SystemUser;
                }
                else
                {
                    var Name2 = _context.CurrentUserName;
                    var Name = user.Identity.GetUserName() ?? null;
                    if (string.IsNullOrEmpty(Name))
                    {
                        var accc = new AccountController();
                        Name = accc.GetUserName();
                    }
                    if (Name != Name2) Name = Name2;
                    var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    string username = identity.Claims.Where(c => c.Type == "name")
                            .Select(c => c.Value).SingleOrDefault() ?? identity.Claims.Where(c => c.Type == "sub")
                            .Select(c => c.Value).SingleOrDefault() ?? Name;
                    _context.CurrentUserName = username ?? Name;

                    username = identity.Claims.Where(c => c.Type == ClaimTypes.GivenName)
                            .Select(c => c.Value).SingleOrDefault() ?? Name;

                    bool GetUserIdentityOrRegister = SecurityHelper.IsAuthorized(user, ApplicationKeys.IncentivePolicy, Name ?? username, new List<Claim>());
                    //IAM User
                                      
                    aUser = UserManager.FindByName(username.Trim()) ?? null;
                    _context.CurrentSystemUser = aUser?.SystemUser ?? null;
                    _context.CurrentUserName = username;
                    return aUser?.SystemUser ?? null;
                }
            }
            return null;
        }

        public string CurrentUserId(string userName)
        {
            if (userName != null)
            {
                var aUser = UserManager.FindByName(userName);
                return aUser.Id;
            }

            return null;
        }
    }
}