using System;
using System.Linq;
using C8.eServices.Mvc.DataAccessLayer;
using C8.eServices.Mvc.Models;
using C8.eServices.Mvc.Keys;

namespace C8.eServices.Mvc.Engines
{
    public class ServiceRequestEngine
    {
        private readonly eServicesDbContext _db;
        private readonly NotificationEngine _notificationEngine;

        public ServiceRequestEngine(eServicesDbContext db)
        {
            _db = db;
            _notificationEngine = new NotificationEngine(db);
        }

        /// <summary>
        /// Generates a unique ticket reference number for service requests
        /// Format: EHC_SR_###_YYYY
        /// </summary>
        public string GenerateRequestReferenceNumber()
        {
            var year = DateTime.Now.Year;
            var prefix = $"EHC_SR_";

            var lastRequest = _db.ServiceRequests
                .Where(sr => sr.RequestReferenceNumber.StartsWith(prefix)
                          && sr.RequestReferenceNumber.EndsWith("_" + year))
                .OrderByDescending(sr => sr.RequestReferenceNumber)
                .Select(sr => sr.RequestReferenceNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastRequest))
            {
                // EHC_SR_###_YYYY → extract ###
                var parts = lastRequest.Split('_');
                if (parts.Length >= 4 && int.TryParse(parts[2], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"EHC_SR_{nextNumber.ToString("D3")}_{year}";
        }

        /// <summary>
        /// Calculates SLA deadlines based on priority (BR34) and category (BR23/BR24)
        /// BR23: Maintenance complaints resolved within 30 working days
        /// BR24: Non-maintenance complaints resolved within 7 working days
        /// BR34: Tiered priority-based SLAs
        /// </summary>
        public void CalculateSlaDeadlines(ServiceRequest serviceRequest)
        {
            var priority = _db.ServiceRequestPriorities.Find(serviceRequest.ServiceRequestPriorityId);
            var category = _db.ServiceRequestCategories.Find(serviceRequest.ServiceRequestCategoryId);

            if (priority != null)
            {
                // Response deadline based on priority (BR34)
                serviceRequest.ResponseDeadline = serviceRequest.DateSubmitted
                    .AddMinutes(priority.ResponseTimeMinutes);

                // Resolution deadline: use priority-based SLA from BR34
                serviceRequest.ResolutionDeadline = serviceRequest.DateSubmitted
                    .AddHours(priority.ResolutionTimeHours);
            }

            // Override resolution deadline with category-based SLA if stricter (BR23/BR24)
            if (category != null)
            {
                DateTime categoryDeadline;
                if (category.Key == ServiceRequestCategoryKeys.MaintenanceRequest)
                {
                    // BR23: Maintenance = 30 working days
                    categoryDeadline = AddWorkingDays(serviceRequest.DateSubmitted, 30);
                }
                else
                {
                    // BR24: Non-maintenance = 7 working days
                    categoryDeadline = AddWorkingDays(serviceRequest.DateSubmitted, 7);
                }

                // Use the earlier deadline (more restrictive)
                if (serviceRequest.ResolutionDeadline == null || categoryDeadline < serviceRequest.ResolutionDeadline)
                {
                    serviceRequest.ResolutionDeadline = categoryDeadline;
                }
            }
        }

        /// <summary>
        /// Checks if SLA is at 50% time and should trigger escalation (BR34)
        /// </summary>
        public bool ShouldEscalate(ServiceRequest serviceRequest)
        {
            if (serviceRequest.ResolutionDeadline == null || serviceRequest.EscalationTriggered)
                return false;

            var totalTime = (serviceRequest.ResolutionDeadline.Value - serviceRequest.DateSubmitted).TotalMinutes;
            var elapsedTime = (DateTime.Now - serviceRequest.DateSubmitted).TotalMinutes;

            // Escalate at 50% of total time (BR34)
            return elapsedTime >= (totalTime * 0.5);
        }

        /// <summary>
        /// Updates service request status
        /// </summary>
        public void UpdateStatus(int serviceRequestId, string statusKey)
        {
            var serviceRequest = _db.ServiceRequests.Find(serviceRequestId);
            if (serviceRequest != null)
            {
                var status = _db.Status.FirstOrDefault(s => s.Key == statusKey);
                if (status != null)
                {
                    serviceRequest.StatusId = status.Id;
                    serviceRequest.ModifiedDateTime = DateTime.Now;

                    if (statusKey == ServiceRequestStatusKeys.Resolved)
                    {
                        serviceRequest.DateResolved = DateTime.Now;
                        _notificationEngine.SendServiceRequestOutcomeNotification(serviceRequest);
                    }
                    else if (statusKey == ServiceRequestStatusKeys.Closed)
                    {
                        serviceRequest.DateClosed = DateTime.Now;
                    }

                    _db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Assigns a service request to the Letting Officer of the selected complex (UC17D).
        /// Creates a Round Robin Queue entry and sends assignment notification.
        /// </summary>
        public Customer AssignToLettingOfficer(ServiceRequest serviceRequest)
        {
            var complex = _db.PreferredComplexAreas.Find(serviceRequest.ComplexId);
            var fallbackId = Convert.ToInt32(
                _db.AppSettings.FirstOrDefault(x => x.Key == AppSettingKeys.LettingOfficer)?.Value ?? "0");

            var clerkId = (complex != null && complex.LettingOfficerId.HasValue)
                ? complex.LettingOfficerId.Value
                : fallbackId;

            if (clerkId == 0) return null;

            var cso = _db.Customers.Find(clerkId);
            if (cso == null) return null;

            var responsibilityType = _db.ResponsibilityTypes
                .FirstOrDefault(r => r.Key == ResponsibilityTypeKeys.ServiceRequestOpen);
            if (responsibilityType == null) return null;

            var statusId = _db.Status.FirstOrDefault(s => s.Key == StatusKeys.Submitted)?.Id ?? 1;

            var queue = new RoundRobinQueue
            {
                ServiceRequestId = serviceRequest.Id,
                PropertyLeaseApplicationId = null,
                TenantComplaintId = null,
                ResponsibilityTypeId = responsibilityType.Id,
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = clerkId,
                StatusId = statusId,
                CreatedDateTime = DateTime.Now,
                DepartmentId = 1
            };
            _db.RoundRobinQueues.Add(queue);

            serviceRequest.AssignedToId = clerkId;
            serviceRequest.DateAssigned = DateTime.Now;
            _db.SaveChanges();

            _notificationEngine.SendServiceRequestAssignmentNotification(serviceRequest, cso);
            return cso;
        }

        /// <summary>
        /// Closes active Round Robin queue entries for a service request at a given step.
        /// </summary>
        public void RoundRobinMarkFinished(int serviceRequestId, string responsibilityTypeKey)
        {
            var responsibilityType = _db.ResponsibilityTypes
                .FirstOrDefault(r => r.Key == responsibilityTypeKey);
            if (responsibilityType == null) return;

            var archivedStatusId = _db.Status.FirstOrDefault(x => x.Key == StatusKeys.Archived).Id;

            var entry = _db.RoundRobinQueues
                .Where(r => r.ServiceRequestId == serviceRequestId
                         && r.ResponsibilityTypeId == responsibilityType.Id
                         && r.StatusId != archivedStatusId)
                .OrderBy(r => r.Id)
                .FirstOrDefault();

            if (entry == null) return;

            entry.StatusId = archivedStatusId;
            entry.EndTaskDateTime = DateTime.Now;
            entry.ModifiedDateTime = DateTime.Now;
            _db.SaveChanges();
        }

        /// <summary>
        /// Opens a new Round Robin Queue entry when transitioning to In Progress.
        /// </summary>
        public void RoundRobinOpenInProgress(ServiceRequest serviceRequest)
        {
            var responsibilityType = _db.ResponsibilityTypes
                .FirstOrDefault(r => r.Key == ResponsibilityTypeKeys.ServiceRequestInProgress);
            if (responsibilityType == null) return;

            if (!serviceRequest.AssignedToId.HasValue) return;

            var statusId = _db.Status.FirstOrDefault(s => s.Key == StatusKeys.Submitted)?.Id ?? 1;

            var queue = new RoundRobinQueue
            {
                ServiceRequestId = serviceRequest.Id,
                PropertyLeaseApplicationId = null,
                TenantComplaintId = null,
                ResponsibilityTypeId = responsibilityType.Id,
                CurrentTaskDateTime = DateTime.Now,
                ClerkId = serviceRequest.AssignedToId.Value,
                StatusId = statusId,
                CreatedDateTime = DateTime.Now,
                DepartmentId = 1
            };
            _db.RoundRobinQueues.Add(queue);
            _db.SaveChanges();
        }

        /// <summary>
        /// Logs audit trail for service request actions
        /// </summary>
        public void LogAuditTrail(int serviceRequestId, string action, string details, int? performedByCustomerId)
        {
            var auditLog = new ServiceRequestAuditLog
            {
                ServiceRequestId = serviceRequestId,
                Action = action,
                Details = details,
                PerformedByCustomerId = performedByCustomerId,
                PerformedAt = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                CreatedDateTime = DateTime.Now,
                DepartmentId = 1
            };

            _db.ServiceRequestAuditLogs.Add(auditLog);
            _db.SaveChanges();
        }

        /// <summary>
        /// Sends service request creation notification to tenant via email (Step 20)
        /// </summary>
        public void SendCreationNotification(ServiceRequest serviceRequest)
        {
            if (string.IsNullOrEmpty(serviceRequest.EmailAddress)) return;

            var subject = $"Service Request Created - {serviceRequest.RequestReferenceNumber}";
            var body = $@"
                <h2>Service Request Created Successfully</h2>
                <p>Dear {serviceRequest.ReportedByName} {serviceRequest.ReportedBySurname},</p>
                <p>Your service request has been successfully logged.</p>
                <p><strong>Request Reference #:</strong> {serviceRequest.RequestReferenceNumber}</p>
                <p><strong>Date Created:</strong> {serviceRequest.DateSubmitted:yyyy/MM/dd HH:mm}</p>
                <p><strong>Category:</strong> {serviceRequest.Category?.Name}</p>
                <p><strong>Priority:</strong> {serviceRequest.Priority?.Name ?? "None"}</p>
                <p><strong>Status:</strong> Open</p>
                <p>You can track the progress of your request by logging into the system and navigating to <strong>My Existing Service Requests</strong>.</p>
                <p>If you have any questions, please contact our Client Services Office.</p>
                <p>Regards,<br/>Property Lease Management</p>
            ";

            try
            {
                // TODO: Implement proper email queueing using EmailQueueItem
                // For now, just log that email would be sent
            }
            catch (Exception) { /* Log error */ }
        }

        /// <summary>
        /// Checks if the current customer is the creator of the service request
        /// Only the creator can edit or delete (UC17E Note 1)
        /// </summary>
        public bool IsCreator(int serviceRequestId, int customerId)
        {
            var serviceRequest = _db.ServiceRequests.Find(serviceRequestId);
            return serviceRequest != null && serviceRequest.CreatedByCustomerId == customerId;
        }

        /// <summary>
        /// Checks if a service request can be edited or deleted
        /// BR37: Only when status is Open
        /// </summary>
        public bool CanEditOrDelete(int serviceRequestId)
        {
            var serviceRequest = _db.ServiceRequests.Find(serviceRequestId);
            if (serviceRequest == null) return false;

            var openStatus = _db.Status.FirstOrDefault(s => s.Key == ServiceRequestStatusKeys.Open);
            return openStatus != null && serviceRequest.StatusId == openStatus.Id;
        }

        /// <summary>
        /// Gets the SLA status summary for a service request
        /// </summary>
        public string GetSlaStatus(ServiceRequest serviceRequest)
        {
            if (serviceRequest.ResolutionDeadline == null)
                return "No SLA Set";

            var now = DateTime.Now;
            if (serviceRequest.DateResolved.HasValue)
            {
                if (serviceRequest.DateResolved.Value <= serviceRequest.ResolutionDeadline.Value)
                    return "Resolved Within SLA";
                else
                    return "Resolved - SLA Breached";
            }

            if (now > serviceRequest.ResolutionDeadline.Value)
                return "SLA Breached";

            var remaining = serviceRequest.ResolutionDeadline.Value - now;
            if (remaining.TotalHours <= 24)
                return $"Due in {remaining.Hours}h {remaining.Minutes}m";

            return $"Due in {remaining.Days} day(s)";
        }

        /// <summary>
        /// Adds working days (excluding weekends) to a date
        /// </summary>
        private DateTime AddWorkingDays(DateTime startDate, int workingDays)
        {
            var currentDate = startDate;
            int addedDays = 0;

            while (addedDays < workingDays)
            {
                currentDate = currentDate.AddDays(1);
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    addedDays++;
                }
            }

            return currentDate;
        }
    }
}
