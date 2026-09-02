using System;
using System.Linq;
using System.Web;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models.AuditTrail;

namespace C8.eServices.Mvc.Helpers
{
    /// <summary>
    /// World-class audit trail logging engine.
    /// Every method is wrapped in try/catch — audit failures NEVER crash business logic.
    /// Each method uses its own DbContext instance to isolate audit transactions.
    /// </summary>
    public static class AuditTrailHelper
    {
        // Admin controller names — actions on these controllers are flagged as admin actions
        private static readonly string[] AdminControllers = new[]
        {
            "RealEstateAdmin", "AreaManager", "ApplicationUserRole", "Report",
            "Admin", "SystemAdmin", "PropertyLeaseApplication"
        };

        #region Login Events

        /// <summary>
        /// Log a successful or failed login attempt.
        /// </summary>
        public static void LogLogin(int systemUserId, bool isSuccessful, string failureReason = null)
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    int? deptId = ResolveDepartmentId(db, systemUserId);

                    db.AuditTrailUserLogins.Add(new AuditTrailUserLogin
                    {
                        SystemUserId = systemUserId,
                        DepartmentId = deptId,
                        EventType = isSuccessful ? "Login" : "LoginFailed",
                        IPAddress = GetClientIP(),
                        UserAgent = GetUserAgent(),
                        EventDateTime = DateTime.Now,
                        IsSuccessful = isSuccessful,
                        FailureReason = isSuccessful ? null : TruncateString(failureReason, 500)
                    });
                    db.Database.CommandTimeout = 30;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChangesWithoutAudit();
                }
            }
            catch (Exception)
            {
                // Swallow — audit must never break business logic
            }
        }

        /// <summary>
        /// Log a user logout.
        /// </summary>
        public static void LogLogout(int systemUserId)
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    int? deptId = ResolveDepartmentId(db, systemUserId);

                    db.AuditTrailUserLogins.Add(new AuditTrailUserLogin
                    {
                        SystemUserId = systemUserId,
                        DepartmentId = deptId,
                        EventType = "Logout",
                        IPAddress = GetClientIP(),
                        UserAgent = GetUserAgent(),
                        EventDateTime = DateTime.Now,
                        IsSuccessful = true,
                        FailureReason = null
                    });
                    db.Database.CommandTimeout = 30;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChangesWithoutAudit();
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        #endregion

        #region Activity Logging

        /// <summary>
        /// Log a user activity or transaction.
        /// </summary>
        public static void LogActivity(int? systemUserId, string controller, string action,
            string description, string activityType = "PageView", bool isAdminAction = false,
            string entityName = null, int? entityId = null, string oldValue = null, string newValue = null)
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    int? deptId = null;
                    if (systemUserId.HasValue && systemUserId.Value > 0)
                    {
                        deptId = ResolveDepartmentId(db, systemUserId.Value);
                    }

                    // Auto-detect admin action from controller name if not explicitly set
                    if (!isAdminAction && !string.IsNullOrEmpty(controller))
                    {
                        isAdminAction = AdminControllers.Any(ac =>
                            controller.Equals(ac, StringComparison.OrdinalIgnoreCase));
                    }

                    db.AuditTrailActivities.Add(new AuditTrailActivity
                    {
                        SystemUserId = systemUserId,
                        DepartmentId = deptId,
                        ActivityType = TruncateString(activityType, 100),
                        Controller = TruncateString(controller, 200),
                        Action = TruncateString(action, 200),
                        Description = TruncateString(description, 1000),
                        EntityName = TruncateString(entityName, 200),
                        EntityId = entityId,
                        OldValue = oldValue,
                        NewValue = newValue,
                        IPAddress = GetClientIP(),
                        UserAgent = GetUserAgent(),
                        IsAdminAction = isAdminAction,
                        ActivityDateTime = DateTime.Now
                    });
                    db.Database.CommandTimeout = 30;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChangesWithoutAudit();
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        #endregion

        #region Password Resets

        /// <summary>
        /// Log a password reset event.
        /// </summary>
        public static void LogPasswordReset(int targetUserId, int resetByUserId, string resetType = "AdminReset")
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    int? deptId = ResolveDepartmentId(db, targetUserId);

                    db.AuditTrailPasswordResets.Add(new AuditTrailPasswordReset
                    {
                        TargetSystemUserId = targetUserId,
                        TargetDepartmentId = deptId,
                        ResetBySystemUserId = resetByUserId,
                        ResetType = TruncateString(resetType, 50),
                        IPAddress = GetClientIP(),
                        ResetDateTime = DateTime.Now
                    });
                    db.Database.CommandTimeout = 30;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChangesWithoutAudit();
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        #endregion

        #region Role Modifications

        /// <summary>
        /// Log a role modification event.
        /// </summary>
        public static void LogRoleModification(int targetUserId, int modifiedByUserId,
            string modificationType, string previousRole = null, string newRole = null)
        {
            try
            {
                using (var db = new eServicesDbContext())
                {
                    int? deptId = ResolveDepartmentId(db, targetUserId);

                    db.AuditTrailRoleModifications.Add(new AuditTrailRoleModification
                    {
                        TargetSystemUserId = targetUserId,
                        TargetDepartmentId = deptId,
                        ModifiedBySystemUserId = modifiedByUserId,
                        ModificationType = TruncateString(modificationType, 50),
                        PreviousRoleName = TruncateString(previousRole, 200),
                        NewRoleName = TruncateString(newRole, 200),
                        IPAddress = GetClientIP(),
                        ModificationDateTime = DateTime.Now
                    });
                    db.Database.CommandTimeout = 30;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChangesWithoutAudit();
                }
            }
            catch (Exception)
            {
                // Swallow
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Resolves the DepartmentId for a given SystemUserId.
        /// Returns null if user not found or has no department.
        /// </summary>
        private static int? ResolveDepartmentId(eServicesDbContext db, int systemUserId)
        {
            try
            {
                var user = db.SystemUsers.Find(systemUserId);
                if (user != null && !string.IsNullOrEmpty(user.StatusId))
                {
                    // DepartmentId is stored on SystemUser but accessed via the DB column
                    // Use raw SQL for safety since DepartmentId may not be in the EF model
                    var result = db.Database.SqlQuery<int?>(
                        "SELECT DepartmentId FROM SystemUsers WHERE Id = @p0", systemUserId
                    ).FirstOrDefault();
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the client IP address from the current HTTP context.
        /// </summary>
        private static string GetClientIP()
        {
            try
            {
                if (HttpContext.Current == null || HttpContext.Current.Request == null)
                    return "N/A";

                string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                if (string.IsNullOrEmpty(ip))
                    ip = HttpContext.Current.Request.UserHostAddress;

                return TruncateString(ip, 100) ?? "N/A";
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        /// <summary>
        /// Gets the user agent string from the current HTTP context.
        /// </summary>
        private static string GetUserAgent()
        {
            try
            {
                if (HttpContext.Current == null || HttpContext.Current.Request == null)
                    return null;

                return TruncateString(HttpContext.Current.Request.UserAgent, 500);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Safely truncates a string to the specified max length.
        /// </summary>
        private static string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        #endregion
    }
}
