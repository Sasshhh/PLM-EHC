using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Helpers;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Models.AuditTrail;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace C8.eServices.Mvc.Controllers
{
    [Authorize]
    public class AuditTrailController : Controller
    {
        private eServicesDbContext db = new eServicesDbContext();

        #region Department Isolation

        /// <summary>
        /// Returns the DepartmentId filter for the current user.
        /// Super Admins / Back Office Admins see all departments (returns null).
        /// Everyone else sees only their own department.
        /// </summary>
        private int? GetDepartmentFilter()
        {
            try
            {
                if (User.IsInRole("Super Administrators") || User.IsInRole("Back Office System Administrator"))
                    return null; // System-wide view

                var username = User.Identity.Name;
                if (string.IsNullOrEmpty(username))
                    return -1; // Force empty results

                var user = db.SystemUsers.FirstOrDefault(u => u.UserName == username);
                if (user == null)
                    return -1;

                var deptId = db.Database.SqlQuery<int?>(
                    "SELECT DepartmentId FROM SystemUsers WHERE Id = @p0", user.Id
                ).FirstOrDefault();

                return deptId ?? -1;
            }
            catch (Exception)
            {
                return -1; // On error, show nothing rather than leaking cross-department data
            }
        }

        /// <summary>
        /// Gets the current SystemUser Id.
        /// </summary>
        private int GetCurrentSystemUserId()
        {
            try
            {
                var username = User.Identity.Name;
                var user = db.SystemUsers.FirstOrDefault(u => u.UserName == username);
                return user?.Id ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        #region Report 1: Active Users with Last Login History

        public ActionResult ActiveUsers(string search, string dateFrom, string dateTo)
        {
            try
            {
                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "ActiveUsers",
                    "Viewed Active Users report", "PageView", true);

                int? deptFilter = GetDepartmentFilter();

                // Get all active system users
                var usersQuery = db.SystemUsers.Where(u => u.IsActive && !u.IsDeleted);

                // Apply department filter
                if (deptFilter.HasValue)
                {
                    int deptId = deptFilter.Value;
                    var userIdsInDept = db.Database.SqlQuery<int>(
                        "SELECT Id FROM SystemUsers WHERE DepartmentId = @p0 AND IsActive = 1 AND IsDeleted = 0", deptId
                    ).ToList();
                    usersQuery = usersQuery.Where(u => userIdsInDept.Contains(u.Id));
                }

                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    usersQuery = usersQuery.Where(u =>
                        u.UserName.ToLower().Contains(search) ||
                        u.FirstName.ToLower().Contains(search) ||
                        u.LastName.ToLower().Contains(search));
                }

                var users = usersQuery.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

                // Get last login for each user
                var userIds = users.Select(u => u.Id).ToList();
                var lastLogins = db.AuditTrailUserLogins
                    .Where(l => userIds.Contains(l.SystemUserId) && l.EventType == "Login" && l.IsSuccessful)
                    .GroupBy(l => l.SystemUserId)
                    .Select(g => new { SystemUserId = g.Key, LastLogin = g.Max(l => l.EventDateTime) })
                    .ToDictionary(x => x.SystemUserId, x => x.LastLogin);

                // Get user roles
                var store = new UserStore<SystemIdentityUser>(db);
                var userManager = new UserManager<SystemIdentityUser>(store);
                var roleData = new Dictionary<int, string>();
                foreach (var u in users)
                {
                    try
                    {
                        var identityUser = userManager.FindByName(u.UserName);
                        if (identityUser != null)
                        {
                            var roles = userManager.GetRoles(identityUser.Id);
                            roleData[u.Id] = string.Join(", ", roles);
                        }
                        else
                        {
                            roleData[u.Id] = "N/A";
                        }
                    }
                    catch
                    {
                        roleData[u.Id] = "N/A";
                    }
                }

                ViewBag.Users = users;
                ViewBag.LastLogins = lastLogins;
                ViewBag.RoleData = roleData;
                ViewBag.Search = search;
                ViewBag.DepartmentFilter = deptFilter;
                ViewBag.DepartmentName = GetDepartmentName(deptFilter);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred loading the Active Users report. Please try again.";
                ViewBag.Users = new List<SystemUser>();
                ViewBag.LastLogins = new Dictionary<int, DateTime>();
                ViewBag.RoleData = new Dictionary<int, string>();
            }

            return View();
        }

        #endregion

        #region Report 2: Activities / Transactions

        public ActionResult ActivityLog(string search, string dateFrom, string dateTo)
        {
            try
            {
                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "ActivityLog",
                    "Viewed Activity Log report", "PageView", true);

                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailActivities.Include(a => a.SystemUser).AsQueryable();

                // Department filter
                if (deptFilter.HasValue)
                    query = query.Where(a => a.DepartmentId == deptFilter.Value);

                // Date range
                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(a => a.ActivityDateTime >= dtFrom.Value);
                if (dtTo.HasValue)
                {
                    var endOfDay = dtTo.Value.Date.AddDays(1);
                    query = query.Where(a => a.ActivityDateTime < endOfDay);
                }

                // Search
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(a =>
                        a.SystemUser.UserName.ToLower().Contains(search) ||
                        a.SystemUser.FirstName.ToLower().Contains(search) ||
                        a.SystemUser.LastName.ToLower().Contains(search) ||
                        a.Description.ToLower().Contains(search) ||
                        a.Controller.ToLower().Contains(search) ||
                        a.Action.ToLower().Contains(search));
                }

                var activities = query.OrderByDescending(a => a.ActivityDateTime).Take(1000).ToList();

                ViewBag.Activities = activities;
                ViewBag.Search = search;
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                ViewBag.DepartmentName = GetDepartmentName(deptFilter);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred loading the Activity Log. Please try again.";
                ViewBag.Activities = new List<AuditTrailActivity>();
            }

            return View();
        }

        #endregion

        #region Report 3: Admin Activities / Transactions

        public ActionResult AdminActivityLog(string search, string dateFrom, string dateTo)
        {
            try
            {
                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "AdminActivityLog",
                    "Viewed Admin Activity Log report", "PageView", true);

                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailActivities.Include(a => a.SystemUser)
                    .Where(a => a.IsAdminAction);

                // Department filter
                if (deptFilter.HasValue)
                    query = query.Where(a => a.DepartmentId == deptFilter.Value);

                // Date range
                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(a => a.ActivityDateTime >= dtFrom.Value);
                if (dtTo.HasValue)
                {
                    var endOfDay = dtTo.Value.Date.AddDays(1);
                    query = query.Where(a => a.ActivityDateTime < endOfDay);
                }

                // Search
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(a =>
                        a.SystemUser.UserName.ToLower().Contains(search) ||
                        a.SystemUser.FirstName.ToLower().Contains(search) ||
                        a.SystemUser.LastName.ToLower().Contains(search) ||
                        a.Description.ToLower().Contains(search));
                }

                var activities = query.OrderByDescending(a => a.ActivityDateTime).Take(1000).ToList();

                ViewBag.Activities = activities;
                ViewBag.Search = search;
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                ViewBag.DepartmentName = GetDepartmentName(deptFilter);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred loading the Admin Activity Log. Please try again.";
                ViewBag.Activities = new List<AuditTrailActivity>();
            }

            return View();
        }

        #endregion

        #region Report 4: Password Reset Activity Trails

        public ActionResult PasswordResets(string search, string dateFrom, string dateTo)
        {
            try
            {
                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "PasswordResets",
                    "Viewed Password Resets report", "PageView", true);

                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailPasswordResets
                    .Include(p => p.TargetSystemUser)
                    .Include(p => p.ResetBySystemUser)
                    .AsQueryable();

                // Department filter
                if (deptFilter.HasValue)
                    query = query.Where(p => p.TargetDepartmentId == deptFilter.Value);

                // Date range
                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(p => p.ResetDateTime >= dtFrom.Value);
                if (dtTo.HasValue)
                {
                    var endOfDay = dtTo.Value.Date.AddDays(1);
                    query = query.Where(p => p.ResetDateTime < endOfDay);
                }

                // Search
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(p =>
                        p.TargetSystemUser.UserName.ToLower().Contains(search) ||
                        p.TargetSystemUser.FirstName.ToLower().Contains(search) ||
                        p.TargetSystemUser.LastName.ToLower().Contains(search) ||
                        p.ResetBySystemUser.UserName.ToLower().Contains(search));
                }

                var resets = query.OrderByDescending(p => p.ResetDateTime).Take(1000).ToList();

                // Get role data for target users
                var store = new UserStore<SystemIdentityUser>(db);
                var userManager = new UserManager<SystemIdentityUser>(store);
                var roleData = new Dictionary<int, string>();
                foreach (var r in resets)
                {
                    if (!roleData.ContainsKey(r.TargetSystemUserId))
                    {
                        try
                        {
                            var identityUser = userManager.FindByName(r.TargetSystemUser?.UserName);
                            roleData[r.TargetSystemUserId] = identityUser != null
                                ? string.Join(", ", userManager.GetRoles(identityUser.Id))
                                : "N/A";
                        }
                        catch { roleData[r.TargetSystemUserId] = "N/A"; }
                    }
                }

                ViewBag.Resets = resets;
                ViewBag.RoleData = roleData;
                ViewBag.Search = search;
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                ViewBag.DepartmentName = GetDepartmentName(deptFilter);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred loading the Password Resets report. Please try again.";
                ViewBag.Resets = new List<AuditTrailPasswordReset>();
                ViewBag.RoleData = new Dictionary<int, string>();
            }

            return View();
        }

        #endregion

        #region Report 5: User Role Modifications

        public ActionResult RoleModifications(string search, string dateFrom, string dateTo)
        {
            try
            {
                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "RoleModifications",
                    "Viewed Role Modifications report", "PageView", true);

                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailRoleModifications
                    .Include(r => r.TargetSystemUser)
                    .Include(r => r.ModifiedBySystemUser)
                    .AsQueryable();

                // Department filter
                if (deptFilter.HasValue)
                    query = query.Where(r => r.TargetDepartmentId == deptFilter.Value);

                // Date range
                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(r => r.ModificationDateTime >= dtFrom.Value);
                if (dtTo.HasValue)
                {
                    var endOfDay = dtTo.Value.Date.AddDays(1);
                    query = query.Where(r => r.ModificationDateTime < endOfDay);
                }

                // Search
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(r =>
                        r.TargetSystemUser.UserName.ToLower().Contains(search) ||
                        r.TargetSystemUser.FirstName.ToLower().Contains(search) ||
                        r.TargetSystemUser.LastName.ToLower().Contains(search) ||
                        r.ModifiedBySystemUser.UserName.ToLower().Contains(search) ||
                        r.NewRoleName.ToLower().Contains(search) ||
                        r.PreviousRoleName.ToLower().Contains(search));
                }

                // Get current roles
                var store = new UserStore<SystemIdentityUser>(db);
                var userManager = new UserManager<SystemIdentityUser>(store);
                var currentRoles = new Dictionary<int, string>();

                var modifications = query.OrderByDescending(r => r.ModificationDateTime).Take(1000).ToList();

                foreach (var mod in modifications)
                {
                    if (!currentRoles.ContainsKey(mod.TargetSystemUserId))
                    {
                        try
                        {
                            var identityUser = userManager.FindByName(mod.TargetSystemUser?.UserName);
                            currentRoles[mod.TargetSystemUserId] = identityUser != null
                                ? string.Join(", ", userManager.GetRoles(identityUser.Id))
                                : "N/A";
                        }
                        catch { currentRoles[mod.TargetSystemUserId] = "N/A"; }
                    }
                }

                ViewBag.Modifications = modifications;
                ViewBag.CurrentRoles = currentRoles;
                ViewBag.Search = search;
                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;
                ViewBag.DepartmentName = GetDepartmentName(deptFilter);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred loading the Role Modifications report. Please try again.";
                ViewBag.Modifications = new List<AuditTrailRoleModification>();
                ViewBag.CurrentRoles = new Dictionary<int, string>();
            }

            return View();
        }

        #endregion

        #region CSV Export

        public ActionResult ExportActiveUsers()
        {
            try
            {
                int? deptFilter = GetDepartmentFilter();
                var usersQuery = db.SystemUsers.Where(u => u.IsActive && !u.IsDeleted);

                if (deptFilter.HasValue)
                {
                    int deptId = deptFilter.Value;
                    var userIdsInDept = db.Database.SqlQuery<int>(
                        "SELECT Id FROM SystemUsers WHERE DepartmentId = @p0 AND IsActive = 1 AND IsDeleted = 0", deptId
                    ).ToList();
                    usersQuery = usersQuery.Where(u => userIdsInDept.Contains(u.Id));
                }

                var users = usersQuery.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();
                var userIds = users.Select(u => u.Id).ToList();
                var lastLogins = db.AuditTrailUserLogins
                    .Where(l => userIds.Contains(l.SystemUserId) && l.EventType == "Login" && l.IsSuccessful)
                    .GroupBy(l => l.SystemUserId)
                    .Select(g => new { SystemUserId = g.Key, LastLogin = g.Max(l => l.EventDateTime) })
                    .ToDictionary(x => x.SystemUserId, x => x.LastLogin);

                var store = new UserStore<SystemIdentityUser>(db);
                var userManager = new UserManager<SystemIdentityUser>(store);

                var sb = new StringBuilder();
                sb.AppendLine("Username,Name,Surname,Role Name,Creation Date,Last Logon Date");
                foreach (var u in users)
                {
                    string role = "N/A";
                    try
                    {
                        var iu = userManager.FindByName(u.UserName);
                        if (iu != null) role = string.Join("; ", userManager.GetRoles(iu.Id));
                    }
                    catch { }

                    DateTime? lastLogin = lastLogins.ContainsKey(u.Id) ? lastLogins[u.Id] : (DateTime?)null;
                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                        EscapeCsv(u.UserName), EscapeCsv(u.FirstName), EscapeCsv(u.LastName),
                        EscapeCsv(role),
                        u.CreatedDateTime?.ToString("yyyy-MM-dd HH:mm") ?? "",
                        lastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "Never"));
                }

                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "ExportActiveUsers",
                    "Exported Active Users CSV", "Export", true);

                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                    "ActiveUsers_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Export failed. Please try again.";
                return RedirectToAction("ActiveUsers");
            }
        }

        public ActionResult ExportActivityLog(string search, string dateFrom, string dateTo, bool adminOnly = false)
        {
            try
            {
                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailActivities.Include(a => a.SystemUser).AsQueryable();

                if (adminOnly) query = query.Where(a => a.IsAdminAction);
                if (deptFilter.HasValue) query = query.Where(a => a.DepartmentId == deptFilter.Value);

                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(a => a.ActivityDateTime >= dtFrom.Value);
                if (dtTo.HasValue) query = query.Where(a => a.ActivityDateTime < dtTo.Value.Date.AddDays(1));

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(a =>
                        a.SystemUser.UserName.ToLower().Contains(search) ||
                        a.Description.ToLower().Contains(search));
                }

                var activities = query.OrderByDescending(a => a.ActivityDateTime).Take(5000).ToList();

                var sb = new StringBuilder();
                sb.AppendLine("Username,Name,Surname,Role Name,Date,Activity/Transaction");
                foreach (var a in activities)
                {
                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                        EscapeCsv(a.SystemUser?.UserName), EscapeCsv(a.SystemUser?.FirstName),
                        EscapeCsv(a.SystemUser?.LastName), "",
                        a.ActivityDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EscapeCsv(a.Description)));
                }

                string fileName = adminOnly ? "AdminActivityLog_" : "ActivityLog_";
                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "ExportActivityLog",
                    "Exported " + (adminOnly ? "Admin " : "") + "Activity Log CSV", "Export", true);

                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                    fileName + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Export failed. Please try again.";
                return RedirectToAction(adminOnly ? "AdminActivityLog" : "ActivityLog");
            }
        }

        public ActionResult ExportPasswordResets(string search, string dateFrom, string dateTo)
        {
            try
            {
                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailPasswordResets
                    .Include(p => p.TargetSystemUser).Include(p => p.ResetBySystemUser).AsQueryable();

                if (deptFilter.HasValue) query = query.Where(p => p.TargetDepartmentId == deptFilter.Value);

                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(p => p.ResetDateTime >= dtFrom.Value);
                if (dtTo.HasValue) query = query.Where(p => p.ResetDateTime < dtTo.Value.Date.AddDays(1));

                var resets = query.OrderByDescending(p => p.ResetDateTime).Take(5000).ToList();

                var sb = new StringBuilder();
                sb.AppendLine("Username,Name,Surname,Role Name,Administrator Username,Password Modification Date");
                foreach (var r in resets)
                {
                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                        EscapeCsv(r.TargetSystemUser?.UserName), EscapeCsv(r.TargetSystemUser?.FirstName),
                        EscapeCsv(r.TargetSystemUser?.LastName), "",
                        EscapeCsv(r.ResetBySystemUser?.UserName),
                        r.ResetDateTime.ToString("yyyy-MM-dd HH:mm:ss")));
                }

                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "ExportPasswordResets",
                    "Exported Password Resets CSV", "Export", true);

                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                    "PasswordResets_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Export failed. Please try again.";
                return RedirectToAction("PasswordResets");
            }
        }

        public ActionResult ExportRoleModifications(string search, string dateFrom, string dateTo)
        {
            try
            {
                int? deptFilter = GetDepartmentFilter();
                var query = db.AuditTrailRoleModifications
                    .Include(r => r.TargetSystemUser).Include(r => r.ModifiedBySystemUser).AsQueryable();

                if (deptFilter.HasValue) query = query.Where(r => r.TargetDepartmentId == deptFilter.Value);

                DateTime? dtFrom = ParseDate(dateFrom);
                DateTime? dtTo = ParseDate(dateTo);
                if (dtFrom.HasValue) query = query.Where(r => r.ModificationDateTime >= dtFrom.Value);
                if (dtTo.HasValue) query = query.Where(r => r.ModificationDateTime < dtTo.Value.Date.AddDays(1));

                var modifications = query.OrderByDescending(r => r.ModificationDateTime).Take(5000).ToList();

                var sb = new StringBuilder();
                sb.AppendLine("Username,Name,Surname,Current Role Name,Administrator Username,Role Modification Date,New Role Name");
                foreach (var m in modifications)
                {
                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                        EscapeCsv(m.TargetSystemUser?.UserName), EscapeCsv(m.TargetSystemUser?.FirstName),
                        EscapeCsv(m.TargetSystemUser?.LastName), EscapeCsv(m.PreviousRoleName),
                        EscapeCsv(m.ModifiedBySystemUser?.UserName),
                        m.ModificationDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EscapeCsv(m.NewRoleName)));
                }

                AuditTrailHelper.LogActivity(GetCurrentSystemUserId(), "AuditTrail", "ExportRoleModifications",
                    "Exported Role Modifications CSV", "Export", true);

                return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                    "RoleModifications_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Export failed. Please try again.";
                return RedirectToAction("RoleModifications");
            }
        }

        #endregion

        #region Private Helpers

        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;
            DateTime result;
            if (DateTime.TryParse(dateString, out result)) return result;
            return null;
        }

        private string GetDepartmentName(int? deptFilter)
        {
            if (!deptFilter.HasValue) return "All Departments (System-Wide)";
            try
            {
                var dept = db.ApplicationEntities.Find(deptFilter.Value);
                return dept?.Name ?? "Unknown Department";
            }
            catch { return "Unknown Department"; }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\"", "\"\"");
        }

        #endregion

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
