using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Keys;
using C8.eServices.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace C8.eServices.Mvc.Helpers
{
    public static class RealEstateWorkAllocationHelper
    {
        /// <summary>
        /// Selects the official/representative clerk for a given responsibility type key.
        /// </summary>
        public static int GetAllocatedClerkId(eServicesDbContext core, RE_Application app, string responsibilityTypeKey)
        {
            try
            {
                // Try each role for this task in order (see RealEstateRoles.RoutingRoles); the first role with an active clerk gets it.
                var activeClerks = new List<Customer>();
                foreach (var targetRole in RealEstateRoles.RoutingRoles(responsibilityTypeKey))
                {
                    var role = core.Roles.FirstOrDefault(r => r.Name == targetRole);
                    if (role == null) continue;

                    var userIds = role.Users.Select(u => u.UserId).ToList();
                    var usernames = core.Users.Where(u => userIds.Contains(u.Id)).Select(u => u.UserName).ToList();
                    var systemUserIds = core.SystemUsers
                        .Where(su => usernames.Contains(su.UserName) && su.IsActive && !su.IsDeleted)
                        .Select(su => su.Id)
                        .ToList();
                    if (!systemUserIds.Any()) continue;

                    activeClerks = core.Customers
                        .Where(c => c.SystemUserId != null && systemUserIds.Contains((int)c.SystemUserId) && c.IsActive && !c.IsDeleted)
                        .ToList();
                    if (activeClerks.Any()) break;
                }

                if (!activeClerks.Any()) return 0;

                // Round Robin algorithm: Select the clerk with the minimum active Real Estate tasks in the queue.
                int selectedClerkId = 0;
                int minTaskCount = int.MaxValue;

                foreach (var clerk in activeClerks)
                {
                    int activeTasks = core.RoundRobinQueues
                        .Count(q => q.ClerkId == clerk.Id && q.Status.Key == StatusKeys.Submitted);

                    if (activeTasks < minTaskCount)
                    {
                        minTaskCount = activeTasks;
                        selectedClerkId = clerk.Id;
                    }
                }

                return selectedClerkId;
            }
            catch (Exception ex)
            {
                WorkAllocationHumanHelper.LogSystemError("Failed to get allocated clerk: " + ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
                return 0;
            }
        }

        /// <summary>
        /// Assigns a new task for Real Estate Application.
        /// </summary>
        public static void AssignRealEstateTask(eServicesDbContext core, int applicationId, string responsibilityTypeKey, int? specificClerkId = null)
        {
            try
            {
                var app = core.RE_Applications.FirstOrDefault(a => a.Id == applicationId && a.IsActive && !a.IsDeleted);
                if (app == null) return;

                var responsibilityType = core.ResponsibilityTypes.FirstOrDefault(r => r.Key == responsibilityTypeKey);
                if (responsibilityType == null) return;

                int clerkId = 0;
                if (specificClerkId.HasValue && specificClerkId.Value > 0)
                {
                    clerkId = specificClerkId.Value;
                }
                else
                {
                    clerkId = GetAllocatedClerkId(core, app, responsibilityTypeKey);
                }

                if (clerkId == 0)
                {
                    // Fallback: use default back office admin customer account
                    var defaultAdmin = core.Customers.FirstOrDefault(c => c.EmailAddress.Contains("admin") && c.IsActive && !c.IsDeleted);
                    if (defaultAdmin != null) clerkId = defaultAdmin.Id;
                }

                var statusList = core.Status.ToList();
                var statusId = statusList.FirstOrDefault(s => s.Key == StatusKeys.Submitted)?.Id ?? 99;

                // Finish all previous work for this responsibility type
                CompleteRealEstateTask(core, applicationId, responsibilityTypeKey);

                // Add to queue
                var queue = new RoundRobinQueue
                {
                    RealEstateApplicationId = applicationId,
                    ResponsibilityTypeId = responsibilityType.Id,
                    CurrentTaskDateTime = DateTime.Now,
                    ClerkId = clerkId,
                    StatusId = statusId
                };

                core.RoundRobinQueues.Add(queue);
                core.SaveChanges();
            }
            catch (Exception ex)
            {
                WorkAllocationHumanHelper.LogSystemError("Failed to assign Real Estate task: " + ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }

        /// <summary>
        /// Archives (completes) the active tasks of a specific responsibility type for the Real Estate application.
        /// </summary>
        public static void CompleteRealEstateTask(eServicesDbContext core, int applicationId, string responsibilityTypeKey)
        {
            try
            {
                var archivedStatus = core.Status.FirstOrDefault(s => s.Key == StatusKeys.Archived);
                if (archivedStatus == null) return;

                var responsibilityType = core.ResponsibilityTypes.FirstOrDefault(r => r.Key == responsibilityTypeKey);
                if (responsibilityType == null) return;

                var pendingTasks = core.RoundRobinQueues
                    .Where(q => q.RealEstateApplicationId == applicationId
                             && q.ResponsibilityTypeId == responsibilityType.Id
                             && q.EndTaskDateTime == null)
                    .ToList();

                foreach (var task in pendingTasks)
                {
                    task.EndTaskDateTime = DateTime.Now;
                    task.StatusId = archivedStatus.Id;
                    core.Entry(task).State = EntityState.Modified;
                }
            }
            catch (Exception ex)
            {
                WorkAllocationHumanHelper.LogSystemError("Failed to complete Real Estate task: " + ex.Message, LogTypeKeys.TryCatchException, ReferenceTypeKeys.ExceptionLog);
            }
        }
    }
}
